using System.Collections.Generic;
using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IFuncionarioRepository
    {
        void Cadastrar(Funcionario funcionario);
        Funcionario Login(string email, string senha);
        Funcionario ObterPorId(int id);
        List<Funcionario> ObterTodos();
        void Atualizar(Funcionario funcionario);
        void Excluir(int id);
    }
}