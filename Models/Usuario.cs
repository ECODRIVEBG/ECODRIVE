using System.ComponentModel.DataAnnotations;

namespace EcoDriveTcc.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        [Display(Name = "Nome completo", Description = "Nome e sobrenome.")]
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nome deve ter no máximo 50 caracteres.")]
        public string Nome { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail não é válido.")]
        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(50, ErrorMessage = "O e-mail deve ter no máximo 50 caracteres.")]
        public string Email { get; set; }

        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 10 caracteres.")]
        public string Senha { get; set; }

        [Display(Name = "CPF")]
        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [RegularExpression(@"^(\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})$", ErrorMessage = "O CPF informado é inválido.")]
        public string CPF { get; set; }
    }
}