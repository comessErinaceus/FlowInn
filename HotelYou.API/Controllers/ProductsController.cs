using FlowInn.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;

namespace FlowInn.API.Controllers
{
    [ApiVersion("1.0")]
    //[Route("api/[controller]")]
    //[Route("v{v:apiVersion}/products")]
    [Route("products")]
    [ApiController]
    public class ProductsV1Controller : ControllerBase
    {
        private readonly ShopContext _context;

        //ProductsController constructor
        public ProductsV1Controller(ShopContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        [HttpGet]
        //Not async version
        //public ActionResult<IEnumerable<Product>> GetAllProducts() {
        //    return Ok(_context.Products.ToArray());
        //}

        //Async version
        //1. Change Signature to Task<ActionResult>
        //2. Change injection to "await" version
        //3. Change the ToArray Method to the Async version .. uses Microsoft.EntityFrameworkCore;
        public async Task<ActionResult> GetAllProducts([FromQuery]ProductQueryParameters queryParameters)
        {
            //all the stuff in the DB as a queryable thing, so we can get parts of the wholl instead of all
            //This is better because someone browsing products does not want 10000+ products at the same time.
            IQueryable<Product> products = _context.Products;
            if(queryParameters.MinPrice != null)
            {
                products = products.Where
                    (
                        p => p.value >= queryParameters.MinPrice.Value
                    );
            }
            if(queryParameters.MaxPrice != null) 
            {
                products = products.Where
                    (
                        p => p.value <= queryParameters.MaxPrice.Value
                    );
            }
            if (!string.IsNullOrEmpty(queryParameters.SearchTerm)) 
            {
                products = products.Where(
                    p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                    p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower())
                    );
            }
            if(!string.IsNullOrEmpty(queryParameters.Sku)) 
            {
                products = products.Where
                    (
                        p => p.Sku.ToLower() == queryParameters.Sku.ToLower()
                    );
            }
            if (!string.IsNullOrEmpty(queryParameters.Name)) 
            {
                products = products.Where
                    (
                        p => p.Name.ToLower().Contains(queryParameters.Name.ToLower())
                    );
            }

            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(
                        queryParameters.SortBy,
                        queryParameters.SortOrder
                        );
                }
            }

            //Pagination below
            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);

            return Ok(await products.ToArrayAsync());
        }
        /*Pagination
         *  The Get All Products supports pagenation, give page of size x to recive sections of the whole data.
         */

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id) {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> PutProduct(int id, Product product) { 
        
        if(id != product.Id) { return BadRequest(); }

            _context.Entry(product).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                //
                if (!_context.Products.Any(p => p.Id == id))
                {
                    return NotFound();
                }
                else {
                    throw;
                }
            }
            
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product) {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();// saves to the database, currently using virtual/RAM only database

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id) {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            //the deleted product
            return product;
        }

        //Delete many endpoints
        [HttpDelete("Delete")]
        public async Task<ActionResult> DeleteMultProducts([FromQuery]int[] ids)
        {
            var products = new List<Product>();
            foreach(var id in ids)
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) { 
                    return NotFound();
                }

                products.Add(product);
            }
            
            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();
            //the deleted product
            return Ok(products);
        }

        /*
         * [FromBody] - gets data from the body of the Post/Put
         * [FromRoute] - Data from the route template
         * [FromQuery] - Data from the URL
         */

    }
    //Copy below
    [ApiVersion("2.0")]
    //[Route("api/[controller]")]
    //[Route("v{v:apiVersion}/products")]
    [Route("products")]
    [ApiController]
    public class ProductsV2Controller : ControllerBase
    {
        private readonly ShopContext _context;

        //ProductsController constructor
        public ProductsV2Controller(ShopContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<ActionResult> GetAllProducts([FromQuery] ProductQueryParameters queryParameters)
        {
            //all the stuff in the DB as a queryable thing, so we can get parts of the wholl instead of all
            //This is better because someone browsing products does not want 10000+ products at the same time.
            IQueryable<Product> products = 
                _context.Products.Where(p => p.IsAvailable == true);
            if (queryParameters.MinPrice != null)
            {
                products = products.Where
                    (
                        p => p.value >= queryParameters.MinPrice.Value
                    );
            }
            if (queryParameters.MaxPrice != null)
            {
                products = products.Where
                    (
                        p => p.value <= queryParameters.MaxPrice.Value
                    );
            }
            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                products = products.Where(
                    p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                    p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower())
                    );
            }
            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where
                    (
                        p => p.Sku.ToLower() == queryParameters.Sku.ToLower()
                    );
            }
            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where
                    (
                        p => p.Name.ToLower().Contains(queryParameters.Name.ToLower())
                    );
            }

            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(
                        queryParameters.SortBy,
                        queryParameters.SortOrder
                        );
                }
            }

            //Pagination below
            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);

            return Ok(await products.ToArrayAsync());
        }
        /*Pagination
         *  The Get All Products supports pagenation, give page of size x to recive sections of the whole data.
         */

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> PutProduct(int id, Product product)
        {

            if (id != product.Id) { return BadRequest(); }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //
                if (!_context.Products.Any(p => p.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;}
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            //the deleted product
            return product;
        }

        //Delete many endpoint
        [HttpPost("{id}")]
        [Route("Delete")]
        public async Task<ActionResult> DeleteMultProducts([FromQuery] int[] ids)
        {
            var products = new List<Product>();
            foreach (var id in ids)
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                products.Add(product);
            }

            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();
            //the deleted product
            return Ok(products);
        }


    }
}