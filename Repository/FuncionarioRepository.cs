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
    public class FuncionarioRepository : IFuncionarioRepository
    {
        private readonly string _conexaoMySQL;

        private const string SelectBase =
            "SELECT u.IdUsuario, u.Nome, u.Email, u.Senha, u.CPF, f.IdFuncionario, f.Cargo, f.Telefone, f.NivelAcesso " +
            "FROM Usuario u INNER JOIN Funcionario f ON f.IdFuncionario = u.IdUsuario ";

        public FuncionarioRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("EcoDriveConnection");
        }

        public Funcionario Login(string Email, string Senha)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    SelectBase + "WHERE u.Email = @Email AND u.Senha = @Senha", conexao);

                cmd.Parameters.Add("@Email", MySqlDbType.VarChar).Value = (object)Email ?? DBNull.Value;
                cmd.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = (object)Senha ?? DBNull.Value;

                MySqlDataReader dr;

                Funcionario funcionario = new Funcionario();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    funcionario = Mapear(dr);
                }

                dr.Close();
                return funcionario;
            }
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

        public void Excluir(int Id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    var cmdFuncionario = new MySqlCommand("DELETE FROM Funcionario WHERE IdFuncionario=@Id", conexao, transacao);
                    cmdFuncionario.Parameters.AddWithValue("@Id", Id);
                    cmdFuncionario.ExecuteNonQuery();

                    var cmdUsuario = new MySqlCommand("DELETE FROM Usuario WHERE IdUsuario=@Id", conexao, transacao);
                    cmdUsuario.Parameters.AddWithValue("@Id", Id);
                    cmdUsuario.ExecuteNonQuery();

                    transacao.Commit();
                }
            }
        }

        public Funcionario ObterFuncionario(int Id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(SelectBase + "WHERE f.IdFuncionario = @Id", conexao);
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = Id;

                MySqlDataReader dr;

                Funcionario funcionario = new Funcionario();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    funcionario = Mapear(dr);
                }

                dr.Close();
                return funcionario;
            }
        }

        public IEnumerable<Funcionario> ObterTodosFuncionarios()
        {
            List<Funcionario> funcList = new List<Funcionario>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(SelectBase + "ORDER BY u.Nome", conexao);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    funcList.Add(MapearLinha(dr));
                }
            }

            return funcList;
        }

        private static Funcionario Mapear(MySqlDataReader dr)
        {
            Funcionario funcionario = new Funcionario();

            funcionario.IdUsuario = Convert.ToInt32(dr["IdUsuario"]);
            funcionario.Nome = (string)(dr["Nome"]);
            funcionario.Email = Convert.ToString(dr["Email"]);
            funcionario.Senha = Convert.ToString(dr["Senha"]);
            funcionario.CPF = Convert.ToString(dr["CPF"]);
            funcionario.IdFuncionario = Convert.ToInt32(dr["IdFuncionario"]);
            funcionario.Cargo = Convert.ToString(dr["Cargo"]);
            funcionario.Telefone = Convert.ToString(dr["Telefone"]);
            funcionario.NivelAcesso = Convert.ToString(dr["NivelAcesso"]);

            return funcionario;
        }

        private static Funcionario MapearLinha(DataRow dr)
        {
            Funcionario funcionario = new Funcionario();

            funcionario.IdUsuario = Convert.ToInt32(dr["IdUsuario"]);
            funcionario.Nome = (string)(dr["Nome"]);
            funcionario.Email = Convert.ToString(dr["Email"]);
            funcionario.Senha = Convert.ToString(dr["Senha"]);
            funcionario.CPF = Convert.ToString(dr["CPF"]);
            funcionario.IdFuncionario = Convert.ToInt32(dr["IdFuncionario"]);
            funcionario.Cargo = Convert.ToString(dr["Cargo"]);
            funcionario.Telefone = Convert.ToString(dr["Telefone"]);
            funcionario.NivelAcesso = Convert.ToString(dr["NivelAcesso"]);

            return funcionario;
        }
    }
}