using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopTracker.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ShopTracker.Security
{
    public class Authorization
    {
        private ShopTrackContext dbContext;
        private HttpContext httpContext;

        public Authorization(ShopTrackContext _dbContext, HttpContext _httpContext)
        {
            dbContext = _dbContext;
            httpContext = _httpContext;
        }

        public bool UserExists(string _username)
        {
            var user = dbContext.Users.Where(u => u.Username == _username).FirstOrDefault();
            return (user != null) ? true : false;
        }

        public bool UserExists(int _id)
        {
            var user = dbContext.Users.Find(_id);
            return (user != null) ? true : false;
        }

        public Models.User GetUserFrom(HttpContext httpContext)
        {
            // Retrieve UserID from session
            //
            int userID = httpContext.Session.GetInt32("UserID") ?? 0;

            // If can't retrieve UserID then user is not logged in
            // Return null
            //
            if (userID == 0) return null;

            // Retrieve user from database
            //
            var user = dbContext.Users.Find(userID);

            // Retrieve additional info
            //
            dbContext.Roles.Where(r => r.RoleID == user.RoleID).Load();
            dbContext.Groups.Where(g => g.UserID == user.UserID).Load();
            dbContext.Purchases.Where(p => p.UserID == user.UserID).Load();
            dbContext.Currencies.Load();

            return user;
        }

        public object LoginUser(string _username, string password)
        {
            // Assume a user with this @_username exists
            //
            // Retrieve user from database
            //
            var user = dbContext.Users.Where(u => u.Username == _username).FirstOrDefault();

            // Check for password match
            //
            if (Protection.HashPassword(password, user.Salt) != user.Password) return null;

            // Retrieve additional info
            dbContext.Roles.Where(r => r.RoleID == user.RoleID).Load();
            dbContext.Groups.Where(g => g.UserID == user.UserID).Load();

            // Set session
            httpContext.Session.SetString("LoggedIn", "ConnectionEstablished");
            httpContext.Session.SetString("UserType", string.Format("UserType.{0}", user.Role.Name));
            httpContext.Session.SetString("Username", user.Username);
            httpContext.Session.SetInt32("UserID", user.UserID);

            return user;
        }

        public Models.User LoginUser(int _id, string password)
        {
            // Retrieve user from database
            var user = dbContext.Users.Find(_id);

            // Check for password match
            if (Protection.HashPassword(password, user.Salt) != user.Password) return null;

            // Retrieve additional info
            dbContext.Roles.Where(r => r.RoleID == user.RoleID).Load();
            dbContext.Groups.Where(g => g.UserID == user.UserID).Load();

            // Set session
            httpContext.Session.SetString("LoggedIn", "ConnectionEstablished");
            httpContext.Session.SetString("UserType", string.Format("UserType.{0}", user.Role.Name));
            httpContext.Session.SetString("Username", user.Username);
            httpContext.Session.SetInt32("UserID", user.UserID);

            return user;
        }

        ~Authorization()
        {
            dbContext.Dispose();
            httpContext = null;
        }
    }
}
