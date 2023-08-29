namespace TogglPotato.WebAPI.Models;

public class TimeEntry
{
    public long Id { get; set; }
    public DateTime Start { get; set; }
    public DateTime Stop { get; set; }
    public int Duration { get; set; }
    public string Description { get; set; } = "";
}