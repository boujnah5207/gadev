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
    public class PermissionsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Permissions/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            return View(db.Budgets_Permissions.ToList());
        }

        //
        // GET: /Permissions/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permission;
                using (BudgetsPermissionsRepository permissionRep = new BudgetsPermissionsRepository())
                {
                    permission = permissionRep.GetEntity(id);
                }

                if (permission != null)
                {
                    if (permission.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(permission);
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.PERMISSIONS_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Permissions/Create

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
        // POST: /Permissions/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Budgets_Permissions budgets_permissions)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    budgets_permissions.CompanyId = CurrentUser.CompanyId;

                    bool wasCreated;
                    using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                    {
                        wasCreated = permissionsRep.Create(budgets_permissions);
                    }

                    if (wasCreated)
                        return RedirectToAction("Index");
                    else
                        return Error(Errors.PERMISSIONS_CREATE_ERROR);

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
        // GET: /Permissions/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permission;
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                {
                    permission = permissionsRep.GetEntity(id);
                }

                if (permission != null)
                {
                    if (permission.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(permission);
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.PERMISSIONS_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Permissions/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Budgets_Permissions budgets_permissions)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permissionFromDB;
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                {
                    permissionFromDB = permissionsRep.GetEntity(budgets_permissions.Id);

                    if (permissionFromDB != null)
                    {
                        if (permissionFromDB.CompanyId == CurrentUser.CompanyId)
                        {
                            permissionFromDB.Name = budgets_permissions.Name;

                            permissionsRep.Update(permissionFromDB);

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.PERMISSIONS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /Permissions/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permission;

                using (BudgetsPermissionsRepository permissiosRep = new BudgetsPermissionsRepository())
                {
                    permission = permissiosRep.GetEntity(id);

                    if (permission != null)
                    {
                        if (permission.CompanyId == CurrentUser.CompanyId)
                        {
                            return View(permission);
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.PROJECTS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Permissions/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Budgets_Permissions permission;

                using (OrdersRepository orderssRep = new OrdersRepository())
                using (BudgetsPermissionsRepository permissionsRep = new BudgetsPermissionsRepository())
                using (BudgetsPermissionsToAllocationRepository permissionsAllocationsRep = new BudgetsPermissionsToAllocationRepository())
                using (BudgetsUsersToPermissionsRepository usersPermissionsRep = new BudgetsUsersToPermissionsRepository())
                {
                    permission = permissionsRep.GetEntity(id);

                    if (permission != null)
                    {
                        if (permission.CompanyId == CurrentUser.CompanyId)
                        {
                            bool noErrors = true;
                            List<int> permissionAllocations = permission.Budgets_PermissionsToAllocation.Select( x => x.Id).ToList();
                            List<int> usersPermissions = permission.Budgets_UsersToPermissions.Select(x => x.Id).ToList();

                            foreach (var itemId in permissionAllocations)
                            {
                                if (!permissionsAllocationsRep.Delete(itemId))
                                    noErrors = false;
                            }

                            foreach (var itemId in usersPermissions)
                            {
                                if (!usersPermissionsRep.Delete(itemId))
                                    noErrors = false;
                            }

                            if (!permissionsRep.Delete(permission.Id))
                                noErrors = false;

                            if (noErrors)
                                return RedirectToAction("Index");
                            else
                                return Error(Errors.PERMISSIONS_DELETE_ERROR);
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.PROJECTS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}