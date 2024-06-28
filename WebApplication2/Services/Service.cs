using System.Data.SqlClient;
using WebApplication2.Models.DTOs;

namespace WebApplication2.Services;

 public class Service : IService
    {
        private string _connectionString;

        public Service(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        public async Task<ClientDTO> GetClient(int clientID)
        {
            var client = new ClientDTO();

            var query = @"
                SELECT c.ID, c.FirstName, c.LastName, c.Address,
                       cr.CarID, cr.DateFrom, cr.DateTo, cr.TotalPrice,
                       cars.VIN, colors.Name AS ColorName, models.Name AS ModelName
                FROM clients c
                INNER JOIN car_rentals cr ON c.ID = cr.ClientID
                INNER JOIN cars cars ON cr.CarID = cars.ID
                INNER JOIN colors colors ON cars.ColorID = colors.ID
                INNER JOIN models models ON cars.ModelID = models.ID
                WHERE c.ID = @clientId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@clientId", clientID);

                await connection.OpenAsync();
                SqlDataReader reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    return null;
                }

                while (reader.Read())
                {
                    if (client.id == 0) 
                    {
                        client.id = (int)reader["ID"];
                        client.firstName = reader["FirstName"].ToString();
                        client.lastName = reader["LastName"].ToString();
                        client.address = reader["Address"].ToString();
                    }

                    client.rentals.Add(new RentalDTO
                    {
                        vin = reader["VIN"].ToString(),
                        color = reader["ColorName"].ToString(),
                        model = reader["ModelName"].ToString(),
                        dateFrom = (DateTime)reader["DateFrom"],
                        dateTo = (DateTime)reader["DateTo"],
                        totalPrice = (int)reader["TotalPrice"]
                    });
                }
            }

            return client;
        }

        public async Task<(int clientId, string? error)> CreateClientAndRental(ClientAndRentalDTO clientAndRentalDTO)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var clientSql = "INSERT INTO clients (FirstName, LastName, Address) OUTPUT INSERTED.ID VALUES (@FirstName, @LastName, @Address)";
                        var clientId = 0;
                        using (var command = new SqlCommand(clientSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@FirstName", clientAndRentalDTO.client.firstName);
                            command.Parameters.AddWithValue("@LastName", clientAndRentalDTO.client.lastName);
                            command.Parameters.AddWithValue("@Address", clientAndRentalDTO.client.address);
                            clientId = (int)await command.ExecuteScalarAsync();
                        }

                        var carSql = "SELECT PricePerDay FROM cars WHERE ID = @CarId";
                        var carPricePerDay = 0;
                        using (var command = new SqlCommand(carSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CarId", clientAndRentalDTO.carId);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (!reader.Read())
                                    throw new Exception("Car not found");
                                carPricePerDay = reader.GetInt32(0);
                            }
                        }

                        var days = clientAndRentalDTO.dateTo.Day - clientAndRentalDTO.dateFrom.Day;
                        var totalPrice = days * carPricePerDay;

                        var rentalSql = "INSERT INTO car_rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice) VALUES (@ClientID, @CarId, @DateFrom, @DateTo, @TotalPrice)";
                        using (var command = new SqlCommand(rentalSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@ClientID", clientId);
                            command.Parameters.AddWithValue("@CarId", clientAndRentalDTO.carId);
                            command.Parameters.AddWithValue("@DateFrom", clientAndRentalDTO.dateFrom);
                            command.Parameters.AddWithValue("@DateTo", clientAndRentalDTO.dateTo);
                            command.Parameters.AddWithValue("@TotalPrice", totalPrice);
                            await command.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        return (clientId, null);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (0, ex.Message);
                    }
                }
            }
        }
    }