using zadanie_zajecia_7.DTO;
using zadanie_zajecia_7.Repositories;

namespace zadanie_zajecia_7.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    public async Task<int> Create(CreateProduct_WarehouseDTO productWarehouse)
    {
        return await _orderRepository.Create(productWarehouse);
    }

    public async Task<int> CreateWithProcedure(CreateProduct_WarehouseDTO productWarehouse)
    {
        return await _orderRepository.CreateWithProcedure(productWarehouse);
    }
}