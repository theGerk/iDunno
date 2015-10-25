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
        public ActionResult ViewProduct(string Id)
        {
            TargetAPI api = new TargetAPI();
            TargetItem item = new TargetItem(api.GetObjectData(Id));
            return Redirect(item.Url);
        }
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
        [HttpGet]
        // GET: Home
        public ActionResult Index()
        {

            return View(new HomeScreen());
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        //POST home
        public ActionResult Index(string search)
        {
            TargetAPI api = new TargetAPI();
            HomeScreen screen = new HomeScreen();
            List<TargetItem> items = new List<TargetItem>();
            foreach (dynamic duo in api.FastSearch(search))
            {
                items.Add(new TargetItem(duo));
            }
            screen.Items = items;
            return View(screen);
        }
    }
}