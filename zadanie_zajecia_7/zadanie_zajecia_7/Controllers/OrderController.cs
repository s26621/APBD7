using Microsoft.AspNetCore.Mvc;
using zadanie_zajecia_7.DTO;
using zadanie_zajecia_7.Services;

namespace zadanie_zajecia_7.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProduct_WarehouseDTO productWarehouse)
    {
        int id = await _orderService.Create(productWarehouse);
        if (id == -1)
        {
            return BadRequest();
        }

        return Ok(id);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateWithProcedure(CreateProduct_WarehouseDTO productWarehouse)
    {
        int id = await _orderService.CreateWithProcedure(productWarehouse);
        if (id == -1)
        {
            return BadRequest();
        }

        return Ok(id);
    }
}