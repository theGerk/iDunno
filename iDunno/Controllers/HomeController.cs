using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iDunno.Models;
using System.Threading.Tasks;
namespace iDunno.Controllers
{
    //Target Product API 


    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        [HttpPost]
        public async Task<ActionResult> Register(iDunno.Models.RegistrationScreen screen)
        {
            if (this.ModelState.IsValid)
            {
                iDunnoDB db = new iDunnoDB();
                await db.CreateUser(screen);
                return RedirectToAction("Index");
            } 
            return View();

        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult Login(LoginScreen screen)
        {
            if(this.ModelState.IsValid)
            {

            }
            return View();
        }
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
    }
}