using System;

namespace EcoDriveTcc.Models
{
    public class HistPgto
    {
        public int IdHistPgto { get; set; }
        public int IdCliente { get; set; }
        public decimal ValorRecarga { get; set; }
        public DateTime DataRecarga { get; set; }
    }
}