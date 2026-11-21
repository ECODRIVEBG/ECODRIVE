using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Controllers
{
    public class HomeController : Controller
    {
        private IPontoRepository _repositoryPonto;
        private IClienteRepository _repositoryCliente;
        private IHistPgtoRepository _repositoryHistPgto;
        private LoginCliente _loginCliente;

        public HomeController(
            IPontoRepository repositoryPonto,
            IClienteRepository repositoryCliente,
            IHistPgtoRepository repositoryHistPgto,
            LoginCliente loginCliente)
        {
            _repositoryPonto = repositoryPonto;
            _repositoryCliente = repositoryCliente;
            _repositoryHistPgto = repositoryHistPgto;
            _loginCliente = loginCliente;
        }

        public IActionResult Index()
        {
            List<Ponto> pontos = new List<Ponto>(_repositoryPonto.ObterAtivos());
            pontos.Sort((a, b) => a.IdPonto.CompareTo(b.IdPonto));

            Dictionary<string, List<Ponto>> bairros = new Dictionary<string, List<Ponto>>();

            foreach (Ponto ponto in pontos)
            {
                if (!bairros.ContainsKey(ponto.Bairro))
                    bairros.Add(ponto.Bairro, new List<Ponto>());

                bairros[ponto.Bairro].Add(ponto);
            }

            return View(bairros);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_loginCliente.GetCliente() != null)
                return RedirectToAction("PainelCliente");

            return View();
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Login([FromForm] Cliente cliente)
        {
            Cliente clienteDB = _repositoryCliente.Login(cliente.Email, cliente.Senha);

            if (clienteDB.Email != null && clienteDB.Senha != null)
            {
                _loginCliente.Login(clienteDB);
                return RedirectToAction("PainelCliente");
            }

            ViewData["MSG_E"] = "error";
            return View();
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            if (_loginCliente.GetCliente() != null)
                return RedirectToAction("PainelCliente");

            return View();
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Cadastrar([FromForm] Cliente cliente)
        {
            Cliente clienteCpf = _repositoryCliente.BuscaCpfCliente(cliente.CPF);
            if (clienteCpf.CPF != null && clienteCpf.CPF != "")
            {
                ViewData["MSG_CPF"] = "error";
            }

            Cliente clienteEmail = _repositoryCliente.BuscaEmailCliente(cliente.Email);
            if (clienteEmail.Email != null && clienteEmail.Email != "")
            {
                ViewData["MSG_Email"] = "error";
            }

            if (ViewData["MSG_CPF"] != null || ViewData["MSG_Email"] != null)
            {
                return View(cliente);
            }

            if (ModelState.IsValid)
            {
                _repositoryCliente.Cadastrar(cliente);

                TempData["MSG_S"] = "🆗";
                return RedirectToAction("Login");
            }

            return View(cliente);
        }

        [ClienteAutorizacao]
        [HttpGet]
        public IActionResult PainelCliente()
        {
            Cliente cliente = _repositoryCliente.ObterCliente(_loginCliente.GetCliente().IdCliente);

            if (cliente.IdCliente == 0 || cliente.Bloqueado)
            {
                _loginCliente.Logout();
                return RedirectToAction("Login");
            }

            _loginCliente.Login(cliente);
            return View(cliente);
        }

        [ClienteAutorizacao]
        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Recarregar(decimal valor, string metodo)
        {
            if (valor < 1 || valor > 1000)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction("PainelCliente");
            }

            if (metodo != "Debito" && metodo != "Credito" && metodo != "Pix")
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction("PainelCliente");
            }

            Cliente cliente = _loginCliente.GetCliente();
            decimal novoSaldo = _repositoryHistPgto.Recarregar(cliente.IdCliente, valor);

            cliente.Saldo = novoSaldo;
            _loginCliente.Login(cliente);

            TempData["MSG_S"] = "🆗";
            return RedirectToAction("PainelCliente");
        }

        [ClienteAutorizacao]
        public IActionResult LogoutCliente()
        {
            _loginCliente.Logout();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}