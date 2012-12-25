﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using GAppsDev.Models.Search;
using GAppsDev.Models.UserModels;
using Mvc4.OpenId.Sample.Security;
using System.Globalization;
using GAppsDev.Models;

namespace GAppsDev.Controllers
{
    public class UsersController : BaseController
    {
        new const int ITEMS_PER_PAGE = 20;
        const string DEFAULT_ORDER = "DESC";

        private Entities db = new Entities();

        //
        // GET: /Users/
        [OpenIdAuthorize]
        public ActionResult LanguageSet()
        {
            using (UsersRepository userRepository = new UsersRepository(CurrentUser.CompanyId))
            using (LanguagesRepository languagesRepository = new LanguagesRepository())
            {
                int lanId = userRepository.GetEntity(CurrentUser.UserId).LanguageId;
                return View(new SelectList(languagesRepository.GetList().ToList(), "Id", "Name", lanId));
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult LanguageSet(int languageId)
        {
            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            {
                User user = usersRep.GetList().SingleOrDefault(x => x.Id == CurrentUser.UserId);
                user.LanguageId = languageId;
                usersRep.Update(user);
                CurrentUser.LanguageCode = user.Language.Code;
                return RedirectToAction("index", "Home");
            }
        }

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = NO_SORT_BY, string order = DEFAULT_ORDER)
        {
            if (Authorized(RoleType.SystemManager))
            {
                AllUsersModel model = new AllUsersModel();
                IEnumerable<User> activeUsers;
                using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
                using (PendingUsersRepository pendingUsersRep = new PendingUsersRepository())
                using (CompaniesRepository companiesRep = new CompaniesRepository())
                {
                    activeUsers = usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId && x.IsActive).ToList();
                    model.PendingUsers = pendingUsersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();
                    model.NonActiveUsers = usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId && !x.IsActive).ToList();

                    model.ActiveUsersCount = activeUsers.Count();
                    model.CanceledUsersCount = model.NonActiveUsers.Count();

                    if (model.ActiveUsers != null && model.PendingUsers != null && model.NonActiveUsers != null)
                    {
                        try
                        {
                            model.UsersLimit = companiesRep.GetEntity(CurrentUser.CompanyId).UsersLimit;
                        }
                        catch
                        {
                            return Error(Loc.Dic.error_database_error);
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
                        return Error(Loc.Dic.error_users_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /Users/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (!Authorized(RoleType.UsersManager))
                return Error(Loc.Dic.error_no_permission);

            User user;
            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            {
                user = usersRep.GetEntity(id, "Language", "User1", "Users1");
            }

            if (user == null)
                return Error(Loc.Dic.error_user_not_found);

            return View(user);
        }

        [ChildActionOnly]
        [OpenIdAuthorize]
        public ActionResult PartialDetails(User user)
        {
            if (!Authorized(RoleType.UsersManager))
                return Error(Loc.Dic.error_no_permission);

            return PartialView(user);
        }

        [OpenIdAuthorize]
        public ActionResult PendingUserDetails(int id = 0)
        {
            if (!Authorized(RoleType.UsersManager))
                return Error(Loc.Dic.error_no_permission);

            PendingUser user;
            using (PendingUsersRepository pendingUsersRep = new PendingUsersRepository())
            {
                user = pendingUsersRep.GetEntity(id, "Language", "User");
            }

            if (user == null)
                return Error(Loc.Dic.error_user_not_found);

            return View(user);
        }

        [ChildActionOnly]
        [OpenIdAuthorize]
        public ActionResult PendingUserPartialDetails(PendingUser user)
        {
            if (!Authorized(RoleType.UsersManager))
                return Error(Loc.Dic.error_no_permission);

            return PartialView(user);
        }

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.SystemManager))
            {
                List<string> roleNames = GetRoleNames();
                List<SelectListItemDB> ApprovalRoutesList = new List<SelectListItemDB>() { new SelectListItemDB() { Id = -1, Name = Loc.Dic.NoApprovalRoute } };
                SelectList languagesList;

                using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
                using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
                using (LanguagesRepository languagesRep = new LanguagesRepository())
                {
                    ApprovalRoutesList.AddRange(
                            routesRep.GetList()
                            .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name })
                            );
                    
                    languagesList = new SelectList(languagesRep.GetList().ToList(), "Id", "Name");
                }

                ViewBag.RolesList = roleNames;
                ViewBag.RoutesList = new SelectList(ApprovalRoutesList, "Id", "Name");
                ViewBag.LanguagesList = languagesList;

                return View();
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult Create(User user, string[] roleNames)
        {
            if (ModelState.IsValid)
            {
                if (user.DefaultApprovalRouteId == -1) user.DefaultApprovalRouteId = null;

                if (Authorized(RoleType.SystemManager))
                {
                    int companyUserCount = 0;
                    int companyUserLimit = 0;

                    using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
                    using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
                    using (PendingUsersRepository pendingUsersRep = new PendingUsersRepository())
                    using (CompaniesRepository companiesRep = new CompaniesRepository())
                    {
                        if (user.DefaultApprovalRouteId.HasValue)
                        {
                            var route = routesRep.GetEntity(user.DefaultApprovalRouteId.Value);
                            if (route == null) return Error(Loc.Dic.error_invalid_form);
                        }

                        try
                        {
                            companyUserCount =
                                usersRep.GetList().Where(x => x.IsActive).Count() +
                                pendingUsersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Count();

                            companyUserLimit = companiesRep.GetEntity(CurrentUser.CompanyId).UsersLimit;
                        }
                        catch
                        {
                            return Error(Loc.Dic.error_database_error);
                        }

                        bool userExists = usersRep.GetList().Any(x => x.CompanyId == CurrentUser.CompanyId && x.Email == user.Email);
                        bool pendingUserExists = pendingUsersRep.GetList().Any(x => x.CompanyId == CurrentUser.CompanyId && x.Email == user.Email);

                        if (userExists || pendingUserExists)
                            return Error(Loc.Dic.error_users_exist_error);
                    }

                    if (companyUserCount >= companyUserLimit)
                        return Error(Loc.Dic.error_users_limit_reached);

                    user.CompanyId = CurrentUser.CompanyId;
                    user.CreationTime = DateTime.Now;

                    RoleType combinedRoles = RoleType.None;
                    List<RoleType> forbiddenRoles = GetForbiddenRoles();

                    if (roleNames != null && roleNames.Count() > 0)
                    {
                        foreach (string roleName in roleNames)
                        {
                            RoleType role;
                            if (Enum.TryParse(roleName, out role) && !forbiddenRoles.Contains(role))
                            {
                                combinedRoles = Roles.CombineRoles(combinedRoles, role);
                            }
                            else
                            {
                                return Error(Loc.Dic.error_invalid_form);
                            }
                        }
                        user.Roles = (int)combinedRoles;

                        user.DefaultApprovalRouteId = user.DefaultApprovalRouteId.HasValue && user.DefaultApprovalRouteId.Value == -1 ? null : user.DefaultApprovalRouteId;

                        bool wasUserCreated;
                        using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
                        {
                            wasUserCreated = usersRep.Create(user);
                        }

                        if (wasUserCreated)
                            return RedirectToAction("Index");
                        else
                            return Error(Loc.Dic.error_users_create_error);
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
                return Error(ModelState);
            }
        }

        [OpenIdAuthorize]
        public ActionResult EditBaskets(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            UserPermissionsModel model = new UserPermissionsModel();
            User user;
            List<Budgets_Baskets> allPermissions;

            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
            {
                user = usersRep.GetEntity(id);

                if (user == null) return Error(Loc.Dic.error_users_get_error);

                model.User = user;
                model.UserPermissions = user.Budgets_UsersToBaskets.Select(x => new UserPermission() { Permission = x.Budgets_Baskets, IsActive = true }).Where(x=>x.Permission.CompanyId == CurrentUser.CompanyId).ToList();

                if (model.UserPermissions == null) return Error(Loc.Dic.error_permissions_get_error);

                allPermissions = permissionsRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();
                if (allPermissions == null)
                    return Error(Loc.Dic.error_database_error);

                List<Budgets_Baskets> allWithoutUserBasket = new List<Budgets_Baskets>();
                foreach (Budgets_Baskets basket in allPermissions)
                    allWithoutUserBasket.Add(basket);

                foreach (UserPermission UserBasket in model.UserPermissions)
                    foreach (Budgets_Baskets basket in allPermissions)
                        if (UserBasket.Permission.Id == basket.Id)
                            allWithoutUserBasket.Remove(basket);

                model.UserId = user.Id;
                model.PermissionsSelectList = new SelectList(allWithoutUserBasket, "Id", "Name");

                return View(model);
            }
        }

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult EditBaskets(UserPermissionsModel model)
        {
            if (ModelState.IsValid)
            {
                if (Authorized(RoleType.SystemManager))
                {
                    User userFromDB;
                    List<Budgets_UsersToBaskets> existingPermissions;
                    bool noErrors = true;

                    using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
                    using (UsersToBasketsRepository userPermissionsRep = new UsersToBasketsRepository())
                    {
                        userFromDB = usersRep.GetEntity(model.UserId);

                        if (userFromDB != null)
                        {
                            if (userFromDB.CompanyId == CurrentUser.CompanyId)
                            {
                                existingPermissions = userPermissionsRep.GetList().Where(x => x.UserId == userFromDB.Id).ToList();

                                if (existingPermissions != null)
                                {
                                    if (model.UserPermissions == null)
                                        return RedirectToAction("Index");

                                    foreach (var permission in model.UserPermissions)
                                    {
                                        if (permission.IsActive)
                                        {
                                            if (!existingPermissions.Any(x => x.BasketId == permission.Permission.Id))
                                            {
                                                Budgets_UsersToBaskets newPermission = new Budgets_UsersToBaskets()
                                                {
                                                    UserId = userFromDB.Id,
                                                    BasketId = permission.Permission.Id,
                                                    CompanyId = CurrentUser.CompanyId
                                                };

                                                if (!userPermissionsRep.Create(newPermission))
                                                    noErrors = false;
                                            }
                                        }
                                        else
                                        {
                                            Budgets_UsersToBaskets existingPermission = existingPermissions.SingleOrDefault(x => x.BasketId == permission.Permission.Id);
                                            if (existingPermission != null)
                                            {
                                                if (!userPermissionsRep.Delete(existingPermission.Id))
                                                    noErrors = false;
                                            }
                                        }
                                    }

                                    if (noErrors)
                                        return RedirectToAction("Index");
                                    else
                                        return Error(Loc.Dic.error_user_edit_permissions_error);
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
                    return Error(Loc.Dic.error_no_permission);
                }
            }
            else
            {
                return Error(ModelState);
            }
        }

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                User user;
                List<SelectListItemDB> ApprovalRoutesList = new List<SelectListItemDB>() { new SelectListItemDB() { Id = -1, Name = Loc.Dic.NoApprovalRoute } };

                using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
                using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
                {
                    user = usersRep.GetEntity(id);

                    ApprovalRoutesList.AddRange(
                            routesRep.GetList()
                            .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name })
                            );

                    if (user != null)
                    {
                        if (user.CompanyId != CurrentUser.CompanyId)
                            return Error(Loc.Dic.error_no_permission);

                        ViewBag.RoutesList = new SelectList(ApprovalRoutesList, "Id", "Name");

                        List<string> roleNames = GetRoleNames();

                        ViewBag.RolesList = roleNames;

                        ViewBag.ExistingRoles =
                            Roles.GetAllRoles((RoleType)user.Roles)
                            .Select(x => x.ToString())
                            .ToList();

                        return View(user);
                    }
                    else
                    {
                        return Error(Loc.Dic.error_user_not_found);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult Edit(User user, string[] roleNames)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (user.DefaultApprovalRouteId == -1) user.DefaultApprovalRouteId = null;

                if (ModelState.IsValid)
                {
                    User userFromDatabase;
                    using (UsersRepository userRep = new UsersRepository(CurrentUser.CompanyId))
                    using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
                    {
                        userFromDatabase = userRep.GetEntity(user.Id);

                        if (userFromDatabase != null)
                        {
                            if (user.DefaultApprovalRouteId.HasValue)
                            {
                                var route = routesRep.GetEntity(user.DefaultApprovalRouteId.Value);
                                if (route == null) return Error(Loc.Dic.error_invalid_form);
                            }

                            RoleType combinedRoles = RoleType.None;
                            List<RoleType> forbiddenRoles = GetForbiddenRoles();

                            foreach (string roleName in roleNames)
                            {
                                RoleType role;
                                if (Enum.TryParse(roleName, out role) && !forbiddenRoles.Contains(role))
                                {
                                    combinedRoles = Roles.CombineRoles(combinedRoles, role);
                                }
                                else
                                {
                                    return Error(Loc.Dic.error_invalid_form);
                                }
                            }

                            userFromDatabase.FirstName = user.FirstName;
                            userFromDatabase.LastName = user.LastName;
                            userFromDatabase.Email = user.Email;
                            userFromDatabase.Roles = (int)combinedRoles;
                            userFromDatabase.DefaultApprovalRouteId = user.DefaultApprovalRouteId.HasValue && user.DefaultApprovalRouteId.Value == -1 ? null : user.DefaultApprovalRouteId;

                            User updatedUser = userRep.Update(userFromDatabase);
                            if (updatedUser != null)
                                return RedirectToAction("Index");
                            else
                                return Error(Loc.Dic.error_database_error);
                        }
                        else
                        {
                            return Error(Loc.Dic.error_user_not_found);
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

        [OpenIdAuthorize]
        public ActionResult EditPending(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                PendingUser user;
                List<SelectListItemDB> usersList = new List<SelectListItemDB> { new SelectListItemDB() { Id = -1, Name = "(ללא) מאשר סופי" } };

                using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
                using (PendingUsersRepository pendingUserRep = new PendingUsersRepository())
                {
                    user = pendingUserRep.GetEntity(id);
                    usersList.AddRange(usersRep.GetList().Where(u => u.CompanyId == CurrentUser.CompanyId && ((RoleType)u.Roles & RoleType.OrdersApprover) == RoleType.OrdersApprover).Select(x => new SelectListItemDB() { Id = x.Id, Name = x.FirstName + " " + x.LastName }));
                }

                if (user.CompanyId != CurrentUser.CompanyId)
                    return Error(Loc.Dic.error_no_permission);

                if (user != null)
                {
                    ViewBag.OrdersApproverId = new SelectList(usersList, "Id", "Name", user.OrdersApproverId.HasValue ? user.OrdersApproverId.Value : -1);

                    List<string> roleNames = GetRoleNames();
                    ViewBag.RolesList = roleNames;

                    ViewBag.ExistingRoles =
                        Roles.GetAllRoles((RoleType)user.Roles)
                        .Select(x => x.ToString())
                        .ToList();

                    return View(user);
                }
                else
                {
                    return Error(Loc.Dic.error_user_not_found);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

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
                                return Error(Loc.Dic.error_no_permission);

                            RoleType combinedRoles = RoleType.None;
                            List<RoleType> forbiddenRoles = GetForbiddenRoles();
                            
                            foreach (string roleName in roleNames)
                            {
                                RoleType role;
                                if (Enum.TryParse(roleName, out role) && !forbiddenRoles.Contains(role))
                                {
                                    combinedRoles = Roles.CombineRoles(combinedRoles, role);
                                }
                                else
                                {
                                    return Error(Loc.Dic.error_invalid_form);
                                }
                            }

                            userFromDatabase.Email = user.Email;
                            userFromDatabase.Roles = (int)combinedRoles;
                            userFromDatabase.OrdersApproverId = user.OrdersApproverId.HasValue && user.OrdersApproverId.Value == -1 ? null : user.OrdersApproverId;

                            pendingUserRep.Update(userFromDatabase);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Loc.Dic.error_user_not_found);
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

        [OpenIdAuthorize]
        public ActionResult CreateApprovalRoute(int id = 0)
        {
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
        public ActionResult CreateApprovalRoute(ApprovalRouteModel model)
        {
            return RedirectToAction("Index");
        }

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                User user;
                using (UsersRepository userRep = new UsersRepository(CurrentUser.CompanyId))
                {
                    user = userRep.GetEntity(id, "Language", "User1", "Users1");
                }

                if (user == null)
                {
                    return Error(Loc.Dic.error_user_not_found);
                }
                if (user.Id == CurrentUser.UserId)
                {
                    return Error(Loc.Dic.error_user_cannot_delete_self);
                }
                if (user.CompanyId != CurrentUser.CompanyId || user.Roles == (int)RoleType.SuperAdmin)
                {
                    return Error(Loc.Dic.error_no_permission);
                }

                return View(user);
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
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
                    user = userRep.GetEntity(id, "User");
                }

                if (user == null)
                {
                    return Error(Loc.Dic.error_user_not_found);
                }
                if (user.CompanyId != CurrentUser.CompanyId)
                {
                    return Error(Loc.Dic.error_no_permission);
                }

                return View(user);
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
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
                        return Error(Loc.Dic.error_user_not_found);
                    }
                    if (user.CompanyId != CurrentUser.CompanyId)
                    {
                        return Error(Loc.Dic.error_no_permission);
                    }

                    pendingUserRep.Delete(user.Id);
                }

                return RedirectToAction("Index");
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
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
                using (UsersRepository userRep = new UsersRepository(CurrentUser.CompanyId))
                {
                    user = userRep.GetEntity(id);

                    if (user == null)
                    {
                        return Error(Loc.Dic.error_user_not_found);
                    }
                    if (user.Id == CurrentUser.UserId)
                    {
                        return Error(Loc.Dic.error_user_cannot_delete_self);
                    }
                    if (user.CompanyId != CurrentUser.CompanyId || user.Roles == (int)RoleType.SuperAdmin)
                    {
                        return Error(Loc.Dic.error_no_permission);
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
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [OpenIdAuthorize]
        public ActionResult UndoDelete(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            User user;
            using (UsersRepository userRep = new UsersRepository(CurrentUser.CompanyId))
            {
                user = userRep.GetEntity(id, "Company", "Language", "User1", "Users1");
            }

            if (user == null)
            {
                return Error(Loc.Dic.error_user_not_found);
            }
            if (user.Id == CurrentUser.UserId)
            {
                return Error(Loc.Dic.error_user_cannot_delete_self);
            }
            if (user.CompanyId != CurrentUser.CompanyId || user.Roles == (int)RoleType.SuperAdmin)
            {
                return Error(Loc.Dic.error_no_permission);
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
                    return Error(Loc.Dic.error_users_limit_reached);
                }
            }
            else
            {
                return Error(Loc.Dic.error_database_error);
            }
        }

        [HttpPost, ActionName("UndoDelete")]
        [OpenIdAuthorize]
        public ActionResult UndoDeleteConfirmed(int id)
        {
            if (!Authorized(RoleType.SystemManager))
                return Error(Loc.Dic.error_no_permission);

            User user;
            using (UsersRepository userRep = new UsersRepository(CurrentUser.CompanyId))
            {
                user = userRep.GetEntity(id, "Company");

                if (user == null)
                {
                    return Error(Loc.Dic.error_user_not_found);
                }
                if (user.Id == CurrentUser.UserId)
                {
                    return Error(Loc.Dic.error_user_cannot_delete_self);
                }
                if (user.CompanyId != CurrentUser.CompanyId || user.Roles == (int)RoleType.SuperAdmin)
                {
                    return Error(Loc.Dic.error_no_permission);
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
                        return Error(Loc.Dic.error_users_limit_reached);
                    }
                }
                else
                {
                    return Error(Loc.Dic.error_database_error);
                }
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
                return Error(Loc.Dic.error_no_permission);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Search(UsersSearchValuesModel model)
        {
            if (Authorized(RoleType.SystemManager))
            {
                using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
                {
                    IQueryable<User> usersQuery = usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId);

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
                        return Error(Loc.Dic.error_users_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
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
        public ActionResult PendingUsersList(IEnumerable<PendingUser> users, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false, bool showUserName = true)
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
            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
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

        private List<string> GetRoleNames()
        {
            List<string> roleNames = Enum.GetNames(typeof(RoleType)).ToList();

            roleNames.Remove(RoleType.None.ToString());

            if (!Authorized(RoleType.SuperAdmin))
            {
                roleNames.Remove(RoleType.SuperApprover.ToString());
                roleNames.Remove(RoleType.SuperAdmin.ToString());
            }

            return roleNames;
        }

        private List<RoleType> GetForbiddenRoles()
        {
            List<RoleType> forbiddenRoles = new List<RoleType>();

            if (!Authorized(RoleType.SuperAdmin))
            {
                forbiddenRoles.Add(RoleType.None);
                forbiddenRoles.Add(RoleType.SuperApprover);
                forbiddenRoles.Add(RoleType.SuperAdmin);
            }

            return forbiddenRoles;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}