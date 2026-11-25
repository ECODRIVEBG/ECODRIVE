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
                return RedirectToAction("Login", "Home");

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
                return View("Localizar");

            var ponto = _repositoryPonto.ObterPorId(idPonto.Value);
            if (ponto == null || !ponto.Ativo)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Ponto = ponto;

            if (string.IsNullOrWhiteSpace(chave))
                return View("Codigo");

            var veiculo = _repositoryVeiculo.ObterPorChave(chave.Trim());
            string erro = ValidarInicio(cliente, veiculo, ponto);
            if (erro != null)
            {
                TempData["MSG_E"] = erro;
                return RedirectToAction(nameof(Index), new { idPonto = ponto.IdPonto });
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
                return RedirectToAction("Login", "Home");

            var ponto = _repositoryPonto.ObterPorId(idPonto);
            var veiculo = string.IsNullOrWhiteSpace(chave) ? null : _repositoryVeiculo.ObterPorChave(chave.Trim());

            string erro = ValidarInicio(cliente, veiculo, ponto);
            if (erro == null && !ponto.Ativo)
                erro = "Esse Ponto P não está ativo.";

            if (erro != null)
            {
                TempData["MSG_E"] = erro;
                return RedirectToAction(nameof(Index));
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
                return RedirectToAction(nameof(Index), new { idPonto = ponto.IdPonto });
            }

            TempData["MSG_S"] = "🆗";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateHttpReferer]
        public IActionResult Encerrar(int idPontoFim)
        {
            var cliente = ObterClienteAtual();
            if (cliente == null)
                return RedirectToAction("Login", "Home");

            var corrida = _repositoryHistorico.ObterCorridaEmAndamento(cliente.IdCliente);
            if (corrida == null)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction(nameof(Index));
            }

            var pontoFim = _repositoryPonto.ObterPorId(idPontoFim);
            if (pontoFim == null || !pontoFim.Ativo)
            {
                TempData["MSG_E"] = "error";
                return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        private static string ValidarInicio(Cliente cliente, Veiculo veiculo, Ponto ponto)
        {
            if (cliente.Saldo < TarifaConstant.ValorPorMinuto)
                return "Saldo insuficiente para iniciar uma corrida. Recarregue sua conta.";

            if (ponto == null)
                return "Ponto P não encontrado.";

            if (veiculo == null)
                return "Ativo não encontrado. Confira o código.";

            if (veiculo.Status_ != StatusVeiculoConstant.Disponivel)
                return "Esse ativo não está disponível.";

            if (veiculo.IdPonto != ponto.IdPonto)
                return "Esse ativo não está no Ponto P selecionado.";

            if (veiculo.NivelBateria.HasValue && veiculo.NivelBateria.Value <= 0)
                return "Esse ativo está sem bateria.";

            return null;
        }
    }
}