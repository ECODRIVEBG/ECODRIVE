using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Repository
{
    public class PontoRepository : IPontoRepository
    {
        private readonly string _conexaoMySQL;

        public PontoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("EcoDriveConnection");
        }

        public List<Ponto> ObterTodos()
        {
            return Consultar("SELECT * FROM Ponto ORDER BY Bairro, Nome");
        }

        public List<Ponto> ObterAtivos()
        {
            return Consultar("SELECT * FROM Ponto WHERE Ativo = TRUE ORDER BY Bairro, Nome");
        }

        public Ponto ObterPorId(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Ponto WHERE IdPonto = @Id", conexao);
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

                MySqlDataReader dr;

                Ponto ponto = null;

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    ponto = new Ponto();
                    ponto.IdPonto = Convert.ToInt32(dr["IdPonto"]);
                    ponto.Nome = Convert.ToString(dr["Nome"]);
                    ponto.Endereco = Convert.ToString(dr["Endereco"]);
                    ponto.Bairro = Convert.ToString(dr["Bairro"]);
                    ponto.Tipo = Convert.ToString(dr["Tipo"]);
                    ponto.Ativo = Convert.ToBoolean(dr["Ativo"]);
                }

                dr.Close();
                return ponto;
            }
        }

        public void Cadastrar(Ponto ponto)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO Ponto (Nome, Endereco, Bairro, Tipo, Ativo) VALUES (@Nome, @Endereco, @Bairro, @Tipo, TRUE)", conexao);

                cmd.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = ponto.Nome;
                cmd.Parameters.Add("@Endereco", MySqlDbType.VarChar).Value = ponto.Endereco;
                cmd.Parameters.Add("@Bairro", MySqlDbType.VarChar).Value = ponto.Bairro;
                cmd.Parameters.Add("@Tipo", MySqlDbType.VarChar).Value = ponto.Tipo;
                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public void Atualizar(Ponto ponto)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE Ponto SET Nome=@Nome, Endereco=@Endereco, Bairro=@Bairro, Tipo=@Tipo WHERE IdPonto=@Id", conexao);

                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = ponto.IdPonto;
                cmd.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = ponto.Nome;
                cmd.Parameters.Add("@Endereco", MySqlDbType.VarChar).Value = ponto.Endereco;
                cmd.Parameters.Add("@Bairro", MySqlDbType.VarChar).Value = ponto.Bairro;
                cmd.Parameters.Add("@Tipo", MySqlDbType.VarChar).Value = ponto.Tipo;
                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public void Desativar(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("UPDATE Ponto SET Ativo = FALSE WHERE IdPonto = @Id", conexao);
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        private List<Ponto> Consultar(string sql)
        {
            List<Ponto> pontoList = new List<Ponto>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conexao);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    pontoList.Add(
                        new Ponto
                        {
                            IdPonto = Convert.ToInt32(dr["IdPonto"]),
                            Nome = Convert.ToString(dr["Nome"]),
                            Endereco = Convert.ToString(dr["Endereco"]),
                            Bairro = Convert.ToString(dr["Bairro"]),
                            Tipo = Convert.ToString(dr["Tipo"]),
                            Ativo = Convert.ToBoolean(dr["Ativo"])
                        });
                }
            }

            return pontoList;
        }
    }
}