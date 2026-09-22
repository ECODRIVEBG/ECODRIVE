using System;

namespace EcoDriveTcc.Models
{
    public class Manutencao
    {
        public int IdManutencao { get; set; }
        public int IdVeiculo { get; set; }
        public int IdFuncionario { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Descricao { get; set; }
        public bool Concluida { get; set; }

        // Preenchidos por JOIN para exibição no painel
        public string TokenVeiculo { get; set; }
        public string TipoVeiculo { get; set; }
        public string NomePonto { get; set; }
    }
}