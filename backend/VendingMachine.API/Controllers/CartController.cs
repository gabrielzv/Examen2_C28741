using Microsoft.AspNetCore.Mvc;
using VendingMachine.Application.DTOs;
using VendingMachine.Application.UseCases;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICalculateOrderTotalUseCase _calculateOrderTotalUseCase;

    public CartController(ICalculateOrderTotalUseCase calculateOrderTotalUseCase)
    {
        _calculateOrderTotalUseCase = calculateOrderTotalUseCase ?? throw new ArgumentNullException(nameof(calculateOrderTotalUseCase));
    }

    [HttpPost("calculate-total")]
    public async Task<IActionResult> CalculateTotal([FromBody] CartDto cart)
    {
        try
        {
            if (cart?.Items == null || !cart.Items.Any())
            {
                return BadRequest(new { message = "El carrito esta vacio" });
            }
            var orderSummary = await _calculateOrderTotalUseCase.ExecuteAsync(cart);
            return Ok(orderSummary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al calcular el total", error = ex.Message });
        }
    }
}
