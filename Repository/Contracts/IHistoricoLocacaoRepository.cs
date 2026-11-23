using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IHistoricoLocacaoRepository
    {
        bool IniciarCorrida(HistoricoLocacao locacao);
        HistoricoLocacao ObterCorridaEmAndamento(int idCliente);
    }
}