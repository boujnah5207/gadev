﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
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
                List<Budgets_Allocations> model;
                List<SelectListItemDB> budgetsList;

                using (BudgetsRepository budgetRep = new BudgetsRepository())
                using (AllocationRepository allocationsRep = new AllocationRepository())
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
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [OpenIdAuthorize]
        public ActionResult AllocationMontheList(int budgetId = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);
            List<Budgets_Allocations> allocationsList = new List<Budgets_Allocations>();
            using (BudgetsRepository budgetsRepository = new BudgetsRepository())
            using (AllocationRepository allocationsRep = new AllocationRepository())
            {
                allocationsList = allocationsRep.GetList("Budgets_AllocationToMonth").Where(x => x.BudgetId == budgetId).OrderBy(x => x.ExternalId).ToList();
                ViewBag.Year = budgetsRepository.GetEntity(budgetId).Year;
            }


           return View(allocationsList);
        }
        //
        // GET: /BudgetAllocations/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Allocations allocation;

                using (AllocationRepository allocationsRep = new AllocationRepository())
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
                        return Error(Loc.Dic.error_no_permission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.error_income_get_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /BudgetAllocations/Create

        [OpenIdAuthorize]
        public ActionResult Create(int id = 0)
        {
            return Error(Loc.Dic.error_no_permission);
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
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_database_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /BudgetAllocations/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_Allocations Budgets_Allocations, int id = 0)
        {
            return Error(Loc.Dic.error_no_permission);
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

                                income = incomesRep.GetEntity(Budgets_Allocations.IncomeId.Value);
                                expense = expensesRep.GetEntity(Budgets_Allocations.ExpenseId.Value);

                                if (income != null && expense != null)
                                {
                                    if (income.BudgetId == budget.Id && expense.BudgetId == budget.Id)
                                    {
                                        decimal? allocatedToExpense;
                                        decimal? allocatedToIncome;

                                        allocatedToIncome = allocationsRep.GetList()
                                             .Where(x => x.IncomeId == income.Id)
                                             .Sum(allocation => (decimal?)allocation.CompanyId);//.Sum(allocation => (decimal?)allocation.Amount);

                                        if ((allocatedToIncome ?? 0) + Budgets_Allocations.CompanyId > income.Amount)//if ((allocatedToIncome ?? 0) + Budgets_Allocations.Amount > income.Amount)
                                            return Error(Loc.Dic.error_income_full_allocation);

                                        allocatedToExpense = allocationsRep.GetList()
                                            .Where(x => x.ExpenseId == expense.Id)
                                            .Sum(allocation => (decimal?)allocation.CompanyId);//.Sum(allocation => (decimal?)allocation.Amount);

                                        if ((allocatedToExpense ?? 0) + Budgets_Allocations.CompanyId > expense.Amount)//if ((allocatedToExpense ?? 0) + Budgets_Allocations.Amount > expense.Amount)
                                            return Error(Loc.Dic.error_expenses_full_allocation);

                                        Budgets_Allocations.CompanyId = CurrentUser.CompanyId;
                                        Budgets_Allocations.BudgetId = budget.Id;
                                        Budgets_Allocations.CompanyId = CurrentUser.CompanyId;

                                        if (allocationsRep.Create(Budgets_Allocations))
                                            return RedirectToAction("Index");
                                        else
                                            return Error(Loc.Dic.error_database_error);
                                    }
                                    else
                                    {
                                        return Error(Loc.Dic.error_invalid_form);
                                    }
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
                        else
                        {
                            return Error(Loc.Dic.error_database_error);
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
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /BudgetAllocations/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            return Error(Loc.Dic.error_no_permission);
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Allocations allocation;
                List<SelectListItemDB> incomesList;
                List<SelectListItemDB> expensesList;

                using (AllocationRepository allocationRep = new AllocationRepository())
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
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_database_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /BudgetAllocations/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_Allocations Budgets_Allocations)
        {
            return Error(Loc.Dic.error_no_permission);
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    Budgets_Allocations allocation;
                    Budgets_Incomes income;
                    Budgets_Expenses expense;

                    using (BudgetsRepository budgetsRep = new BudgetsRepository())
                    using (BudgetsIncomesRepository incomesRep = new BudgetsIncomesRepository())
                    using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                    using (ExpensesToIncomeRepository allocationsRep = new ExpensesToIncomeRepository())
                    using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                    {
                        allocation = allocationsRep.GetEntity(Budgets_Allocations.Id);

                        if (allocation != null)
                        {
                            if (allocation.CompanyId == CurrentUser.CompanyId)
                            {
                                income = incomesRep.GetEntity(Budgets_Allocations.IncomeId.Value);
                                expense = expensesRep.GetEntity(Budgets_Allocations.ExpenseId.Value);

                                if (income != null && expense != null)
                                {
                                    if (income.BudgetId == allocation.BudgetId && expense.BudgetId == allocation.BudgetId)
                                    {
                                        decimal? totalUsed;
                                        decimal? allocatedToExpense;
                                        decimal? allocatedToIncome;

                                        totalUsed = 0; //totalUsed = ordersRep.GetList()
                                        //    .Where(order => order.BudgetAllocationId == Budgets_Allocations.Id && order.StatusId >= (int)StatusType.ApprovedPendingInvoice)
                                        //    .Sum(x => (decimal?)x.Price);

                                        if ((totalUsed ?? 0) > Budgets_Allocations.CompanyId)//if ((totalUsed ?? 0) > Budgets_Allocations.Amount)
                                            return Error(Loc.Dic.error_allocations_amount_is_used);

                                        allocatedToIncome = allocationsRep.GetList()
                                             .Where(x => x.IncomeId == income.Id && x.Id != Budgets_Allocations.Id)
                                             .Sum(alloc => (decimal?)alloc.CompanyId);//.Sum(alloc => (decimal?)alloc.Amount);

                                        if ((allocatedToIncome ?? 0) + Budgets_Allocations.CompanyId > income.Amount)//if ((allocatedToIncome ?? 0) + Budgets_Allocations.Amount > income.Amount)
                                            return Error(Loc.Dic.error_income_full_allocation);

                                        allocatedToExpense = allocationsRep.GetList()
                                            .Where(x => x.ExpenseId == expense.Id && x.Id != Budgets_Allocations.Id)
                                            .Sum(alloc => (decimal?)alloc.CompanyId);//.Sum(alloc => (decimal?)alloc.Amount);

                                        if ((allocatedToExpense ?? 0) + Budgets_Allocations.CompanyId > expense.Amount)//if ((allocatedToExpense ?? 0) + Budgets_Allocations.Amount > expense.Amount)
                                            return Error(Loc.Dic.error_expenses_full_allocation);

                                        allocation.IncomeId = Budgets_Allocations.IncomeId;
                                        allocation.ExpenseId = Budgets_Allocations.ExpenseId;
                                        allocation.CompanyId = Budgets_Allocations.CompanyId;//allocation.Amount = Budgets_Allocations.Amount;

                                        Budgets_Allocations update = allocationsRep.Update(allocation);

                                        if (update != null)
                                            return RedirectToAction("Index");
                                        else
                                            return Error(Loc.Dic.error_allocations_get_error);
                                    }
                                    else
                                    {
                                        return Error(Loc.Dic.error_invalid_form);

                                    }
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
                        else
                        {
                            return Error(Loc.Dic.error_allocations_get_error);
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
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /BudgetAllocations/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            return Error(Loc.Dic.error_no_permission);
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Allocations allocation;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                using (AllocationRepository allocationsRep = new AllocationRepository())
                using (BudgetsPermissionsToAllocationRepository allocationsPermissionsRep = new BudgetsPermissionsToAllocationRepository())
                {
                    allocation = allocationsRep.GetEntity(id, "Budgets_Incomes", "Budgets_Expenses");

                    if (allocation != null)
                    {
                        if (allocation.CompanyId == CurrentUser.CompanyId)
                        {
                            if (false) //if (allocation.Orders.All(x => x.StatusId < (int)StatusType.ApprovedPendingInvoice))
                            {
                                return View(allocation);
                            }
                            else
                            {
                                return Error(Loc.Dic.error_allocations_has_approved_orders);
                            }
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_income_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /BudgetAllocations/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            return Error(Loc.Dic.error_no_permission);
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Allocations allocation;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                using (AllocationRepository allocationsRep = new AllocationRepository())
                using (BudgetsPermissionsToAllocationRepository allocationsPermissionsRep = new BudgetsPermissionsToAllocationRepository())
                {
                    allocation = allocationsRep.GetEntity(id, "Budgets_Incomes", "Budgets_Expenses");

                    if (allocation != null)
                    {
                        if (allocation.CompanyId == CurrentUser.CompanyId)
                        {
                            if (false) //if (allocation.Orders.All(x => x.StatusId < (int)StatusType.ApprovedPendingInvoice))
                            {
                                bool noErrors = true;
                                List<Order> allocationOrders = new List<Order>();//List<Order> allocationOrders = allocation.Orders.ToList();

                                foreach (var item in allocationOrders)
                                {
                                    item.StatusId = (int)StatusType.Declined;
                                    item.LastStatusChangeDate = DateTime.Now;

                                    item.OrderApproverNotes = YOUR_ALLOCATION_WAS_REVOKED;
                                    item.NextOrderApproverId = null;

                                    if (ordersRep.Update(item) == null)
                                        noErrors = false;
                                }

                                List<int> allocationPermission = allocation.Budgets_BasketsToAllocation.Select(x => x.Id).ToList();
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
                                    return Error(Loc.Dic.error_allocations_delete_error);
                            }
                            else
                            {
                                return Error(Loc.Dic.error_allocations_has_approved_orders);
                            }
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_income_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}