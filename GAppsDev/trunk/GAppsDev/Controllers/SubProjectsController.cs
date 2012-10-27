﻿using System;
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
    public class SubProjectsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /SubProjects/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            return View(db.Projects_SubProject.ToList());
        }

        //
        // GET: /SubProjects/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_SubProject subProject;
                using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                {
                    subProject = subProjectsRep.GetEntity(id);
                }

                if (subProject != null)
                {
                    if (subProject.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(subProject);
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.SUB_PROJECTS_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // GET: /SubProjects/Create

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
        // POST: /SubProjects/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Projects_SubProject projects_subproject)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    projects_subproject.CompanyId = CurrentUser.CompanyId;
                    projects_subproject.IsActive = true;

                    bool wasCreated;
                    using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                    {
                        wasCreated = subProjectsRep.Create(projects_subproject);
                    }

                    if (wasCreated)
                        return RedirectToAction("Index");
                    else
                        return Error(Errors.SUB_PROJECTS_CREATE_ERROR);

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
        // GET: /SubProjects/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_SubProject subProject;
                using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                {
                    subProject = subProjectsRep.GetEntity(id);
                }

                if (subProject != null)
                {
                    if (subProject.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(subProject);
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
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /SubProjects/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Projects_SubProject projects_subproject)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_SubProject subProjectFromDB;
                using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                {
                    subProjectFromDB = subProjectsRep.GetEntity(projects_subproject.Id);

                    if (subProjectFromDB != null)
                    {
                        if (subProjectFromDB.CompanyId == CurrentUser.CompanyId)
                        {
                            subProjectFromDB.Name = projects_subproject.Name;

                            subProjectsRep.Update(subProjectFromDB);

                            return RedirectToAction("Index");
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
        // GET: /SubProjects/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_SubProject subProject;
                using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                {
                    subProject = subProjectsRep.GetEntity(id);
                }

                if (subProject != null)
                {
                    if (subProject.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(subProject);
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
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /SubProjects/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_SubProject subProject;

                using (OrdersRepository orderssRep = new OrdersRepository())
                using (SubProjectsRepository subProjectsRep = new SubProjectsRepository())
                {
                    subProject = subProjectsRep.GetEntity(id);

                    if (subProject != null)
                    {
                        if (subProject.CompanyId == CurrentUser.CompanyId)
                        {
                            subProject.IsActive = false;
                            Projects_SubProject update = subProjectsRep.Update(subProject);

                            if (update != null)
                                return View(subProject);
                            else
                                return Error(Errors.SUB_PROJECTS_GET_ERROR);
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