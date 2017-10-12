using System;
using System.Linq;
using ShopTracker.Data;
using ShopTracker.Models;
using ShopTracker.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ShopTracker.MessageDistribute;
using static ShopTracker.Security.Authentication;
using static ShopTracker.MessageDistribute.MessageDistribute;
using Microsoft.EntityFrameworkCore;

namespace ShopTracker.Controllers
{
    [ServiceFilter(typeof(ShopTracker.Filters.AuthorizedFilter))]
    public class MeasureController : Controller
    {
        private ShopTrackContext DbContext;

        public MeasureController(ShopTrackContext context)
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
            List<Models.Measure> measures = DbContext.Measures.ToList();
            DbContext.Items.Load();

            return View(measures);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult New(string name)
        {
            // Check for null input
            //
            if (name == null || name == "")
            {
                AddErrorMessage(TempData, "Can't have an empty measure!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            //
            
            // Check if measure exists
            //
            if (DbContext.Measures.Where(c => c.Name.ToLower() == name.ToLower()).FirstOrDefault() != null)
            {
                AddErrorMessage(TempData, "The measure you tried to add already exists!");
                return RedirectToAction("Index", "Home");
            }

            // Attempt to create new measure
            //
            Measure newMeasure = new Measure() { Name = name };

            // If model not valid
            //
            if (TryValidateModel(newMeasure) == false)
            {
                AddErrorMessage(TempData, "Make sure the measure name is between 3 and 25 characters long.", "Invalid Data!");
                return RedirectToAction("Index", "Home");
            }

            // Model is valid
            // Attempt to save it to the database
            //
            try
            {
                DbContext.Measures.Add(newMeasure);
                DbContext.SaveChanges();

                // Saved successfully
                //
                AddOkMessage(TempData, "You have successfully added a new measure!");
            }
            catch
            {
                AddErrorMessage(TempData, "An error was encountered while saving the new measure to the database!");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            // If user not logged or admin gtfo
            //
            bool Admin = HasRole(HttpContext, "Admin");
            bool Mod = HasRole(HttpContext, "Moderator");
            if (!(Admin || Mod))
            {
                AddErrorMessage(TempData, "This area is with restricted access!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is loged in
            // AND
            // User is admin or moderator
            //

            // Retrieve measure from database
            //
            var measure = DbContext.Measures.Find(id);

            // If measure doesn't exist return to index
            //
            if (measure == null)
            {
                AddErrorMessage(TempData, "The measure you are trying to edit does not exist!");
                return RedirectToAction("Index");
            }

            //
            // Measure exists
            //

            // Render view, pass category as param
            //
            return View(measure);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult Edit(int id, string name)
        {
            // If user not logged in or admin
            // Invalidate edit request
            //
            bool Admin = HasRole(HttpContext, "Admin");
            bool Mod = HasRole(HttpContext, "Moderator");
            if (!(Admin || Mod))
            {
                AddErrorMessage(TempData, "You don't have rights to do that!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            // AND
            // User is admin/moderator
            //

            // Retrieve the measure from the database
            //
            var measureToEdit = DbContext.Measures.Find(id);
            if (measureToEdit == null)
            {
                AddErrorMessage(TempData, "The measure you were trying to edit does not exist!");
                return RedirectToAction("Index");
            }

            // Attempt to edit
            //
            try
            {
                measureToEdit.Name = name;
                DbContext.Measures.Update(measureToEdit);
                DbContext.SaveChanges();

                // Updated successfully
                //
                AddOkMessage(TempData, "Measure updated successfully!");
            }
            catch
            {
                AddErrorMessage(TempData, "Could not save changes to the measure :(");
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
                AddErrorMessage(TempData, "This action is restricted to superiors only", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            // AND
            // User is admin
            //

            // Retrieve measure with id @id
            // Check if measure with id @id exists
            //
            var measureToDelete = DbContext.Measures.Where(c => c.MeasureID == id).FirstOrDefault();
            if (measureToDelete == null)
            {
                AddErrorMessage(TempData, "The measure you are trying to delete doesn't exist");
                return RedirectToAction("Index");
            }

            //
            // Measure exists
            //

            // Attempt to delete measure
            //
            try
            {
                DbContext.Measures.Remove(measureToDelete);
                DbContext.SaveChanges();

                // Measure deleted successfully
                //
                AddOkMessage(TempData, "The measure was successfully deleted");
            }
            catch
            {
                AddErrorMessage(TempData, "An error was encountered while trying to delete the measure");
            }

            return RedirectToAction("Index");
        }
    }
}