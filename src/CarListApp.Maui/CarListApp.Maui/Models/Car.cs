using SQLite;

namespace CarListApp.Maui.Models;

[Table("Cars")]
public class Car
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    [MaxLength(12), Unique]
    public string Vin { get; set; }
    
}