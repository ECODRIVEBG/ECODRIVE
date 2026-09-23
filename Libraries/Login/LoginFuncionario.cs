using EcoDriveTcc.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace EcoDriveTcc.Libraries.Login
{
    public class LoginFuncionario
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ChaveSessao = "FuncionarioLogado";

        public LoginFuncionario(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Login(Funcionario funcionario)
        {
            var json = JsonConvert.SerializeObject(funcionario);
            _httpContextAccessor.HttpContext.Session.SetString(ChaveSessao, json);
        }

        public Funcionario GetFuncionario()
        {
            var json = _httpContextAccessor.HttpContext.Session.GetString(ChaveSessao);
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<Funcionario>(json);
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext.Session.Remove(ChaveSessao);
        }
    }
}