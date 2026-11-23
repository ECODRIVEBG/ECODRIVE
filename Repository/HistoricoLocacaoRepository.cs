using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Repository
{
    public class HistoricoLocacaoRepository : IHistoricoLocacaoRepository
    {
        private readonly string _conexaoMySQL;

        private const string SelectBase =
            "SELECT h.IdHistLoc, h.IdCliente, h.IdVeiculo, h.IdPontoInicio, h.IdPontoFim, h.InicioCorrida, h.FimCorrida, " +
            "h.ValorCorrida, h.Problema, v.Chave AS TokenVeiculo, v.Tipo AS TipoVeiculo, " +
            "pini.Nome AS NomePontoInicio, pfim.Nome AS NomePontoFim " +
            "FROM Historico_loc h " +
            "INNER JOIN Veiculo v ON v.IdVeiculo = h.IdVeiculo " +
            "INNER JOIN Ponto pini ON pini.IdPonto = h.IdPontoInicio " +
            "LEFT JOIN Ponto pfim ON pfim.IdPonto = h.IdPontoFim ";

        public HistoricoLocacaoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("EcoDriveConnection");
        }

        public HistoricoLocacao ObterCorridaEmAndamento(int idCliente)
        {
            List<HistoricoLocacao> lista = Consultar(
                SelectBase + "WHERE h.IdCliente = @IdCliente AND h.FimCorrida IS NULL", idCliente);

            return lista.Count > 0 ? lista[0] : null;
        }

        private List<HistoricoLocacao> Consultar(string sql, int idCliente)
        {
            List<HistoricoLocacao> histList = new List<HistoricoLocacao>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = idCliente;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    histList.Add(
                        new HistoricoLocacao
                        {
                            IdHistLoc = Convert.ToInt32(dr["IdHistLoc"]),
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            IdVeiculo = Convert.ToInt32(dr["IdVeiculo"]),
                            IdPontoInicio = Convert.ToInt32(dr["IdPontoInicio"]),
                            IdPontoFim = dr["IdPontoFim"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["IdPontoFim"]),
                            InicioCorrida = Convert.ToDateTime(dr["InicioCorrida"]),
                            FimCorrida = dr["FimCorrida"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FimCorrida"]),
                            ValorCorrida = dr["ValorCorrida"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["ValorCorrida"]),
                            Problema = Convert.ToBoolean(dr["Problema"]),
                            TokenVeiculo = Convert.ToString(dr["TokenVeiculo"]),
                            TipoVeiculo = Convert.ToString(dr["TipoVeiculo"]),
                            NomePontoInicio = Convert.ToString(dr["NomePontoInicio"]),
                            NomePontoFim = dr["NomePontoFim"] == DBNull.Value ? null : Convert.ToString(dr["NomePontoFim"])
                        });
                }
            }

            return histList;
        }
    }
}