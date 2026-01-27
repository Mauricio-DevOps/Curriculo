using System.ComponentModel.DataAnnotations;

namespace Curriculo.Models;

public class ContactFormViewModel
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(150, ErrorMessage = "O nome deve ter no maximo 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu email.")]
    [EmailAddress(ErrorMessage = "Email invalido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o assunto.")]
    [StringLength(150, ErrorMessage = "O assunto deve ter no maximo 150 caracteres.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escreva sua mensagem.")]
    [StringLength(4000, ErrorMessage = "A mensagem deve ter no maximo 4000 caracteres.")]
    public string Message { get; set; } = string.Empty;
}
