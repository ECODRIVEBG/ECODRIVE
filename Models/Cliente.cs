using System.ComponentModel.DataAnnotations;

namespace EcoDriveTcc.Models
{
    public class Cliente : Usuario
    {
        public int IdCliente { get; set; }

        [Display(Name = "Nome social")]
        [StringLength(50, ErrorMessage = "O nome social deve ter no máximo 50 caracteres.")]
        public string NomeSocial { get; set; }

        public bool Bloqueado { get; set; }

        public decimal Saldo { get; set; }
    }
}