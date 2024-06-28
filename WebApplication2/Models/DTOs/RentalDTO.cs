namespace WebApplication2.Models.DTOs;

public class RentalDTO
{
    public string vin {  get; set; }
    public string color { get; set; }
    public string model { get; set; }
    public DateTime dateFrom { get; set; }
    public DateTime dateTo { get; set; }
    public int totalPrice { get; set; }
}