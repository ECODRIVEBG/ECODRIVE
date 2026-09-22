using System.Collections.Generic;
using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IVeiculoRepository
    {
        List<Veiculo> ObterTodos();
        List<Veiculo> ObterPorTipo(string tipo);
        Veiculo ObterPorId(int id);
        Veiculo ObterPorChave(string chave);
        void Cadastrar(Veiculo veiculo);
        void AtualizarStatus(int id, string status);
        void Excluir(int id);
        int ContarPorStatus(string status);
    }
}