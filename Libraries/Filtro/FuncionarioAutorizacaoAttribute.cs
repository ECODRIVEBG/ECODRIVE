using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models;
using EcoDriveTcc.Models.Constants;

namespace EcoDriveTcc.Libraries.Filtro
{
    public class FuncionarioAutorizacaoAttribute : Attribute, IAuthorizationFilter
    {
        private string _nivelAcessoAutorizado;

        public FuncionarioAutorizacaoAttribute(string NivelAcessoAutorizado = NivelAcessoConstant.Funcionario)
        {
            _nivelAcessoAutorizado = NivelAcessoAutorizado;
        }

        LoginFuncionario _loginFuncionario;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            _loginFuncionario = (LoginFuncionario)context.HttpContext.RequestServices.GetService(typeof(LoginFuncionario));
            Funcionario funcionario = _loginFuncionario.GetFuncionario();

            if (funcionario == null)
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
            }
            else
            {
                if (funcionario.NivelAcesso == NivelAcessoConstant.Funcionario && _nivelAcessoAutorizado == NivelAcessoConstant.Admin)
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}