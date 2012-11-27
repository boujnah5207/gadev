using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using Mvc4.OpenId.Sample.Security;
using BL;

namespace GAppsDev.Controllers
{
    public class BudgetsController : BaseController
    {
        private Entities db = new Entities();

        [OpenIdAuthorize]
        public ActionResult Home()
        {
            return View();
        }

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets = db.Budgets.Include("Company").Where(x => x.CompanyId == CurrentUser.CompanyId);
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
                    budget = budgetsRep.GetEntity(id, "Company");
                }

                if (budget != null)
                {
                    if (budget.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(budget);
                    }
                    else
                    {
                        return Error(Loc.Dic.error_no_permission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.error_budgets_get_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
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
                return Error(Loc.Dic.error_no_permission);
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
                                return Error(Loc.Dic.error_budgets_year_exists);

                            wasCreated = budgetRep.Create(budget);
                        }

                        if (wasCreated)
                            return RedirectToAction("Index");
                        else
                            return Error(Loc.Dic.error_budgets_create_error);
                    }
                    else
                    {
                        return Error(Loc.Dic.error_budgets_year_passed);
                    }
                }
                else
                {
                    return Error(ModelState);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [OpenIdAuthorize]
        public ActionResult Import(int? id)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);
            if (!id.HasValue)
                return View();
            using (BudgetsRepository budgetsRepository = new BudgetsRepository())
            {
                return View(budgetsRepository.GetEntity(id.Value).Year);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file, int? id, int? year, string budgetType)
        {
            if (Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);
            if (file != null && file.ContentLength <= 0) return Error(Loc.Dic.error_invalid_form);

            if (id.HasValue)
            {
                if (budgetType == "Year")
                {
                    string moved = Interfaces.ImportYearBudget(file.InputStream, CurrentUser.CompanyId, id.Value);
                    if (moved == "OK") return RedirectToAction("index");
                    else return Error(moved);
                }
                if (budgetType == "Month")
                {
                    string moved = Interfaces.ImportMonthBudget(file.InputStream, CurrentUser.CompanyId, id.Value);
                    if (moved == "OK") return RedirectToAction("index");
                    else return Error(moved);
                }
            }
            else if (year.HasValue)
            {
                using (BudgetsRepository budgetsRepository = new BudgetsRepository())
                {
                    Budget newBudget = new Budget();
                    newBudget.Year = year.Value;
                    newBudget.CompanyId = CurrentUser.CompanyId;
                    newBudget.IsActive = false;
                    budgetsRepository.Create(newBudget);
                    if (budgetType == "Year")
                    {
                        string moved = Interfaces.ImportYearBudget(file.InputStream, CurrentUser.CompanyId, newBudget.Id);
                        if (moved == "OK") return RedirectToAction("index");
                        else return Error(moved);
                    }
                    if (budgetType == "Month")
                    {
                        string moved = Interfaces.ImportMonthBudget(file.InputStream, CurrentUser.CompanyId, newBudget.Id);
                        if (moved == "OK") return RedirectToAction("index");
                        else return Error(moved);
                    }
                }

            }
            return Error(Loc.Dic.error_invalid_form);
        }

        [OpenIdAuthorize]
        public ActionResult Export(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budget budgetFromDb;
                List<Budgets_Allocations> allocations = new List<Budgets_Allocations>();

                using (BudgetsRepository budgetsRep = new BudgetsRepository())
                {
                    budgetFromDb = budgetsRep.GetEntity(id, "Budgets_Allocations.Budgets_AllocationToMonth");

                    if (budgetFromDb != null)
                    {
                        allocations = budgetFromDb.Budgets_Allocations.ToList();
                    }
                }

                if (allocations != null)
                {
                    StringBuilder builder = new StringBuilder();

                    foreach (var allocation in allocations)
                    {
                        for (int monthNumber = 1; monthNumber <= 12; monthNumber++)
                        {
                            var allocationMonth = allocation.Budgets_AllocationToMonth.SingleOrDefault(x => x.MonthId == monthNumber);
                            decimal monthAmount = allocationMonth == null ? 0 : allocationMonth.Amount;

                            builder.Append(String.Format("{0} ", monthAmount));
                        }

                        builder.AppendLine();
                    }

                    return File(Encoding.UTF8.GetBytes(builder.ToString()),
                     "text/plain",
                      string.Format("{0} - Budget {1}.txt", CurrentUser.CompanyName, budgetFromDb.Year));
                }
                else
                {
                    return Error(Loc.Dic.error_database_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
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
                            Budget oldBudget = budgetRep.GetList().Where(b => b.CompanyId == CurrentUser.CompanyId).SingleOrDefault(x => x.IsActive);

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
                            return Error(Loc.Dic.error_budgets_already_active);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_budgets_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [ChildActionOnly]
        public ActionResult SubMenu()
        {
            return PartialView();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}