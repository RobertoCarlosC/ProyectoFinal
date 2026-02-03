using Microsoft.AspNetCore.Mvc;
using EnerGym.Api.Models;

namespace EnerGym.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private static List<Product> products = new()
        {
            new Product { Id = 1, Nombre = "Proteína Whey", Precio = 29.99m, Stock = 10 },
            new Product { Id = 2, Nombre = "Creatina", Precio = 19.99m, Stock = 15 }
        };

        [HttpGet]
        public IActionResult GetProducts()
        {
            return Ok(products);
        }
    }
}

