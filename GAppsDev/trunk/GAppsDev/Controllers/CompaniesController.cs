using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class CompaniesController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Companies/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            return View(db.Companies.ToList().Where(x=>x.Id == CurrentUser.CompanyId));
        }

        //
        // GET: /Companies/Details/5
        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Company company = db.Companies.Single(c => c.Id == id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        //
        // GET: /Companies/Create

        [OpenIdAuthorize]

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Companies/Create
        [OpenIdAuthorize]

        [HttpPost]
        public ActionResult Create(Company company)
        {
            if (ModelState.IsValid)
            {
                db.Companies.AddObject(company);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(company);
        }

        //
        // GET: /Companies/Edit/5
        [OpenIdAuthorize]

        public ActionResult Edit(int id = 0)
        {
            Company company = db.Companies.Single(c => c.Id == id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        //
        // POST: /Companies/Edit/5
        [OpenIdAuthorize]

        [HttpPost]
        public ActionResult Edit(Company company)
        {
            if (ModelState.IsValid)
            {
                db.Companies.Attach(company);
                db.ObjectStateManager.ChangeObjectState(company, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(company);
        }

        //
        // GET: /Companies/Delete/5
        [OpenIdAuthorize]

        public ActionResult Delete(int id = 0)
        {
            Company company = db.Companies.Single(c => c.Id == id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        //
        // POST: /Companies/Delete/5
        [OpenIdAuthorize]

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Company company = db.Companies.Single(c => c.Id == id);
            db.Companies.DeleteObject(company);
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