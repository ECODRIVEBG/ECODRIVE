namespace EcoDriveTcc.Models
{
    public class Ponto
    {
        public int IdPonto { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string Bairro { get; set; }
        public string Tipo { get; set; }
        public bool Ativo { get; set; }
    }
}