using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.CompilerServices;
using zadanie_zajecia_7.DTO;
using zadanie_zajecia_7.Models;

namespace zadanie_zajecia_7.Repositories;

public class OrderRepository : IOrderRepository
{
    private IConfiguration _configuration;

    public OrderRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> Create(CreateProduct_WarehouseDTO productWarehouse)
    {
        (int idProduct, int idWarehouse, int amount, DateTime createdAt) = productWarehouse;
        
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        
         // 1. Sprawdzamy, czy produkt o podanym identyfikatorze istnieje. Następnie
         //    sprawdzamy, czy magazyn o podanym identyfikatorze istnieje. Wartość
         //    ilości przekazana w żądaniu powinna być większa niż 0.
                
        command.CommandText = "SELECT Count(1) FROM Product WHERE IdProduct = @idProduct";
        command.Parameters.AddWithValue("idProduct", idProduct);
        
        // to nie jest związane z jedynką, ale boję się to przestawiać
        await connection.OpenAsync();

        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();
        
        await sqlDataReader.ReadAsync();

        int ile = int.Parse(sqlDataReader.GetString(0));

        if (ile == 0) return -1;
        
        command.CommandText = "SELECT Count(1) FROM Warehouse WHERE IdWarehouse = @idWarehouse";
        command.Parameters.AddWithValue("idWarehouse", idWarehouse);
        
        await command.ExecuteReaderAsync();

        await sqlDataReader.ReadAsync();

        ile = int.Parse(sqlDataReader.GetString(0));

        if (ile == 0) return -1;
        
        if (amount <= 0) return -1;

        // 2. Możemy dodać produkt do magazynu tylko wtedy, gdy istnieje
        //    zamówienie zakupu produktu w tabeli Order. Dlatego sprawdzamy, czy w
        //    tabeli Order istnieje rekord z IdProduktu i Ilością (Amount), które
        //    odpowiadają naszemu żądaniu. Data utworzenia zamówienia powinna
        //    być wcześniejsza niż data utworzenia w żądaniu.
        
        command.CommandText = "SELECT Count(IdOrder), IdOrder FROM Order WHERE IdProduct = @idProduct AND Amount = @amount AND CreatedAt < @createdAt GROUP BY IdOrder";
        command.Parameters.AddWithValue("idProduct", idProduct);
        command.Parameters.AddWithValue("amount", amount);
        command.Parameters.AddWithValue("createdAt", createdAt);
        
        await command.ExecuteReaderAsync();
        
        await sqlDataReader.ReadAsync();

        if (sqlDataReader.GetString(0).IsNullOrEmpty() || int.Parse(sqlDataReader.GetString(0)) == 0) return -1;

        int idOrder = int.Parse(sqlDataReader.GetString(1));
        
        
        // 3. Sprawdzamy, czy to zamówienie zostało przypadkiem zrealizowane.
        //    Sprawdzamy, czy nie ma wiersza z danym IdOrder w tabeli
        //    Product_Warehouse.
        
        command.CommandText = "SELECT Count(1) FROM Product_Warehouse WHERE IdOrder = @idOrder";
        command.Parameters.AddWithValue("idOrder", idOrder);
        
        await command.ExecuteReaderAsync();

        await sqlDataReader.ReadAsync();

        ile = int.Parse(sqlDataReader.GetString(0));

        if (ile > 0) return -1;
        
        // 4. Aktualizujemy kolumnę FullfilledAt zamówienia na aktualną datę i godzinę. (UPDATE)


        DateTime fullfilledAt = DateTime.Now;
        
        command.CommandText = "UPDATE Order SET FullfilledAt = @fullfilledAt WHERE IdOrder = @idOrder";
        command.Parameters.AddWithValue("fullfilledAt", fullfilledAt);
        
        await command.ExecuteReaderAsync();

        
        
        //  5. Wstawiamy rekord do tabeli Product_Warehouse. Kolumna Price
        //     powinna odpowiadać cenie produktu pomnożonej przez kolumnę Amount
        //     z naszego zamówienia. Ponadto wstawiamy wartość CreatedAt zgodnie
        //     z aktualnym czasem. (INSERT)
        
        // potrzebujemy: IdProductWarehouse (autoindeksowanie?), IdWarehouse, IdProduct, IdOrder,
        //               Amount, Price i CreatedAt
        
        // mamy: IdProduct, IdWarehouse, Amount z argumentu funkcji
        //       IdOrder z wyżej
        //       CreatedAt ma być aktualną datą
        
        
        // zostaje nam tylko Price, które musimy obliczyć za pomocą zapytania o cenę produktu
        
        command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @idProduct";
        command.Parameters.AddWithValue("idProduct", idProduct);
        
        await command.ExecuteReaderAsync();
        await sqlDataReader.ReadAsync();
                
        int cena = int.Parse(sqlDataReader.GetString(0))*amount;
        
        DateTime createdAtDoWstawienia = DateTime.Now;

        string createdAtString = createdAtDoWstawienia.ToString("yyyy-MM-dd HH:mm:ss.fff");
        
        // jedziemy! 
        
        command.CommandText = "INSERT INTO Product_Warehouse VALUES " +
                              "(" +
                              idWarehouse + " " + idProduct + " " + idOrder + " " + amount + " " + cena +" " + createdAtString 
                              + ")";
        
        await command.ExecuteReaderAsync();
            
        command.CommandText = "SELECT IdProductWarehouse FROM Product_Warehouse WHERE CreatedAt = (SELECT MAX(CreatedAt) FROM ProductWarehouse)";
        
        await command.ExecuteReaderAsync();
        await sqlDataReader.ReadAsync();
        return int.Parse(sqlDataReader.GetString(0));
    }

    public async Task<int> CreateWithProcedure(CreateProduct_WarehouseDTO productWarehouse)
    {
        (int idProduct, int idWarehouse, int amount, DateTime createdAt) = productWarehouse;
        
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        command.CommandText = "EXECUTE AddProductToWarehouse(@idProduct, @idWarehouse, @amount, @createdAt)";
        command.Parameters.AddWithValue("idProduct", idProduct);
        command.Parameters.AddWithValue("idWarehouse", idWarehouse);
        command.Parameters.AddWithValue("amount", amount);
        command.Parameters.AddWithValue("createdAt", createdAt);
        
        
        await connection.OpenAsync();

        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();
        
        await sqlDataReader.ReadAsync();
        
        if (!sqlDataReader.Read()) return -1;
        return int.Parse(sqlDataReader.GetString(0));
        
    }
}