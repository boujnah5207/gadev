using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;
using Mvc4.OpenId.Sample.Security;
using DA;

namespace GAppsDev.Controllers
{
    public class LocationsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Locations/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var locations = db.Locations.Include("Company").Where(x => x.CompanyId == CurrentUser.CompanyId);
            return View(locations.ToList());
        }

        //
        // GET: /Locations/Details/5
        [OpenIdAuthorize]

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
        [OpenIdAuthorize]

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name");
            return View();
        }

        //
        // POST: /Locations/Create
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Location location)
        {
            
            if (ModelState.IsValid)
            {
                location.CompanyId = CurrentUser.CompanyId;
                using (LocationsRepository locationsRepository = new LocationsRepository())
                {
                    locationsRepository.Create(location);
                }
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", location.CompanyId);
            return View(location);
        }

        //
        // GET: /Locations/Edit/5
        [OpenIdAuthorize]
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
        [OpenIdAuthorize]
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
        [OpenIdAuthorize]
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
        [OpenIdAuthorize]
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