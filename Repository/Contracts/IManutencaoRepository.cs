using System.Collections.Generic;
using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IManutencaoRepository
    {
        void Abrir(Manutencao manutencao);
        void Concluir(int idManutencao);
        List<Manutencao> ObterEmAberto();
    }
}