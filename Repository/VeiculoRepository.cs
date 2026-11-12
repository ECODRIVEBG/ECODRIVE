using System;
using System.Collections.Generic;
using System.Data;
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

        public VeiculoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("EcoDriveConnection");
        }

        public List<Veiculo> ObterTodos()
        {
            return Consultar(SelectBase + "ORDER BY v.Tipo, v.IdVeiculo", null, null);
        }

        public List<Veiculo> ObterPorTipo(string tipo)
        {
            return Consultar(SelectBase + "WHERE v.Tipo = @Tipo ORDER BY v.IdVeiculo", "@Tipo", tipo);
        }

        public Veiculo ObterPorId(int id)
        {
            List<Veiculo> lista = Consultar(SelectBase + "WHERE v.IdVeiculo = @Id", "@Id", id);
            return lista.Count > 0 ? lista[0] : null;
        }

        public Veiculo ObterPorChave(string chave)
        {
            List<Veiculo> lista = Consultar(SelectBase + "WHERE v.Chave = @Chave", "@Chave", chave);
            return lista.Count > 0 ? lista[0] : null;
        }

        public void Cadastrar(Veiculo veiculo)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO Veiculo (IdPonto, Tipo, Status_, NivelBateria, Chave) " +
                    "VALUES (@IdPonto, @Tipo, 'Disponivel', @NivelBateria, @Chave)", conexao);

                cmd.Parameters.Add("@IdPonto", MySqlDbType.Int32).Value = veiculo.IdPonto;
                cmd.Parameters.Add("@Tipo", MySqlDbType.VarChar).Value = veiculo.Tipo;
                cmd.Parameters.Add("@NivelBateria", MySqlDbType.Int32).Value = (object)veiculo.NivelBateria ?? DBNull.Value;
                cmd.Parameters.Add("@Chave", MySqlDbType.VarChar).Value = veiculo.Chave;
                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public void AtualizarStatus(int id, string status)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("UPDATE Veiculo SET Status_ = @Status WHERE IdVeiculo = @Id", conexao);
                cmd.Parameters.Add("@Status", MySqlDbType.VarChar).Value = status;
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("DELETE FROM Veiculo WHERE IdVeiculo = @Id", conexao);
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public int ContarPorStatus(string status)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Veiculo WHERE Status_ = @Status", conexao);
                cmd.Parameters.Add("@Status", MySqlDbType.VarChar).Value = status;

                int total = Convert.ToInt32(cmd.ExecuteScalar());

                conexao.Close();
                return total;
            }
        }

        public int ContarBateriaBaixa(int limite)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM Veiculo WHERE NivelBateria IS NOT NULL AND NivelBateria < @Limite", conexao);
                cmd.Parameters.Add("@Limite", MySqlDbType.Int32).Value = limite;

                int total = Convert.ToInt32(cmd.ExecuteScalar());

                conexao.Close();
                return total;
            }
        }

        private List<Veiculo> Consultar(string sql, string nomeParametro, object valorParametro)
        {
            List<Veiculo> veiList = new List<Veiculo>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conexao);

                if (nomeParametro != null)
                {
                    cmd.Parameters.AddWithValue(nomeParametro, valorParametro);
                }

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    veiList.Add(
                        new Veiculo
                        {
                            IdVeiculo = Convert.ToInt32(dr["IdVeiculo"]),
                            IdPonto = Convert.ToInt32(dr["IdPonto"]),
                            Tipo = Convert.ToString(dr["Tipo"]),
                            Status_ = Convert.ToString(dr["Status_"]),
                            NivelBateria = dr["NivelBateria"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["NivelBateria"]),
                            Chave = Convert.ToString(dr["Chave"]),
                            NomePonto = Convert.ToString(dr["NomePonto"])
                        });
                }
            }

            return veiList;
        }
    }
}