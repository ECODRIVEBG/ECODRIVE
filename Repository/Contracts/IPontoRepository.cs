using System.Collections.Generic;
using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IPontoRepository
    {
        List<Ponto> ObterTodos();
        List<Ponto> ObterAtivos();
        Ponto ObterPorId(int id);
        void Cadastrar(Ponto ponto);
        void Atualizar(Ponto ponto);
        void Desativar(int id);
    }
}