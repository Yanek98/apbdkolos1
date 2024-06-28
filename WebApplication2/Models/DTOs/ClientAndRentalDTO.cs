namespace WebApplication2.Models.DTOs;

public class ClientAndRentalDTO
{
    public ClientRequestDTO client {  get; set; }
    public int carId { get; set; }
    public DateTime dateFrom { get; set; }
    public DateTime dateTo { get; set; }
}

public class ClientRequestDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string address { get; set; }
}