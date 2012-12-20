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
    public class UsersToJobsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /UsersToJobs/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var hr_userstojobs = db.HR_UsersToJobs.Include("HR_Jobs").Include("User");
            return View(hr_userstojobs.ToList());
        }

        //
        // GET: /UsersToJobs/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            HR_UsersToJobs hr_userstojobs = db.HR_UsersToJobs.Single(h => h.Id == id);
            if (hr_userstojobs == null)
            {
                return HttpNotFound();
            }
            ViewBag.JobDescription = db.HR_JobsDescriptions.Where(x => x.JobId == hr_userstojobs.JobId).ToList();
            return View(hr_userstojobs);
        }

        //
        // GET: /UsersToJobs/Create
        
        [OpenIdAuthorize]
        public ActionResult Create()
        {
            ViewBag.JobId = new SelectList(db.HR_Jobs, "Id", "Name");
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email");
            return View();
        }

        //
        // POST: /UsersToJobs/Create
        
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(HR_UsersToJobs hr_userstojobs)
        {
            if (ModelState.IsValid)
            {
                db.HR_UsersToJobs.AddObject(hr_userstojobs);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JobId = new SelectList(db.HR_Jobs, "Id", "Name", hr_userstojobs.JobId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", hr_userstojobs.UserId);
            return View(hr_userstojobs);
        }

        //
        // GET: /UsersToJobs/Edit/5
        
        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            HR_UsersToJobs hr_userstojobs = db.HR_UsersToJobs.Single(h => h.Id == id);
            if (hr_userstojobs == null)
            {
                return HttpNotFound();
            }
            ViewBag.JobId = new SelectList(db.HR_Jobs, "Id", "Name", hr_userstojobs.JobId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", hr_userstojobs.UserId);
            return View(hr_userstojobs);
        }

        //
        // POST: /UsersToJobs/Edit/5
        
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(HR_UsersToJobs hr_userstojobs)
        {
            if (ModelState.IsValid)
            {
                db.HR_UsersToJobs.Attach(hr_userstojobs);
                db.ObjectStateManager.ChangeObjectState(hr_userstojobs, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JobId = new SelectList(db.HR_Jobs, "Id", "Name", hr_userstojobs.JobId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", hr_userstojobs.UserId);
            return View(hr_userstojobs);
        }

        //
        // GET: /UsersToJobs/Delete/5
        
        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            HR_UsersToJobs hr_userstojobs = db.HR_UsersToJobs.Single(h => h.Id == id);
            if (hr_userstojobs == null)
            {
                return HttpNotFound();
            }
            return View(hr_userstojobs);
        }

        //
        // POST: /UsersToJobs/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            HR_UsersToJobs hr_userstojobs = db.HR_UsersToJobs.Single(h => h.Id == id);
            db.HR_UsersToJobs.DeleteObject(hr_userstojobs);
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