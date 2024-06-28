using WebApplication2.Models.DTOs;

namespace WebApplication2.Services;

public interface IService
{
    public Task<ClientDTO> GetClient(int clientId);
    public Task<(int clientId, string? error)> CreateClientAndRental(ClientAndRentalDTO clientAndRentalDto);
}