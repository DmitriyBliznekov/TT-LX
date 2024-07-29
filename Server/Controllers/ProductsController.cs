using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Controllers;

[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ServerContext _context;
    private readonly IGenerator<Product> _productGenerator;

    public ProductsController(ServerContext context, IGenerator<Product> productGenerator)
    {
        _context = context;
        _productGenerator = productGenerator;
    }

    [HttpGet("/fetch_all")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProduct() => await _context.Products.ToListAsync();

    [HttpPost("/generate_data")]
    public async Task<ActionResult<Product>> PostProduct([FromQuery] int count)
    {
        var products = _productGenerator.GenerateSet(count);

        await _context.Set<Product>().AddRangeAsync(products);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("/clear_db")]
    public async Task<IActionResult> ClearDb()
    {
        await _context.Products.ExecuteDeleteAsync();

        return NoContent();
    }
}