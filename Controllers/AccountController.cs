using System;
using System.Linq;
using ShopTracker.Data;
using System.Diagnostics;
using ShopTracker.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ShopTracker.MessageDistribute;
using Microsoft.EntityFrameworkCore;
using ShopTracker.Filters;
using static ShopTracker.Security.Authentication;
using static ShopTracker.MessageDistribute.MessageDistribute;

namespace ShopTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopTrackContext DbContext;

        public AccountController(ShopTrackContext context)
        {
            DbContext = context;
        }

        public ActionResult Index()
        {
            // Check if the user is loged and is admin
            // If he is not then redirect him to the home page
            if (!(IsLogged(HttpContext) && HasRole(HttpContext, "Admin"))) return Redirect("/Home");

            //
            // User is signed in
            // AND
            // User is admin
            //

            // Retrieve all the users from the database in a list
            //
            List<Models.User> users = DbContext.Users.ToList();
            DbContext.Groups.Load();
            DbContext.Roles.Load();

            // Show the admin page
            //
            return View(users);
        }

        [ServiceFilter(typeof(AuthorizedFilter))]
        public ActionResult Home()
        {
            // Initialize new User Retrieving Process, providing the current DbContext and HttpContext
            //
            Authorization RetrieveUser = new Authorization(DbContext, HttpContext);

            // Retrieve user in object
            // If there is no user with such id the GetUser method will return a null object
            //
            var user = RetrieveUser.GetUserFrom(HttpContext);

            // If the user exists and is loaded then go to Account/Home page
            //
            if (user != null) return View(user);

            // User did not exist or wasn't loaded
            // Redirect to login page
            //
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            // If user is already logged in redirect to the home page
            //
            if (IsLogged(HttpContext)) return Redirect("/Home");

            // If user is not logged in show him the login page
            //
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult Login(string username, string password)
        {
            // If the user is authenticated redirect to the home page
            //
            if (IsLogged(HttpContext)) return RedirectToAction("Index", "Home");

            //
            // The code below is to be executed if the user is not logged in
            //

            // Initialize new Login Procedure,  providing the current DbContext and HttpContext
            //
            Authorization LoginProced = new Authorization(DbContext, HttpContext);

            // Check if the given username actually refers to a records in the database
            // If it does not refer to a record then redirect to login page with corresponding error
            //
            if (!LoginProced.UserExists(username))
            {
                ViewData["Errors"] = "HasErrors";
                ViewData["ErrorsList"] = new Dictionary<string, string>()
                {
                    { "Username", "Wrong username" },
                    { "Password", "Wrong password" }
                };
                return View();  // Wrong username
            }

            // If the user exists then proceed to check the password
            // If the password is incorrect the LoginUser method will return a null object
            // In case of wrong password redirect to login page with corresponding error
            //
            var user = LoginProced.LoginUser(username, password);
            if (user == null)
            {
                ViewData["Errors"] = "HasErrors";
                ViewData["ErrorsList"] = new Dictionary<string, string>()
                {
                    { "Username", "Wrong username" },
                    { "Password", "Wrong password" }
                };
                return View(); // Wrong password
            }

            // If everything set a welcome message
            // and go to the home page
            //
            AddMessage(TempData, new Message()
            {
                Type = "Success",
                Title = "Welcome!",
                Body = "We're glad to see you!"
            });
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult New()
        {
            // If the user is already authenticated no need to show the sign up page
            // Redirect user to the home page
            //
            if (IsLogged(HttpContext)) return Redirect("/Home");

            //
            // User is not signed
            //

            // Show user the sign up page
            //
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public ActionResult New(string username, string password)
        {
            // If the user is already authenticated no need to show the sign up page
            //
            if (IsLogged(HttpContext)) return RedirectToAction("Index", "Home");

            //
            // User is not logged in
            //

            // Check if username already exists
            //
            if (DbContext.Users.Where(u => u.Username == username).FirstOrDefault() != null)
            {
                AddErrorMessage(TempData, "This username already exists!");
                return View(); // Username exists
            }

            //
            // Username does note exist in the database
            //

            // Make new model for the new user
            // Pass the username and password provided by the user
            //
            Models.User newUser = new Models.User()
            {
                Username = username,
                Password = password
            };

            // Try to validate the new model
            //
            if (TryValidateModel(newUser) == false) { return View(newUser); } // Model not valid

            //
            // Model is valid
            //

            // Secure the password through hashing
            //
            newUser.SecurePassword();

            // Try to make new database record from new model
            //
            try
            {
                DbContext.Users.Add(newUser);
                DbContext.SaveChanges();

                //
                // New record added successfully
                //

                // Add the default Group to the new User
                //
                Models.Group defGroup = new Models.Group() { Name = "MyGroup" };
                defGroup.UserID = DbContext.Users.Last().UserID;
                DbContext.Groups.Add(defGroup);
                DbContext.SaveChanges();

                // Set a welcoming/helping message
                //
                AddOkMessage(TempData, "You successfully signed up for the best expenses tracker! You can now login and start tracking!", "Welcome to ShopTracker!");
                
                // Redirect to the login page
                //
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Something went wrong
                // Redirect user to a new sign up form
                //
                AddErrorMessage(TempData, ex.ToString());
                AddErrorMessage(TempData, "Error while making a new record!");
                return RedirectToAction("New");
            }
        }
        
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ServiceFilter(typeof(AuthorizedFilter))]
        public ActionResult ChangePassword(int id, string oldPassword, string newPassword, string confirmPassword)
        {
            // Check if password is confirmed
            //
            if (newPassword != confirmPassword)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Confirmation Error!",
                    Body = "The confirm password doesn't match!"
                });

                return RedirectToAction("Home");
            }

            // Check if user with UserID == @id exists
            //
            Authorization Authorizer = new Authorization(DbContext, HttpContext);
            if (!Authorizer.UserExists(id))
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Non-existent User",
                    Body = string.Format("User with id {0} does not exist!", id)
                });

                return RedirectToAction("Index", "Home");
            }

            // Retrieve the user to change password
            // Retrieve the currently logged user
            //
            var userToChange = DbContext.Users.Find(id);
            var userCurrent = Authorizer.GetUserFrom(HttpContext);

            // Check if oldPassword is correct
            if (Protection.HashPassword(oldPassword, userToChange.Salt) != userToChange.Password)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Confirmation Error!",
                    Body = "Wrong current password!"
                });

                return RedirectToAction("Home");
            }

            // If current user is not admin
            // or is not the owner of the account whose password is being changed
            // redirect to the home page
            //
            if (userCurrent.Role.Name != "Admin" && userCurrent.UserID != id)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "You Lack Power!",
                    Body = "You can't do what you just tried to do!"
                });

                return RedirectToAction("Index", "Home");
            }

            //
            // userToChange exists
            // AND
            // userCurrent is either admin or the user who's trying to change his password
            //

            // Attempt to change the password
            //
            try
            {
                userToChange.Password = newPassword;
                userToChange.SecurePassword();
                DbContext.Update(userToChange);
                DbContext.SaveChanges();

                // Updated password successfully
                //
                AddMessage(TempData, new Message()
                {
                    Type = "Success",
                    Title = "Password Changed Successfully!",
                    Body = "The password has been changed successfully!"
                });

                return (userCurrent.Role.Name == "Admin") ? RedirectToAction("Index", "Account") : RedirectToAction("Logout");
            }
            catch
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Error While Changing Password!",
                    Body = "The password could not be changed successfully!"
                });

                return (userCurrent.Role.Name == "Admin") ? RedirectToAction("Index", "Account") : RedirectToAction("Home", "Account");
            }

        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ServiceFilter(typeof(AuthorizedFilter))]
        public ActionResult Delete(int id, string username)
        {
            // Check if user with UserID == @id exists
            //
            Authorization Authorizer = new Authorization(DbContext, HttpContext);
            if (!Authorizer.UserExists(id))
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Non-existent User",
                    Body = string.Format("User with id {0} does not exist!", id)
                });

                return RedirectToAction("Index", "Home");
            }

            // Retrieve the user to be deleted
            // and user currently logged in
            var userToDelete = DbContext.Users.Find(id);
            var userCurrent = Authorizer.GetUserFrom(HttpContext);
            
            // Check if confirmation is OKAY
            // To delete an account you have to type in the username
            // in the field of the form for delete
            //
            if (userToDelete.Username != username)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Confirmation Not Successfull!",
                    Body = "You failed to confirm the total removal of your account."
                });

                return (userCurrent.Role.Name == "Admin") ? RedirectToAction("Index", "Account") : RedirectToAction("Home", "Account");
            }

            // If user logged in is not admin
            // OR
            // user logged in has different UserID than @id
            // redirect to the home page
            //
            if (userCurrent.Role.Name != "Admin" && userCurrent.UserID != id)
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "You Lack Power!",
                    Body = "You can't do what you just tried to do!"
                });

                return RedirectToAction("Index", "Home");
            }

            //
            // userToDelete exists
            // AND
            // userCurrent is either admin or the user who's trying to delete his account
            //

            // Delete user from database
            //
            try
            {
                DbContext.Remove(userToDelete);
                DbContext.SaveChanges();

                // The user has been successfully removed
                //
                AddMessage(TempData, new Message()
                {
                    Type = "Warning",
                    Title = "Account Deleted Successfully!",
                    Body = "I'm sorry to see you go :( Contact me and share your oppinion. It means the world to me!"
                });

                // If the user logged in is the admin
                // redirect him to the admin page
                // else redirect to the home page
                //
                return (userCurrent.Role.Name == "Admin") ? RedirectToAction("Index", "Account") : RedirectToAction("Logout");
            }
            catch
            {
                AddMessage(TempData, new Message()
                {
                    Type = "Danger",
                    Title = "Error On Delete!",
                    Body = "An error occured while trying to remove the user record from the database!"
                });

                return RedirectToAction("Index", "Home");
            }
        }

        [ServiceFilter(typeof(AuthorizedFilter))]
        public ActionResult Logout()
        {
            // Clear the session
            // => sign out the user
            //
            HttpContext.Session.Clear();

            // Redirect user to the home page
            //
            AddInfoMessage(TempData, "You have been logged out!");
            return RedirectToAction("Index", "Home");
        }
    }
}
