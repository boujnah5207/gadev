using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using GAppsDev.Models;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class ApprovalRoutesController : BaseController
    {
        const int ITEMS_PER_PAGE = 10;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string DEFAULT_SORT = "name";
        const string DEFAULT_DESC_ORDER = "DESC";

        private Entities db = new Entities();

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.UsersManager)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Users_ApprovalRoutes> routes;
            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            {
                routes = routesRep.GetList();

                if (routes == null) return Error(Loc.Dic.error_orders_get_error);

                routes = Pagination(routes, page, sortby, order);

                return View(routes.ToList());
            }
        }

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            Users_ApprovalRoutes route;
            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            {
                route = routesRep.GetEntity(id, "Users_ApprovalStep.User");
            }

            if (route == null) return Error(Loc.Dic.error_database_error);

            return View(route);
        }

        [OpenIdAuthorize]
        [ChildActionOnly]
        public ActionResult PartialDetails(Users_ApprovalRoutes route)
        {
            return PartialView(route);
        }

        [OpenIdAuthorize]
        public ActionResult Create(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            List<User> usersSelectList;
            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            {
                usersSelectList =
                    usersRep.GetList()
                    .Where(user => ((RoleType)user.Roles & RoleType.OrdersApprover) == RoleType.OrdersApprover)
                    .ToList();
            }

            ViewBag.UsersSelectList = new SelectList(usersSelectList, "Id", "FullName");
            return View();
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(ApprovalRouteModel model)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            Users_ApprovalRoutes newApprovalRoute = new Users_ApprovalRoutes();
            newApprovalRoute.Name = model.Name;

            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            {
                var usersIds = model.Steps.Select(x => x.UserId).Distinct();
                List<User> routeApprovers = usersRep.GetList().Where(x => usersIds.Contains(x.Id)).ToList();
                if(usersIds.Count() != routeApprovers.Count) return Error(Loc.Dic.error_invalid_form);

                foreach (var approver in routeApprovers)
                {
                    if (!Roles.HasRole(approver.Roles, RoleType.OrdersApprover)) 
                        return Error(Loc.Dic.error_invalid_form);
                }

                foreach (var step in model.Steps)
                {
                    newApprovalRoute.Users_ApprovalStep.Add(
                        new Users_ApprovalStep()
                        {
                            UserId = step.UserId,
                            StepNumber = step.StepNumber
                        }
                    );
                }

                if (!routesRep.Create(newApprovalRoute)) return Error(Loc.Dic.error_database_error);
            }

            return RedirectToAction("Index");
        }

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            ApprovalRouteModel model = new ApprovalRouteModel();
            List<User> usersSelectList;
            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            {
                model.ApprovalRoute = routesRep.GetEntity(id, "Users_ApprovalStep.User");
                if (model.ApprovalRoute == null) return Error(Loc.Dic.error_database_error);

                usersSelectList =
                    usersRep.GetList()
                    .Where(user => ((RoleType)user.Roles & RoleType.OrdersApprover) == RoleType.OrdersApprover)
                    .ToList();
            }

            ViewBag.UsersSelectList = new SelectList(usersSelectList, "Id", "FullName");
            return View(model);
        }

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult Edit(ApprovalRouteModel model)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);
            if (!ModelState.IsValid) return Error(Loc.Dic.error_invalid_form);

            Users_ApprovalRoutes newApprovalRoute;

            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            using (ApprovalStepsRepository stepsRep = new ApprovalStepsRepository())
            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            {
                newApprovalRoute = routesRep.GetEntity(model.ApprovalRoute.Id, "Users_ApprovalStep");
                if (newApprovalRoute == null) return Error(Loc.Dic.error_database_error);

                var usersIds = model.Steps.Select(x => x.UserId).Distinct();
                List<User> routeApprovers = usersRep.GetList().Where(x => usersIds.Contains(x.Id)).ToList();
                if (usersIds.Count() != routeApprovers.Count) return Error(Loc.Dic.error_invalid_form);

                foreach (var approver in routeApprovers)
                {
                    if (!Roles.HasRole(approver.Roles, RoleType.OrdersApprover))
                        return Error(Loc.Dic.error_invalid_form);
                }

                foreach (var step in newApprovalRoute.Users_ApprovalStep)
                {
                    stepsRep.Delete(step.Id);
                }

                foreach (var step in model.Steps)
                {
                    newApprovalRoute.Users_ApprovalStep.Add(
                        new Users_ApprovalStep()
                        {
                            UserId = step.UserId,
                            StepNumber = step.StepNumber
                        }
                    );
                }

                if (routesRep.Update(newApprovalRoute) == null) return Error(Loc.Dic.error_database_error);
            }

            return RedirectToAction("Index");
        }

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            Users_ApprovalRoutes route;
            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            {
                route = routesRep.GetEntity(id, "Users_ApprovalStep.User");
            }

            if (route == null) return Error(Loc.Dic.error_database_error);

            return View(route);
        }

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            Users_ApprovalRoutes route;
            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            {
                route = routesRep.GetEntity(id);
                if (route == null) return Error(Loc.Dic.error_database_error);

                if (!routesRep.Delete(id)) return Error(Loc.Dic.error_database_error);
            }

            return RedirectToAction("Index");
        }

        [ChildActionOnly]
        public ActionResult List(IEnumerable<Users_ApprovalRoutes> routes, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = isOrdered;
            ViewBag.IsPaged = isPaged;
            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = currPage;
            ViewBag.NumberOfPages = numberOfPages;

            ViewBag.IsCheckBoxed = isCheckBoxed;

            return PartialView(routes);
        }

        [OpenIdAuthorize]
        private IEnumerable<Users_ApprovalRoutes> Pagination(IEnumerable<Users_ApprovalRoutes> routes, int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            int numberOfItems = routes.Count();
            int numberOfPages = numberOfItems / ITEMS_PER_PAGE;
            if (numberOfItems % ITEMS_PER_PAGE != 0)
                numberOfPages++;

            if (page <= 0)
                page = FIRST_PAGE;
            if (page > numberOfPages)
                page = numberOfPages;

            if (sortby != NO_SORT_BY)
            {
                Func<Func<Users_ApprovalRoutes, dynamic>, IEnumerable<Users_ApprovalRoutes>> orderFunction;

                if (order == DEFAULT_DESC_ORDER)
                    orderFunction = x => routes.OrderByDescending(x);
                else
                    orderFunction = x => routes.OrderBy(x);

                switch (sortby)
                {
                    case "name":
                    default:
                        routes = orderFunction(x => x.Name);
                        break;
                }
            }

            routes = routes
                .Skip((page - 1) * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = page;
            ViewBag.NumberOfPages = numberOfPages;

            return routes;
        }

        [ChildActionOnly]
        public ActionResult StepsList(List<Users_ApprovalStep> steps, string baseUrl)
        {
            steps = steps.OrderBy( x => x.StepNumber).ToList();

            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = false;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = NO_SORT_BY;
            ViewBag.Order = DEFAULT_DESC_ORDER;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 0;

            ViewBag.IsCheckBoxed = false;

            return PartialView(steps);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}