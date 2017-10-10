using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ShopTracker.Security.Authentication;
using static ShopTracker.MessageDistribute.MessageDistribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace ShopTracker.Filters
{
    public class AuthorizedFilter : IActionFilter
    {
        private readonly ITempDataDictionaryFactory factory;

        public AuthorizedFilter(ITempDataDictionaryFactory _factory)
        {
            factory = _factory;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            ITempDataDictionary tempData = factory.GetTempData(context.HttpContext);

            if (!IsLogged(context.HttpContext, "You must be logged in to do this!", tempData)) context.Result = new RedirectToActionResult("Login", "Account", null);

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
