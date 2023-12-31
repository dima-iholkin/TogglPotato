using System.Text.Json.Serialization;

namespace TogglPotato.WebAPI.Domain.Models;

public class TimeEntry : ICloneable
{
    // Clone:

    public object Clone()
    {
        TimeEntry newTimeEntry = (TimeEntry)this.MemberwiseClone();
        return newTimeEntry;
    }

    // Properties:

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Id { get; set; }

    public bool Billable { get; set; }

    [JsonPropertyName("created_with")]
    public string CreatedWith { get; set; } = "";

    public string? Description { get; set; }

    public int Duration { get; set; }

    [JsonPropertyName("project_id")]
    public int? ProjectId { get; set; }

    public DateTime Start { get; set; }

    public DateTime Stop { get; set; }

    [JsonPropertyName("task_id")]
    public int? TaskId { get; set; }

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("workspace_id")]
    public int WorkspaceId { get; set; }

    // Custom property:

    [JsonIgnore]
    public bool Modified { get; set; } = false;
}