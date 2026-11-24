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

        public bool IniciarCorrida(HistoricoLocacao locacao)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdVeiculo = new MySqlCommand(
                        "UPDATE Veiculo SET Status_ = @EmUso WHERE IdVeiculo = @IdVeiculo AND Status_ = @Disponivel",
                        conexao, transacao);

                    cmdVeiculo.Parameters.Add("@EmUso", MySqlDbType.VarChar).Value = StatusVeiculoConstant.EmUso;
                    cmdVeiculo.Parameters.Add("@Disponivel", MySqlDbType.VarChar).Value = StatusVeiculoConstant.Disponivel;
                    cmdVeiculo.Parameters.Add("@IdVeiculo", MySqlDbType.Int32).Value = locacao.IdVeiculo;

                    if (cmdVeiculo.ExecuteNonQuery() == 0)
                    {
                        transacao.Rollback();
                        return false;
                    }

                    MySqlCommand cmdCorrida = new MySqlCommand(
                        "INSERT INTO Historico_loc (IdCliente, IdVeiculo, IdPontoInicio, InicioCorrida, Problema) " +
                        "VALUES (@IdCliente, @IdVeiculo, @IdPontoInicio, @InicioCorrida, FALSE)", conexao, transacao);

                    cmdCorrida.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = locacao.IdCliente;
                    cmdCorrida.Parameters.Add("@IdVeiculo", MySqlDbType.Int32).Value = locacao.IdVeiculo;
                    cmdCorrida.Parameters.Add("@IdPontoInicio", MySqlDbType.Int32).Value = locacao.IdPontoInicio;
                    cmdCorrida.Parameters.Add("@InicioCorrida", MySqlDbType.DateTime).Value = locacao.InicioCorrida;
                    cmdCorrida.ExecuteNonQuery();

                    transacao.Commit();
                }

                conexao.Close();
                return true;
            }
        }

        public HistoricoLocacao ObterCorridaEmAndamento(int idCliente)
        {
            List<HistoricoLocacao> lista = Consultar(
                SelectBase + "WHERE h.IdCliente = @IdCliente AND h.FimCorrida IS NULL", idCliente);

            return lista.Count > 0 ? lista[0] : null;
        }

        public decimal EncerrarCorrida(HistoricoLocacao corrida, int idPontoFim, DateTime fimCorrida, decimal valorCorrida, int? novaBateria)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                decimal novoSaldo;

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdCorrida = new MySqlCommand(
                        "UPDATE Historico_loc SET IdPontoFim = @IdPontoFim, FimCorrida = @FimCorrida, ValorCorrida = @Valor " +
                        "WHERE IdHistLoc = @IdHistLoc", conexao, transacao);

                    cmdCorrida.Parameters.Add("@IdPontoFim", MySqlDbType.Int32).Value = idPontoFim;
                    cmdCorrida.Parameters.Add("@FimCorrida", MySqlDbType.DateTime).Value = fimCorrida;
                    cmdCorrida.Parameters.Add("@Valor", MySqlDbType.Decimal).Value = valorCorrida;
                    cmdCorrida.Parameters.Add("@IdHistLoc", MySqlDbType.Int32).Value = corrida.IdHistLoc;
                    cmdCorrida.ExecuteNonQuery();

                    MySqlCommand cmdVeiculo = new MySqlCommand(
                        "UPDATE Veiculo SET Status_ = @Disponivel, IdPonto = @IdPontoFim, NivelBateria = @Bateria " +
                        "WHERE IdVeiculo = @IdVeiculo", conexao, transacao);

                    cmdVeiculo.Parameters.Add("@Disponivel", MySqlDbType.VarChar).Value = StatusVeiculoConstant.Disponivel;
                    cmdVeiculo.Parameters.Add("@IdPontoFim", MySqlDbType.Int32).Value = idPontoFim;
                    cmdVeiculo.Parameters.Add("@Bateria", MySqlDbType.Int32).Value = (object)novaBateria ?? DBNull.Value;
                    cmdVeiculo.Parameters.Add("@IdVeiculo", MySqlDbType.Int32).Value = corrida.IdVeiculo;
                    cmdVeiculo.ExecuteNonQuery();

                    MySqlCommand cmdSaldo = new MySqlCommand(
                        "UPDATE Cliente SET Saldo = Saldo - @Valor WHERE IdCliente = @IdCliente", conexao, transacao);

                    cmdSaldo.Parameters.Add("@Valor", MySqlDbType.Decimal).Value = valorCorrida;
                    cmdSaldo.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = corrida.IdCliente;
                    cmdSaldo.ExecuteNonQuery();

                    MySqlCommand cmdLer = new MySqlCommand(
                        "SELECT Saldo FROM Cliente WHERE IdCliente = @IdCliente", conexao, transacao);

                    cmdLer.Parameters.Add("@IdCliente", MySqlDbType.Int32).Value = corrida.IdCliente;

                    novoSaldo = Convert.ToDecimal(cmdLer.ExecuteScalar());

                    transacao.Commit();
                }

                conexao.Close();
                return novoSaldo;
            }
        }

        public void ReportarProblema(HistoricoLocacao corrida, DateTime fimCorrida)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdCorrida = new MySqlCommand(
                        "UPDATE Historico_loc SET FimCorrida = @FimCorrida, ValorCorrida = 0, Problema = TRUE " +
                        "WHERE IdHistLoc = @IdHistLoc", conexao, transacao);

                    cmdCorrida.Parameters.Add("@FimCorrida", MySqlDbType.DateTime).Value = fimCorrida;
                    cmdCorrida.Parameters.Add("@IdHistLoc", MySqlDbType.Int32).Value = corrida.IdHistLoc;
                    cmdCorrida.ExecuteNonQuery();

                    MySqlCommand cmdVeiculo = new MySqlCommand(
                        "UPDATE Veiculo SET Status_ = @Manutencao WHERE IdVeiculo = @IdVeiculo", conexao, transacao);

                    cmdVeiculo.Parameters.Add("@Manutencao", MySqlDbType.VarChar).Value = StatusVeiculoConstant.Manutencao;
                    cmdVeiculo.Parameters.Add("@IdVeiculo", MySqlDbType.Int32).Value = corrida.IdVeiculo;
                    cmdVeiculo.ExecuteNonQuery();

                    MySqlCommand cmdManutencao = new MySqlCommand(
                        "INSERT INTO Manutencao (IdVeiculo, IdFuncionario, DataInicio, Descricao, Concluida) " +
                        "VALUES (@IdVeiculo, NULL, NOW(), @Descricao, FALSE)", conexao, transacao);

                    cmdManutencao.Parameters.Add("@IdVeiculo", MySqlDbType.Int32).Value = corrida.IdVeiculo;
                    cmdManutencao.Parameters.Add("@Descricao", MySqlDbType.VarChar).Value = "Mau funcionamento reportado pelo cliente durante a corrida.";
                    cmdManutencao.ExecuteNonQuery();

                    transacao.Commit();
                }

                conexao.Close();
            }
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