namespace TimeMyGames.Models;

public class MyGameDto
{
    public int AppId { get; set; }
    public string Name { get; set; }
    public int PlaytimeMinutes { get; set; }
    public double PlaytimeHours => Math.Round(PlaytimeMinutes / 60.0, 1);
}
