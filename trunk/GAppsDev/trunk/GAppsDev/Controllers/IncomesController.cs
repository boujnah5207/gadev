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
    public class IncomesController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Income/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets_incomes = db.Budgets_Incomes.Include("Budget").Include("Budgets_Incomes_types").Include("Budgets_Incomes_Institutions");
            return View(budgets_incomes.ToList());
        }

        //
        // GET: /Income/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Incomes income;
                using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                {
                    income = incomesRep.GetEntity(id, "Budget", "Budgets_Incomes_types", "Budgets_Incomes_Institutions");
                }

                if (income != null)
                {
                    if (income.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(income);
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.INCOME_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Income/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.SystemManager))
            {
                using (BudgetsRepository budgetRep = new BudgetsRepository())
                using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                using (IncomeTypesRepository incomeTypesRep = new IncomeTypesRepository())
                using (InstitutionsRepository institutionsRep = new InstitutionsRepository())
                {
                    List<SelectListItemFromDB> budgetsList = budgetRep.GetList()
                        .Where(budget => budget.CompanyId == CurrentUser.CompanyId && !budget.IsViewOnly)
                        .Select(a => new { Id = a.Id, Name = a.Year })
                        .AsEnumerable()
                        .Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.Name.ToString() })
                        .ToList();

                    List<SelectListItemFromDB> incomeTypesList = incomeTypesRep.GetList()
                        .Where(type => type.CompanyId == CurrentUser.CompanyId)
                        .Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.Name })
                        .ToList();

                    List<SelectListItemFromDB> institutionsList = institutionsRep.GetList()
                        .Where(type => type.CompanyId == CurrentUser.CompanyId)
                        .Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.Name })
                        .ToList();

                    ViewBag.BudgetId = new SelectList(budgetsList, "Id", "Name");
                    ViewBag.BudgetIncomeTypeId = new SelectList(incomeTypesList, "Id", "Name");
                    ViewBag.BudgetsIncomeInstitutions = new SelectList(institutionsList, "Id", "Name");
                }

                return View();
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Income/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_Incomes budgets_incomes)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    Budget budget;
                    Budgets_Incomes_types incomeType;
                    Budgets_Incomes_Institutions institution;

                    using (IncomeTypesRepository incomeTypesRep = new IncomeTypesRepository())
                    using (BudgetsRepository budgetRep = new BudgetsRepository())
                    using (InstitutionsRepository InstitutionsRep = new InstitutionsRepository())
                    {
                        budget = budgetRep.GetEntity(budgets_incomes.BudgetId);
                        incomeType = incomeTypesRep.GetEntity(budgets_incomes.BudgetIncomeTypeId);

                        if (budgets_incomes.BudgetsIncomeInstitutions.HasValue)
                            institution = InstitutionsRep.GetEntity(budgets_incomes.BudgetsIncomeInstitutions.Value);
                        else
                            institution = null;
                    }

                    if (budget != null && incomeType != null && (!budgets_incomes.BudgetsIncomeInstitutions.HasValue || institution != null))
                    {
                        if (budget.CompanyId == CurrentUser.CompanyId && incomeType.CompanyId == CurrentUser.CompanyId && (!budgets_incomes.BudgetsIncomeInstitutions.HasValue || institution.CompanyId == CurrentUser.CompanyId))
                        {
                            bool wasCreated;
                            using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                            {
                                wasCreated = incomesRep.Create(budgets_incomes);
                            }

                            if (wasCreated)
                                return RedirectToAction("Index");
                            else
                                return Error(Errors.INCOME_CREATE_ERROR);
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.DATABASE_ERROR);
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
        // GET: /Income/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Incomes income;

                using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                using (BudgetsRepository budgetRep = new BudgetsRepository())
                using (IncomeTypesRepository incomeTypesRep = new IncomeTypesRepository())
                using (InstitutionsRepository institutionsRep = new InstitutionsRepository())
                {
                    income = incomesRep.GetEntity(id);

                    try
                    {
                        List<SelectListItemFromDB> budgetsList = budgetRep.GetList()
                            .Where(budget => budget.CompanyId == CurrentUser.CompanyId && !budget.IsViewOnly)
                            .Select(a => new { Id = a.Id, Name = a.Year })
                            .AsEnumerable()
                            .Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.Name.ToString() })
                            .ToList();

                        List<SelectListItemFromDB> incomeTypesList = incomeTypesRep.GetList()
                            .Where(type => type.CompanyId == CurrentUser.CompanyId)
                            .Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.Name })
                            .ToList();

                        List<SelectListItemFromDB> institutionsList = institutionsRep.GetList()
                            .Where(type => type.CompanyId == CurrentUser.CompanyId)
                            .Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.Name })
                            .ToList();
                        institutionsList.Insert(0, new SelectListItemFromDB() { Id = null, Name = "" });

                        ViewBag.BudgetId = new SelectList(budgetsList, "Id", "Name", income.BudgetId);
                        ViewBag.BudgetIncomeTypeId = new SelectList(incomeTypesList, "Id", "Name", income.BudgetIncomeTypeId);
                        ViewBag.BudgetsIncomeInstitutions = new SelectList(institutionsList, "Id", "Name", income.BudgetsIncomeInstitutions);
                    }
                    catch
                    {
                        return Error(Errors.DATABASE_ERROR);
                    }
                }

                if (income != null)
                {
                    if (income.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(income);
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.INCOME_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Income/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_Incomes budgets_incomes)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    Budgets_Incomes incomeFromDB;
                    Budget budget;
                    Budgets_Incomes_types incomeType;
                    Budgets_Incomes_Institutions institution;

                    using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                    using (IncomeTypesRepository incomeTypesRep = new IncomeTypesRepository())
                    using (BudgetsRepository budgetRep = new BudgetsRepository())
                    using (InstitutionsRepository InstitutionsRep = new InstitutionsRepository())
                    {
                        incomeFromDB = incomesRep.GetEntity(budgets_incomes.Id);
                        budget = budgetRep.GetEntity(budgets_incomes.BudgetId);
                        incomeType = incomeTypesRep.GetEntity(budgets_incomes.BudgetIncomeTypeId);

                        if (budgets_incomes.BudgetsIncomeInstitutions.HasValue)
                            institution = InstitutionsRep.GetEntity(budgets_incomes.BudgetsIncomeInstitutions.Value);
                        else
                            institution = null;

                        if (incomeFromDB != null)
                        {
                            if (budget != null && incomeType != null && (!budgets_incomes.BudgetsIncomeInstitutions.HasValue || institution != null))
                            {
                                if (incomeFromDB.CompanyId == CurrentUser.CompanyId && budget.CompanyId == CurrentUser.CompanyId && incomeType.CompanyId == CurrentUser.CompanyId && (!budgets_incomes.BudgetsIncomeInstitutions.HasValue || institution.CompanyId == CurrentUser.CompanyId))
                                {
                                    if (incomeFromDB.Budget.IsViewOnly)
                                        return Error(Errors.BUDGETS_YEAR_PASSED);

                                    if (budgets_incomes.Amount < incomeFromDB.Amount)
                                    {
                                        decimal? allocatedIncome;
                                        using (ExpensesToIncomeRepository allocationsRep = new ExpensesToIncomeRepository())
                                        using (OrdersRepository orderRep = new OrdersRepository())
                                        {
                                            allocatedIncome = allocationsRep.GetList()
                                                .Where(x => x.IncomeId == incomeFromDB.Id)
                                                .Sum(allocation => (decimal?)allocation.Amount);
                                        }

                                        if (allocatedIncome.HasValue && allocatedIncome > budgets_incomes.Amount)
                                            return Error(Errors.INCOME_ALLOCATIONS_EXEEDS_AMOUNT);
                                    }

                                    incomeFromDB.BudgetId = budgets_incomes.BudgetId;
                                    incomeFromDB.BudgetIncomeTypeId = budgets_incomes.BudgetIncomeTypeId;
                                    incomeFromDB.BudgetsIncomeInstitutions = budgets_incomes.BudgetsIncomeInstitutions;
                                    incomeFromDB.CustomName = budgets_incomes.CustomName;
                                    incomeFromDB.Amount = budgets_incomes.Amount;

                                    Budgets_Incomes update = incomesRep.Update(incomeFromDB);

                                    if (update != null)
                                        return RedirectToAction("Index");
                                    else
                                        return Error(Errors.INCOME_CREATE_ERROR);
                                }
                                else
                                {
                                    return Error(Errors.NO_PERMISSION);
                                }
                            }
                            else
                            {
                                return Error(Errors.DATABASE_ERROR);
                            }
                        }
                        else
                        {
                            return Error(Errors.INCOME_GET_ERROR);
                        }
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
        // GET: /Income/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Budgets_Incomes budgets_incomes = db.Budgets_Incomes.Single(b => b.Id == id);
            if (budgets_incomes == null)
            {
                return HttpNotFound();
            }
            return View(budgets_incomes);
        }

        //
        // POST: /Income/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budgets_Incomes budgets_incomes = db.Budgets_Incomes.Single(b => b.Id == id);
            db.Budgets_Incomes.DeleteObject(budgets_incomes);
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