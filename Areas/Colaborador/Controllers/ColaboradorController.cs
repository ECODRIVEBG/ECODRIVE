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

        private void PreencherMenu()
        {
            Funcionario funcionarioLogado = _loginFuncionario.GetFuncionario();
            ViewBag.Nome = funcionarioLogado.Nome;
            ViewBag.NivelAcesso = funcionarioLogado.NivelAcesso;
        }

        public IActionResult Index()
        {
            PreencherMenu();
            return View(_repositoryFuncionario.ObterTodosFuncionarios());
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            PreencherMenu();
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

            PreencherMenu();
            return View(funcionario);
        }

        [HttpGet]
        public IActionResult Atualizar(int id)
        {
            PreencherMenu();
            Funcionario funcionario = _repositoryFuncionario.ObterFuncionario(id);
            return View(funcionario);
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Atualizar([FromForm] Funcionario funcionario)
        {
            if (ModelState.IsValid)
            {
                _repositoryFuncionario.Atualizar(funcionario);

                TempData["MSG_S"] = "Registro atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            PreencherMenu();
            return View(funcionario);
        }

        [ValidateHttpReferer]
        public IActionResult Excluir(int id)
        {
            _repositoryFuncionario.Excluir(id);

            TempData["MSG_S"] = "Registro excluído com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}