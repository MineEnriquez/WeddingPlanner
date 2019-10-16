using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;    //MMGC: For entity handling
using Microsoft.AspNetCore.Identity;    //MMGC:  For password hashing.
using Microsoft.AspNetCore.Http;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private WeddingPlannerContext dbContext;
        public HomeController(WeddingPlannerContext context) { dbContext = context; }

        [Route("/")]
        [HttpGet]
        public IActionResult Index()
        {
            return View("Index");
        }

        //-----------------
        [HttpPost("Register")]
        // public IActionResult Register(User _user)
        public IActionResult Register(ModelForLoginPage information)
        {
            User _user = information.Register;
            // Check initial ModelState
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (dbContext.Users.Any(u => u.Email == _user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Register.Email", "Email already in use!");
                    return View("Index");
                    // You may consider returning to the View at this point
                }
                // Initializing a PasswordHasher object, providing our User class as its
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                _user.Password = Hasher.HashPassword(_user, _user.Password);

                dbContext.Add(_user);
                dbContext.SaveChanges();
                ViewBag.Email = _user.Email;
                return View("Index");  //if registration is successfull, what? return to the first page and wait for the user to login?
                //or maybe go to the success page with the user already registered and logged in?
            }
            else
            {
                // Oh no!  We need to return a ViewResponse to preserve the ModelState, and the errors it now contains!
                return View("Index");
            }
    }

        [Route("Login")]
        [HttpGet]
        public IActionResult CompleteRegistration()
        {
            return View("Login");
        }

        [Route("Login")]
        [HttpPost]
        // public IActionResult Login(LoginUser userSubmission)
        public IActionResult Login(ModelForLoginPage information)
        {
            LoginUser userSubmission = information.Login;
            HttpContext.Session.Clear();
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);

                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Password", "Invalid Email/Password");
                    //Clean up the session's user Id:
                    return View("Login");

                }

                if(HttpContext.Session.GetInt32("UserId")==null){
                    HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                }
                return Redirect("Dashboard");
            }
            else
            {
                // Oh no!  We need to return a ViewResponse to preserve the ModelState, and the errors it now contains!
                return View("Login");
            }
        }

        public void CleanUpUserId()
        {    
                    HttpContext.Session.Clear();
        }

        // //--------------
        // [Route("Success")]
        // public IActionResult Success()
        // {
        //     if(HttpContext.Session.GetInt32("UserId")==null){
        //         return Redirect("/");
        //     }
        //     return View("Success");
        // }

        [Route("Dashboard")]
        [HttpGet]
        public IActionResult DisplayEvents()
        {
            if(HttpContext.Session.GetInt32("UserId")==null){
                return Redirect("/");
            }
            return View("Dashboard");
        }

        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
