using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;

namespace GAppsDev.Controllers
{
    public class LocationsController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /Locations/

        public ActionResult Index()
        {
            var locations = db.Locations.Include("Company");
            return View(locations.ToList());
        }

        //
        // GET: /Locations/Details/5

        public ActionResult Details(int id = 0)
        {
            Location location = db.Locations.Single(l => l.Id == id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        //
        // GET: /Locations/Create

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name");
            return View();
        }

        //
        // POST: /Locations/Create

        [HttpPost]
        public ActionResult Create(Location location)
        {
            if (ModelState.IsValid)
            {
                db.Locations.AddObject(location);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", location.CompanyId);
            return View(location);
        }

        //
        // GET: /Locations/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Location location = db.Locations.Single(l => l.Id == id);
            if (location == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", location.CompanyId);
            return View(location);
        }

        //
        // POST: /Locations/Edit/5

        [HttpPost]
        public ActionResult Edit(Location location)
        {
            if (ModelState.IsValid)
            {
                db.Locations.Attach(location);
                db.ObjectStateManager.ChangeObjectState(location, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", location.CompanyId);
            return View(location);
        }

        //
        // GET: /Locations/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Location location = db.Locations.Single(l => l.Id == id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        //
        // POST: /Locations/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Location location = db.Locations.Single(l => l.Id == id);
            db.Locations.DeleteObject(location);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}