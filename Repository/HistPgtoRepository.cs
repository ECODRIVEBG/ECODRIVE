using System;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Repository
{
    public class HistPgtoRepository : IHistPgtoRepository
    {
        private readonly string _conexaoMySQL;

        public HistPgtoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("EcoDriveConnection");
        }

        public decimal Recarregar(int idCliente, decimal valor)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                decimal novoSaldo;

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdRecarga = new MySqlCommand(
                        "INSERT INTO HistPgto (IdCliente, ValorRecarga, DataRecarga) VALUES (@IdCliente, @Valor, NOW())",
                        conexao, transacao);

                    cmdRecarga.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = idCliente;
                    cmdRecarga.Parameters.Add("@Valor", MySqlDbType.Decimal).Value = valor;
                    cmdRecarga.ExecuteNonQuery();

                    MySqlCommand cmdSaldo = new MySqlCommand(
                        "UPDATE Cliente SET Saldo = Saldo + @Valor WHERE IdCliente = @IdCliente", conexao, transacao);

                    cmdSaldo.Parameters.Add("@Valor", MySqlDbType.Decimal).Value = valor;
                    cmdSaldo.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = idCliente;
                    cmdSaldo.ExecuteNonQuery();

                    MySqlCommand cmdLer = new MySqlCommand(
                        "SELECT Saldo FROM Cliente WHERE IdCliente = @IdCliente", conexao, transacao);

                    cmdLer.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = idCliente;

                    novoSaldo = Convert.ToDecimal(cmdLer.ExecuteScalar());

                    transacao.Commit();
                }

                conexao.Close();
                return novoSaldo;
            }
        }
    }
}