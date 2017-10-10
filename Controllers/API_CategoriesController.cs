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
    [Route("api/categories")]
    public class API_CategoriesController : Controller
    {
        private readonly ShopTrackContext DbContext;

        public API_CategoriesController(ShopTrackContext context)
        {
            DbContext = context;
        }

        [HttpGet]
        public IActionResult All()
        {
            // Retrieve all categories from the database
            //
            var categories = DbContext.Categories.ToList();

            // Return as JSON
            //
            return new ObjectResult(categories);
        }
    }
}