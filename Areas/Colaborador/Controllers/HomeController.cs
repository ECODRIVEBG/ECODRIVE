using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models;
using EcoDriveTcc.Models.Constants;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Areas.Colaborador.Controllers
{
    [Area("Colaborador")]
    public class HomeController : Controller
    {
        private IFuncionarioRepository _repositoryFuncionario;
        private LoginFuncionario _loginFuncionario;

        public HomeController(IFuncionarioRepository repositoryFuncionario, LoginFuncionario loginFuncionario)
        {
            _repositoryFuncionario = repositoryFuncionario;
            _loginFuncionario = loginFuncionario;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Login([FromForm] Funcionario funcionario)
        {
            Funcionario funcionarioDB = _repositoryFuncionario.Login(funcionario.Email, funcionario.Senha);

            if (funcionarioDB.Email != null && funcionarioDB.Senha != null)
            {
                _loginFuncionario.Login(funcionarioDB);
                return new RedirectResult(Url.Action("Index", "Monitoramento"));
            }
            else
            {
                ViewData["MSG_E"] = "Usuário não encontrado, verifique o e-mail e senha digitado!";
                return View();
            }
        }

        public IActionResult Logout()
        {
            _loginFuncionario.Logout();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Cadastro([FromForm] Funcionario funcionario)
        {
            funcionario.NivelAcesso = NivelAcessoConstant.Funcionario;

            if (ModelState.IsValid)
            {
                _repositoryFuncionario.Cadastrar(funcionario);

                TempData["MSG_S"] = "Cadastro realizado com sucesso!";
                return RedirectToAction("Login");
            }

            return View(funcionario);
        }
    }
}