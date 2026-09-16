using System.ComponentModel.DataAnnotations;

namespace Pawlog.Api.Models;

public class EntryRequest
{
    public DateOnly Date { get; set; }

    [Required]
    [MaxLength(80)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Story { get; set; } = string.Empty;

    [MaxLength(120)]
    public string TrainingGoal { get; set; } = string.Empty;

    public bool GoalCompleted { get; set; }
}
