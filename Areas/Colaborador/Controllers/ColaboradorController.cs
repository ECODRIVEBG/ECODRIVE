using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models;
using EcoDriveTcc.Models.Constants;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Areas.Colaborador.Controllers
{
    [Area("Colaborador")]
    [FuncionarioAutorizacao(NivelAcessoConstant.Admin)]
    public class ColaboradorController : Controller
    {
        private IFuncionarioRepository _repositoryFuncionario;
        private LoginFuncionario _loginFuncionario;

        public ColaboradorController(IFuncionarioRepository repositoryFuncionario, LoginFuncionario loginFuncionario)
        {
            _repositoryFuncionario = repositoryFuncionario;
            _loginFuncionario = loginFuncionario;
        }

        public IActionResult Index()
        {
            var funcionarioLogado = _loginFuncionario.GetFuncionario();
            ViewBag.Nome = funcionarioLogado.Nome;
            ViewBag.NivelAcesso = funcionarioLogado.NivelAcesso;

            return View(_repositoryFuncionario.ObterTodos());
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            var funcionarioLogado = _loginFuncionario.GetFuncionario();
            ViewBag.Nome = funcionarioLogado.Nome;
            ViewBag.NivelAcesso = funcionarioLogado.NivelAcesso;

            return View();
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Cadastrar([FromForm] Funcionario funcionario)
        {
            if (ModelState.IsValid)
            {
                _repositoryFuncionario.Cadastrar(funcionario);

                TempData["MSG_S"] = "Registro salvo com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            var funcionarioLogado = _loginFuncionario.GetFuncionario();
            ViewBag.Nome = funcionarioLogado.Nome;
            ViewBag.NivelAcesso = funcionarioLogado.NivelAcesso;

            return View(funcionario);
        }
    }
}