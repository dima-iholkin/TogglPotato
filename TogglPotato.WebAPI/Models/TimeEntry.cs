namespace TogglPotato.WebAPI.Models;

public class TimeEntry
{
    public int Id { get; set; }
    public DateTime Start { get; set; }
    public DateTime Stop { get; set; }
    public int Duration { get; set; }
}