﻿using System;
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

        [OpenIdAuthorize]
        public ActionResult Import()
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

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file, int year = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (year < DateTime.Now.Year - 1)
                        return Error(Errors.BUDGETS_YEAR_PASSED);

                    List<Budgets_ExpensesToIncomes> createdAllocations = new List<Budgets_ExpensesToIncomes>();
                    Budget newBudget = new Budget()
                    {
                        CompanyId = CurrentUser.CompanyId,
                        IsActive = false,
                        Year = year
                    };

                    bool wasCreated;
                    using (BudgetsRepository budgetsRep = new BudgetsRepository())
                    {
                        wasCreated = budgetsRep.Create(newBudget);
                    }

                    if (wasCreated)
                    {
                        byte[] fileBytes = new byte[file.InputStream.Length];
                        file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.InputStream.Length));
                        string fileContent = System.Text.Encoding.Default.GetString(fileBytes);

                        string[] fileLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        int firstValuesLine = 4;

                        bool noErros = true;
                        string errorType = String.Empty;
                        using (ExpensesToIncomeRepository allocationsRep = new ExpensesToIncomeRepository())
                        {
                            for (int i = firstValuesLine; i < fileLines.Length; i++)
                            {
                                string[] fileValues = fileLines[i].Split('\t');
                                Budgets_ExpensesToIncomes newAllocation;

                                try
                                {
                                    newAllocation = new Budgets_ExpensesToIncomes()
                                    {
                                        BudgetId = newBudget.Id,
                                        CompanyId = CurrentUser.CompanyId,
                                        IncomeId = null,
                                        ExpenseId = null,
                                        Amount = null,
                                        January = Decimal.Parse(fileValues[4]),
                                        February = Decimal.Parse(fileValues[6]),
                                        March = Decimal.Parse(fileValues[8]),
                                        April = Decimal.Parse(fileValues[10]),
                                        May = Decimal.Parse(fileValues[12]),
                                        June = Decimal.Parse(fileValues[14]),
                                        July = Decimal.Parse(fileValues[16]),
                                        August = Decimal.Parse(fileValues[18]),
                                        September = Decimal.Parse(fileValues[20]),
                                        October = Decimal.Parse(fileValues[22]),
                                        November = Decimal.Parse(fileValues[24]),
                                        December = Decimal.Parse(fileValues[26])
                                    };
                                }
                                catch
                                {
                                    noErros = false;
                                    errorType = Loc.Dic.Error_FileParseError;
                                    break;
                                }

                                if (allocationsRep.Create(newAllocation))
                                {
                                    createdAllocations.Add(newAllocation);
                                }
                                else
                                {
                                    noErros = false;
                                    errorType = Errors.DATABASE_ERROR; 
                                    break;
                                }
                            }
                        }

                        if (noErros)
                        {
                            return View();
                        }
                        else
                        {
                            using (BudgetsRepository budgetsRep = new BudgetsRepository())
                            using (ExpensesToIncomeRepository allocationsRep = new ExpensesToIncomeRepository())
                            {
                                foreach (var allocation in createdAllocations)
                                {
                                    allocationsRep.Delete(allocation.Id);
                                }

                                budgetsRep.Delete(newBudget.Id);
                            }

                            return Error(errorType);
                        }
                    }
                    else
                    {
                        return Error(Errors.BUDGETS_CREATE_ERROR);
                    }
                }
                else
                {
                    return Error(Errors.INVALID_FORM);
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