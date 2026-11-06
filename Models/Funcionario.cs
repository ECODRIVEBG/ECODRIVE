using System.ComponentModel.DataAnnotations;

namespace EcoDriveTcc.Models
{
    public class Funcionario : Usuario
    {
        public int IdFuncionario { get; set; }

        [Display(Name = "Cargo")]
        [Required(ErrorMessage = "O cargo é obrigatório.")]
        [StringLength(50, ErrorMessage = "O cargo deve ter no máximo 50 caracteres.")]
        public string Cargo { get; set; }

        [Display(Name = "Telefone")]
        [StringLength(30, ErrorMessage = "O telefone deve ter no máximo 30 caracteres.")]
        public string Telefone { get; set; }

        [Display(Name = "Nível de acesso")]
        public string NivelAcesso { get; set; }
    }
}