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
    public class ProjectsController : BaseController
    {
        private Entities db = new Entities();

        //
        // GET: /Projects/

        [OpenIdAuthorize]
        public ActionResult Index()
        {
            var projects_parentproject = db.Projects_ParentProject.Include("Company").Where(x => x.CompanyId == CurrentUser.CompanyId);
            return View(projects_parentproject.ToList());
        }

        //
        // GET: /Projects/Details/5

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_ParentProject Project;
                using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                {
                    Project = projectsRep.GetEntity(id);
                }

                if (Project != null)
                {
                    if (Project.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(Project);
                    }
                    else
                    {
                        return Error(Loc.Dic.error_no_permission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.error_projects_get_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /Projects/Create

        [OpenIdAuthorize]
        public ActionResult Create()
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

        //
        // POST: /Projects/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Projects_ParentProject projects_parentproject)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (ModelState.IsValid)
                {
                    projects_parentproject.CompanyId = CurrentUser.CompanyId;
                    projects_parentproject.IsActive = true;

                    bool wasCreated;
                    using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                    {
                        wasCreated = projectsRep.Create(projects_parentproject);
                    }

                    if (wasCreated)
                        return RedirectToAction("Index");
                    else
                        return Error(Loc.Dic.error_projects_create_error);

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
        // GET: /Projects/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_ParentProject Project;
                using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                {
                    Project = projectsRep.GetEntity(id);
                }

                if (Project != null)
                {
                    if (Project.CompanyId == CurrentUser.CompanyId)
                    {
                        return View(Project);
                    }
                    else
                    {
                        return Error(Loc.Dic.error_no_permission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.error_projects_get_error);
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /Projects/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Projects_ParentProject projects_parentproject)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_ParentProject projectFromDB;
                using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                {
                    projectFromDB = projectsRep.GetEntity(projects_parentproject.Id);

                    if (projectFromDB != null)
                    {
                        if (projectFromDB.CompanyId == CurrentUser.CompanyId)
                        {
                            projectFromDB.Name = projects_parentproject.Name;

                            projectsRep.Update(projectFromDB);

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_projects_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // GET: /Projects/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_ParentProject project;

                using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                {
                    project = projectsRep.GetEntity(id);

                    if (project != null)
                    {
                        if (project.CompanyId == CurrentUser.CompanyId)
                        {
                            return View(project);
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_projects_get_error);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.error_no_permission);
            }
        }

        //
        // POST: /Projects/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Projects_ParentProject project;

                using (OrdersRepository orderssRep = new OrdersRepository(CurrentUser.CompanyId))
                using (ParentProjectsRepository projectsRep = new ParentProjectsRepository())
                {
                    project = projectsRep.GetEntity(id);

                    if (project != null)
                    {
                        if (project.CompanyId == CurrentUser.CompanyId)
                        {
                            project.IsActive = false;
                            Projects_ParentProject update = projectsRep.Update(project);

                            if (update != null)
                                return RedirectToAction("Index");
                            else
                                return Error(Loc.Dic.error_projects_get_error);
                        }
                        else
                        {
                            return Error(Loc.Dic.error_no_permission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.error_projects_get_error);
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