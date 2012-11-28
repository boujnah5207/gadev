using System;
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
    public class ExpensesController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Expenses/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var budgets_expenses = db.Budgets_Expenses.Include("Department").Include("Projects_ParentProject").Include("Projects_SubProject").Where(x => x.CompanyId == CurrentUser.CompanyId);
            return View(budgets_expenses.ToList());
        }

        //
        // GET: /Expenses/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Expenses expense;
                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                {
                    expense = expensesRep.GetEntity(id, "Budget", "Department", "Projects_ParentProject", "Projects_SubProject");
                }

                if (expense != null)
                {
                    if (expense.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(expense);
                    }
                    else
                    {
                        return Error(Loc.Dic.error_no_permission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.error_expenses_get_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /Expenses/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.SystemManager))
            {
                using (BudgetsRepository budgetRep = new BudgetsRepository())
                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                using (DepartmentsRepository departmentsRep = new DepartmentsRepository())
                using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                {
                    List<SelectListItemDB> budgetsList = budgetRep.GetList()
                        .Where(budget => budget.CompanyId == CurrentUser.CompanyId && budget.Year >= (DateTime.Now.Year - 1))
                        .Select(a => new { Id = a.Id, Name = a.Year })
                        .AsEnumerable()
                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() })
                        .ToList();

                    List<SelectListItemDB> departmentsList = departmentsRep.GetList()
                        .Where(department => department.CompanyId == CurrentUser.CompanyId)
                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name })
                        .ToList();

                    List<SelectListItemDB> projectsList = projectsRep.GetList()
                        .Where(project => project.CompanyId == CurrentUser.CompanyId && project.IsActive)
                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name })
                        .ToList();

                    List<SelectListItemDB> subProjectsList = subProjectsRep.GetList()
                       .Where(subProject => subProject.CompanyId == CurrentUser.CompanyId && subProject.IsActive)
                       .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name })
                       .ToList();

                    ViewBag.BudgetId = new SelectList(budgetsList, "Id", "Name");
                    ViewBag.DepartmentId = new SelectList(departmentsList, "Id", "Name");
                    ViewBag.ParentProjectId = new SelectList(projectsList, "Id", "Name");
                    ViewBag.SubProjectId = new SelectList(subProjectsList, "Id", "Name");
                }

                return View();
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /Expenses/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_Expenses budgets_expenses)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    Budget budget;
                    Department department;
                    Projects_ParentProject project;
                    Projects_SubProject subProject;

                    using (BudgetsRepository budgetRep = new BudgetsRepository())
                    using (DepartmentsRepository departmentsRep = new DepartmentsRepository())
                    using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                    using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                    {
                        budget = budgetRep.GetEntity(budgets_expenses.BudgetId);
                        department = departmentsRep.GetEntity(budgets_expenses.DepartmentId);
                        project = projectsRep.GetEntity(budgets_expenses.ParentProjectId);
                        subProject = subProjectsRep.GetEntity(budgets_expenses.SubProjectId);
                    }

                    if (budget != null && department != null && project != null && subProject != null)
                    {
                        if (budget.CompanyId == CurrentUser.CompanyId && department.CompanyId == CurrentUser.CompanyId && project.CompanyId == CurrentUser.CompanyId && subProject.CompanyId == CurrentUser.CompanyId)
                        {
                            if (project.IsActive && subProject.IsActive)
                            {
                                bool wasCreated;
                                budgets_expenses.CompanyId = CurrentUser.CompanyId;

                                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                                {
                                    wasCreated = expensesRep.Create(budgets_expenses);
                                }

                                if (wasCreated)
                                    return RedirectToAction("Index");
                                else
                                    return Error(Loc.Dic.error_expenses_create_error);
                            }
                            else
                            {
                                return Error(Loc.Dic.error_invalid_form);
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
        // GET: /Expenses/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Expenses expense;

                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                using (BudgetsRepository budgetRep = new BudgetsRepository())
                using (DepartmentsRepository departmentsRep = new DepartmentsRepository())
                using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                {
                    expense = expensesRep.GetEntity(id);

                    if (expense != null)
                    {
                        if (expense.CompanyId == CurrentUser.CompanyId)
                        {
                            List<SelectListItemDB> budgetsList;
                            List<SelectListItemDB> departmentsList;
                            List<SelectListItemDB> projectsList;
                            List<SelectListItemDB> subProjectsList;

                            try
                            {
                                budgetsList = budgetRep.GetList()
                                    .Where(budget => budget.CompanyId == CurrentUser.CompanyId && budget.Year >= (DateTime.Now.Year - 1))
                                    .Select(a => new { Id = a.Id, Name = a.Year })
                                    .AsEnumerable()
                                    .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() })
                                    .ToList();

                                departmentsList = departmentsRep.GetList()
                                    .Where(type => type.CompanyId == CurrentUser.CompanyId)
                                    .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name })
                                    .ToList();

                                projectsList = projectsRep.GetList()
                                    .Where(type => type.CompanyId == CurrentUser.CompanyId)
                                    .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name })
                                    .ToList();

                                subProjectsList = subProjectsRep.GetList()
                                   .Where(type => type.CompanyId == CurrentUser.CompanyId)
                                   .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name })
                                   .ToList();
                            }
                            catch
                            {
                                return Error(Loc.Dic.error_database_error);
                            }

                            ViewBag.BudgetId = new SelectList(budgetsList, "Id", "Name", expense.BudgetId);
                            ViewBag.DepartmentId = new SelectList(departmentsList, "Id", "Name", expense.DepartmentId);
                            ViewBag.ParentProjectId = new SelectList(projectsList, "Id", "Name", expense.ParentProjectId);
                            ViewBag.SubProjectId = new SelectList(subProjectsList, "Id", "Name", expense.SubProjectId);

                            return View(expense);
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
        // POST: /Expenses/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_Expenses budgets_expenses)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    Budgets_Expenses expenseFromDB;
                    Budget budget;
                    Department department;
                    Projects_ParentProject project;
                    Projects_SubProject subProject;

                    using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                    using (BudgetsRepository budgetRep = new BudgetsRepository())
                    using (DepartmentsRepository departmentsRep = new DepartmentsRepository())
                    using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                    using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                    {
                        expenseFromDB = expensesRep.GetEntity(budgets_expenses.Id);

                        budget = budgetRep.GetEntity(budgets_expenses.BudgetId);
                        department = departmentsRep.GetEntity(budgets_expenses.DepartmentId);
                        project = projectsRep.GetEntity(budgets_expenses.ParentProjectId);
                        subProject = subProjectsRep.GetEntity(budgets_expenses.SubProjectId);

                        if (expenseFromDB != null)
                        {
                            if (budget != null && department != null && project != null && subProject != null)
                            {
                                if (budget.CompanyId == CurrentUser.CompanyId && department.CompanyId == CurrentUser.CompanyId && project.CompanyId == CurrentUser.CompanyId && subProject.CompanyId == CurrentUser.CompanyId)
                                {
                                    if (project.IsActive && subProject.IsActive)
                                    {
                                        if (budgets_expenses.Amount < expenseFromDB.Amount)
                                        {
                                            decimal? allocatedToExpense;
                                            using (ExpensesToIncomeRepository allocationsRep = new ExpensesToIncomeRepository())
                                            {
                                                allocatedToExpense = allocationsRep.GetList()
                                                    .Where(x => x.ExpenseId == expenseFromDB.Id)
                                                    .Sum(allocation => (decimal?)allocation.CompanyId); //.Sum(allocation => (decimal?)allocation.Amount);
                                            }

                                            if (allocatedToExpense.HasValue && allocatedToExpense > budgets_expenses.Amount)
                                                return Error(Loc.Dic.error_expenses_allocations_exeeds_amount);
                                        }

                                        expenseFromDB.BudgetId = budgets_expenses.BudgetId;
                                        expenseFromDB.DepartmentId = budgets_expenses.DepartmentId;
                                        expenseFromDB.ParentProjectId = budgets_expenses.ParentProjectId;
                                        expenseFromDB.SubProjectId = budgets_expenses.SubProjectId;
                                        expenseFromDB.Amount = budgets_expenses.Amount;
                                        expenseFromDB.CustomName = budgets_expenses.CustomName;

                                        Budgets_Expenses update = expensesRep.Update(expenseFromDB);

                                        if (update != null)
                                            return RedirectToAction("Index");
                                        else
                                            return Error(Loc.Dic.error_expenses_create_error);
                                    }
                                    else
                                    {
                                        return Error(Loc.Dic.error_invalid_form);
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
                        else
                        {
                            return Error(Loc.Dic.error_expenses_get_error);
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
        // GET: /Expenses/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Expenses expense;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                {
                    expense = expensesRep.GetEntity(id, "Budget", "Department", "Projects_ParentProject", "Projects_SubProject");

                    if (expense != null)
                    {
                        if (expense.CompanyId == CurrentUser.CompanyId)
                        {
                            
                            if (
                                !ordersRep.GetList()
                                .Where(x => x.Budgets_Allocations.ExpenseId == expense.Id)
                                .Any(o => o.StatusId >= (int)StatusType.ApprovedPendingInvoice)
                                )
                            {
                                return View(expense);
                            }
                            else
                            {
                                return Error(Loc.Dic.error_expenses_delete_has_approved_orders);
                            }
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_expenses_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /Expenses/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Expenses expense;
                using (BudgetsExpensesRepository expensesRep = new BudgetsExpensesRepository())
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                using (AllocationRepository allocationsRep = new AllocationRepository())
                using (BudgetsPermissionsToAllocationRepository permissionAllocationsRep = new BudgetsPermissionsToAllocationRepository())
                {
                    expense = expensesRep.GetEntity(id, "Budget", "Budgets_Incomes_types", "Budgets_Incomes_Institutions");

                    if (expense != null)
                    {
                        if (expense.CompanyId == CurrentUser.CompanyId)
                        {
                            List<Budgets_Allocations> expenseAllocations;
                            List<Budgets_BasketsToAllocation> expensePermissions;
                            List<Order> expenseOrders = ordersRep.GetList().Where(x => x.Budgets_Allocations.ExpenseId == expense.Id).ToList();

                            if (!expenseOrders.Any(o => o.StatusId >= (int)StatusType.ApprovedPendingInvoice))
                            {
                                try
                                {
                                    expenseAllocations = allocationsRep.GetList().Where(x => x.ExpenseId == expense.Id).ToList();
                                    expensePermissions = permissionAllocationsRep.GetList().Where(x => x.Budgets_Allocations.ExpenseId == expense.Id).ToList();

                                    foreach (var item in expenseOrders)
                                    {
                                        ordersRep.Delete(item.Id);
                                    }

                                    foreach (var item in expensePermissions)
                                    {
                                        permissionAllocationsRep.Delete(item.Id);
                                    }

                                    foreach (var item in expenseAllocations)
                                    {
                                        allocationsRep.Delete(item.Id);
                                    }

                                    expensesRep.Delete(expense.Id);
                                }
                                catch
                                {
                                    return Error(Loc.Dic.error_database_error);
                                }

                                return RedirectToAction("Index");
                            }
                            else
                            {
                                return Error(Loc.Dic.error_expenses_delete_has_approved_orders);
                            }
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_expenses_get_error);
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