namespace wt_lab7_7postgres_efcore_kobzeva.Models;

public class Property
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal Price { get; set; }
    public double Area { get; set; }
    public int Rooms { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = "";
}
