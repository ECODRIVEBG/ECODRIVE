using System.Collections.Generic;
using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IFuncionarioRepository
    {
        Funcionario Login(string Email, string Senha);

        void Cadastrar(Funcionario funcionario);

        void Atualizar(Funcionario funcionario);

        void Excluir(int Id);

        Funcionario ObterFuncionario(int Id);

        IEnumerable<Funcionario> ObterTodosFuncionarios();
    }
}