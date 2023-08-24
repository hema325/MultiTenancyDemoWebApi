using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenancyWebApi.Data;
using MultiTenancyWebApi.Dtos;
using MultiTenancyWebApi.Entities;

namespace MultiTenancyWebApi.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(CreateProductDto dto)
        {
            _context.Products.Add(new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Rate = dto.Rate,
            });
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Products.ToListAsync());
        }
    }
}
