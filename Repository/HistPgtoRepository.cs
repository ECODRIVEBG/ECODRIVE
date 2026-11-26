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

                decimal novoSaldo = 0;

                MySqlCommand cmdRecarga = new MySqlCommand(
                    "INSERT INTO HistPgto (IdCliente, ValorRecarga, DataRecarga) VALUES (@IdCliente, @Valor, NOW())", conexao);

                cmdRecarga.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = idCliente;
                cmdRecarga.Parameters.Add("@Valor", MySqlDbType.Decimal).Value = valor;
                cmdRecarga.ExecuteNonQuery();

                MySqlCommand cmdSaldo = new MySqlCommand(
                    "UPDATE Cliente SET Saldo = Saldo + @Valor WHERE IdCliente = @IdCliente", conexao);

                cmdSaldo.Parameters.Add("@Valor", MySqlDbType.Decimal).Value = valor;
                cmdSaldo.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = idCliente;
                cmdSaldo.ExecuteNonQuery();

                MySqlCommand cmdLer = new MySqlCommand(
                    "SELECT Saldo FROM Cliente WHERE IdCliente = @IdCliente", conexao);

                cmdLer.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = idCliente;

                MySqlDataReader dr = cmdLer.ExecuteReader();
                while (dr.Read())
                {
                    novoSaldo = Convert.ToDecimal(dr["Saldo"]);
                }
                dr.Close();

                conexao.Close();
                return novoSaldo;
            }
        }
    }
}