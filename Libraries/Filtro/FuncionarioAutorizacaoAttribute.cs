using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EcoDriveTcc.Libraries.Login;
using EcoDriveTcc.Models.Constants;

namespace EcoDriveTcc.Libraries.Filtro
{
    public class FuncionarioAutorizacaoAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _nivelExigido;

        public FuncionarioAutorizacaoAttribute(string nivelExigido = null)
        {
            _nivelExigido = nivelExigido;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var loginFuncionario = (LoginFuncionario)context.HttpContext.RequestServices.GetService(typeof(LoginFuncionario));
            var funcionario = loginFuncionario.GetFuncionario();

            if (funcionario == null)
            {
                context.Result = new RedirectToActionResult("Login", "Home", new { area = "Colaborador" });
                return;
            }

            if (_nivelExigido == NivelAcessoConstant.Admin && funcionario.NivelAcesso != NivelAcessoConstant.Admin)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}