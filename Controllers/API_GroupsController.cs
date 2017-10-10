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
    [Route("api/groups")]
    public class API_GroupsController : Controller
    {
        private readonly ShopTrackContext DbContext;

        public API_GroupsController(ShopTrackContext context) => DbContext = context;

        [HttpGet("{id}")]
        public IActionResult All(int id)
        {
            // Retrieve all groups belonging to that user @id
            //
            var groups = DbContext.Groups.Where(g => g.UserID == id).ToList();
            DbContext.Users.Load();
            DbContext.Purchases.Load();
            DbContext.Currencies.Load();

            // Return as JSON
            //
            return new ObjectResult(groups);
        }

        [Route("/api/groups/details/{id}")]
        public IActionResult Details(int id)
        {
            // Retrieve group from db

            var group = DbContext.Groups.Find(id);

            if (group == null) return new BadRequestResult();
            
            DbContext.Currencies.Load();

            return new ObjectResult(group);
        }

        [HttpPost]
        public IActionResult New(int userId, string name, int prefcurrId)
        {
            // Check if userID matches logged in user

            if (HttpContext.Session.GetInt32("UserID") != userId) return new BadRequestResult();

            // Check if name is empty

            if (name == "") return new BadRequestResult();

            try
            {
                Group newGroup = new Group()
                {
                    UserID = userId,
                    Name = name,
                    PrefCurrID = prefcurrId
                };

                DbContext.Groups.Add(newGroup);
                DbContext.SaveChanges();

                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION WHILE PATCH-ING ACCOUNT! MESSAGE:\n{0}", ex.ToString());
                return new BadRequestResult();
            }
        }

        [HttpPatch]
        public IActionResult Change(int groupId, string name, int prefcurrId)
        {
            // Retrieve the group that is to be updated

            var groupToUpdate = DbContext.Groups.Find(groupId);
            if (groupToUpdate == null)
            {
                return new BadRequestResult();
            }

            if (name != groupToUpdate.Name) groupToUpdate.Name = name;

            if (prefcurrId != groupToUpdate.PrefCurrID) groupToUpdate.PrefCurrID = prefcurrId;

            try
            {
                DbContext.Update(groupToUpdate);
                DbContext.SaveChanges();

                return Ok("Updated!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION WHILE PATCH-ING ACCOUNT! MESSAGE:\n{0}", ex.ToString());
                return new BadRequestResult();
            }
        }

        [HttpDelete]
        public IActionResult Delete(int groupId)
        {
            var groupToDelete = DbContext.Groups.Find(groupId);

            if (groupToDelete == null) return new BadRequestResult();

            try
            {
                DbContext.Remove(groupToDelete);
                DbContext.SaveChanges();

                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION CAUGHT WHILE TRYING TO DELETE GROUP! MESSAGE:\n{0}", ex.ToString());

                return new BadRequestResult();
            }
        }
    }
}