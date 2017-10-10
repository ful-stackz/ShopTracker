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
    public class CurrencyController : Controller
    {
        private ShopTrackContext DbContext;

        public CurrencyController(ShopTrackContext context)
        {
            DbContext = context;
        }

        public ActionResult Index()
        {
            // If user is not logged in or not admin
            // redirect to the home page
            //
            if (!IsLogged(HttpContext) || !HasRole(HttpContext, "Admin"))
            {
                AddErrorMessage(TempData, "You don't have access to this part of the website!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            // AND
            // User is admin
            //

            // Retrieve all the currencies in a list
            //
            List<Models.Currency> currencies = DbContext.Currencies.ToList();
            DbContext.Items.Load();

            return View(currencies);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult New(string name, string fullName)
        {
            // If user not logged in redirect to the home page
            //
            if (!IsLogged(HttpContext, "You must be logged in to do this!", TempData)) return RedirectToAction("Login", "Account");

            // Check for null input
            //
            if (name == null || name == "" || fullName == null || fullName == "")
            {
                AddErrorMessage(TempData, "Can't have an empty currency!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            //

            // Format currency name
            //
            name.ToUpper();
            fullName = char.ToUpper(fullName[0]) + fullName.Substring(1);

            // Check if currency exists
            //
            if (DbContext.Currencies.Where(c => c.Name == name).FirstOrDefault() != null)
            {
                AddErrorMessage(TempData, "The currency you tried to add already exists!");
                return RedirectToAction("Index", "Home");
            }

            // Attempt to create new currency
            //
            Currency newCurrency = new Currency()
            {
                Name = name,
                FullName = fullName
            };

            // If model not valid
            //
            if (TryValidateModel(newCurrency) == false)
            {
                AddErrorMessage(TempData, "Make sure the currency name is 3 characters long and the full name doesn't exceed 20 characters.");
                return RedirectToAction("Index", "Home");
            }

            // Model is valid
            // Attempt to save it to the database
            //
            try
            {
                DbContext.Currencies.Add(newCurrency);
                DbContext.SaveChanges();

                // Saved successfully
                //
                AddOkMessage(TempData, "You have successfully added a new currency!");
            }
            catch
            {
                AddErrorMessage(TempData, "An error was encountered while saving the new currency to the database!");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            // If user not logged or admin gtfo
            //
            bool Logged = IsLogged(HttpContext);
            bool Admin = HasRole(HttpContext, "Admin");
            bool Mod = HasRole(HttpContext, "Moderator");
            if (!(Logged && (Admin || Mod)))
            {
                AddErrorMessage(TempData, "This area is with restricted access!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is loged in
            // AND
            // User is admin or moderator
            //

            // Retrieve currency from database
            //
            var currency = DbContext.Currencies.Find(id);

            // If currency doesn't exist return to index
            //
            if (currency == null)
            {
                AddErrorMessage(TempData, "The currency you are trying to edit does not exist!");
                return RedirectToAction("Index");
            }

            //
            // Currency exists
            //

            // Render view, pass currency as param
            //
            return View(currency);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult Edit(int id, string name, string fullName)
        {
            // If user not logged in or admin
            // Invalidate edit request
            //
            bool Logged = IsLogged(HttpContext);
            bool Admin = HasRole(HttpContext, "Admin");
            bool Mod = HasRole(HttpContext, "Moderator");
            if (!(Logged && (Admin || Mod)))
            {
                AddErrorMessage(TempData, "You don't have rights to do that!", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            // AND
            // User is admin/moderator
            //

            // Check for null input
            //
            if (name == null || name == "" || fullName == null || fullName == "")
            {
                AddErrorMessage(TempData, "Can't have an empty currency!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            //

            // Format currency name
            //
            name.ToUpper();
            fullName = char.ToUpper(fullName[0]) + fullName.Substring(1);

            // Retrieve the currency from the database
            //
            var currencyToEdit = DbContext.Currencies.Find(id);
            if (currencyToEdit == null)
            {
                AddErrorMessage(TempData, "The currency you were trying to edit does not exist!");
                return RedirectToAction("Index");
            }

            // Attempt to edit
            //
            try
            {
                currencyToEdit.Name = name;
                currencyToEdit.FullName = fullName;
                DbContext.Currencies.Update(currencyToEdit);
                DbContext.SaveChanges();

                // Updated successfully
                //
                AddOkMessage(TempData, "Currency updated successfully!");
            }
            catch
            {
                AddErrorMessage(TempData, "Could not save changes to the currency :(");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            // Only admin can delete stuff
            //
            if (!IsLogged(HttpContext) || !HasRole(HttpContext, "Admin"))
            {
                AddErrorMessage(TempData, "This action is restricted to superiors only", "Restricted Access!");
                return RedirectToAction("Index", "Home");
            }

            //
            // User is logged in
            // AND
            // User is admin
            //

            // Retrieve currency with id @id
            // Check if currency with id @id exists
            //
            var currencyToDelete = DbContext.Currencies.Where(c => c.CurrencyID == id).FirstOrDefault();
            if (currencyToDelete == null)
            {
                AddErrorMessage(TempData, "The currency you are trying to delete doesn't exist");
                return RedirectToAction("Index");
            }

            //
            // Currency exists
            //

            // Attempt to delete currency
            //
            try
            {
                DbContext.Currencies.Remove(currencyToDelete);
                DbContext.SaveChanges();

                // Currency deleted successfully
                //
                AddOkMessage(TempData, "The currency was successfully deleted");
            }
            catch
            {
                AddErrorMessage(TempData, "An error was encountered while trying to delete the currency");
            }

            return RedirectToAction("Index");
        }
    }
}