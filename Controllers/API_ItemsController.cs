using System;
using System.Linq;
using ShopTracker.Data;
using ShopTracker.Models;
using ShopTracker.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShopTracker.MessageDistribute;
using static ShopTracker.Security.Authentication;
using static ShopTracker.MessageDistribute.MessageDistribute;

namespace ShopTracker.Controllers
{
    [Produces("application/json")]
    [Route("api/items")]
    public class API_ItemsController : Controller
    {
        private readonly ShopTrackContext DbContext;

        public API_ItemsController(ShopTrackContext context)
        {
            DbContext = context;
        }

        [HttpGet]
        public IActionResult All()
        {
            var items = DbContext.Items.ToList();
            DbContext.Categories.Load();
            DbContext.Measures.Load();

            return new ObjectResult(items);
        }

        [HttpGet("{id}")]
        public IActionResult Load(int id)
        {
            // Retrieve item with @id
            //
            var item = DbContext.Items.Find(id);
            DbContext.Categories.Load();
            DbContext.Measures.Load();

            return new ObjectResult(item);
        }

        [Route("/api/items/search/{search}")]
        public IActionResult Search(string search)
        {
            search.ToLower();
            var itemsFound = DbContext.Items.Where(i => i.Name.ToLower().Contains(search)).ToList();
            try
            {
                DbContext.Categories.Load();
                DbContext.Measures.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DEBUG!" + ex.ToString());
            }


            return new ObjectResult(itemsFound);
        }

    }
}