using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace StoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private static List<Cart> Carts = new List<Cart>();

        [HttpPost]
        public IActionResult CreateCart([FromBody] Cart cart)
        {
            // Add the cart to the list
            Carts.Add(cart);
            
            Store.Instance.Purchase(cart);
            // Return the newly created cart
            return Ok(cart);
        }
    }

}