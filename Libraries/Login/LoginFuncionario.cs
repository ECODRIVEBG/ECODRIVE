using Newtonsoft.Json;
using EcoDriveTcc.Models;

namespace EcoDriveTcc.Libraries.Login
{
    public class LoginFuncionario
    {
        private string Key = "Login.Colaborador";
        private Sessao.Sessao _sessao;

        public LoginFuncionario(Sessao.Sessao sessao)
        {
            _sessao = sessao;
        }

        public void Login(Funcionario funcionario)
        {
            string funcionarioJSONString = JsonConvert.SerializeObject(funcionario);

            _sessao.Cadastrar(Key, funcionarioJSONString);
        }

        public Funcionario GetFuncionario()
        {
            if (_sessao.Existe(Key))
            {
                string funcionarioJSONString = _sessao.Consultar(Key);
                return JsonConvert.DeserializeObject<Funcionario>(funcionarioJSONString);
            }
            else
            {
                return null;
            }
        }

        public void Logout()
        {
            _sessao.RemoverTodos();
        }
    }
}