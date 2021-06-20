using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NGCore_Blog.Data;
using NGCore_Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace NGCore_Blog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("[action]")]
        [Authorize(policy: "RequiredLoggedIn")]
        public IActionResult GetProduct()
        {
            return Ok(_db.Products.ToList());
        }


        [HttpPost("[action]")]
        [Authorize(policy: "RequiredAdministrationRole")]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel formdata)
        {

            var newproduct = new ProductModel
            {
                Name = formdata.Name,
                Description = formdata.Description,
                ImageUrl = formdata.ImageUrl,
                OutOfStock = formdata.OutOfStock,
                Price = formdata.Price
            };
            await _db.Products.AddAsync(newproduct);
            await _db.SaveChangesAsync();
            return Ok(new JsonResult("Product is Sucessfully Added!!!"));
        }



        //fromroute for getting id api/contrllername/action/id
        //frombody for remaining values
        [HttpPut("[action]/{id}")]
        [Authorize(policy: "RequiredAdministrationRole")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductModel formdata)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findProduct = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (findProduct == null)
            {
                return NotFound();
            }

            //if the product is found
            findProduct.Name = formdata.Name;
            findProduct.Description = formdata.Description;
            findProduct.ImageUrl = formdata.ImageUrl;
            findProduct.OutOfStock = formdata.OutOfStock;
            findProduct.Price = formdata.Price;
            _db.Entry(findProduct).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok(new JsonResult("Product With Id" +id+"is updated"));
        }

        [HttpDelete("[action]/{id}")]
        [Authorize(policy: "RequiredAdministrationRole")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findproduct =  await _db.Products.FindAsync(id);
            if(findproduct==null)
            {
                return NotFound();
            }
            _db.Products.Remove(findproduct);
            await _db.SaveChangesAsync();
            return Ok(new JsonResult("Product With Id" + id + "is deleted Sucessfully!!"));
        }



    }
}
