using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models;
using EcoDriveTcc.Models.Constants;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Controllers
{
    [ClienteAutorizacao]
    public class AlugarController : Controller
    {
        private IPontoRepository _repositoryPonto;
        private IClienteRepository _repositoryCliente;
        private LoginCliente _loginCliente;

        public AlugarController(
            IPontoRepository repositoryPonto,
            IClienteRepository repositoryCliente,
            LoginCliente loginCliente)
        {
            _repositoryPonto = repositoryPonto;
            _repositoryCliente = repositoryCliente;
            _loginCliente = loginCliente;
        }

        private Cliente ObterClienteAtual()
        {
            var cliente = _repositoryCliente.ObterCliente(_loginCliente.GetCliente().IdCliente);

            if (cliente.IdCliente == 0 || cliente.Bloqueado)
            {
                _loginCliente.Logout();
                return null;
            }

            _loginCliente.Login(cliente);
            return cliente;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cliente = ObterClienteAtual();
            if (cliente == null)
                return RedirectToAction("Login", "Home");

            ViewBag.Pontos = _repositoryPonto.ObterAtivos();
            ViewBag.SaldoInsuficiente = cliente.Saldo < TarifaConstant.ValorPorMinuto;

            return View("Localizar");
        }
    }
}