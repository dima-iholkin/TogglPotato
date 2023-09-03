using System.Text.Json.Serialization;

namespace TogglPotato.WebAPI.Models;

public class TimeEntry
{
    // Cloning:

    public TimeEntry Clone()
    {
        return new TimeEntry()
        {
            Id = this.Id,
            Billable = this.Billable,
            CreatedWith = this.CreatedWith,
            Description = this.Description,
            Duration = this.Duration,
            ProjectId = this.ProjectId,
            Start = this.Start,
            Stop = this.Stop,
            TaskId = this.TaskId,
            UserId = this.UserId,
            WorkspaceId = this.WorkspaceId,
            Modified = this.Modified
        };
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