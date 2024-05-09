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
    
    // "idProduct": 1,
    // "idWarehouse": 2,
    // "amount": 20,
    // "createdAt": "2012-04-23T18:25:43.511Z"

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
    
    [HttpPost("/procedure")]
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