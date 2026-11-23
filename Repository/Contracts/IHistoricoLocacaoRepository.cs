using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IHistoricoLocacaoRepository
    {
        HistoricoLocacao ObterCorridaEmAndamento(int idCliente);
    }
}