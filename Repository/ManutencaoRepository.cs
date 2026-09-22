using System;
using System.Collections.Generic;
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

        public ManutencaoRepository(IConfiguration configuration)
        {
            _conexaoMySQL = configuration.GetConnectionString("EcoDriveConnection");
        }

        public void Abrir(Manutencao manutencao)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    var cmdManutencao = new MySqlCommand(
                        "INSERT INTO Manutencao (IdVeiculo, IdFuncionario, DataInicio, Descricao, Concluida) " +
                        "VALUES (@IdVeiculo, @IdFuncionario, NOW(), @Descricao, FALSE)", conexao, transacao);
                    cmdManutencao.Parameters.AddWithValue("@IdVeiculo", manutencao.IdVeiculo);
                    cmdManutencao.Parameters.AddWithValue("@IdFuncionario", manutencao.IdFuncionario);
                    cmdManutencao.Parameters.AddWithValue("@Descricao", (object)manutencao.Descricao ?? DBNull.Value);
                    cmdManutencao.ExecuteNonQuery();

                    var cmdVeiculo = new MySqlCommand(
                        "UPDATE Veiculo SET Status_ = @Status WHERE IdVeiculo = @IdVeiculo", conexao, transacao);
                    cmdVeiculo.Parameters.AddWithValue("@Status", StatusVeiculoConstant.Manutencao);
                    cmdVeiculo.Parameters.AddWithValue("@IdVeiculo", manutencao.IdVeiculo);
                    cmdVeiculo.ExecuteNonQuery();

                    transacao.Commit();
                }
            }
        }

        public void Concluir(int idManutencao)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    var cmdBuscaVeiculo = new MySqlCommand(
                        "SELECT IdVeiculo FROM Manutencao WHERE IdManutencao = @Id", conexao, transacao);
                    cmdBuscaVeiculo.Parameters.AddWithValue("@Id", idManutencao);
                    int idVeiculo = Convert.ToInt32(cmdBuscaVeiculo.ExecuteScalar());

                    var cmdManutencao = new MySqlCommand(
                        "UPDATE Manutencao SET Concluida = TRUE, DataFim = NOW() WHERE IdManutencao = @Id", conexao, transacao);
                    cmdManutencao.Parameters.AddWithValue("@Id", idManutencao);
                    cmdManutencao.ExecuteNonQuery();

                    var cmdVeiculo = new MySqlCommand(
                        "UPDATE Veiculo SET Status_ = @Status WHERE IdVeiculo = @IdVeiculo", conexao, transacao);
                    cmdVeiculo.Parameters.AddWithValue("@Status", StatusVeiculoConstant.Disponivel);
                    cmdVeiculo.Parameters.AddWithValue("@IdVeiculo", idVeiculo);
                    cmdVeiculo.ExecuteNonQuery();

                    transacao.Commit();
                }
            }
        }

        public List<Manutencao> ObterEmAberto()
        {
            var lista = new List<Manutencao>();
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(
                    "SELECT m.IdManutencao, m.IdVeiculo, m.IdFuncionario, m.DataInicio, m.DataFim, m.Descricao, m.Concluida, " +
                    "v.Chave AS TokenVeiculo, v.Tipo AS TipoVeiculo, p.Nome AS NomePonto " +
                    "FROM Manutencao m " +
                    "INNER JOIN Veiculo v ON v.IdVeiculo = m.IdVeiculo " +
                    "INNER JOIN Ponto p ON p.IdPonto = v.IdPonto " +
                    "WHERE m.Concluida = FALSE ORDER BY m.DataInicio", conexao);

                using (var leitor = cmd.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        lista.Add(new Manutencao
                        {
                            IdManutencao = leitor.GetInt32("IdManutencao"),
                            IdVeiculo = leitor.GetInt32("IdVeiculo"),
                            IdFuncionario = leitor.GetInt32("IdFuncionario"),
                            DataInicio = leitor.GetDateTime("DataInicio"),
                            Descricao = leitor.IsDBNull(leitor.GetOrdinal("Descricao")) ? null : leitor.GetString("Descricao"),
                            Concluida = leitor.GetBoolean("Concluida"),
                            TokenVeiculo = leitor.GetString("TokenVeiculo"),
                            TipoVeiculo = leitor.GetString("TipoVeiculo"),
                            NomePonto = leitor.GetString("NomePonto")
                        });
                    }
                }
            }
            return lista;
        }
    }
}