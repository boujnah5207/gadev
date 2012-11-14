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
        public const string YOUR_ALLOCATION_WAS_REVOKED = "מערכת: הקצאה זאת בוטלה.";

        private Entities db = new Entities();

        //
        // GET: /BudgetAllocations/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            if (Authorized(RoleType.SystemManager))
            {
                List<Budgets_ExpensesToIncomes> model;
                List<SelectListItemDB> budgetsList;

                using (BudgetsRepository budgetRep = new BudgetsRepository())
                using (BudgetsExpensesToIncomesRepository allocationsRep = new BudgetsExpensesToIncomesRepository())
                {
                    model = allocationsRep.GetList("Budgets_Expenses", "Budgets_Incomes").Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();

                    budgetsList = budgetRep.GetList()
                        .Where(budget => budget.CompanyId == CurrentUser.CompanyId && budget.Year >= (DateTime.Now.Year - 1))
                        .Select(a => new { Id = a.Id, Name = a.Year })
                        .AsEnumerable()
                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() })
                        .ToList();
                }

                ViewBag.BudgetId = new SelectList(budgetsList, "Id", "Name");
                return View(model);
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
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
                if (ModelState.IsValid)
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

                                income = incomesRep.GetEntity(budgets_expensestoincomes.IncomeId.Value);
                                expense = expensesRep.GetEntity(budgets_expensestoincomes.ExpenseId.Value);

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
                                        budgets_expensestoincomes.BudgetId = budget.Id;
                                        budgets_expensestoincomes.CompanyId = CurrentUser.CompanyId;

                                        if (allocationsRep.Create(budgets_expensestoincomes))
                                            return RedirectToAction("Index");
                                        else
                                            return Error(Errors.DATABASE_ERROR);
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
                    return Error(ModelState);
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
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_ExpensesToIncomes allocation;
                List<SelectListItemDB> incomesList;
                List<SelectListItemDB> expensesList;

                using (BudgetsExpensesToIncomesRepository allocationRep = new BudgetsExpensesToIncomesRepository())
                using (BudgetsRepository budgetsRep = new BudgetsRepository())
                using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                {
                    allocation = allocationRep.GetEntity(id);

                    if (allocation != null)
                    {
                        if (allocation.CompanyId == CurrentUser.CompanyId)
                        {
                            
                                incomesList = incomesRep.GetList()
                                    .Where(income => income.CompanyId == CurrentUser.CompanyId && income.BudgetId == allocation.BudgetId)
                                    .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.CustomName })
                                    .ToList();

                                expensesList = expensesRep.GetList()
                                    .Where(expense => expense.CompanyId == CurrentUser.CompanyId && expense.BudgetId == allocation.BudgetId)
                                    .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.CustomName })
                                    .ToList();

                                ViewBag.IncomeId = new SelectList(incomesList, "Id", "Name", allocation.IncomeId);
                                ViewBag.ExpenseId = new SelectList(expensesList, "Id", "Name", allocation.ExpenseId);

                                return View(allocation);
                            
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
        // POST: /BudgetAllocations/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_ExpensesToIncomes budgets_expensestoincomes)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    Budgets_ExpensesToIncomes allocation;
                    Budgets_Incomes income;
                    Budgets_Expenses expense;

                    using (BudgetsRepository budgetsRep = new BudgetsRepository())
                    using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                    using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                    using (ExpensesToIncomeRepository allocationsRep = new ExpensesToIncomeRepository())
                    using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                    {
                        allocation = allocationsRep.GetEntity(budgets_expensestoincomes.Id);

                        if (allocation != null)
                        {
                            if (allocation.CompanyId == CurrentUser.CompanyId)
                            {
                                    income = incomesRep.GetEntity(budgets_expensestoincomes.IncomeId.Value);
                                    expense = expensesRep.GetEntity(budgets_expensestoincomes.ExpenseId.Value);

                                    if (income != null && expense != null)
                                    {
                                        if (income.BudgetId == allocation.BudgetId && expense.BudgetId == allocation.BudgetId)
                                        {
                                            decimal? totalUsed;
                                            decimal? allocatedToExpense;
                                            decimal? allocatedToIncome;

                                            totalUsed = ordersRep.GetList()
                                                .Where(order => order.BudgetAllocationId == budgets_expensestoincomes.Id && order.StatusId >= (int)StatusType.ApprovedPendingInvoice)
                                                .Sum(x => (decimal?)x.Price);

                                            if ((totalUsed ?? 0) > budgets_expensestoincomes.Amount)
                                                return Error(Errors.ALLOCATIONS_AMOUNT_IS_USED);

                                            allocatedToIncome = allocationsRep.GetList()
                                                 .Where(x => x.IncomeId == income.Id && x.Id != budgets_expensestoincomes.Id)
                                                 .Sum(alloc => (decimal?)alloc.Amount);

                                            if ((allocatedToIncome ?? 0) + budgets_expensestoincomes.Amount > income.Amount)
                                                return Error(Errors.INCOME_FULL_ALLOCATION);

                                            allocatedToExpense = allocationsRep.GetList()
                                                .Where(x => x.ExpenseId == expense.Id && x.Id != budgets_expensestoincomes.Id)
                                                .Sum(alloc => (decimal?)alloc.Amount);

                                            if ((allocatedToExpense ?? 0) + budgets_expensestoincomes.Amount > expense.Amount)
                                                return Error(Errors.EXPENSES_FULL_ALLOCATION);

                                            allocation.IncomeId = budgets_expensestoincomes.IncomeId;
                                            allocation.ExpenseId = budgets_expensestoincomes.ExpenseId;
                                            allocation.Amount = budgets_expensestoincomes.Amount;

                                            Budgets_ExpensesToIncomes update = allocationsRep.Update(allocation);

                                            if (update != null)
                                                return RedirectToAction("Index");
                                            else
                                                return Error(Errors.ALLOCATIONS_GET_ERROR);
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
                            }
                            else
                            {
                                return Error(Errors.NO_PERMISSION);
                            }
                        }
                        else
                        {
                            return Error(Errors.ALLOCATIONS_GET_ERROR);
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
        // GET: /BudgetAllocations/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_ExpensesToIncomes allocation;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                using (BudgetsExpensesToIncomesRepository allocationsRep = new BudgetsExpensesToIncomesRepository())
                using (BudgetsPermissionsToAllocationRepository allocationsPermissionsRep = new BudgetsPermissionsToAllocationRepository())
                {
                    allocation = allocationsRep.GetEntity(id, "Budgets_Incomes", "Budgets_Expenses");

                    if (allocation != null)
                    {
                        if (allocation.CompanyId == CurrentUser.CompanyId)
                        {
                            if (allocation.Orders.All(x => x.StatusId < (int)StatusType.ApprovedPendingInvoice))
                            {
                                return View(allocation);
                            }
                            else
                            {
                                return Error(Errors.ALLOCATIONS_HAS_APPROVED_ORDERS);
                            }
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
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /BudgetAllocations/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_ExpensesToIncomes allocation;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                using (BudgetsExpensesToIncomesRepository allocationsRep = new BudgetsExpensesToIncomesRepository())
                using (BudgetsPermissionsToAllocationRepository allocationsPermissionsRep = new BudgetsPermissionsToAllocationRepository())
                {
                    allocation = allocationsRep.GetEntity(id, "Budgets_Incomes", "Budgets_Expenses");

                    if (allocation != null)
                    {
                        if (allocation.CompanyId == CurrentUser.CompanyId)
                        {
                            if (allocation.Orders.All(x => x.StatusId < (int)StatusType.ApprovedPendingInvoice))
                            {
                                bool noErrors = true;
                                List<Order> allocationOrders = allocation.Orders.ToList();

                                foreach (var item in allocationOrders)
                                {
                                    item.BudgetAllocationId = null;
                                    item.StatusId = (int)StatusType.Declined;
                                    item.LastStatusChangeDate = DateTime.Now;

                                    item.OrderApproverNotes = YOUR_ALLOCATION_WAS_REVOKED;
                                    item.NextOrderApproverId = null;

                                    if (ordersRep.Update(item) == null)
                                        noErrors = false;
                                }

                                List<int> allocationPermission = allocation.Budgets_PermissionsToAllocation.Select(x => x.Id).ToList();
                                foreach (var itemId in allocationPermission)
                                {
                                    if (!allocationsPermissionsRep.Delete(itemId))
                                        noErrors = false;
                                }

                                if (!allocationsRep.Delete(allocation.Id))
                                    noErrors = false;

                                if (noErrors)
                                    return RedirectToAction("Index");
                                else
                                    return Error(Errors.ALLOCATIONS_DELETE_ERROR);
                            }
                            else
                            {
                                return Error(Errors.ALLOCATIONS_HAS_APPROVED_ORDERS);
                            }
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