using Microsoft.AspNetCore.Mvc;
using VendingMachine.Application.DTOs;
using VendingMachine.Application.UseCases;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IGetCashRegisterStatusUseCase _getCashRegisterStatusUseCase;
    private readonly IProcessPaymentUseCase _processPaymentUseCase;

    public PaymentController(
        IGetCashRegisterStatusUseCase getCashRegisterStatusUseCase,
        IProcessPaymentUseCase processPaymentUseCase)
    {
        _getCashRegisterStatusUseCase = getCashRegisterStatusUseCase ?? throw new ArgumentNullException(nameof(getCashRegisterStatusUseCase));
        _processPaymentUseCase = processPaymentUseCase ?? throw new ArgumentNullException(nameof(processPaymentUseCase));
    }

    [HttpGet("cash-register")]
    public async Task<ActionResult<List<MoneySlotDto>>> GetCashRegisterStatus()
    {
        try
        {
            var result = await _getCashRegisterStatusUseCase.ExecuteAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error getting cash register status", error = ex.Message });
        }
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentResultDto>> ProcessPayment([FromBody] ProcessPaymentRequestDto request)
    {
        try
        {
            if (request?.Payment == null)
            {
                return BadRequest(new { message = "Payment information is required" });
            }

            if (request.TotalCost <= 0)
            {
                return BadRequest(new { message = "Total cost must be greater than zero" });
            }

            var result = await _processPaymentUseCase.ExecuteAsync(request.Payment, request.TotalCost, request.Cart);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error processing payment", error = ex.Message });
        }
    }
}
