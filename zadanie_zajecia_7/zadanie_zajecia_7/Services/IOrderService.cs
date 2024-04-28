using zadanie_zajecia_7.DTO;

namespace zadanie_zajecia_7.Services;

public interface IOrderService
{
    Task<int> Create(CreateProduct_WarehouseDTO productWarehouse);

    Task<int> CreateWithProcedure(CreateProduct_WarehouseDTO productWarehouse);
}