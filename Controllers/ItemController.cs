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
    public class ItemController : Controller
    {
        private ShopTrackContext DbContext;

        public ItemController(ShopTrackContext context)
        {
            DbContext = context;
        }

        [Route("/Items")]
        public ActionResult Index()
        {
            bool Admin = HasRole(HttpContext, "Admin");
            bool Mod = HasRole(HttpContext, "Moderator");
            if (!(Admin || Mod))
            {
                AddErrorMessage(TempData, "You dont't have access to this area!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is admin
            //

            // Retrieve all items from the database
            //
            List<Item> items = DbContext.Items.ToList();
            DbContext.Categories.Load();
            DbContext.Measures.Load();

            // Retrieve all categories and measures
            // for creating new items
            //
            List<Measure> measures = DbContext.Measures.ToList();
            List<Category> categories = DbContext.Categories.ToList();
            //
            // Pass the lists to the view data so they can be used in partial views
            //
            ViewData["Measures"] = measures;
            ViewData["Categories"] = categories;

            // Pass all the items to the view
            //
            return View(items);
        }

        [Route("/Items/Browse")]
        public ActionResult Browse()
        {
            // Retrieve all items from the database
            //
            List<Item> items = DbContext.Items.ToList();
            DbContext.Categories.Load();
            DbContext.Measures.Load();

            // Pass the userID to the viewdata
            // to be used in the quick-purchase form
            ViewData.Add("UserId", HttpContext.Session.GetInt32("UserID"));

            // Pass the list to the view
            //
            return View(items);
        }

        [HttpGet]
        public ActionResult New()
        {
            // Display the form
            //
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult New(string name, string description, int category, int measure)
        {
            bool HasError = false;
            Item newItem = null;

            // Check for null name
            //
            if (name == "" || name == null || name.Length > 25)
            {
                AddErrorMessage(TempData, "Please make sure that the name is not empty or longer than 25 characters!");
                HasError = true;
            }

            // Check for null category/measure
            //
            if (category == 0 || measure == 0)
            {
                AddErrorMessage(TempData, "Please specify the category and measure before submitting!");
                HasError = true;
            }

            // Check if item exists
            //
            if (DbContext.Items.Where(i => i.Name == name).FirstOrDefault() != null &&
                DbContext.Items.Where(i => i.Category.CategoryID == category).FirstOrDefault() != null &&
                DbContext.Items.Where(i => i.Measure.MeasureID == measure).FirstOrDefault() != null)
            {
                AddErrorMessage(TempData, "The item you're trying to add already exists!");
                HasError = true;
            }

            // If no error has been encountered so far
            // Attempt to create model and validate
            //
            if (!HasError)
            {
                // Create new model
                //
                newItem = new Item()
                {
                    Name = name,
                    Description = description,
                    Bought = 0,
                    CategoryID = category,
                    MeasureID = measure
                };

                // Try to validate model
                // if it fails return to form with errors
                //
                if (TryValidateModel(newItem) == false)
                {
                    AddErrorMessage(TempData, "Your entry is not valid!");
                    HasError = true;
                }
            }
            
            // If still no error
            // Attempt to save to database
            //
            if (!HasError)
            {
                try
                {
                    DbContext.Items.Add(newItem);
                    DbContext.SaveChanges();

                    // Saved successfully
                    //
                    AddOkMessage(TempData, "The new item was saved successfully!");
                }
                catch (DbUpdateException ex)
                {
                    AddErrorMessage(TempData, "An error was encountered while trying to add the new item.");
                    AddErrorMessage(TempData, ex.ToString(), "DEBUG DbContext Exception");
                    HasError = true;
                }
            }

            // If there was an error
            // Load all categories and measures
            // pass them to ViewData and redirect to New
            //
            if (HasError)
            {
                ViewData["Categories"] = DbContext.Categories.ToList();
                ViewData["Measures"] = DbContext.Measures.ToList();
                return View();
            }

            // If no error was encountered
            // return to 'browse' section
            //
            return RedirectToAction("Browse", "Item");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            // User must be signed in and admin/mod
            //
            if (!(HasRole(HttpContext, "Admin") || HasRole(HttpContext, "Moderator"))) return RedirectToAction("Index", "Home");

            //
            // User is admin/mod
            //

            // Retrieve item from database
            //
            var item = DbContext.Items.Find(id);
            DbContext.Categories.Load();
            DbContext.Measures.Load();
            ViewData.Add("Categories", DbContext.Categories.ToList());
            ViewData.Add("Measures", DbContext.Measures.ToList());
            TempData.Add("CurrentCategory", item.Category);
            TempData.Add("CurrentMeasure", item.Measure);

            // Check if item exists
            //
            if (item == null)
            {
                AddErrorMessage(TempData, "The item you tried to edit doesn't exist!");
                return RedirectToAction("Index");
            }

            // Item exists
            // render edit view
            //
            return View(item);
        }
        
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult Edit(int id, string name, string description, int category, int measure)
        {
            // User must be signed in and admin/mod
            //
            if (!(HasRole(HttpContext, "Admin") || HasRole(HttpContext, "Moderator"))) return RedirectToAction("Index", "Home");

            //
            // User is admin/mod
            //

            // Validate changes
            //
            if (name == "" || name.Length > 25)
            {
                AddErrorMessage(TempData, "Make sure the name is not empty and doesn't exceed 25 characters!");
                return View(DbContext.Items.Find(id));
            }

            // Retrieve item from database
            //
            var itemToEdit = DbContext.Items.Find(id);

            //Edit item
            //
            itemToEdit.Name = name;
            itemToEdit.Description = description;
            itemToEdit.CategoryID = category;
            itemToEdit.MeasureID = measure;

            // Validate item
            //
            if (TryValidateModel(itemToEdit) == false)
            {
                AddErrorMessage(TempData, "The data you entered is invalid!");
                return RedirectToAction("Index");
            }

            // Attempt to apply changes
            //
            try
            {
                DbContext.Items.Update(itemToEdit);
                DbContext.SaveChanges();

                // Successfully edited
                //
                AddOkMessage(TempData, "Item edited successfully!");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                AddErrorMessage(TempData, ex.ToString());
                AddErrorMessage(TempData, "Error while tryin to update item. Review exception thrown.");

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            // User must be signed in and admin/mod
            //
            if (!(HasRole(HttpContext, "Admin") || HasRole(HttpContext, "Moderator"))) return RedirectToAction("Index", "Home");

            //
            // User is admin/mod
            //

            // Retrieve item
            //
            var itemToDelete = DbContext.Items.Find(id);

            // Check if item exists
            //
            if (itemToDelete == null)
            {
                AddErrorMessage(TempData, "The item you are trying to delete doesn't exist!");
                return RedirectToAction("Index");
            }

            // Attempt to delete category
            //
            try
            {
                DbContext.Items.Remove(itemToDelete);
                DbContext.SaveChanges();

                // Category deleted successfully
                //
                AddOkMessage(TempData, "The item was successfully deleted");
            }
            catch (Exception ex)
            {
                AddErrorMessage(TempData, ex.ToString());
                AddErrorMessage(TempData, "An error was encountered while trying to delete the item. Check exception thrown.");
            }

            return RedirectToAction("Index");
        }
    }
}