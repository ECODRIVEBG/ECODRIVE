using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models;
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

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Login([FromForm] Funcionario funcionario)
        {
            Funcionario funcionarioDB = _repositoryFuncionario.Login(funcionario.Email, funcionario.Senha);

            if (funcionarioDB != null)
            {
                _loginFuncionario.Login(funcionarioDB);
                return new RedirectResult(Url.Action(nameof(Login), "Monitoramento"));
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
            return RedirectToAction(nameof(Login));
        }
    }
}