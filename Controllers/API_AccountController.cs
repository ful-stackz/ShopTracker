using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopTracker.Data;
using static ShopTracker.Security.Authentication;

namespace ShopTracker.Controllers
{
    [Produces("application/json")]
    [Route("api/user")]
    public class API_AccountController : Controller
    {
        ShopTrackContext DbContext;

        public API_AccountController(ShopTrackContext context) => DbContext = context;

        [Route("/api/user/current")]
        public IActionResult GetLogged()
        {
            if (!IsLogged(HttpContext)) return null;

            // Retrieve user from database
            var user = DbContext.Users.Find(HttpContext.Session.GetInt32("UserID"));

            return new ObjectResult(user);
        }
    }
}