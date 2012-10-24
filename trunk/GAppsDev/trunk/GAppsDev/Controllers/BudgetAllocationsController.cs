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
    public class BudgetAllocationsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /BudgetAllocations/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Include("Budgets_Expenses").Include("Budgets_Incomes");
            return View(budgets_expensestoincomes.ToList());
        }

        //
        // GET: /BudgetAllocations/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_ExpensesToIncomes allocation;
                using (BudgetsExpensesToIncomesRepository allocationsRep = new BudgetsExpensesToIncomesRepository())
                {
                    allocation = allocationsRep.GetEntity(id, "Budgets_Incomes", "Budgets_Expenses");
                }

                if (allocation != null)
                {
                    if (allocation.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(allocation);
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
        // GET: /BudgetAllocations/Create

        [OpenIdAuthorize]
        public ActionResult Create(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budget budget;
                List<SelectListItemDB> incomesList;
                List<SelectListItemDB> expensesList;

                using (BudgetsRepository budgetsRep = new BudgetsRepository())
                using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                {
                    budget = budgetsRep.GetEntity(id);

                    if (budget != null)
                    {
                        if (budget.CompanyId == CurrentUser.CompanyId)
                        {
                            if (!budget.IsViewOnly)
                            {
                                incomesList = incomesRep.GetList()
                                    .Where(income => income.CompanyId == CurrentUser.CompanyId && income.BudgetId == budget.Id)
                                    .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.CustomName })
                                    .ToList();

                                expensesList = expensesRep.GetList()
                                    .Where(expense => expense.CompanyId == CurrentUser.CompanyId && expense.BudgetId == budget.Id)
                                    .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.CustomName })
                                    .ToList();

                                ViewBag.BudgetId = id;
                                ViewBag.IncomeId = new SelectList(incomesList, "Id", "Name");
                                ViewBag.ExpenseId = new SelectList(expensesList, "Id", "Name");

                                return View();
                            }
                            else
                            {
                                return Error(Errors.BUDGETS_YEAR_PASSED);
                            }
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
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /BudgetAllocations/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_ExpensesToIncomes budgets_expensestoincomes, int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budget budget;
                Budgets_Incomes income;
                Budgets_Expenses expense;

                using (BudgetsRepository budgetsRep = new BudgetsRepository())
                using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                using (ExpensesToIncomeRepository allocationsRep = new ExpensesToIncomeRepository())
                {
                    budget = budgetsRep.GetEntity(id);

                    if (budget != null)
                    {
                        if (budget.CompanyId == CurrentUser.CompanyId)
                        {
                            if (!budget.IsViewOnly)
                            {
                                income = incomesRep.GetEntity(budgets_expensestoincomes.IncomeId);
                                expense = expensesRep.GetEntity(budgets_expensestoincomes.ExpenseId);

                                if (income != null && expense != null)
                                {
                                    if (income.BudgetId == budget.Id && expense.BudgetId == budget.Id)
                                    {
                                        decimal? allocatedToExpense;
                                        decimal? allocatedToIncome;

                                        allocatedToIncome = allocationsRep.GetList()
                                             .Where(x => x.IncomeId == income.Id)
                                             .Sum(allocation => (decimal?)allocation.Amount);

                                        if ((allocatedToIncome ?? 0) + budgets_expensestoincomes.Amount > income.Amount)
                                            return Error(Errors.INCOME_FULL_ALLOCATION);

                                        allocatedToExpense = allocationsRep.GetList()
                                            .Where(x => x.ExpenseId == expense.Id)
                                            .Sum(allocation => (decimal?)allocation.Amount);

                                        if ((allocatedToExpense ?? 0) + budgets_expensestoincomes.Amount > expense.Amount)
                                            return Error(Errors.EXPENSES_FULL_ALLOCATION);

                                        budgets_expensestoincomes.CompanyId = CurrentUser.CompanyId;
                                        allocationsRep.Create(budgets_expensestoincomes);
                                    }
                                    else
                                    {
                                        return Error(Errors.INVALID_FORM);
                                    }
                                }
                                else
                                {
                                    return Error(Errors.DATABASE_ERROR);
                                }

                                return RedirectToAction("Index");
                            }
                            else
                            {
                                return Error(Errors.BUDGETS_YEAR_PASSED);
                            }
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
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /BudgetAllocations/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            Budgets_ExpensesToIncomes budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Single(b => b.Id == id);
            if (budgets_expensestoincomes == null)
            {
                return HttpNotFound();
            }
            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "CustomName", budgets_expensestoincomes.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "CustomName", budgets_expensestoincomes.IncomeId);
            return View(budgets_expensestoincomes);
        }

        //
        // POST: /BudgetAllocations/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_ExpensesToIncomes budgets_expensestoincomes)
        {
            if (ModelState.IsValid)
            {
                db.Budgets_ExpensesToIncomes.Attach(budgets_expensestoincomes);
                db.ObjectStateManager.ChangeObjectState(budgets_expensestoincomes, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ExpenseId = new SelectList(db.Budgets_Expenses, "Id", "CustomName", budgets_expensestoincomes.ExpenseId);
            ViewBag.IncomeId = new SelectList(db.Budgets_Incomes, "Id", "CustomName", budgets_expensestoincomes.IncomeId);
            return View(budgets_expensestoincomes);
        }

        //
        // GET: /BudgetAllocations/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            Budgets_ExpensesToIncomes budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Single(b => b.Id == id);
            if (budgets_expensestoincomes == null)
            {
                return HttpNotFound();
            }
            return View(budgets_expensestoincomes);
        }

        //
        // POST: /BudgetAllocations/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Budgets_ExpensesToIncomes budgets_expensestoincomes = db.Budgets_ExpensesToIncomes.Single(b => b.Id == id);
            db.Budgets_ExpensesToIncomes.DeleteObject(budgets_expensestoincomes);
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