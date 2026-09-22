using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using EcoDriveTcc.Models;
using EcoDriveTcc.Models.Constants;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Repository
{
    public class FuncionarioRepository : IFuncionarioRepository
    {
        private readonly string _conexaoMySQL;

        public FuncionarioRepository(IConfiguration configuration)
        {
            _conexaoMySQL = configuration.GetConnectionString("EcoDriveConnection");
        }

        public void Cadastrar(Funcionario funcionario)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    var cmdUsuario = new MySqlCommand(
                        "INSERT INTO Usuario (Nome, Email, Senha, CPF) VALUES (@Nome, @Email, @Senha, @CPF); " +
                        "SELECT LAST_INSERT_ID();", conexao, transacao);
                    cmdUsuario.Parameters.AddWithValue("@Nome", funcionario.Nome);
                    cmdUsuario.Parameters.AddWithValue("@Email", funcionario.Email);
                    cmdUsuario.Parameters.AddWithValue("@Senha", funcionario.Senha);
                    cmdUsuario.Parameters.AddWithValue("@CPF", funcionario.CPF);

                    int idGerado = System.Convert.ToInt32(cmdUsuario.ExecuteScalar());

                    var cmdFuncionario = new MySqlCommand(
                        "INSERT INTO Funcionario (IdFuncionario, Cargo, Telefone, NivelAcesso) " +
                        "VALUES (@IdFuncionario, @Cargo, @Telefone, @NivelAcesso);", conexao, transacao);
                    cmdFuncionario.Parameters.AddWithValue("@IdFuncionario", idGerado);
                    cmdFuncionario.Parameters.AddWithValue("@Cargo", funcionario.Cargo);
                    cmdFuncionario.Parameters.AddWithValue("@Telefone", (object)funcionario.Telefone ?? System.DBNull.Value);
                    cmdFuncionario.Parameters.AddWithValue("@NivelAcesso", funcionario.NivelAcesso ?? NivelAcessoConstant.Funcionario);
                    cmdFuncionario.ExecuteNonQuery();

                    transacao.Commit();
                }
            }
        }

        public Funcionario Login(string email, string senha)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(
                    "SELECT u.IdUsuario, u.Nome, u.Email, u.Senha, u.CPF, f.IdFuncionario, f.Cargo, f.Telefone, f.NivelAcesso " +
                    "FROM Usuario u INNER JOIN Funcionario f ON f.IdFuncionario = u.IdUsuario " +
                    "WHERE u.Email = @Email AND u.Senha = @Senha", conexao);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Senha", senha);

                using (var leitor = cmd.ExecuteReader())
                {
                    if (leitor.Read())
                        return Mapear(leitor);
                }
            }
            return null;
        }

        public Funcionario ObterPorId(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(
                    "SELECT u.IdUsuario, u.Nome, u.Email, u.Senha, u.CPF, f.IdFuncionario, f.Cargo, f.Telefone, f.NivelAcesso " +
                    "FROM Usuario u INNER JOIN Funcionario f ON f.IdFuncionario = u.IdUsuario WHERE f.IdFuncionario = @Id", conexao);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var leitor = cmd.ExecuteReader())
                {
                    if (leitor.Read())
                        return Mapear(leitor);
                }
            }
            return null;
        }

        public List<Funcionario> ObterTodos()
        {
            var lista = new List<Funcionario>();
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                var cmd = new MySqlCommand(
                    "SELECT u.IdUsuario, u.Nome, u.Email, u.Senha, u.CPF, f.IdFuncionario, f.Cargo, f.Telefone, f.NivelAcesso " +
                    "FROM Usuario u INNER JOIN Funcionario f ON f.IdFuncionario = u.IdUsuario ORDER BY u.Nome", conexao);

                using (var leitor = cmd.ExecuteReader())
                {
                    while (leitor.Read())
                        lista.Add(Mapear(leitor));
                }
            }
            return lista;
        }

        public void Atualizar(Funcionario funcionario)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    var cmdUsuario = new MySqlCommand(
                        "UPDATE Usuario SET Nome=@Nome, Email=@Email, CPF=@CPF WHERE IdUsuario=@Id", conexao, transacao);
                    cmdUsuario.Parameters.AddWithValue("@Nome", funcionario.Nome);
                    cmdUsuario.Parameters.AddWithValue("@Email", funcionario.Email);
                    cmdUsuario.Parameters.AddWithValue("@CPF", funcionario.CPF);
                    cmdUsuario.Parameters.AddWithValue("@Id", funcionario.IdFuncionario);
                    cmdUsuario.ExecuteNonQuery();

                    var cmdFuncionario = new MySqlCommand(
                        "UPDATE Funcionario SET Cargo=@Cargo, Telefone=@Telefone, NivelAcesso=@NivelAcesso WHERE IdFuncionario=@Id",
                        conexao, transacao);
                    cmdFuncionario.Parameters.AddWithValue("@Cargo", funcionario.Cargo);
                    cmdFuncionario.Parameters.AddWithValue("@Telefone", (object)funcionario.Telefone ?? System.DBNull.Value);
                    cmdFuncionario.Parameters.AddWithValue("@NivelAcesso", funcionario.NivelAcesso);
                    cmdFuncionario.Parameters.AddWithValue("@Id", funcionario.IdFuncionario);
                    cmdFuncionario.ExecuteNonQuery();

                    transacao.Commit();
                }
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    var cmdFuncionario = new MySqlCommand("DELETE FROM Funcionario WHERE IdFuncionario=@Id", conexao, transacao);
                    cmdFuncionario.Parameters.AddWithValue("@Id", id);
                    cmdFuncionario.ExecuteNonQuery();

                    var cmdUsuario = new MySqlCommand("DELETE FROM Usuario WHERE IdUsuario=@Id", conexao, transacao);
                    cmdUsuario.Parameters.AddWithValue("@Id", id);
                    cmdUsuario.ExecuteNonQuery();

                    transacao.Commit();
                }
            }
        }

        private static Funcionario Mapear(MySqlDataReader leitor)
        {
            return new Funcionario
            {
                IdUsuario = leitor.GetInt32("IdUsuario"),
                Nome = leitor.GetString("Nome"),
                Email = leitor.GetString("Email"),
                Senha = leitor.GetString("Senha"),
                CPF = leitor.GetString("CPF"),
                IdFuncionario = leitor.GetInt32("IdFuncionario"),
                Cargo = leitor.GetString("Cargo"),
                Telefone = leitor.IsDBNull(leitor.GetOrdinal("Telefone")) ? null : leitor.GetString("Telefone"),
                NivelAcesso = leitor.GetString("NivelAcesso")
            };
        }
    }
}