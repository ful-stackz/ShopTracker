using System;
using System.Linq;
using ShopTracker.Data;
using ShopTracker.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ShopTracker.MessageDistribute;
using static ShopTracker.Security.Authentication;
using static ShopTracker.MessageDistribute.MessageDistribute;

namespace ShopTracker.Controllers
{
    [ServiceFilter(typeof(ShopTracker.Filters.AuthorizedFilter))]
    public class GroupController : Controller
    {
        private ShopTrackContext DbContext;

        public GroupController(ShopTrackContext _dbContext)
        {
            DbContext = _dbContext;
        }

        public IActionResult Index()
        {
            // If this is not the admin redirect to the home page
            //
            if (!HasRole(HttpContext, "Admin")) return RedirectToAction("Index", "Home");

            //
            // User is admin
            //

            // Get all the groups fro the database
            //
            var groups = DbContext.Groups.ToList();

            // Let user see the Group control panel
            //
            return View(groups);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult New(string name, string password)
        {
            // If the group exists for this user then return error
            //
            Security.Authorization DbRetriever = new Security.Authorization(DbContext, HttpContext);
            Models.User currUser = DbRetriever.GetUserFrom(HttpContext);

            // If the UserID in session doesn't reflects an actual user
            // Redirect to the sign up page
            //
            if (currUser == null) return RedirectToAction("Index", "Home");

            // If a group with the same name, bound to this user exists
            // Redirect to user's home page with errors set
            //
            if (currUser.Groups.Where(g => g.Name == name).FirstOrDefault() != null)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Group duplicate",
                    Body = "A group with the same name exists"
                });
                TempData["Errors"] = "HasErrors";
                TempData["ErrorsList"] = new Dictionary<string, string>() { { "GroupName", "A group with that name already exists" } };
                return RedirectToAction("Home", "Account");
            }

            // Make new group model with the given name
            //
            Models.Group newGroup = new Models.Group()
            {
                Name = name,
                UserID = currUser.UserID
            };

            // Try to validate new model
            // If it fails redirect to user home page passing the newGroup model to display errors
            //
            if (TryValidateModel(newGroup) == false)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Model validation",
                    Body = "Model is not valid"
                });
                TempData["Errors"] = "HasErrors";
                TempData["ErrorsList"] = new Dictionary<string, string>() { { "Group", "The group couldn't be validated" } };
                return RedirectToAction("Home", "Account");
            }

            // Try to make new database record
            //
            try
            {
                DbContext.Groups.Add(newGroup);
                DbContext.SaveChanges();

                // If everything is okay go to user's home page
                //
                AddMessage(TempData, new Message()
                {
                    Type = "Success",
                    Title = "Making new DB record",
                    Body = "Saved successfully!"
                });
                return RedirectToAction("Home", "Account");
            }
            catch
            {
                // If it fails to add new record to db
                // Redirect to users home page
                //
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Making new DB record",
                    Body = "Error while trying to save to DB!"
                });
                return RedirectToAction("Home", "Account");
            }
        }

        [HttpGet]
        public ActionResult Details(int _id)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            // If the logged in user does not own the group he is trying to delete = deny
            //
            // Retrieve currently logged user
            //
            Authorization UserRetriever = new Authorization(DbContext, HttpContext);
            var user = UserRetriever.GetUserFrom(HttpContext);

            // Retrieve the group in question
            //
            var group = DbContext.Groups.Find(id);

            if (group.UserID != user.UserID)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "No rights to do this action",
                    Body = "The group are attempting to delete is not yours!"
                });
                
                return RedirectToAction("Home", "Account");
            }

            //
            // User is logged in
            // AND
            // User owns the group
            //

            // If this is the only group that the user has
            // deny removal
            //
            if (user.Groups.Count == 1)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Group delete error",
                    Body = "You can't delete your only group! Make another and then delete this one."
                });

                return RedirectToAction("Home", "Account");
            }

            try
            {
                DbContext.Remove(group);
                DbContext.SaveChanges();

                AddMessage(TempData, new Message()
                {
                    Type = "Success",
                    Title = "Group removal",
                    Body = "You sucessfully removed the group!"
                });
            }
            catch
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Group removal error",
                    Body = "An error was encountered while trying to remove group db record"
                });
            }

            return RedirectToAction("Home", "Account");
        }
    }
}