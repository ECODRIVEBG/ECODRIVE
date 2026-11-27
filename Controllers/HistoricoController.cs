using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Controllers
{
    [ClienteAutorizacao]
    public class HistoricoController : Controller
    {
        private IHistoricoLocacaoRepository _repositoryHistorico;
        private LoginCliente _loginCliente;

        public HistoricoController(IHistoricoLocacaoRepository repositoryHistorico, LoginCliente loginCliente)
        {
            _repositoryHistorico = repositoryHistorico;
            _loginCliente = loginCliente;
        }

        public IActionResult Index()
        {
            var cliente = _loginCliente.GetCliente();
            return View(_repositoryHistorico.ObterPorCliente(cliente.IdCliente));
        }
    }
}