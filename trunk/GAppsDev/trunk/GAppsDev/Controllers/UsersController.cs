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
using GAppsDev.Models.Search;
using GAppsDev.Models.UserModels;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class UsersController : BaseController
    {
        const int ITEMS_PER_PAGE = 7;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string DEFAULT_ORDER = "DESC";

        private Entities db = new Entities();

        //
        // GET: /Users/

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = NO_SORT_BY, string order = DEFAULT_ORDER)
        {
            if (Authorized(RoleType.SystemManager))
            {
                AllUsersModel model = new AllUsersModel();
                IEnumerable<User> activeUsers;

                using (UsersRepository usersRep = new UsersRepository())
                using (PendingUsersRepository pendingUsersRep = new PendingUsersRepository())
                using (CompaniesRepository companiesRep = new CompaniesRepository())
                {
                    activeUsers = usersRep.GetList("Department").Where(x => x.CompanyId == CurrentUser.CompanyId && x.IsActive).ToList();
                    model.PendingUsers = pendingUsersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();
                    model.NonActiveUsers = usersRep.GetList("Department").Where(x => x.CompanyId == CurrentUser.CompanyId && !x.IsActive).ToList();

                    if (model.ActiveUsers != null && model.PendingUsers != null && model.NonActiveUsers != null)
                    {
                        try
                        {
                            model.UsersLimit = companiesRep.GetEntity(CurrentUser.CompanyId).UsersLimit;
                        }
                        catch
                        {
                            return Error(Errors.DATABASE_ERROR);
                        }

                        int numberOfItems = activeUsers.Count();
                        int numberOfPages = numberOfItems / ITEMS_PER_PAGE;
                        if (numberOfItems % ITEMS_PER_PAGE != 0)
                            numberOfPages++;

                        if (page <= 0)
                            page = FIRST_PAGE;
                        if (page > numberOfPages)
                            page = numberOfPages;

                        if (sortby != NO_SORT_BY)
                        {
                            Func<Func<User, dynamic>, IEnumerable<User>> orderFunction;

                            if (order == DEFAULT_ORDER)
                                orderFunction = x => activeUsers.OrderByDescending(x);
                            else
                                orderFunction = x => activeUsers.OrderBy(x);

                            switch (sortby)
                            {
                                case "username":
                                default:
                                    activeUsers = orderFunction(x => x.FirstName + " " + x.LastName);
                                    break;
                                case "roles":
                                    activeUsers = orderFunction(x => ((RoleType)x.Roles).ToString());
                                    break;
                                case "email":
                                    activeUsers = orderFunction(x => x.Email);
                                    break;
                                case "creation":
                                    activeUsers = orderFunction(x => x.CreationTime);
                                    break;
                                case "login":
                                    activeUsers = orderFunction(x => x.LastLogInTime);
                                    break;
                            }
                        }

                        activeUsers = activeUsers
                            .Skip((page - 1) * ITEMS_PER_PAGE)
                            .Take(ITEMS_PER_PAGE)
                            .ToList();

                        ViewBag.Sortby = sortby;
                        ViewBag.Order = order;
                        ViewBag.CurrPage = page;
                        ViewBag.NumberOfPages = numberOfPages;

                        model.ActiveUsers = activeUsers.ToList();

                        return View(model);
                    }
                    else
                    {
                        return Error(Errors.USERS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Users/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            User user = db.Users.Single(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // GET: /Users/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.SystemManager))
            {
                List<string> roleNames = Enum.GetNames(typeof(RoleType)).ToList();
                List<SelectListItemFromDB> usersList = new List<SelectListItemFromDB>() { new SelectListItemFromDB() { Id = -1, Name = "(ללא) מאשר סופי" } };
                SelectList departmentsList;

                using (DepartmentsRepository departmentsRep = new DepartmentsRepository())
                using (UsersRepository usersRep = new UsersRepository())
                {
                    usersList.AddRange(usersRep.GetList().Where(user => user.CompanyId == CurrentUser.CompanyId && ((RoleType)user.Roles & RoleType.OrdersApprover) == RoleType.OrdersApprover).Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.FirstName + " " + x.LastName }));
                    departmentsList = new SelectList(departmentsRep.GetList().ToList(), "Id", "Name");
                }

                roleNames.Remove(RoleType.None.ToString());
                roleNames.Remove(RoleType.SuperAdmin.ToString());

                ViewBag.RolesList = roleNames;
                ViewBag.UsersList = new SelectList(usersList, "Id", "Name");
                ViewBag.DepartmentsList = departmentsList;

                return View();
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Users/Create

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult Create(PendingUser user, string[] roleNames)
        {
            if (ModelState.IsValid)
            {
                if (Authorized(RoleType.SystemManager))
                {
                    int companyUserCount = 0;
                    int companyUserLimit = 0;

                    using (UsersRepository usersRep = new UsersRepository())
                    using (PendingUsersRepository pendingUsersRep = new PendingUsersRepository())
                    using (CompaniesRepository companiesRep = new CompaniesRepository())
                    {
                        try
                        {
                            companyUserCount =
                                usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId && x.IsActive).Count() +
                                pendingUsersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Count();
                            
                            companyUserLimit = companiesRep.GetEntity(CurrentUser.CompanyId).UsersLimit;
                        }
                        catch
                        {
                            return Error(Errors.DATABASE_ERROR);
                        }

                        bool userExists = usersRep.GetList().Any(x => x.CompanyId == CurrentUser.CompanyId && x.Email == user.Email);
                        bool pendingUserExists = pendingUsersRep.GetList().Any(x => x.CompanyId == CurrentUser.CompanyId && x.Email == user.Email);

                        if (userExists || pendingUserExists)
                            return Error(Errors.USERS_EXIST_ERROR);
                    }

                    if (companyUserCount >= companyUserLimit)
                        return Error(Errors.USERS_LIMIT_REACHED);

                    user.CompanyId = CurrentUser.CompanyId;
                    user.CreationDate = DateTime.Now;

                    RoleType combinedRoles = RoleType.None;
                    foreach (string roleName in roleNames)
                    {
                        RoleType role;
                        if (Enum.TryParse(roleName, out role) && role != RoleType.SuperAdmin)
                        {
                            combinedRoles = Roles.CombineRoles(combinedRoles, role);
                        }
                        else
                        {
                            return Error(Errors.INVALID_FORM);
                        }
                    }
                    user.Roles = (int)combinedRoles;

                    bool wasUserCreated;
                    using (PendingUsersRepository pendingUserRep = new PendingUsersRepository())
                    {
                        wasUserCreated = pendingUserRep.Create(user);
                    }

                    if (wasUserCreated)
                        return RedirectToAction("Index");
                    else
                        return Error(Errors.USERS_CREATE_ERROR);
                }
                else
                {
                    return Error(Errors.NO_PERMISSION);
                }
            }
            else
            {
                return Error(ModelState);
            }
        }

        //
        // GET: /Users/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if(Authorized(RoleType.SystemManager))
            {
                User user;
                using(UserRepository userRep = new UserRepository())
                {
                    user = userRep.GetEntity(id);
                }

                if(user != null)
                {
                    if (user.CompanyId != CurrentUser.CompanyId)
                        return Error(Errors.NO_PERMISSION);

                    List<string> roleNames = Enum.GetNames(typeof(RoleType)).ToList();
                    roleNames.Remove(RoleType.None.ToString());
                    roleNames.Remove(RoleType.SuperAdmin.ToString());
                    ViewBag.RolesList = roleNames;

                    ViewBag.ExistingRoles = 
                        Roles.GetAllRoles((RoleType)user.Roles)
                        .Select(x => x.ToString())
                        .ToList();

                    return View(user);
                }
                else
                {
                    return Error(Errors.USER_NOT_FOUND);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Users/Edit/5

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult Edit(User user, string[] roleNames)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    User userFromDatabase;
                    using (UsersRepository userRep = new UsersRepository())
                    {
                        userFromDatabase = userRep.GetEntity(user.Id);

                        if (userFromDatabase != null)
                        {
                            if (userFromDatabase.CompanyId != CurrentUser.CompanyId)
                                return Error(Errors.NO_PERMISSION);

                            RoleType combinedRoles = RoleType.None;
                            foreach (string roleName in roleNames)
                            {
                                RoleType role;
                                if (Enum.TryParse(roleName, out role) && role != RoleType.SuperAdmin)
                                {
                                    combinedRoles = Roles.CombineRoles(combinedRoles, role);
                                }
                                else
                                {
                                    return Error(Errors.INVALID_FORM);
                                }
                            }

                            user.Roles = (int)combinedRoles;
                            user.CompanyId = userFromDatabase.CompanyId;
                            user.IsActive = userFromDatabase.IsActive;
                            user.LastLogInTime = userFromDatabase.LastLogInTime;
                            user.CreationTime = userFromDatabase.CreationTime;

                            userRep.Update(user);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Errors.USER_NOT_FOUND);
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


        [OpenIdAuthorize]
        public ActionResult EditPending(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                PendingUser user;
                using (PendingUsersRepository pendingUserRep = new PendingUsersRepository())
                {
                    user = pendingUserRep.GetEntity(id);
                }

                if (user.CompanyId != CurrentUser.CompanyId)
                    return Error(Errors.NO_PERMISSION);

                if (user != null)
                {
                    List<string> roleNames = Enum.GetNames(typeof(RoleType)).ToList();
                    roleNames.Remove(RoleType.None.ToString());
                    roleNames.Remove(RoleType.SuperAdmin.ToString());
                    ViewBag.RolesList = roleNames;

                    ViewBag.ExistingRoles =
                        Roles.GetAllRoles((RoleType)user.Roles)
                        .Select(x => x.ToString())
                        .ToList();

                    return View(user);
                }
                else
                {
                    return Error(Errors.USER_NOT_FOUND);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Users/Edit/5

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult EditPending(PendingUser user, string[] roleNames)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    PendingUser userFromDatabase;
                    using (PendingUsersRepository pendingUserRep = new PendingUsersRepository())
                    {
                        userFromDatabase = pendingUserRep.GetEntity(user.Id);

                        if (userFromDatabase != null)
                        {
                            if (userFromDatabase.CompanyId != CurrentUser.CompanyId)
                                return Error(Errors.NO_PERMISSION);

                            RoleType combinedRoles = RoleType.None;
                            foreach (string roleName in roleNames)
                            {
                                RoleType role;
                                if (Enum.TryParse(roleName, out role) && role != RoleType.SuperAdmin)
                                {
                                    combinedRoles = Roles.CombineRoles(combinedRoles, role);
                                }
                                else
                                {
                                    return Error(Errors.INVALID_FORM);
                                }
                            }

                            user.Roles = (int)combinedRoles;
                            user.CompanyId = userFromDatabase.CompanyId;

                            pendingUserRep.Update(user);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Errors.USER_NOT_FOUND);
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
        // GET: /Users/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                User user;
                using (UserRepository userRep = new UserRepository())
                {
                    user = userRep.GetEntity(id);
                }

                if (user == null)
                {
                    return Error(Errors.USER_NOT_FOUND);
                }
                if (user.Id == CurrentUser.UserId)
                {
                    return Error(Errors.USER_CANNOT_DELETE_SELF);
                }
                if (user.CompanyId != CurrentUser.CompanyId || user.Roles == (int)RoleType.SuperAdmin)
                {
                    return Error(Errors.NO_PERMISSION);
                }

                return View(user);
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult DeletePending(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                PendingUser user;
                using (PendingUsersRepository userRep = new PendingUsersRepository())
                {
                    user = userRep.GetEntity(id);
                }

                if (user == null)
                {
                    return Error(Errors.USER_NOT_FOUND);
                }
                if (user.CompanyId != CurrentUser.CompanyId)
                {
                    return Error(Errors.NO_PERMISSION);
                }

                return View(user);
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [HttpPost, ActionName("DeletePending")]
        [OpenIdAuthorize]
        public ActionResult DeletePendingConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                PendingUser user;
                using (PendingUsersRepository pendingUserRep = new PendingUsersRepository())
                {
                    user = pendingUserRep.GetEntity(id);

                    if (user == null)
                    {
                        return Error(Errors.USER_NOT_FOUND);
                    }
                    if (user.CompanyId != CurrentUser.CompanyId)
                    {
                        return Error(Errors.NO_PERMISSION);
                    }

                    pendingUserRep.Delete(user.Id);
                }

                return RedirectToAction("Index");
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Users/Delete/5

        [HttpPost, ActionName("Delete")]
        [OpenIdAuthorize]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                User user;
                using (CookiesRepository cookieRep = new CookiesRepository())
                using (UserRepository userRep = new UserRepository())
                {
                    user = userRep.GetEntity(id);

                    if (user == null)
                    {
                        return Error(Errors.USER_NOT_FOUND);
                    }
                    if (user.Id == CurrentUser.UserId)
                    {
                        return Error(Errors.USER_CANNOT_DELETE_SELF);
                    }
                    if (user.CompanyId != CurrentUser.CompanyId || user.Roles == (int)RoleType.SuperAdmin)
                    {
                        return Error(Errors.NO_PERMISSION);
                    }

                    user.IsActive = false;
                    userRep.Update(user);

                    Cooky expiredCookie = cookieRep.GetList().SingleOrDefault(x => x.UserId == user.Id);
                    if (expiredCookie != null)
                    {
                        cookieRep.Delete(expiredCookie.Id);
                    }
                }

                return RedirectToAction("Index");
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult UndoDelete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                User user;
                using (UserRepository userRep = new UserRepository())
                {
                    user = userRep.GetEntity(id);
                }

                if (user == null)
                {
                    return Error(Errors.USER_NOT_FOUND);
                }
                if (user.Id == CurrentUser.UserId)
                {
                    return Error(Errors.USER_CANNOT_DELETE_SELF);
                }
                if (user.CompanyId != CurrentUser.CompanyId || user.Roles == (int)RoleType.SuperAdmin)
                {
                    return Error(Errors.NO_PERMISSION);
                }

                bool? canAddUsers = CompanyCanAddUsers();
                if (canAddUsers.HasValue)
                {
                    if (canAddUsers.Value)
                    {
                        return View(user);
                    }
                    else
                    {
                        return Error(Errors.USERS_LIMIT_REACHED);
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

        [HttpPost, ActionName("UndoDelete")]
        [OpenIdAuthorize]
        public ActionResult UndoDeleteConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                User user;
                using (UserRepository userRep = new UserRepository())
                {
                    user = userRep.GetEntity(id);

                    if (user == null)
                    {
                        return Error(Errors.USER_NOT_FOUND);
                    }
                    if (user.Id == CurrentUser.UserId)
                    {
                        return Error(Errors.USER_CANNOT_DELETE_SELF);
                    }
                    if (user.CompanyId != CurrentUser.CompanyId || user.Roles == (int)RoleType.SuperAdmin)
                    {
                        return Error(Errors.NO_PERMISSION);
                    }

                    bool? canAddUsers = CompanyCanAddUsers();
                    if (canAddUsers.HasValue)
                    {
                        if (canAddUsers.Value)
                        {
                            user.IsActive = true;
                            userRep.Update(user);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Errors.USERS_LIMIT_REACHED);
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

        [OpenIdAuthorize]
        public ActionResult Search()
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
        public ActionResult Search(UsersSearchValuesModel model)
        {
            if (Authorized(RoleType.SystemManager))
            {
                using (UsersRepository usersRep = new UsersRepository())
                {
                    IQueryable<User> usersQuery = usersRep.GetList("Budget_Departments").Where(x => x.CompanyId == CurrentUser.CompanyId);

                    if (usersQuery != null)
                    {
                        if (!String.IsNullOrEmpty(model.FirstName))
                            usersQuery = usersQuery.Where(x => x.FirstName == model.FirstName);

                        if (!String.IsNullOrEmpty(model.LastName))
                            usersQuery = usersQuery.Where(x => x.LastName == model.LastName);

                        if (model.Role.HasValue && model.Role.Value != -1)
                            usersQuery = usersQuery.Where(x => ((RoleType)x.Roles & (RoleType)model.Role.Value) == (RoleType)model.Role.Value);
                        
                        if (!String.IsNullOrEmpty(model.Email))
                            usersQuery = usersQuery.Where(x => x.Email == model.Email);

                        if (model.CreationMin.HasValue && model.CreationMax.HasValue && model.CreationMax.Value < model.CreationMin.Value)
                            model.CreationMax = null;

                        if (model.CreationMin.HasValue)
                            usersQuery = usersQuery.Where(x => x.CreationTime >= model.CreationMin.Value);

                        if (model.CreationMax.HasValue)
                            usersQuery = usersQuery.Where(x => x.CreationTime <= model.CreationMax.Value);

                        return View(usersQuery.ToList());
                    }
                    else
                    {
                        return Error(Errors.USERS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [ChildActionOnly]
        public ActionResult SearchForm(bool isExpanding, bool isCollapsed)
        {
            UsersSearchFormModel model = new UsersSearchFormModel();

            var allRoles = Enum.GetValues(typeof(RoleType));
            List<SelectListItem> rolesList = new List<SelectListItem>();
            foreach (var role in allRoles)
            {
                rolesList.Add(new SelectListItem() { Value = ((int)role).ToString(), Text = ((RoleType)role).ToString() });
            }

            model.RolesList = new SelectList(rolesList, "Value", "Text");

            ViewBag.IsExpanding = isExpanding;
            ViewBag.IsCollapsed = isCollapsed;
            
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult List(IEnumerable<User> users, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false, bool showUserName = true)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = isOrdered;
            ViewBag.IsPaged = isPaged;
            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = currPage;
            ViewBag.NumberOfPages = numberOfPages;

            ViewBag.IsCheckBoxed = isCheckBoxed;
            ViewBag.ShowUserName = showUserName;

            return PartialView(users);
        }

        [ChildActionOnly]
        public ActionResult SimpleList(IEnumerable<User> users, string baseUrl)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = false;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = null;
            ViewBag.Order = null;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 1;

            ViewBag.IsCheckBoxed = false;

            return PartialView("List", users);
        }

        private bool? CompanyCanAddUsers()
        {
            int companyUserCount = 0;
            int companyUserLimit = 0;
            using (UsersRepository usersRep = new UsersRepository())
            using (PendingUsersRepository pendingUsersRep = new PendingUsersRepository())
            using (CompaniesRepository companiesRep = new CompaniesRepository())
            {
                try
                {
                    companyUserCount =
                        usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId && x.IsActive).Count() +
                        pendingUsersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Count();

                    companyUserLimit = companiesRep.GetEntity(CurrentUser.CompanyId).UsersLimit;
                }
                catch
                {
                    return null;
                }
            }

            return companyUserCount < companyUserLimit;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}