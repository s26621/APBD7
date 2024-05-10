using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using zadanie_zajecia_7.DTO;

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

        // 1. Sprawdzamy, czy produkt o podanym identyfikatorze istnieje. Następnie
        //    sprawdzamy, czy magazyn o podanym identyfikatorze istnieje. Wartość
        //    ilości przekazana w żądaniu powinna być większa niż 0.
        
        
        bool czy = await CzyProduktIstnieje(idProduct);

        if (!czy) return -1;

        czy = await CzyMagazynIstnieje(idWarehouse);

        if (!czy) return -1;
        
        if (amount <= 0) return -1;

        // 2. Możemy dodać produkt do magazynu tylko wtedy, gdy istnieje
        //    zamówienie zakupu produktu w tabeli Order. Dlatego sprawdzamy, czy w
        //    tabeli Order istnieje rekord z IdProduktu i Ilością (Amount), które
        //    odpowiadają naszemu żądaniu. Data utworzenia zamówienia powinna
        //    być wcześniejsza niż data utworzenia w żądaniu.



        int idOrder = await WezIdZamowienia(idProduct, amount, createdAt);

        if (idOrder == -1) return -1;
        
        
        // 3. Sprawdzamy, czy to zamówienie zostało przypadkiem zrealizowane.
        //    Sprawdzamy, czy nie ma wiersza z danym IdOrder w tabeli
        //    Product_Warehouse.


        czy = await CzyJuzZrealizowano(idOrder);

        if (czy) return -1;
        
        // 4. Aktualizujemy kolumnę FullfilledAt zamówienia na aktualną datę i godzinę. (UPDATE)


        AktualizujDate(idOrder);

        
        
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


        int cena = await DajCene(idProduct, amount);
        
        string createdAtString = createdAt.ToString("yyyy-MM-dd HH:mm:ss.fff");
        
        // jedziemy! 
                
        WstawRekord(idWarehouse, idProduct, idOrder, amount, cena, createdAtString);
            
        
        return await DajIndeks();
    }

    public async Task<int> CreateWithProcedure(CreateProduct_WarehouseDTO productWarehouse)
    {
        (int idProduct, int idWarehouse, int amount, DateTime createdAt) = productWarehouse;
        
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        command.CommandText = "EXECUTE AddProductToWarehouse @idProduct, @idWarehouse, @amount, @createdAt";
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
    
    // ======================== ODTĄD FUNKCJE POMOCNICZE ===============================

    private async Task<bool> CzyProduktIstnieje(int idProduct)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
                
        command.CommandText = "SELECT Count(1) FROM Product WHERE IdProduct = @idProduct";
        command.Parameters.AddWithValue("idProduct", idProduct);
        
        await connection.OpenAsync();

        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();
        
        await sqlDataReader.ReadAsync();
        
        return sqlDataReader.GetInt32(0) > 0;
    }

    private async Task<bool> CzyMagazynIstnieje(int idWarehouse)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT Count(1) FROM Warehouse WHERE IdWarehouse = @idWarehouse";
        command.Parameters.AddWithValue("idWarehouse", idWarehouse);
                                
        await connection.OpenAsync();

        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();
                                        
        await sqlDataReader.ReadAsync();
                                        
        return sqlDataReader.GetInt32(0) > 0;
    }

    private async Task<int> WezIdZamowienia(int idProduct, int amount, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        command.CommandText = "SELECT Count(IdOrder), IdOrder FROM Order WHERE IdProduct = @idProduct2 AND Amount = @amount AND CreatedAt < @createdAt GROUP BY IdOrder";
        command.Parameters.AddWithValue("idProduct2", idProduct);
        command.Parameters.AddWithValue("amount", amount);
        command.Parameters.AddWithValue("createdAt", createdAt);
        
        await connection.OpenAsync();

        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();
        
        await sqlDataReader.ReadAsync();

        if (sqlDataReader.GetInt32(0) == 0) return -1;
        return sqlDataReader.GetInt32(1);
    }

    private async Task<bool> CzyJuzZrealizowano(int idOrder)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        command.CommandText = "SELECT Count(1) FROM Product_Warehouse WHERE IdOrder = @idOrder";
        command.Parameters.AddWithValue("idOrder", idOrder);
        
        await connection.OpenAsync();

        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();

        await sqlDataReader.ReadAsync();

        return sqlDataReader.GetInt32(0) > 0;
    }

    private async void AktualizujDate(int idOrder)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        await connection.OpenAsync();

        DateTime fullfilledAt = DateTime.Now;
        
        command.CommandText = "UPDATE Order SET FullfilledAt = @fullfilledAt WHERE IdOrder = @idOrder3";
        command.Parameters.AddWithValue("fullfilledAt", fullfilledAt);
        command.Parameters.AddWithValue("idOrder3", idOrder);

        
        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();
    }

    private async Task<int> DajCene(int idProduct, int amount)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @idProduct";
        command.Parameters.AddWithValue("idProduct", idProduct);
        
        await connection.OpenAsync();

        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();
        await sqlDataReader.ReadAsync();
                
        return sqlDataReader.GetInt32(0) * amount;
    }
    
    private async void WstawRekord(int idWarehouse, int idProduct, int idOrder, int amount, int cena, string createdAtString)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        command.CommandText = "INSERT INTO Product_Warehouse VALUES " +
                              "(" +
                              idWarehouse + " " + idProduct + " " + idOrder + " " + amount + " " + cena +" " + createdAtString 
                              + ")";
        
        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> DajIndeks()
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand();

        command.Connection = connection;
        
        command.CommandText = "SELECT IdProductWarehouse FROM Product_Warehouse WHERE CreatedAt = (SELECT MAX(CreatedAt) FROM ProductWarehouse)";
        await connection.OpenAsync();

        await using SqlDataReader sqlDataReader = await command.ExecuteReaderAsync();
        await sqlDataReader.ReadAsync();

        return sqlDataReader.GetInt32(0);
    }
}