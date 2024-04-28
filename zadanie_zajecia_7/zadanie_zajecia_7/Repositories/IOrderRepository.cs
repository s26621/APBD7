using zadanie_zajecia_7.DTO;
using zadanie_zajecia_7.Models;

namespace zadanie_zajecia_7.Repositories;

public interface IOrderRepository
{
    Task<int> Create(CreateProduct_WarehouseDTO productWarehouse);

    Task<int> CreateWithProcedure(CreateProduct_WarehouseDTO productWarehouse);
}