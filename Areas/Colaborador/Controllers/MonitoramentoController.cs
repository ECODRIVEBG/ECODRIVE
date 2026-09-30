using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models;
using EcoDriveTcc.Models.Constants;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Areas.Colaborador.Controllers
{
    [Area("Colaborador")]
    [FuncionarioAutorizacao]
    public class MonitoramentoController : Controller
    {
        private IVeiculoRepository _repositoryVeiculo;
        private IManutencaoRepository _repositoryManutencao;
        private LoginFuncionario _loginFuncionario;

        public MonitoramentoController(
            IVeiculoRepository repositoryVeiculo,
            IManutencaoRepository repositoryManutencao,
            LoginFuncionario loginFuncionario)
        {
            _repositoryVeiculo = repositoryVeiculo;
            _repositoryManutencao = repositoryManutencao;
            _loginFuncionario = loginFuncionario;
        }

        public IActionResult Index(string aba = "Bike")
        {
            var funcionario = _loginFuncionario.GetFuncionario();
            ViewBag.Nome = funcionario.Nome;
            ViewBag.NivelAcesso = funcionario.NivelAcesso;

            ViewBag.Aba = aba;
            ViewBag.Disponiveis = _repositoryVeiculo.ContarPorStatus(StatusVeiculoConstant.Disponivel);
            ViewBag.EmUso = _repositoryVeiculo.ContarPorStatus(StatusVeiculoConstant.EmUso);
            ViewBag.EmManutencao = _repositoryVeiculo.ContarPorStatus(StatusVeiculoConstant.Manutencao);

            if (aba == "Manutencao")
            {
                return View("Manutencao", _repositoryManutencao.ObterEmAberto());
            }

            string tipo = aba == "Patinete" ? TipoVeiculoConstant.Patinete : TipoVeiculoConstant.Bike;
            return View("Index", _repositoryVeiculo.ObterPorTipo(tipo));
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult AbrirManutencao(string token, string descricao)
        {
            var funcionario = _loginFuncionario.GetFuncionario();
            var veiculo = _repositoryVeiculo.ObterPorChave(token);

            if (veiculo == null)
            {
                TempData["MSG_E"] = "Ativo não encontrado para esse token.";
                return RedirectToAction(nameof(Index), new { aba = "Manutencao" });
            }

            _repositoryManutencao.Abrir(new Manutencao
            {
                IdVeiculo = veiculo.IdVeiculo,
                IdFuncionario = funcionario.IdFuncionario,
                Descricao = descricao
            });

            TempData["MSG_S"] = "Manutenção registrada.";
            return RedirectToAction(nameof(Index), new { aba = "Manutencao" });
        }
    }
}