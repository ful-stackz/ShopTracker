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
    [ServiceFilter(typeof(ShopTracker.Filters.AuthorizedFilter))]
    public class CategoryController : Controller
    {
        private ShopTrackContext DbContext;

        public CategoryController(ShopTrackContext context)
        {
            DbContext = context;
        }

        public ActionResult Index()
        {
            // If user is not logged in or not admin
            // redirect to the home page
            //
            if (!HasRole(HttpContext, "Admin"))
            {
                AddErrorMessage(TempData, "You don't have access to this part of the website!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            // AND
            // User is admin
            //

            // Retrieve all the categories in a list
            //
            List<Models.Category> categories = DbContext.Categories.ToList();
            DbContext.Items.Load();

            return View(categories);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult New(string name)
        {
            // Check for null input
            //
            if (name == null || name == "")
            {
                AddErrorMessage(TempData, "Can't have an empty category!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            //

            // Format category name
            //
            name.ToLower();
            name = char.ToUpper(name[0]) + name.Substring(1);

            // Check if category exists
            //
            if (DbContext.Categories.Where(c => c.Name == name).FirstOrDefault() != null)
            {
                AddErrorMessage(TempData, "The category you tried to add already exists!");
                return RedirectToAction("Index", "Home");
            }

            // Attempt to create new category
            //
            Category newCategory = new Category()
            {
                Name = name
            };

            // If model not valid
            //
            if (TryValidateModel(newCategory) == false)
            {
                AddErrorMessage(TempData, "Make sure the category name is between 3 and 25 characters long.");
                return RedirectToAction("Index", "Home");
            }

            // Model is valid
            // Attempt to save it to the database
            //
            try
            {
                DbContext.Categories.Add(newCategory);
                DbContext.SaveChanges();

                // Saved successfully
                //
                AddOkMessage(TempData, "You have successfully added a new category!");
            }
            catch
            {
                AddErrorMessage(TempData, "An error was encountered while saving the new category to the database!");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            bool Admin = HasRole(HttpContext, "Admin");
            bool Mod = HasRole(HttpContext, "Moderator");
            if (!(Admin || Mod))
            {
                AddErrorMessage(TempData, "This area is with restricted access!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is admin or moderator
            //

            // Retrieve category from database
            //
            var category = DbContext.Categories.Find(id);

            // If category doesn't exist return to index
            //
            if (category == null)
            {
                AddErrorMessage(TempData, "The category you are trying to edit does not exist!");
                return RedirectToAction("Index");
            }

            //
            // Category exists
            //

            // Render view, pass category as param
            //
            return View(category);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult Edit(int id, string name)
        {
            bool Admin = HasRole(HttpContext, "Admin");
            bool Mod = HasRole(HttpContext, "Moderator");
            if (!(Admin || Mod))
            {
                AddErrorMessage(TempData, "You don't have rights to do that!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is admin/moderator
            //

            // Retrieve the category from the database
            //
            var categoryToEdit = DbContext.Categories.Find(id);
            if (categoryToEdit == null)
            {
                AddErrorMessage(TempData, "The category you were trying to edit does not exist!");
                return RedirectToAction("Index");
            }

            // Format new name
            //
            name.ToLower();
            name = char.ToUpper(name[0]) + name.Substring(1);

            // Attempt to edit
            //
            try
            {
                categoryToEdit.Name = name;
                DbContext.Categories.Update(categoryToEdit);
                DbContext.SaveChanges();

                // Updated successfully
                //
                AddOkMessage(TempData, "Category updated successfully!");
            }
            catch
            {
                AddErrorMessage(TempData, "Could not save changes to the category :(");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            // Only admin can delete stuff
            //
            if (!HasRole(HttpContext, "Admin"))
            {
                AddErrorMessage(TempData, "This action is restricted to superiors only!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is admin
            //

            // Retrieve category with id @id
            // Check if category with id @id exists
            //
            var categoryToDelete = DbContext.Categories.Where(c => c.CategoryID == id).FirstOrDefault();
            if (categoryToDelete == null)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Non-existent Content",
                    Body = "The category you are trying to delete doesn't exist"
                });

                return RedirectToAction("Index");
            }

            //
            // Category exists
            //

            // Attempt to delete category
            //
            try
            {
                DbContext.Categories.Remove(categoryToDelete);
                DbContext.SaveChanges();

                // Category deleted successfully
                //
                AddMessage(TempData, new Message()
                {
                    Type = "Success",
                    Title = "Successfully Deleted",
                    Body = "The category was successfully deleted"
                });
            }
            catch
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Error While Removing",
                    Body = "An error was encountered while trying to delete the category"
                });
            }

            return RedirectToAction("Index");
        }
    }
}