using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Areas.Colaborador.Controllers
{
    [Area("Colaborador")]
    [FuncionarioAutorizacao]
    public class ClienteController : Controller
    {
        private IClienteRepository _repositoryCliente;
        private LoginFuncionario _loginFuncionario;

        public ClienteController(IClienteRepository repositoryCliente, LoginFuncionario loginFuncionario)
        {
            _repositoryCliente = repositoryCliente;
            _loginFuncionario = loginFuncionario;
        }

        public IActionResult Index()
        {
            Funcionario funcionarioLogado = _loginFuncionario.GetFuncionario();
            ViewBag.Nome = funcionarioLogado.Nome;
            ViewBag.NivelAcesso = funcionarioLogado.NivelAcesso;

            return View(_repositoryCliente.ObterTodosClientes());
        }

        [ValidateHttpReferer]
        public IActionResult Ativar(int id)
        {
            _repositoryCliente.Ativar(id);

            TempData["MSG_S"] = "🆗";
            return RedirectToAction("Index");
        }

        [ValidateHttpReferer]
        public IActionResult Desativar(int id)
        {
            _repositoryCliente.Desativar(id);

            TempData["MSG_S"] = "🆗";
            return RedirectToAction("Index");
        }
    }
}