namespace WebApplication2.Models.DTOs;

public class ClientDTO
{
    public int id { get; set; }

    public string firstName { get; set; } = null!;

    public string lastName { get; set; } = null!;

    public string address { get; set; } = null!;

    public virtual ICollection<RentalDTO> rentals { get; set; } = new List<RentalDTO>();
}