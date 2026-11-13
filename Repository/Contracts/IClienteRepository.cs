using System.Collections.Generic;
using EcoDriveTcc.Models;

namespace EcoDriveTcc.Repository.Contracts
{
    public interface IClienteRepository
    {
        Cliente Login(string Email, string Senha);

        void Cadastrar(Cliente cliente);

        void Atualizar(Cliente cliente);

        void Excluir(int Id);

        Cliente ObterCliente(int Id);

        IEnumerable<Cliente> ObterTodosClientes();

        void Ativar(int id);

        void Desativar(int id);

        Cliente BuscaCpfCliente(string CPF);

        Cliente BuscaEmailCliente(string email);
    }
}