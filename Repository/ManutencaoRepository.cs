using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using EcoDriveTcc.Models;
using EcoDriveTcc.Models.Constants;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Repository
{
    public class ManutencaoRepository : IManutencaoRepository
    {
        private readonly string _conexaoMySQL;

        public ManutencaoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("EcoDriveConnection");
        }

        public void Abrir(Manutencao manutencao)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdManutencao = new MySqlCommand(
                        "INSERT INTO Manutencao (IdVeiculo, IdFuncionario, DataInicio, Descricao, Concluida) " +
                        "VALUES (@IdVeiculo, @IdFuncionario, NOW(), @Descricao, FALSE)", conexao, transacao);

                    cmdManutencao.Parameters.Add("@IdVeiculo", MySqlDbType.Int32).Value = manutencao.IdVeiculo;
                    cmdManutencao.Parameters.Add("@IdFuncionario", MySqlDbType.Int32).Value = (object)manutencao.IdFuncionario ?? DBNull.Value;
                    cmdManutencao.Parameters.Add("@Descricao", MySqlDbType.VarChar).Value = (object)manutencao.Descricao ?? DBNull.Value;
                    cmdManutencao.ExecuteNonQuery();

                    MySqlCommand cmdVeiculo = new MySqlCommand(
                        "UPDATE Veiculo SET Status_ = @Status WHERE IdVeiculo = @IdVeiculo", conexao, transacao);

                    cmdVeiculo.Parameters.Add("@Status", MySqlDbType.VarChar).Value = StatusVeiculoConstant.Manutencao;
                    cmdVeiculo.Parameters.Add("@IdVeiculo", MySqlDbType.Int32).Value = manutencao.IdVeiculo;
                    cmdVeiculo.ExecuteNonQuery();

                    transacao.Commit();
                }

                conexao.Close();
            }
        }

        public void Concluir(int idManutencao)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdBuscaVeiculo = new MySqlCommand(
                        "SELECT IdVeiculo FROM Manutencao WHERE IdManutencao = @Id", conexao, transacao);

                    cmdBuscaVeiculo.Parameters.Add("@Id", MySqlDbType.Int32).Value = idManutencao;

                    int idVeiculo = Convert.ToInt32(cmdBuscaVeiculo.ExecuteScalar());

                    MySqlCommand cmdManutencao = new MySqlCommand(
                        "UPDATE Manutencao SET Concluida = TRUE, DataFim = NOW() WHERE IdManutencao = @Id", conexao, transacao);

                    cmdManutencao.Parameters.Add("@Id", MySqlDbType.Int32).Value = idManutencao;
                    cmdManutencao.ExecuteNonQuery();

                    MySqlCommand cmdVeiculo = new MySqlCommand(
                        "UPDATE Veiculo SET Status_ = @Status WHERE IdVeiculo = @IdVeiculo", conexao, transacao);

                    cmdVeiculo.Parameters.Add("@Status", MySqlDbType.VarChar).Value = StatusVeiculoConstant.Disponivel;
                    cmdVeiculo.Parameters.Add("@IdVeiculo", MySqlDbType.Int32).Value = idVeiculo;
                    cmdVeiculo.ExecuteNonQuery();

                    transacao.Commit();
                }

                conexao.Close();
            }
        }

        public List<Manutencao> ObterEmAberto()
        {
            List<Manutencao> manList = new List<Manutencao>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT m.IdManutencao, m.IdVeiculo, m.IdFuncionario, m.DataInicio, m.DataFim, m.Descricao, m.Concluida, " +
                    "v.Chave AS TokenVeiculo, v.Tipo AS TipoVeiculo, p.Nome AS NomePonto " +
                    "FROM Manutencao m " +
                    "INNER JOIN Veiculo v ON v.IdVeiculo = m.IdVeiculo " +
                    "INNER JOIN Ponto p ON p.IdPonto = v.IdPonto " +
                    "WHERE m.Concluida = FALSE ORDER BY m.DataInicio", conexao);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    manList.Add(
                        new Manutencao
                        {
                            IdManutencao = Convert.ToInt32(dr["IdManutencao"]),
                            IdVeiculo = Convert.ToInt32(dr["IdVeiculo"]),
                            IdFuncionario = dr["IdFuncionario"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["IdFuncionario"]),
                            DataInicio = Convert.ToDateTime(dr["DataInicio"]),
                            DataFim = dr["DataFim"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["DataFim"]),
                            Descricao = dr["Descricao"] == DBNull.Value ? null : Convert.ToString(dr["Descricao"]),
                            Concluida = Convert.ToBoolean(dr["Concluida"]),
                            TokenVeiculo = Convert.ToString(dr["TokenVeiculo"]),
                            TipoVeiculo = Convert.ToString(dr["TipoVeiculo"]),
                            NomePonto = Convert.ToString(dr["NomePonto"])
                        });
                }
            }

            return manList;
        }
    }
}