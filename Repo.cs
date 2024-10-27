using System;

public class Repo {
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string html_url { get; set; }
    public required List<string> Topics { get; set; } 
    public required List<string> Images { get; set; } 
}
