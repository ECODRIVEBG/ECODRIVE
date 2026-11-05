using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcoDriveTcc.Models;
using EcoDriveTcc.Repository.Contracts;

namespace EcoDriveTcc.Controllers
{
    public class HomeController : Controller
    {
        private IPontoRepository _repositoryPonto;

        public HomeController(IPontoRepository repositoryPonto)
        {
            _repositoryPonto = repositoryPonto;
        }

        public IActionResult Index()
        {
            List<Ponto> pontos = new List<Ponto>(_repositoryPonto.ObterAtivos());
            pontos.Sort((a, b) => a.IdPonto.CompareTo(b.IdPonto));

            Dictionary<string, List<Ponto>> bairros = new Dictionary<string, List<Ponto>>();

            foreach (Ponto ponto in pontos)
            {
                if (!bairros.ContainsKey(ponto.Bairro))
                    bairros.Add(ponto.Bairro, new List<Ponto>());

                bairros[ponto.Bairro].Add(ponto);
            }

            return View(bairros);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}