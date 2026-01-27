using System.ComponentModel.DataAnnotations;

namespace Curriculo.Models;

public class AssistantChatRequest
{
    [Required]
    public string? Prompt { get; set; }

    public string? PreviousResponseId { get; set; }

    public bool? UseFileSearch { get; set; }
}
