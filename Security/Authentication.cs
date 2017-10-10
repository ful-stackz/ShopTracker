using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using static ShopTracker.MessageDistribute.MessageDistribute;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ShopTracker.Security
{
    public class Authentication
    {
        public static bool IsLogged(HttpContext httpContext, string errorMessage = null, ITempDataDictionary tempData = null)
        {
            bool step1 = httpContext.Session.GetString("LoggedIn") == "ConnectionEstablished";
            bool step2 = httpContext.Session.GetInt32("UserID") != null;

            if (errorMessage == null) return (step1 && step2) ? true : false;
            else if (step1 && step2) return true;
            else AddErrorMessage(tempData, errorMessage, "Login Required!"); return false;
        }

        public static bool HasRole(HttpContext httpContext, string role)
        {
            return (httpContext.Session.GetString("UserType") == "UserType." + role) ? true : false;
        }
    }
}
