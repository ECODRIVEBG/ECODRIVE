using System;
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
        private IVeiculoRepository _repositoryVeiculo;
        private IHistoricoLocacaoRepository _repositoryHistorico;
        private IClienteRepository _repositoryCliente;
        private LoginCliente _loginCliente;

        public AlugarController(
            IPontoRepository repositoryPonto,
            IVeiculoRepository repositoryVeiculo,
            IHistoricoLocacaoRepository repositoryHistorico,
            IClienteRepository repositoryCliente,
            LoginCliente loginCliente)
        {
            _repositoryPonto = repositoryPonto;
            _repositoryVeiculo = repositoryVeiculo;
            _repositoryHistorico = repositoryHistorico;
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
        public IActionResult Index(int? idPonto, string chave)
        {
            var cliente = ObterClienteAtual();
            if (cliente == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.Pontos = _repositoryPonto.ObterAtivos();

            var corrida = _repositoryHistorico.ObterCorridaEmAndamento(cliente.IdCliente);
            if (corrida != null)
            {
                int minutosCobrados = TarifaConstant.CalcularMinutos(corrida.InicioCorrida, DateTime.Now);
                ViewBag.Corrida = corrida;
                ViewBag.MinutosDecorridos = (int)Math.Floor((DateTime.Now - corrida.InicioCorrida).TotalMinutes);
                ViewBag.ValorEstimado = TarifaConstant.CalcularValor(minutosCobrados);
                return View("Encerrar");
            }

            ViewBag.SaldoInsuficiente = cliente.Saldo < TarifaConstant.ValorPorMinuto;

            if (idPonto == null)
            {
                return View("Localizar");
            }

            var ponto = _repositoryPonto.ObterPorId(idPonto.Value);
            if (ponto == null || !ponto.Ativo)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction("Index");
            }

            ViewBag.Ponto = ponto;

            if (string.IsNullOrWhiteSpace(chave))
            {
                return View("Codigo");
            }

            var veiculo = _repositoryVeiculo.ObterPorChave(chave.Trim());

            if (cliente.Saldo < TarifaConstant.ValorPorMinuto)
            {
                TempData["MSG_E"] = "Saldo insuficiente para iniciar uma corrida. Recarregue sua conta.";
                return RedirectToAction("Index", new { idPonto = ponto.IdPonto });
            }

            if (veiculo == null)
            {
                TempData["MSG_E"] = "Ativo não encontrado. Confira o código.";
                return RedirectToAction("Index", new { idPonto = ponto.IdPonto });
            }

            if (veiculo.Status_ != StatusVeiculoConstant.Disponivel)
            {
                TempData["MSG_E"] = "Esse ativo não está disponível.";
                return RedirectToAction("Index", new { idPonto = ponto.IdPonto });
            }

            if (veiculo.IdPonto != ponto.IdPonto)
            {
                TempData["MSG_E"] = "Esse ativo não está no Ponto P selecionado.";
                return RedirectToAction("Index", new { idPonto = ponto.IdPonto });
            }

            if (veiculo.NivelBateria.HasValue && veiculo.NivelBateria.Value <= 0)
            {
                TempData["MSG_E"] = "Esse ativo está sem bateria.";
                return RedirectToAction("Index", new { idPonto = ponto.IdPonto });
            }

            ViewBag.Veiculo = veiculo;
            return View("Confirmar");
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Iniciar(int idPonto, string chave)
        {
            var cliente = ObterClienteAtual();
            if (cliente == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var ponto = _repositoryPonto.ObterPorId(idPonto);
            if (ponto == null || !ponto.Ativo)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction("Index");
            }

            var veiculo = _repositoryVeiculo.ObterPorChave(chave);

            if (cliente.Saldo < TarifaConstant.ValorPorMinuto)
            {
                TempData["MSG_E"] = "Saldo insuficiente para iniciar uma corrida. Recarregue sua conta.";
                return RedirectToAction("Index");
            }

            if (veiculo == null || veiculo.Status_ != StatusVeiculoConstant.Disponivel || veiculo.IdPonto != ponto.IdPonto)
            {
                TempData["MSG_E"] = "Esse ativo não está disponível neste Ponto P.";
                return RedirectToAction("Index", new { idPonto = ponto.IdPonto });
            }

            if (veiculo.NivelBateria.HasValue && veiculo.NivelBateria.Value <= 0)
            {
                TempData["MSG_E"] = "Esse ativo está sem bateria.";
                return RedirectToAction("Index", new { idPonto = ponto.IdPonto });
            }

            bool iniciou = _repositoryHistorico.IniciarCorrida(new HistoricoLocacao
            {
                IdCliente = cliente.IdCliente,
                IdVeiculo = veiculo.IdVeiculo,
                IdPontoInicio = ponto.IdPonto,
                InicioCorrida = DateTime.Now
            });

            if (!iniciou)
            {
                TempData["MSG_E"] = "Esse ativo acabou de ser utilizado. Escolha outro.";
                return RedirectToAction("Index", new { idPonto = ponto.IdPonto });
            }

            TempData["MSG_S"] = "🆗";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Encerrar(int idPontoFim)
        {
            var cliente = ObterClienteAtual();
            if (cliente == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var corrida = _repositoryHistorico.ObterCorridaEmAndamento(cliente.IdCliente);
            if (corrida == null)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction("Index");
            }

            var pontoFim = _repositoryPonto.ObterPorId(idPontoFim);
            if (pontoFim == null || !pontoFim.Ativo)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction("Index");
            }

            DateTime fim = DateTime.Now;
            int minutos = TarifaConstant.CalcularMinutos(corrida.InicioCorrida, fim);
            decimal valor = TarifaConstant.CalcularValor(minutos);

            int? novaBateria = null;
            var veiculo = _repositoryVeiculo.ObterPorId(corrida.IdVeiculo);
            if (veiculo != null && veiculo.NivelBateria.HasValue)
            {
                novaBateria = Math.Max(0, veiculo.NivelBateria.Value - minutos * BateriaConstant.ConsumoPorMinuto);
            }

            decimal novoSaldo = _repositoryHistorico.EncerrarCorrida(corrida, pontoFim.IdPonto, fim, valor, novaBateria);

            cliente.Saldo = novoSaldo;
            _loginCliente.Login(cliente);

            TempData["MSG_S"] = "🆗";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Problema()
        {
            var cliente = _loginCliente.GetCliente();
            var corrida = _repositoryHistorico.ObterCorridaEmAndamento(cliente.IdCliente);

            if (corrida == null)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult ReportarProblema()
        {
            var cliente = _loginCliente.GetCliente();
            var corrida = _repositoryHistorico.ObterCorridaEmAndamento(cliente.IdCliente);

            if (corrida == null)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction("Index");
            }

            _repositoryHistorico.ReportarProblema(corrida, DateTime.Now);

            TempData["MSG_S"] = "🆗";
            return RedirectToAction("Index");
        }
    }
}