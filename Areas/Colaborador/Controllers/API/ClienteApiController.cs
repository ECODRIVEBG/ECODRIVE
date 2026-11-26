using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Areas.Colaborador.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteApiController : Controller
    {
        private IClienteRepository _clienteRepository;

        public ClienteApiController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var clientes = _clienteRepository.ObterTodosClientes();
            return Ok(clientes);
        }

        [HttpPost]
        public IActionResult Cadastrar(Cliente cliente)
        {
            if (cliente == null)
            {
                return BadRequest(new
                {
                    mensagem = "Os dados do cliente são obrigatórios."
                });
            }

            _clienteRepository.Cadastrar(cliente);

            return Ok(new
            {
                mensagem = "Cadastro realizado com sucesso.",
                cliente = cliente
            });
        }

        [HttpGet("{id}")]
        public IActionResult BuscarPorId(int id)
        {
            var cliente = _clienteRepository.ObterCliente(id);

            if (cliente == null)
            {
                return NotFound(new
                {
                    mensagem = "Cliente não encontrado."
                });
            }

            return Ok(cliente);
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clienteExistente = _clienteRepository.ObterCliente(id);

            if (clienteExistente == null)
            {
                return NotFound(new
                {
                    mensagem = "Cliente não encontrado"
                });
            }

            cliente.IdCliente = id;

            _clienteRepository.Atualizar(cliente);

            return Ok(new
            {
                mensagem = "Cliente atualizado com sucesso."
            });
        }

        [HttpDelete("{id}")]
        public IActionResult Excluir(int id)
        {
            var cliente = _clienteRepository.ObterCliente(id);

            if (cliente == null)
            {
                return NotFound(new
                {
                    mensagem = "Cliente não encontrado."
                });
            }

            _clienteRepository.Excluir(id);

            return Ok(new
            {
                mensagem = "Cliente excluído com sucesso."
            });
        }
    }
}