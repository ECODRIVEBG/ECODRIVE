using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Repository
{
    public class PontoRepository : IPontoRepository
    {
        private readonly string _conexaoMySQL;

        public PontoRepository(IConfiguration configuration)
        {
            _conexaoMySQL = configuration.GetConnectionString("EcoDriveConnection");
        }

        public List<Ponto> ObterTodos() => Consultar("SELECT * FROM Ponto ORDER BY Bairro, Nome");
        public List<Ponto> ObterAtivos() => Consultar("SELECT * FROM Ponto WHERE Ativo = TRUE ORDER BY Bairro, Nome");

        public Ponto ObterPorId(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand("SELECT * FROM Ponto WHERE IdPonto = @Id", conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                using (var leitor = cmd.ExecuteReader())
                {
                    if (leitor.Read())
                        return Mapear(leitor);
                }
            }
            return null;
        }

        public void Cadastrar(Ponto ponto)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(
                    "INSERT INTO Ponto (Nome, Endereco, Bairro, Tipo, Ativo) VALUES (@Nome, @Endereco, @Bairro, @Tipo, TRUE)", conexao);
                cmd.Parameters.AddWithValue("@Nome", ponto.Nome);
                cmd.Parameters.AddWithValue("@Endereco", ponto.Endereco);
                cmd.Parameters.AddWithValue("@Bairro", ponto.Bairro);
                cmd.Parameters.AddWithValue("@Tipo", ponto.Tipo);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Ponto ponto)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(
                    "UPDATE Ponto SET Nome=@Nome, Endereco=@Endereco, Bairro=@Bairro, Tipo=@Tipo WHERE IdPonto=@Id", conexao);
                cmd.Parameters.AddWithValue("@Nome", ponto.Nome);
                cmd.Parameters.AddWithValue("@Endereco", ponto.Endereco);
                cmd.Parameters.AddWithValue("@Bairro", ponto.Bairro);
                cmd.Parameters.AddWithValue("@Tipo", ponto.Tipo);
                cmd.Parameters.AddWithValue("@Id", ponto.IdPonto);
                cmd.ExecuteNonQuery();
            }
        }

        public void Desativar(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand("UPDATE Ponto SET Ativo = FALSE WHERE IdPonto = @Id", conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private List<Ponto> Consultar(string sql)
        {
            var lista = new List<Ponto>();
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(sql, conexao);
                using (var leitor = cmd.ExecuteReader())
                {
                    while (leitor.Read())
                        lista.Add(Mapear(leitor));
                }
            }
            return lista;
        }

        private static Ponto Mapear(MySqlDataReader leitor)
        {
            return new Ponto
            {
                IdPonto = leitor.GetInt32("IdPonto"),
                Nome = leitor.GetString("Nome"),
                Endereco = leitor.GetString("Endereco"),
                Bairro = leitor.GetString("Bairro"),
                Tipo = leitor.GetString("Tipo"),
                Ativo = leitor.GetBoolean("Ativo")
            };
        }
    }
}
