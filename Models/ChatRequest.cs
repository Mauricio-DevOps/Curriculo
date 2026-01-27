using System.ComponentModel.DataAnnotations;

namespace Curriculo.Models;

public class ChatRequest
{
    [Required]
    public string Prompt { get; set; } = string.Empty;

    public string? PreviousResponseId { get; set; }

    public bool UseFileSearch { get; set; } = true;
}
