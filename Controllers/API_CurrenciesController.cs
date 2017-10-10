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
    [Route("api/currencies")]
    public class API_CurrenciesController : Controller
    {
        private readonly ShopTrackContext DbContext;

        public API_CurrenciesController(ShopTrackContext context)
        {
            DbContext = context;
        }

        [HttpGet]
        public IActionResult All()
        {
            // Retrieve all currencies
            //
            var currencies = DbContext.Currencies.ToList();

            // Return as JSON
            //
            return new ObjectResult(currencies);
        }
    }
}