namespace EcoDriveTcc.Models
{
    public class Veiculo
    {
        public int IdVeiculo { get; set; }
        public int IdPonto { get; set; }
        public string Tipo { get; set; }      
        public string Status_ { get; set; }    
        public int? NivelBateria { get; set; } 
        public string Chave { get; set; }

       
        public string NomePonto { get; set; }
    }
}