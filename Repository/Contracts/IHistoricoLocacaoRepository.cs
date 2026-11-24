using System;
using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IHistoricoLocacaoRepository
    {
        bool IniciarCorrida(HistoricoLocacao locacao);
        HistoricoLocacao ObterCorridaEmAndamento(int idCliente);
        decimal EncerrarCorrida(HistoricoLocacao corrida, int idPontoFim, DateTime fimCorrida, decimal valorCorrida, int? novaBateria);
        void ReportarProblema(HistoricoLocacao corrida, DateTime fimCorrida);
    }
}