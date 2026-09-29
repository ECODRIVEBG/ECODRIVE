using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Libraries.Filtro;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models.Constants;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Areas.Colaborador.Controllers
{
    [Area("Colaborador")]
    [FuncionarioAutorizacao]
    public class MonitoramentoController : Controller
    {
        private IVeiculoRepository _repositoryVeiculo;
        private LoginFuncionario _loginFuncionario;

        public MonitoramentoController(IVeiculoRepository repositoryVeiculo, LoginFuncionario loginFuncionario)
        {
            _repositoryVeiculo = repositoryVeiculo;
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

            string tipo = aba == "Patinete" ? TipoVeiculoConstant.Patinete : TipoVeiculoConstant.Bike;
            return View(_repositoryVeiculo.ObterPorTipo(tipo));
        }
    }
}