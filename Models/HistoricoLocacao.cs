using System;

namespace EcoDriveTcc.Models
{
    public class HistoricoLocacao
    {
        public int IdHistLoc { get; set; }
        public int IdCliente { get; set; }
        public int IdVeiculo { get; set; }
        public int IdPontoInicio { get; set; }
        public int? IdPontoFim { get; set; }
        public DateTime InicioCorrida { get; set; }
        public DateTime? FimCorrida { get; set; }
        public decimal? ValorCorrida { get; set; }
        public bool Problema { get; set; }

        public string TokenVeiculo { get; set; }
        public string TipoVeiculo { get; set; }
        public string NomePontoInicio { get; set; }
        public string NomePontoFim { get; set; }
    }
}