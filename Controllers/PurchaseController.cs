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
    public class PurchaseController : Controller
    {
        private ShopTrackContext DbContext;

        public PurchaseController(ShopTrackContext context)
        {
            DbContext = context;
        }

        public ActionResult Index()
        {
            // Pass user data to view
            ViewData.Add("UserID", HttpContext.Session.GetInt32("UserID"));

            return View();
        }

        [HttpGet]
        public ActionResult New()
        {
            // Pass user ID to view
            //
            ViewData.Add("UserID", HttpContext.Session.GetInt32("UserID"));

            // User is logged in
            // redirect to new purchase creation
            //
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult New(int userId, int item, int group, string quantity, string price, int currency, string date, string provider)
        {
            // Convert date
            IFormatProvider culture = new System.Globalization.CultureInfo("bg-BG", true);
            DateTime dateR = DateTime.Parse(date, culture);

            // Convert strings to be ready to turn into type decimal
            if (quantity.Contains(".")) quantity = quantity.Replace(".", ",");
            if (price.Contains(".")) price = price.Replace(".", ",");
            
            // Convert to decimal
            decimal quantityR = Decimal.Parse(quantity, culture);
            decimal priceR = Decimal.Parse(price, culture);

            // Check if item valid
            var checkItem = DbContext.Items.Find(item);
            if (checkItem == null)
            {
                AddErrorMessage(TempData, "Search your item, add it to your purchase and try again!");
                return RedirectToAction("New");
            }

            // Check if group is valid
            if (DbContext.Groups.Find(group) == null || DbContext.Groups.Find(group).UserID != userId)
            {
                AddErrorMessage(TempData, "Check the group you have selected and try again!");
                return RedirectToAction("New");
            }

            // Check if currency is valid
            if (DbContext.Currencies.Find(currency) == null)
            {
                AddErrorMessage(TempData, "Check the currency you picked!");
                return RedirectToAction("New");
            }

            //
            // Everything seems okay
            //

            // Attempt to create new purchase model
            // and validate
            //
            Purchase newPurchase = new Purchase()
            {
                UserID = userId,
                GroupID = group,
                ItemID = item,
                CurrencyID = currency,
                Date = dateR,
                Price = priceR,
                Quantity = quantityR,
                Provider = provider
            };
            if (TryValidateModel(newPurchase) == false)
            {
                AddErrorMessage(TempData, "Something went wrong! Please try again!");
                return RedirectToAction("New");
            }

            // Attempt to create new DB record
            //
            try
            {
                DbContext.Purchases.Add(newPurchase);
                DbContext.SaveChanges();

                // Added successfully
                // Increase numbers of bough of item
                DbContext.Items.Find(item).Bought += 1;
                DbContext.SaveChanges();

                AddOkMessage(TempData, "Purchase successfully added!");
                return RedirectToAction("New");
            }
            catch (Exception ex)
            {
                AddErrorMessage(TempData, ex.ToString());
                AddErrorMessage(TempData, "An error occurred!");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            // Retrieve purchase to edit
            var purchaseToEdit = DbContext.Purchases.Find(id);
            if (purchaseToEdit == null)
            {
                AddErrorMessage(TempData, "The purchase you are trying to access doesn't exist!");
                return RedirectToAction("Index", "Purchase");
            }

            // Purchase exists, load additional data
            DbContext.Currencies.Load();
            DbContext.Categories.Load();
            DbContext.Measures.Load();
            DbContext.Groups.Load();
            DbContext.Users.Load();
            DbContext.Items.Load();

            // Check if user can edit this purchase
            if (!HasRole(HttpContext, "Admin") && purchaseToEdit.Group.UserID != HttpContext.Session.GetInt32("UserID"))
            {
                AddErrorMessage(TempData, "Restricted access!");
                return RedirectToAction("Index", "Home");
            }

            // User can edit this purchase
            // Render View
            return View(purchaseToEdit);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult Edit(int userId, int purchaseId, int group, string quantity, string price, int currency, string date, string provider)
        {
            // Check input
            if (quantity == null || quantity == "")
            {
                AddErrorMessage(TempData, "Make sure you entered the quantity!");
                return RedirectToAction("Edit", "Purchase", $"{purchaseId}");
            }
            if (price == null || price == "")
            {
                AddErrorMessage(TempData, "Make sure you entered the price!");
                return RedirectToAction("Edit", "Purchase", $"{purchaseId}");
            }
            if (date == null || date == "")
            {
                AddErrorMessage(TempData, "Make sure you entered the date in the format of day/month/year");
                return RedirectToAction("Edit", "Purchase", $"{purchaseId}");
            }

            // Prepare input for conversion
            if (quantity.Contains(".")) quantity = quantity.Replace(".", ",");
            if (price.Contains(".")) price = price.Replace(".", ",");
            IFormatProvider culture = new System.Globalization.CultureInfo("bg-BG", true);

            // Convert input to correct format
            decimal priceR = Convert.ToDecimal(price);
            DateTime dateR = DateTime.Parse(date, culture);
            decimal quantityR = Convert.ToDecimal(quantity);

            // Retrieve purchase to update
            var purchaseToUpdate = DbContext.Purchases.Find(purchaseId);
            if (purchaseToUpdate == null)
            {
                AddErrorMessage(TempData, "You are trying to edit a non-existent purchase!");
                return RedirectToAction("Index", "Purchase");
            }

            // Attempt to update purchase
            try
            {
                purchaseToUpdate.Price = priceR;
                purchaseToUpdate.Quantity = quantityR;
                purchaseToUpdate.GroupID = group;
                purchaseToUpdate.Date = dateR;
                purchaseToUpdate.Provider = provider;

                DbContext.Purchases.Update(purchaseToUpdate);
                DbContext.SaveChanges();

                // Update successfully
                AddOkMessage(TempData, "Purchase update successfully!");
            }
            catch (Exception ex)
            {
                AddErrorMessage(TempData, ex.ToString());
                AddErrorMessage(TempData, "Error while updating purchase!");
            }

            return RedirectToAction("Index", "Purchase");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            // Check if purchase exists
            var purchaseToDelete = DbContext.Purchases.Find(id);
            DbContext.Groups.Load();
            DbContext.Users.Load();
            if (purchaseToDelete == null)
            {
                AddErrorMessage(TempData, "The purchase you are trying to delete doesn't exist!");
                return RedirectToAction("Index", "Purchase");
            }

            // Check if purchase belongs to user logged in
            // or if user logged in is admin
            if (purchaseToDelete.Group.UserID != HttpContext.Session.GetInt32("UserID") && !HasRole(HttpContext, "Admin"))
            {
                AddErrorMessage(TempData, "Restricted access!");
                return RedirectToAction("Index", "Purchase");
            }

            // User owns the purchase or user is admin

            // Attempt to delete purchase
            try
            {
                DbContext.Remove(purchaseToDelete);
                DbContext.SaveChanges();

                // Removed successfully
                AddOkMessage(TempData, "Purchase removed successfully!");
            }
            catch (Exception ex)
            {
                AddErrorMessage(TempData, ex.ToString());
                AddErrorMessage(TempData, "Error while trying to delete purchase!");
            }

            return RedirectToAction("Index", "Purchase");
        }
    }
}