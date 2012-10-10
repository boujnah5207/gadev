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
using GAppsDev.Models.UserModels;
using Mvc4.OpenId.Sample.Security;

namespace GAppsDev.Controllers
{
    public class UsersController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Users/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            AllUsersModel model = new AllUsersModel();

            using (UsersRepository usersRep = new UsersRepository())
            using (PendingUsersRepository pendingUsersRep = new PendingUsersRepository())
            using (CompaniesRepository companiesRep = new CompaniesRepository())
            {
                model.ActiveUsers = usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId && x.IsActive).ToList();
                model.PendingUsers = pendingUsersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();
                model.NonActiveUsers = usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId && !x.IsActive).ToList();

                model.UsersLimit = companiesRep.GetEntity(CurrentUser.CompanyId).UsersLimit;
            }

            return View(model);
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

                roleNames.Remove(RoleType.None.ToString());
                roleNames.Remove(RoleType.SuperAdmin.ToString());

                ViewBag.RolesList = roleNames;

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
            User user = db.Users.Single(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", user.CompanyId);
            return View(user);
        }

        //
        // POST: /Users/Edit/5

        [HttpPost]
        [OpenIdAuthorize]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Attach(user);
                db.ObjectStateManager.ChangeObjectState(user, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", user.CompanyId);
            return View(user);
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
        public ActionResult EditPending(PendingUser user)
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
                using (PendingUserRepository userRep = new PendingUserRepository())
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
                using (PendingUserRepository pendingUserRep = new PendingUserRepository())
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