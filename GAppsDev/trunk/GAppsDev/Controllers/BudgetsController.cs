using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using GAppsDev.Models.ErrorModels;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class BudgetsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Budgets/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets = db.Budgets.Include("Company");
            return View(budgets.ToList());
        }

        //
        // GET: /Budgets/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budget budget;
                using (BudgetsRepository budgetsRep = new BudgetsRepository())
                {
                    budget = budgetsRep.GetEntity(id);
                }

                if (budget != null)
                {
                    if (budget.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(budget);
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.BUDGETS_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Budgets/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.SystemManager))
            {
                return View();
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Budgets/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budget budget)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    if (budget.Year >= DateTime.Now.Year)
                    {
                        budget.CompanyId = CurrentUser.CompanyId;
                        budget.IsActive = false;

                        bool wasCreated;
                        using (BudgetsRepository budgetRep = new BudgetsRepository())
                        {
                            bool yearExists = budgetRep.GetList().Any(x => x.CompanyId == CurrentUser.CompanyId && x.Year == budget.Year);

                            if (yearExists)
                                return Error(Errors.BUDGETS_YEAR_EXISTS);

                            wasCreated = budgetRep.Create(budget);
                        }

                        if (wasCreated)
                            return RedirectToAction("Index");
                        else
                            return Error(Errors.BUDGETS_CREATE_ERROR);
                    }
                    else
                    {
                        return Error(Errors.BUDGETS_YEAR_PASSED);
                    }
                }
                else
                {
                    return Error(ModelState);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Budgets/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Budget budget = db.Budgets.Single(b => b.Id == id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget.CompanyId);
            return View(budget);
        }

        //
        // POST: /Budgets/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budget budget)
        {
            if (ModelState.IsValid)
            {
                db.Budgets.Attach(budget);
                db.ObjectStateManager.ChangeObjectState(budget, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", budget.CompanyId);
            return View(budget);
        }

        //
        // GET: /Budgets/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Budget budget = db.Budgets.Single(b => b.Id == id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            return View(budget);
        }

        //
        // POST: /Budgets/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budget budget = db.Budgets.Single(b => b.Id == id);
            db.Budgets.DeleteObject(budget);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [OpenIdAuthorize]
        public ActionResult Activate(int id = 0)
        {
            Budget budget = db.Budgets.Single(b => b.Id == id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            return View(budget);
        }

        [OpenIdAuthorize]
        [HttpPost, ActionName("Activate")]
        public ActionResult ActivateConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budget budget;
                using (BudgetsRepository budgetRep = new BudgetsRepository())
                {
                    budget = budgetRep.GetEntity(id);

                    if (budget != null)
                    {
                        if (!budget.IsActive)
                        {
                            Budget oldBudget = budgetRep.GetList().Where( b => b.CompanyId == CurrentUser.CompanyId).SingleOrDefault(x => x.IsActive);

                            if (oldBudget != null)
                            {
                                oldBudget.IsActive = false;
                                budgetRep.Update(oldBudget);
                            }

                            budget.IsActive = true;
                            budgetRep.Update(budget);

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Errors.BUDGETS_ALREADY_ACTIVE);
                        }
                    }
                    else
                    {
                        return Error(Errors.BUDGETS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}