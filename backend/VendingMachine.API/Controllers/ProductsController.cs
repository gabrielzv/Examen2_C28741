using Microsoft.AspNetCore.Mvc;
using VendingMachine.Application.UseCases;

namespace VendingMachine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IGetProductsUseCase _getProductsUseCase;

    public ProductsController(IGetProductsUseCase getProductsUseCase)
    {
        _getProductsUseCase = getProductsUseCase ?? throw new ArgumentNullException(nameof(getProductsUseCase));
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            var products = await _getProductsUseCase.ExecuteAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving products", error = ex.Message });
        }
    }
}
