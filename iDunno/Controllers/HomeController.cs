﻿using System;
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
        public async Task<ActionResult> ViewProduct(string Id, string SearchID)
        {
            iDunnoDB db = new iDunnoDB();
            TargetAPI api = new TargetAPI();
            TargetItem item = new TargetItem(api.GetObjectData(Id));
            await db.LogClick(SearchID, Id);
            
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
                return RedirectToAction("Index");   
            }
            return View();
        }
        [HttpGet]
        // GET: Home
        public async Task<ActionResult> Index()
        {
            iDunnoDB db = new iDunnoDB();
            return View(new HomeView(new HomeScreen() { PopularItems = await db.GetTopProducts(), CurrentUser = await db.GetUserById((await db.getCurrentSession()).User) }));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        //POST home
        public async Task<ActionResult> Index(string search, HomeScreen screen)
        {
            iDunnoDB db = new iDunnoDB();
            var iable = await db.LogSearch(search);
            TargetAPI api = new TargetAPI();
            screen.SearchQuery = search;
            screen.QueryID = iable.Id.ToString();
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