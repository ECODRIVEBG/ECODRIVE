using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Repository
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly string _conexaoMySQL;

        private const string SelectBase =
            "SELECT u.IdUsuario, u.Nome, u.Email, u.Senha, u.CPF, c.IdCliente, c.NomeSocial, c.Bloqueado, c.Saldo " +
            "FROM Usuario u INNER JOIN Cliente c ON c.IdCliente = u.IdUsuario ";

        public ClienteRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("EcoDriveConnection");
        }

        public Cliente Login(string Email, string Senha)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    SelectBase + "WHERE u.Email = @Email AND u.Senha = @Senha AND c.Bloqueado = FALSE", conexao);

                cmd.Parameters.Add("@Email", MySqlDbType.VarChar).Value = (object)Email ?? DBNull.Value;
                cmd.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = (object)Senha ?? DBNull.Value;

                MySqlDataReader dr;

                Cliente cliente = new Cliente();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    cliente = Mapear(dr);
                }

                dr.Close();
                return cliente;
            }
        }

        public IEnumerable<Cliente> ObterTodosClientes()
        {
            List<Cliente> cliList = new List<Cliente>();

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
                    cliList.Add(
                        new Cliente
                        {
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            Nome = (string)(dr["Nome"]),
                            Email = Convert.ToString(dr["Email"]),
                            Senha = Convert.ToString(dr["Senha"]),
                            CPF = Convert.ToString(dr["CPF"]),
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            NomeSocial = Convert.ToString(dr["NomeSocial"]),
                            Bloqueado = Convert.ToBoolean(dr["Bloqueado"]),
                            Saldo = Convert.ToDecimal(dr["Saldo"])
                        });
                }
            }

            return cliList;
        }

        public void Cadastrar(Cliente cliente)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdUsuario = new MySqlCommand(
                        "INSERT INTO Usuario (Nome, Email, Senha, CPF) VALUES (@Nome, @Email, @Senha, @CPF); " +
                        "SELECT LAST_INSERT_ID();", conexao, transacao);

                    cmdUsuario.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = cliente.Nome;
                    cmdUsuario.Parameters.Add("@Email", MySqlDbType.VarChar).Value = cliente.Email;
                    cmdUsuario.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = cliente.Senha;
                    cmdUsuario.Parameters.Add("@CPF", MySqlDbType.VarChar).Value = cliente.CPF;

                    int idGerado = Convert.ToInt32(cmdUsuario.ExecuteScalar());

                    MySqlCommand cmdCliente = new MySqlCommand(
                        "INSERT INTO Cliente (IdCliente, NomeSocial, Bloqueado, Saldo) " +
                        "VALUES (@IdCliente, @NomeSocial, FALSE, 0.00)", conexao, transacao);

                    cmdCliente.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = idGerado;
                    cmdCliente.Parameters.Add("@NomeSocial", MySqlDbType.VarChar).Value = (object)cliente.NomeSocial ?? DBNull.Value;
                    cmdCliente.ExecuteNonQuery();

                    transacao.Commit();
                }

                conexao.Close();
            }
        }

        public void Atualizar(Cliente cliente)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdUsuario = new MySqlCommand(
                        "UPDATE Usuario SET Nome=@Nome, Email=@Email, Senha=@Senha, CPF=@CPF WHERE IdUsuario=@Id",
                        conexao, transacao);

                    cmdUsuario.Parameters.Add("@Id", MySqlDbType.Int32).Value = cliente.IdCliente;
                    cmdUsuario.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = cliente.Nome;
                    cmdUsuario.Parameters.Add("@Email", MySqlDbType.VarChar).Value = cliente.Email;
                    cmdUsuario.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = cliente.Senha;
                    cmdUsuario.Parameters.Add("@CPF", MySqlDbType.VarChar).Value = cliente.CPF;
                    cmdUsuario.ExecuteNonQuery();

                    MySqlCommand cmdCliente = new MySqlCommand(
                        "UPDATE Cliente SET NomeSocial=@NomeSocial WHERE IdCliente=@Id", conexao, transacao);

                    cmdCliente.Parameters.Add("@Id", MySqlDbType.Int32).Value = cliente.IdCliente;
                    cmdCliente.Parameters.Add("@NomeSocial", MySqlDbType.VarChar).Value = (object)cliente.NomeSocial ?? DBNull.Value;
                    cmdCliente.ExecuteNonQuery();

                    transacao.Commit();
                }

                conexao.Close();
            }
        }

        public void Excluir(int Id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdCliente = new MySqlCommand("DELETE FROM Cliente WHERE IdCliente=@Id", conexao, transacao);
                    cmdCliente.Parameters.Add("@Id", MySqlDbType.Int32).Value = Id;
                    cmdCliente.ExecuteNonQuery();

                    MySqlCommand cmdUsuario = new MySqlCommand("DELETE FROM Usuario WHERE IdUsuario=@Id", conexao, transacao);
                    cmdUsuario.Parameters.Add("@Id", MySqlDbType.Int32).Value = Id;
                    cmdUsuario.ExecuteNonQuery();

                    transacao.Commit();
                }

                conexao.Close();
            }
        }

        public Cliente ObterCliente(int Id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(SelectBase + "WHERE c.IdCliente = @Id", conexao);
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = Id;

                MySqlDataReader dr;

                Cliente cliente = new Cliente();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    cliente = Mapear(dr);
                }

                dr.Close();
                return cliente;
            }
        }

        public void Ativar(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("UPDATE Cliente SET Bloqueado = FALSE WHERE IdCliente = @Id", conexao);
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public void Desativar(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("UPDATE Cliente SET Bloqueado = TRUE WHERE IdCliente = @Id", conexao);
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public Cliente BuscaCpfCliente(string CPF)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT CPF FROM Usuario WHERE CPF = @CPF", conexao);
                cmd.Parameters.Add("@CPF", MySqlDbType.VarChar).Value = (object)CPF ?? DBNull.Value;

                MySqlDataReader dr;

                Cliente cliente = new Cliente();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    cliente.CPF = (string)(dr["CPF"]);
                }

                dr.Close();
                return cliente;
            }
        }

        public Cliente BuscaEmailCliente(string email)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT Email FROM Usuario WHERE Email = @Email", conexao);
                cmd.Parameters.Add("@Email", MySqlDbType.VarChar).Value = (object)email ?? DBNull.Value;

                MySqlDataReader dr;

                Cliente cliente = new Cliente();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    cliente.Email = (string)(dr["Email"]);
                }

                dr.Close();
                return cliente;
            }
        }

        private static Cliente Mapear(MySqlDataReader dr)
        {
            Cliente cliente = new Cliente();

            cliente.IdUsuario = Convert.ToInt32(dr["IdUsuario"]);
            cliente.Nome = (string)(dr["Nome"]);
            cliente.Email = Convert.ToString(dr["Email"]);
            cliente.Senha = Convert.ToString(dr["Senha"]);
            cliente.CPF = Convert.ToString(dr["CPF"]);
            cliente.IdCliente = Convert.ToInt32(dr["IdCliente"]);
            cliente.NomeSocial = Convert.ToString(dr["NomeSocial"]);
            cliente.Bloqueado = Convert.ToBoolean(dr["Bloqueado"]);
            cliente.Saldo = Convert.ToDecimal(dr["Saldo"]);

            return cliente;
        }
    }
}