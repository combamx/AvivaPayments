using AvivaPayments.Application.Dtos;
using AvivaPayments.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvivaPayments.Api.Controllers;

[ApiController]
[Route ( "api/[controller]" )]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController ( IOrderService orderService )
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder ( [FromBody] CreateOrderRequest request )
    {
        var result = await _orderService.CreateOrderAsync ( request );
        return CreatedAtAction ( nameof ( GetById ) , new { id = result.Id } , result );
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetAll ( )
    {
        var result = await _orderService.GetOrdersAsync ( );
        return Ok ( result );
    }

    [HttpGet ( "{id:guid}" )]
    public async Task<ActionResult<OrderResponse>> GetById ( Guid id )
    {
        var result = await _orderService.GetOrderByIdAsync ( id );
        if (result is null) return NotFound ( );
        return Ok ( result );
    }

    [HttpPost ( "{id:guid}/cancel" )]
    public async Task<IActionResult> Cancel ( Guid id )
    {
        var ok = await _orderService.CancelOrderAsync ( id );
        if (!ok) return NotFound ( );
        return NoContent ( );
    }

    [HttpPost ( "{id:guid}/pay" )]
    public async Task<IActionResult> Pay ( Guid id )
    {
        var ok = await _orderService.PayOrderAsync ( id );
        if (!ok) return NotFound ( );
        return NoContent ( );
    }
}
