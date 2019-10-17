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

        // ------------LOGIN AND REGISTRATION--------------
        #region Login and Registration 
        [Route("/")]
        [HttpGet]
        public IActionResult Index()
        {
            return View("Index");
        }

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
                return View("Index");  
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

                if (HttpContext.Session.GetInt32("UserId") == null)
                {
                    HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                    HttpContext.Session.SetString("Name", userInDb.FirstName);
                }
                return Redirect("Dashboard");
            }
            else
            {
                // Oh no!  We need to return a ViewResponse to preserve the ModelState, and the errors it now contains!
                return View("Login");
            }
        }
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }
        public void CleanUpUserId()
        {
            HttpContext.Session.Clear();
        }
        #endregion

        //-------------------
        [Route("Dashboard")]
        [HttpGet]
        public IActionResult DisplayEvents()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return Redirect("/");
            }
            List<Wedding> ListOfWeddings = dbContext.Weddings.Include( g => g.Guests)
                                .ThenInclude( u => u.GuestUser).ToList();
            int? _userId = HttpContext.Session.GetInt32("UserId");
            //
            List<int> myPlannedEvents = ListOfWeddings.Where(u => u.UserId == _userId).Select(a => a.WeddingId).ToList(); 
            List<int> myEvents = dbContext.Guests.Where(u => u.UserId == _userId).Select( w => w.WeddingId).ToList();
            List<Wedding> DashboardList = new List<Wedding>();
            
            foreach(Wedding w in ListOfWeddings)
            {
                if (myPlannedEvents.Contains(w.WeddingId))
                {
                    w.ActionName = "Delete";
                }
                else if(myEvents.Contains(w.WeddingId))
                {
                    w.ActionName = "Un-RSVP";
                }
                else {
                    w.ActionName = "RSVP";
                }
                DashboardList.Add(w);
            }

            return View("Dashboard", DashboardList);
        }


        [Route("AddWedding")]
        [HttpGet]
        public IActionResult DisplayTheAddWeddingPage()
        {
            ViewBag.Name = HttpContext.Session.GetString("Name");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View("AddWedding");
        }

        [Route("CreateWedding")]
        [HttpPost]
        public IActionResult CreateWedding(Wedding _newWedding)
        {
           if(ModelState.IsValid)
            {
                dbContext.Weddings.Add(_newWedding);
                dbContext.SaveChanges();

                return Redirect("/Dashboard");
            }
            else
            {
                // Oh no!  We need to return a ViewResponse to preserve the ModelState, and the errors it now contains!
                return View("AddWedding");
            }
        }

        [Route("DisplayWedding/{_wedid}")]
        [HttpGet]
        public IActionResult DisplaysTheWeddingInformation(int _wedid)
        {
            Wedding theEvent = dbContext.Weddings
                                .Include( g => g.Guests)
                                .ThenInclude( u => u.GuestUser)
                                .FirstOrDefault( w=> w.WeddingId == _wedid);
            return View("DisplayWedding", theEvent);
        }

        [Route("Dashboard/{_action}/{_wedid}")]
        [HttpGet]
        public IActionResult RegisterForAnEvent(string _action, int _wedid)
        {
            int? _userid = HttpContext.Session.GetInt32("UserId");
            if(_action=="Un-RSVP"){
                Guest gg = dbContext.Guests.Where(x => x.UserId==_userid && x.WeddingId==_wedid).FirstOrDefault();
                dbContext.Guests.Remove(gg);
                dbContext.SaveChanges();
            }
            else if(_action=="RSVP"){
                Guest g = new Guest();
                g.UserId = (int)_userid;
                g.WeddingId = _wedid;
                dbContext.Guests.Add(g);
                dbContext.SaveChanges();
            }
            else if(_action == "Delete"){
                List<Guest> registeredGuests = dbContext.Guests.Where(g => g.WeddingId == _wedid).ToList(); 
                foreach(Guest theguest in registeredGuests){
                    dbContext.Guests.Remove(theguest);
                }
                dbContext.SaveChanges();
                dbContext.Weddings.Remove(dbContext.Weddings.Find(_wedid));
                dbContext.SaveChanges();
            }

         return Redirect("/Dashboard");

        }

        // ------------------ MISCELANEOUS -------------
        #region  Miscellaneous
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}
