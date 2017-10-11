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
    [Route("api/purchases")]
    public class API_PurchasesController : Controller
    {
        private readonly ShopTrackContext DbContext;

        public API_PurchasesController(ShopTrackContext context)
        {
            DbContext = context;
        }

        [Route("/api/purchases/group")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult GroupAll(int id)
        {
            // Retrieve all purchases from the databases
            // and load dependent data
            //
            var purchases = DbContext.Purchases.Where(p => p.GroupID == id).ToList();
            DbContext.Users.Load();
            DbContext.Items.Load();
            DbContext.Categories.Load();
            DbContext.Measures.Load();
            DbContext.Currencies.Load();

            // Return as JSON
            //
            return new ObjectResult(purchases);
        }

        [Route("/api/purchases/user")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult UserAll(int id)
        {
            // Retrieve all purchases from the databases
            // and load dependent data
            //
            var purchases = DbContext.Purchases.Where(p => p.UserID == id).ToList();
            DbContext.Users.Load();
            DbContext.Items.Load();
            DbContext.Categories.Load();
            DbContext.Measures.Load();
            DbContext.Currencies.Load();

            // Return as JSON
            //
            return new ObjectResult(purchases);
        }

        [Route("/api/purchases/new")]
        [ServiceFilter(typeof(ShopTracker.Filters.AuthorizedFilter))]
        [HttpPost]
        public IActionResult NewPurchase(int userId, int groupId, int itemId, int currency, string quantity, string price, string date, string provider)
        {
            // Check for null input

            AddInfoMessage(TempData, string.Format("userId: {0}; groupId: {1}; itemId: {2}; currency: {3}; quantity: {4}; price: {5}; date: {6}",
                userId, groupId, itemId, currency, quantity, price, date));

            if (quantity == "" || price == "" || date == "")
            {
                AddErrorMessage(TempData, "Invalid input!");
                return RedirectToAction("Browse", "Item");
            }

            // Check for existing user, group, item

            var user = DbContext.Users.Find(userId);
            var item = DbContext.Items.Find(itemId);
            var group = DbContext.Groups.Find(groupId);

            if (user == null || item == null || group == null || group.UserID != userId)
            {
                AddErrorMessage(TempData, "Invalied user or item data!");
                return RedirectToAction("Browse", "Item");
            }

            // Convert date

            IFormatProvider culture = new System.Globalization.CultureInfo("bg-BG", true);
            DateTime dateR = DateTime.Parse(date, culture);

            // Convert strings to be ready to turn into type decimal

            if (quantity.Contains(".")) quantity = quantity.Replace(".", ",");
            if (price.Contains(".")) price = price.Replace(".", ",");

            // Convert to decimal

            decimal quantityR = Decimal.Parse(quantity, culture);
            decimal priceR = Decimal.Parse(price, culture);

            // Attempt to create new purchase model

            Purchase newPurchase = new Purchase()
            {
                UserID = userId,
                GroupID = groupId,
                ItemID = itemId,
                CurrencyID = currency,
                Date = dateR,
                Price = priceR,
                Quantity = quantityR,
                Provider = provider
            };

            // Attempt to validate new model

            if (TryValidateModel(newPurchase) == false)
            {
                AddErrorMessage(TempData, "Something went wrong! Please try again!");
                return RedirectToAction("New", "Purchase");
            }

            // Attempt to create new DB record

            try
            {
                DbContext.Purchases.Add(newPurchase);
                DbContext.SaveChanges();

                // Added successfully
                // Increase numbers of bough of item

                DbContext.Items.Find(itemId).Bought += 1;
                DbContext.SaveChanges();

                AddOkMessage(TempData, "Purchase successfully added!");
                return RedirectToAction("Browse", "Item");
            }
            catch (Exception ex)
            {
                AddErrorMessage(TempData, ex.ToString());
                AddErrorMessage(TempData, "An error occurred!");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}