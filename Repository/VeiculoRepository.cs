using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Repository
{
    public class VeiculoRepository : IVeiculoRepository
    {
        private readonly string _conexaoMySQL;
        private const string SelectBase =
            "SELECT v.IdVeiculo, v.IdPonto, v.Tipo, v.Status_, v.NivelBateria, v.Chave, p.Nome AS NomePonto " +
            "FROM Veiculo v INNER JOIN Ponto p ON p.IdPonto = v.IdPonto ";

        public VeiculoRepository(IConfiguration configuration)
        {
            _conexaoMySQL = configuration.GetConnectionString("EcoDriveConnection");
        }

        public List<Veiculo> ObterTodos() => Consultar(SelectBase + "ORDER BY v.Tipo, v.IdVeiculo", null);

        public List<Veiculo> ObterPorTipo(string tipo) =>
            Consultar(SelectBase + "WHERE v.Tipo = @Tipo ORDER BY v.IdVeiculo", cmd => cmd.Parameters.AddWithValue("@Tipo", tipo));

        public Veiculo ObterPorId(int id)
        {
            var lista = Consultar(SelectBase + "WHERE v.IdVeiculo = @Id", cmd => cmd.Parameters.AddWithValue("@Id", id));
            return lista.Count > 0 ? lista[0] : null;
        }

        public Veiculo ObterPorChave(string chave)
        {
            var lista = Consultar(SelectBase + "WHERE v.Chave = @Chave", cmd => cmd.Parameters.AddWithValue("@Chave", chave));
            return lista.Count > 0 ? lista[0] : null;
        }

        public void Cadastrar(Veiculo veiculo)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(
                    "INSERT INTO Veiculo (IdPonto, Tipo, Status_, NivelBateria, Chave) " +
                    "VALUES (@IdPonto, @Tipo, 'Disponivel', @NivelBateria, @Chave)", conexao);
                cmd.Parameters.AddWithValue("@IdPonto", veiculo.IdPonto);
                cmd.Parameters.AddWithValue("@Tipo", veiculo.Tipo);
                cmd.Parameters.AddWithValue("@NivelBateria", (object)veiculo.NivelBateria ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Chave", veiculo.Chave);
                cmd.ExecuteNonQuery();
            }
        }

        public void AtualizarStatus(int id, string status)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand("UPDATE Veiculo SET Status_ = @Status WHERE IdVeiculo = @Id", conexao);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand("DELETE FROM Veiculo WHERE IdVeiculo = @Id", conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public int ContarPorStatus(string status)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand("SELECT COUNT(*) FROM Veiculo WHERE Status_ = @Status", conexao);
                cmd.Parameters.AddWithValue("@Status", status);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private List<Veiculo> Consultar(string sql, Action<MySqlCommand> configurarParametros)
        {
            var lista = new List<Veiculo>();
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(sql, conexao);
                configurarParametros?.Invoke(cmd);
                using (var leitor = cmd.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        lista.Add(new Veiculo
                        {
                            IdVeiculo = leitor.GetInt32("IdVeiculo"),
                            IdPonto = leitor.GetInt32("IdPonto"),
                            Tipo = leitor.GetString("Tipo"),
                            Status_ = leitor.GetString("Status_"),
                            NivelBateria = leitor.IsDBNull(leitor.GetOrdinal("NivelBateria")) ? (int?)null : leitor.GetInt32("NivelBateria"),
                            Chave = leitor.GetString("Chave"),
                            NomePonto = leitor.GetString("NomePonto")
                        });
                    }
                }
            }
            return lista;
        }
    }
}