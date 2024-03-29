﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using DB;
using Mvc4.OpenId.Sample.Security;
using GAppsDev.Models;
using Rotativa;
using BaseLibraries;
using System.IO;
using GAppsDev.Models.Search;
using System.Globalization;
using System.Text;
using GAppsDev.Models.FileModels;
using Roles = DA.Roles;
using System.Data.Objects;
using GAppsDev.ExtraClasses;
using SystemFile = System.IO.File;

namespace GAppsDev.Controllers
{
    public class OrdersController : BaseController
    {
        public const string DEFAULT_SORT = "lastChange";

        const string INVOICE_FOLDER_NAME = "Invoices";
        const string RECEIPT_FOLDER_NAME = "Receipts";

        const string PRINT_PASSWORD = "S#At7e5eqes2Tres$aph6C5apRu=aZ!BrE_u-a-!suwRU4R9D3EzaTRU&pe=&Ehe";

        [OpenIdAuthorize]
        public ActionResult Home()
        {
            return View();
        }

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.OrdersViewer)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Order> orders;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                orders = ordersRep.GetListWithCanceled("Orders_Statuses", "Supplier", "User");

                if (orders == null) return Error(Loc.Dic.error_orders_get_error);

                orders = Pagination(orders, page, sortby, order);

                return View(orders.ToList());
            }
        }

        [OpenIdAuthorize]
        public ActionResult MyOrders(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Order> orders;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                orders = ordersRep.GetListWithCanceled("Orders_Statuses", "Supplier").Where(x => x.UserId == CurrentUser.UserId);

                if (orders == null) return Error(Loc.Dic.error_orders_get_error);

                orders = Pagination(orders, page, sortby, order);

                return View(orders.ToList());
            }
        }

        [OpenIdAuthorize]
        public ActionResult PendingOrders(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.OrdersApprover)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Order> orders;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                orders = ordersRep.GetList("Orders_Statuses", "Supplier", "User")
                    .Where(x =>
                        x.NextOrderApproverId == CurrentUser.UserId &&
                        x.StatusId != (int)StatusType.Declined &&
                        x.StatusId != (int)StatusType.PendingOrderCreator
                        );

                if (orders == null) return Error(Loc.Dic.error_orders_get_error);

                orders = Pagination(orders, page, sortby, order);

                return View(orders.ToList());
            }
        }

        [OpenIdAuthorize]
        public ActionResult DelayingOrders(int page = FIRST_PAGE, string sortby = "creation", string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.OrdersViewer)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Order> orders;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                DateTime CurrentTime = DateTime.Now;
                TimeSpan twoDays = TimeSpan.FromDays(2);
                orders = ordersRep.GetList("Orders_Statuses", "Supplier", "User")
                    .Where(x =>
                        EntityFunctions.DiffHours(x.CreationDate, CurrentTime) >= 48 &&
                        x.StatusId < (int)StatusType.ApprovedPendingInvoice &&
                        x.StatusId != (int)StatusType.Declined &&
                        x.StatusId != (int)StatusType.OrderCancelled
                        );

                if (orders == null) return Error(Loc.Dic.error_orders_get_error);

                orders = Pagination(orders, page, sortby, order);

                return View(orders.ToList());
            }
        }

        [OpenIdAuthorize]
        public ActionResult ModifyStatus(int id = 0)
        {
            if (!Authorized(RoleType.OrdersApprover)) return Error(Loc.Dic.error_no_permission);

            OrdersRepository.ExeedingOrderData model;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            using (OrderToItemRepository orderItemsRep = new OrderToItemRepository())
            {
                StatusType minStatus = Authorized(RoleType.OrdersApprover) ? StatusType.ApprovedPendingInvoice : StatusType.Pending;
                model = ordersRep.GetOrderWithExeedingData(id, minStatus, "Orders_Statuses", "Supplier", "User", "User1", "Orders_OrderToItem.Orders_Items", "Orders_OrderToAllocation", "Orders_OrderToAllocation.Budgets_Allocations", "Budget");

                if (model == null) return Error(Loc.Dic.error_order_get_error);
                if ((model.OriginalOrder.NextOrderApproverId != CurrentUser.UserId) && !Authorized(RoleType.SuperApprover)) return Error(Loc.Dic.error_no_permission);
                if (model.OriginalOrder.StatusId == (int)StatusType.OrderCancelled) return Error(Loc.Dic.error_order_is_canceled);
                if (model.OriginalOrder.StatusId >= (int)StatusType.ApprovedPendingInvoice) return Error(Loc.Dic.error_order_already_approved);

                return View(model);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult ModifyStatus(string approverNotes, string selectedStatus, int id = 0)
        {
            int? historyActionId = null;
            if (!Authorized(RoleType.OrdersApprover)) return Error(Loc.Dic.error_no_permission);
            if (approverNotes != null && approverNotes.Length > 250) return Error(Loc.Dic.error_order_notes_too_long);

            Order orderFromDB;
            using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, id))
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            using (AllocationRepository allocationsRep = new AllocationRepository(CurrentUser.CompanyId))
            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            {
                orderFromDB = ordersRep.GetEntity(id, "User", "Users_ApprovalRoutes");

                if (orderFromDB == null) return Error(Loc.Dic.error_order_get_error);
                if ((orderFromDB.NextOrderApproverId != CurrentUser.UserId) && !Authorized(RoleType.SuperApprover)) return Error(Loc.Dic.error_no_permission);
                if (orderFromDB.StatusId == (int)StatusType.OrderCancelled) return Error(Loc.Dic.error_order_is_canceled);
                if (orderFromDB.StatusId >= (int)StatusType.ApprovedPendingInvoice) return Error(Loc.Dic.error_order_already_approved);

                List<Users_ApprovalStep> orderRouteSteps = orderFromDB.Users_ApprovalRoutes.Users_ApprovalStep.OrderBy(x => x.StepNumber).ToList();
                Users_ApprovalStep firstStep = orderRouteSteps.FirstOrDefault();
                Users_ApprovalStep currentStep = orderRouteSteps.Single(x => x.StepNumber == orderFromDB.ApprovalRouteStep);
                Users_ApprovalStep nextStep = orderRouteSteps.FirstOrDefault(x => x.StepNumber > currentStep.StepNumber);

                if (selectedStatus == Loc.Dic.ApproveOrder)
                {
                    if (Authorized(RoleType.SuperApprover) || nextStep == null)
                    {
                        orderFromDB.ApprovalRouteStep = null;
                        orderFromDB.NextOrderApproverId = null;
                        orderFromDB.StatusId = (int)StatusType.ApprovedPendingInvoice;
                        historyActionId = (int)HistoryActions.PassedApprovalRoute;
                    }
                    else
                    {
                        orderFromDB.ApprovalRouteStep = nextStep.StepNumber;
                        orderFromDB.NextOrderApproverId = nextStep.UserId;
                        orderFromDB.StatusId = (int)StatusType.PartiallyApproved;
                        historyActionId = (int)HistoryActions.PartiallyApproved;
                    }
                }
                else if (selectedStatus == Loc.Dic.DeclineOrder)
                {
                    orderFromDB.NextOrderApproverId = null;
                    orderFromDB.StatusId = (int)StatusType.Declined;
                    historyActionId = (int)HistoryActions.Declined;
                }
                else if (selectedStatus == Loc.Dic.SendBackToUser)
                {
                    orderFromDB.ApprovalRouteStep = firstStep.StepNumber;
                    orderFromDB.NextOrderApproverId = firstStep.UserId;
                    orderFromDB.StatusId = (int)StatusType.PendingOrderCreator;
                    historyActionId = (int)HistoryActions.ReturnedToCreator;
                }

                orderFromDB.LastStatusChangeDate = DateTime.Now;

                if (ordersRep.Update(orderFromDB) == null) return Error(Loc.Dic.error_database_error);

                Orders_History orderHistory = new Orders_History();
                if (historyActionId.HasValue) ordersHistoryRep.Create(orderHistory, historyActionId.Value, approverNotes);

                //EmailMethods emailMethods = new EmailMethods("NOREPLY@pqdev.com", Loc.Dic.OrdersSystem, "noreply50100200");

                //string emailSubject = String.Format("{0} {1} {2} {3} {4}", Loc.Dic.Order, orderFromDB.OrderNumber, Translation.Status((StatusType)orderFromDB.StatusId), Loc.Dic.By, CurrentUser.FullName);
                //StringBuilder emailBody = new StringBuilder();
                
                //emailBody.AppendLine(emailSubject);
                //emailBody.AppendLine();
                //emailBody.AppendLine(String.Format("{0}: {1}", Loc.Dic.SeeDetailsAt, Url.Action("Details", "Orders", new { id = id }, "http")));

                //emailMethods.sendGoogleEmail(orderFromDB.User.Email, orderFromDB.User.FirstName, emailSubject, emailBody.ToString());
                
                SendNotifications.OrderStatusChanged(orderFromDB, CurrentUser, Url);
                if (orderFromDB.StatusId == (int)StatusType.PartiallyApproved)
                    SendNotifications.OrderPendingApproval(orderFromDB, Url);

                return RedirectToAction("PendingOrders");
            }
        }

        [OpenIdAuthorize]
        public ActionResult UploadInvoiceFile(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            Order order;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id);
            }

            if (order == null) return Error(Loc.Dic.error_order_not_found);
            if (order.StatusId < (int)StatusType.ApprovedPendingInvoice) return Error(Loc.Dic.error_order_not_approved);

            ViewBag.OrderID = id;
            if (order.StatusId > (int)StatusType.ApprovedPendingInvoice)
            {
                UploadInvoiceModel model = new UploadInvoiceModel();
                model.InvoiceNumber = order.InvoiceNumber;
                model.InvoiceDate = order.InvoiceDate.Value;
                model.ValueDate = order.ValueDate.Value;
                model.isUpdate = true;

                return View(model);
            }
            else
            {
                return View();
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult UploadInvoiceFile(UploadInvoiceModel model, int id = 0)
        {
            int? historyActionId = null;

            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            if (!ModelState.IsValid)
            {
                ViewBag.OrderID = id;
                return View(model);
            }

            Order order;
            bool isModelValid = true;

            using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, id))
            using (OrderToAllocationRepository orderAlloRep = new OrderToAllocationRepository())
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id, "Budget", "Orders_OrderToAllocation");

                if (order == null) return Error(Loc.Dic.error_order_get_error);

                if (order.StatusId < (int)StatusType.ApprovedPendingInvoice) return Error(Loc.Dic.error_order_not_approved);
                DateTime minValueDate = new DateTime(order.Budget.Year, order.Orders_OrderToAllocation.Max(x => x.MonthId), FIRST_DAY_OF_MONTH);
                if (model.ValueDate < minValueDate)
                {
                    isModelValid = false;
                    ModelState.AddModelError("ValueDate", Loc.Dic.error_ValueDateHaveToBeLaterThenLatestAllocationDate);
                }
                if (!model.File.FileName.Contains('.'))
                {
                    isModelValid = false;
                    ModelState.AddModelError("File", Loc.Dic.validation_FileNameIsInvalid);
                }

                if (!isModelValid)
                {
                    ViewBag.OrderID = id;
                    return View(model);
                }

                if (!model.isUpdate)
                {
                    order.StatusId = (int)StatusType.InvoiceScannedPendingOrderCreator;
                    order.LastStatusChangeDate = DateTime.Now;
                }
                order.InvoiceNumber = model.InvoiceNumber;
                order.InvoiceDate = model.InvoiceDate;
                order.ValueDate = model.ValueDate;
                if (ordersRep.Update(order) == null) return Error(Loc.Dic.error_database_error);

                historyActionId = (int)HistoryActions.InvoiceScanned;
                Orders_History orderHistory = new Orders_History();
                if (historyActionId.HasValue) ordersHistoryRep.Create(orderHistory, historyActionId.Value);

                SaveUniqueFile(model.File, INVOICE_FOLDER_NAME, id);

                //EmailMethods emailMethods = new EmailMethods("NOREPLY@pqdev.com", Loc.Dic.OrdersSystem, "noreply50100200");
                //emailMethods.sendGoogleEmail(order.User.Email, order.User.FirstName, Loc.Dic.OrderStatusUpdateEvent, Loc.Dic.OrderStatusOf + " " + order.OrderNumber + " " + Loc.Dic.OrderStatusChangedTo + Translation.Status((StatusType)order.StatusId) + "\n" + Url.Action("MyOrders", "Orders", null, "http"));

                SendNotifications.OrderStatusChanged(order, CurrentUser, Url);

                return RedirectToAction("Index");
            }
        }

        [OpenIdAuthorize]
        public ActionResult UploadReceiptFile(int id = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.Error_NoPermission);

            Order order;
            using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = orderRep.GetEntity(id);

                if (order == null) return Error(Loc.Dic.Error_OrderNotFound);
                if (order.StatusId < (int)StatusType.InvoiceExportedToFile) return Error(Loc.Dic.error_wrongStatus);

                ViewBag.OrderId = id;
                if (order.StatusId >= (int)StatusType.ReceiptScanned)
                {
                    UploadReceiptModel model = new UploadReceiptModel();
                    model.isUpdate = true;

                    return View(model);
                }
                else
                {
                    return View();
                }
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult UploadReceiptFile(UploadReceiptModel model, int id = 0)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.Error_NoPermission);

            if (!ModelState.IsValid)
            {
                ViewBag.OrderID = id;
                return View(model);
            }

            Order order;
            bool isModelValid = true;
            int? historyActionId = null;



            using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, id))
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id);

                if (order == null) return Error(Loc.Dic.Error_OrderNotFound);
                if (order.StatusId < (int)StatusType.InvoiceExportedToFile) return Error(Loc.Dic.error_wrongStatus);

                if (!model.File.FileName.Contains('.'))
                {
                    isModelValid = false;
                    ModelState.AddModelError("File", Loc.Dic.validation_FileNameIsInvalid);
                }

                if (!isModelValid)
                {
                    ViewBag.OrderID = id;
                    return View(model);
                }

                if (!model.isUpdate)
                {
                    order.StatusId = (int)StatusType.ReceiptScanned;
                    order.LastStatusChangeDate = DateTime.Now;
                    if (ordersRep.Update(order) == null) return Error(Loc.Dic.Error_DatabaseError);
                }

                SaveUniqueFile(model.File, RECEIPT_FOLDER_NAME, id);
                historyActionId = (int)HistoryActions.ReceiptScanned;
                Orders_History orderHistory = new Orders_History();
                if (historyActionId.HasValue) ordersHistoryRep.Create(orderHistory, historyActionId.Value);
                
                //EmailMethods emailMethods = new EmailMethods("NOREPLY@pqdev.com", Loc.Dic.OrdersSystem, "noreply50100200");
                //emailMethods.sendGoogleEmail(order.User.Email, order.User.FirstName, Loc.Dic.OrderStatusUpdateEvent, Loc.Dic.OrderStatusOf + order.OrderNumber + Loc.Dic.OrderStatusChangedTo + Translation.Status((StatusType)order.StatusId) + Url.Action("MyOrders", "Orders", null, "http"));

                SendNotifications.OrderStatusChanged(order, CurrentUser, Url);

                return RedirectToAction("Index");
            }
        }

        [OpenIdAuthorize]
        public ActionResult DownloadInvoice(int id = 0)
        {
            Order order;

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id);

                if (order == null) return Error(Loc.Dic.Error_OrderNotFound);
                if (!Authorized(RoleType.OrdersViewer) && order.UserId != CurrentUser.UserId) return Error(Loc.Dic.Error_NoPermission);
                if (order.StatusId < (int)StatusType.InvoiceScannedPendingOrderCreator) return Error(Loc.Dic.Error_InvoiceNotScanned);

                UploadedFile file = GetUniqueFile(INVOICE_FOLDER_NAME, id);
                if (file == null) return Error(Loc.Dic.Error_InvoiceFileNotFound);

                return File(file.Content, file.MimeType, String.Format("Order_{0}-Invoice_{1}.{2}", order.OrderNumber, order.InvoiceNumber, file.Extension));
            }
        }

        [OpenIdAuthorize]
        public ActionResult DownloadReceipt(int id = 0)
        {
            Order order;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id);

                if (order == null) return Error(Loc.Dic.Error_OrderNotFound);
                if (!Authorized(RoleType.OrdersViewer) && order.UserId != CurrentUser.UserId) return Error(Loc.Dic.Error_NoPermission);
                if (order.StatusId < (int)StatusType.ReceiptScanned) return Error(Loc.Dic.Error_ReceiptNotScanned);

                UploadedFile file = GetUniqueFile(RECEIPT_FOLDER_NAME, id);
                if (file == null) return Error(Loc.Dic.Error_ReceiptFileNotFound);

                return File(file.Content, file.MimeType, String.Format("Order_{0}-Receipt.{1}", order.OrderNumber, file.Extension));
            }
        }

        //[OpenIdAuthorize]
        public ActionResult PrintOrderToScreen(string password, int userId, int userRoles, int id = 0, int companyId = 0, string languageCode = "he", string coinSign = "$")
        {
            if (password != PRINT_PASSWORD) return Error(Loc.Dic.Error_NoPermission);

            CultureInfo ci = new CultureInfo(languageCode);
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture =
            CultureInfo.CreateSpecificCulture(ci.Name);

            Order order;
            using (OrdersRepository ordersRep = new OrdersRepository(companyId))
            {
                order = ordersRep.GetEntity(id, "User", "Company", "Supplier", "Orders_OrderToItem.Orders_Items");
            }

            if (!Roles.HasRole(userRoles, RoleType.OrdersViewer) && order.UserId != userId) return Error(Loc.Dic.Error_NoPermission);
            if (order == null) return Error(Loc.Dic.error_order_not_found);

            string fileName = order.Company.Id + ".png";
            string path = Path.Combine(Server.MapPath("~/Content/LogoImages/"), fileName);

            ViewBag.LogoExists = System.IO.File.Exists(path);

            ViewBag.LanguageCode = languageCode;
            ViewBag.CompanyCoinSign = coinSign;

            return View(order);
        }

        [OpenIdAuthorize]
        public ActionResult DownloadOrderAsPdf(int id = 0)
        {
            string cookieName = OpenIdMembershipService.LOGIN_COOKIE_NAME;
            HttpCookie cookie = Request.Cookies[cookieName];
            Dictionary<string, string> cookies = new Dictionary<string, string>();
            cookies.Add(cookieName, cookie.Value);
            int? historyActionId = null;

            Order order;
            using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, id))

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id);

                historyActionId = (int)HistoryActions.OrderPrinted;
                Orders_History orderHistory = new Orders_History();
                if (historyActionId.HasValue) ordersHistoryRep.Create(orderHistory, historyActionId.Value);
            }

            return new ActionAsPdf("PrintOrderToScreen", new { password = PRINT_PASSWORD, id = id, companyId = CurrentUser.CompanyId, userId = CurrentUser.UserId, userRoles = CurrentUser.Roles, languageCode = CurrentUser.LanguageCode, coinSign = CurrentUser.CompanyCoinSign }) { FileName = String.Format("Order_{0}.pdf", order.OrderNumber) };
        }

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {

            OrdersRepository.ExeedingOrderData model = new OrdersRepository.ExeedingOrderData();

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            using (OrderToItemRepository orderItemsRep = new OrderToItemRepository())
            {
                StatusType minStatus = Authorized(RoleType.OrdersApprover) ? StatusType.ApprovedPendingInvoice : StatusType.Pending;
                model = ordersRep.GetOrderWithExeedingData(id, minStatus, "Orders_Statuses", "Supplier", "User", "Orders_OrderToItem.Orders_Items", "Orders_OrderToAllocation", "Orders_OrderToAllocation.Budgets_Allocations", "Budget", "User1");

                if (model == null) return Error(Loc.Dic.error_order_get_error);
                if (!Authorized(RoleType.OrdersViewer) && model.OriginalOrder.UserId != CurrentUser.UserId) return Error(Loc.Dic.error_no_permission);

                return View(model);
            }
        }

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);

            using (SuppliersRepository suppliersRep = new SuppliersRepository(CurrentUser.CompanyId))
            using (BudgetsRepository budgetsRep = new BudgetsRepository(CurrentUser.CompanyId))
            using (AllocationRepository allocationsRep = new AllocationRepository(CurrentUser.CompanyId))
            {
                List<Supplier> allSuppliers = suppliersRep.GetList().Where(x => x.ExternalId != null).OrderBy(s => s.Name).ToList();
                if (!allSuppliers.Any()) return Error(Loc.Dic.error_no_suppliers_for_order);

                ViewBag.SupplierId = new SelectList(allSuppliers, "Id", "Name");

                Budget activeBudget = budgetsRep.GetList().SingleOrDefault(x => x.IsActive);
                if (activeBudget == null) return Error(Loc.Dic.error_no_active_budget);

                ViewBag.Allocations = allocationsRep.GetUserAllocations(CurrentUser.UserId, activeBudget.Id);
                if (!((List<Budgets_Allocations>)ViewBag.Allocations).Any()) return Error(Loc.Dic.error_user_have_no_allocations);
                ViewBag.BudgetYear = activeBudget.Year;
                return View();
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(CreateOrderModel model)
        {
            /// Validating user input
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);
            if (!ModelState.IsValid) return Error(Loc.Dic.error_invalid_form);
            if (String.IsNullOrEmpty(model.ItemsString)) return Error(Loc.Dic.error_invalid_form);
            List<Orders_OrderToItem> ItemsList = ItemsFromString(model.ItemsString, 0);
            if (ItemsList == null || ItemsList.Count == 0) return Error(Loc.Dic.error_invalid_form);
            if (model.IsFutureOrder && !Authorized(RoleType.FutureOrderWriter)) return Error(Loc.Dic.Error_NoPermission);
            if (model.Allocations == null || model.Allocations.Where(x => x.IsActive).Count() == 0) return Error(Loc.Dic.error_invalid_form);
            model.Allocations = model.Allocations.Where(x => x.IsActive).ToList();
            decimal totalOrderPrice = (decimal.Floor(ItemsList.Sum(x => x.SingleItemPrice * x.Quantity) * 1000) / 1000);
            decimal totalAllocation = (decimal.Floor(model.Allocations.Sum(x => x.Amount) * 1000) / 1000);
            if (totalOrderPrice != totalAllocation) return Error(Loc.Dic.error_order_insufficient_allocation);

            // Initializing needed temporary variables
            bool allowExeeding = true;
            List<Orders_OrderToAllocation> AllocationsToCreate = new List<Orders_OrderToAllocation>();

            // Setting order properties
            model.Order.UserId = CurrentUser.UserId;
            model.Order.Price = totalOrderPrice;
            model.Order.IsFutureOrder = model.IsFutureOrder;

            using (AllocationRepository allocationsRep = new AllocationRepository(CurrentUser.CompanyId))
            using (BudgetsRepository budgetsRep = new BudgetsRepository(CurrentUser.CompanyId))
            using (ApprovalRoutesRepository routesRep = new ApprovalRoutesRepository(CurrentUser.CompanyId))
            {
                Budget currentBudget = budgetsRep.GetList().SingleOrDefault(x => x.IsActive);
                if (currentBudget == null) return Error(Loc.Dic.error_database_error);
                model.Order.BudgetId = currentBudget.Id;

                int[] orderAllocationsIds = model.Allocations.Select(x => x.AllocationId).Distinct().ToArray();
                var allocationsData = allocationsRep.GetAllocationsData(orderAllocationsIds, StatusType.Pending);
                bool IsValidAllocations =
                    (allocationsData.Count == orderAllocationsIds.Length) &&
                    model.Allocations.All(x => (x.MonthId == null || (x.MonthId >= 1 && x.MonthId <= 12)) && x.Amount > 0);
                if (!IsValidAllocations) return Error(Loc.Dic.error_invalid_form);

                if (model.IsFutureOrder)
                {
                    foreach (var allocationData in allocationsData)
                    {
                        List<OrderAllocation> allocationMonths = model.Allocations.Where(x => x.AllocationId == allocationData.AllocationId).ToList();

                        foreach (var month in allocationMonths)
                        {
                            if (month.MonthId == DateTime.Now.Month)
                            {
                                OrderAllocation currentAllocation = model.Allocations.SingleOrDefault(x => x.AllocationId == allocationData.AllocationId);

                                var newAllocations = allocationsRep.GenerateOrderAllocations(allocationData, currentAllocation.Amount, allowExeeding, DateTime.Now.Month);
                                if (newAllocations == null) return Error(Loc.Dic.error_order_insufficient_allocation);

                                AllocationsToCreate.AddRange(newAllocations);
                            }
                            else
                            {
                                var monthData = allocationData.Months.SingleOrDefault(x => x.MonthId == month.MonthId);
                                if (!allowExeeding && month.Amount > monthData.RemainingAmount) return Error(Loc.Dic.error_order_insufficient_allocation);

                                Orders_OrderToAllocation newAllocation = new Orders_OrderToAllocation()
                                {
                                    AllocationId = allocationData.AllocationId,
                                    MonthId = month.MonthId.Value,
                                    Amount = month.Amount,
                                    OrderId = 0
                                };

                                AllocationsToCreate.Add(newAllocation);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var allocationData in allocationsData)
                    {
                        OrderAllocation currentAllocation = model.Allocations.SingleOrDefault(x => x.AllocationId == allocationData.AllocationId);

                        var newAllocations = allocationsRep.GenerateOrderAllocations(allocationData, currentAllocation.Amount, allowExeeding, DateTime.Now.Month);
                        if (newAllocations == null) return Error(Loc.Dic.error_order_insufficient_allocation);

                        AllocationsToCreate.AddRange(newAllocations);
                    }
                }

                if (!CurrentUser.OrdersApprovalRouteId.HasValue)
                {
                    model.Order.NextOrderApproverId = null;
                    model.Order.StatusId = (int)StatusType.ApprovedPendingInvoice;
                }
                else
                {
                    Users_ApprovalRoutes orderRoute = routesRep.GetEntity(CurrentUser.OrdersApprovalRouteId.Value, "Users_ApprovalStep");
                    Users_ApprovalStep firstStep = orderRoute.Users_ApprovalStep.OrderBy(x => x.StepNumber).FirstOrDefault();

                    if (firstStep != null)
                    {
                        model.Order.ApprovalRouteId = orderRoute.Id;
                        model.Order.ApprovalRouteStep = firstStep.StepNumber;
                        model.Order.NextOrderApproverId = firstStep.UserId;
                        model.Order.StatusId = (int)StatusType.Pending;
                    }
                    else
                    {
                        model.Order.ApprovalRouteStep = null;
                        model.Order.NextOrderApproverId = null;
                        model.Order.StatusId = (int)StatusType.ApprovedPendingInvoice;
                    }
                }
            }

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            using (OrderToItemRepository orderToItemRep = new OrderToItemRepository())
            using (OrderToAllocationRepository orderAllocationsRep = new OrderToAllocationRepository())
            {
                bool creationError = false;
                if (!ordersRep.Create(model.Order)) return Error(Loc.Dic.error_orders_create_error);

                model.Order = ordersRep.GetEntity(model.Order.Id, "User", "User1");

                foreach (Orders_OrderToItem item in ItemsList)
                {
                    item.OrderId = model.Order.Id;

                    if (!orderToItemRep.Create(item))
                    {
                        creationError = true;
                        break;
                    }
                }

                foreach (var allocation in AllocationsToCreate)
                {
                    allocation.OrderId = model.Order.Id;

                    if (!orderAllocationsRep.Create(allocation))
                    {
                        creationError = true;
                        break;
                    }
                }

                if (creationError)
                {
                    ordersRep.Delete(model.Order.Id);
                    return Error(Loc.Dic.error_orders_create_error);
                }
            }

            using (OrdersHistoryRepository ordersHistoryRepository = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, model.Order.Id))
            {
                Orders_History orderHis = new Orders_History();
                ordersHistoryRepository.Create(orderHis, (int)HistoryActions.Created, model.NotesForApprover);
            }

            if (model.Order.NextOrderApproverId.HasValue)
            {
                SendNotifications.OrderPendingApproval(model.Order, Url);
            }

            return RedirectToAction("MyOrders");
        }

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);

            CreateOrderModel model = new CreateOrderModel();
            using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
            using (AllocationRepository allocationsRep = new AllocationRepository(CurrentUser.CompanyId))
            using (BudgetsRepository budgetsRep = new BudgetsRepository(CurrentUser.CompanyId))
            {
                model.Order = orderRep.GetEntity(id, "Supplier", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items", "Orders_OrderToAllocation.Budgets_Allocations");
                if (model.Order == null) return Error(Loc.Dic.error_order_not_found);
                if (model.Order.UserId != CurrentUser.UserId) return Error(Loc.Dic.error_no_permission);

                Budget activeBudget = budgetsRep.GetList().SingleOrDefault(x => x.CompanyId == CurrentUser.CompanyId && x.IsActive);
                if (activeBudget == null) return Error(Loc.Dic.error_no_active_budget);

                model.IsFutureOrder = model.Order.IsFutureOrder;

                List<Budgets_Allocations> userAllocations = allocationsRep.GetUserAllocations(CurrentUser.UserId, activeBudget.Id, id);
                if (!userAllocations.Any()) return Error(Loc.Dic.error_user_have_no_allocations);

                model.Allocations = new List<OrderAllocation>();
                List<Orders_OrderToAllocation> validOrderAllocations = model.Order.Orders_OrderToAllocation.ToList();

                var distinctAllocationIds = validOrderAllocations.Select(x => x.AllocationId).Distinct().ToList();
                foreach (var allocationId in distinctAllocationIds)
                {
                    var combineSplitted = validOrderAllocations.Where(x => x.MonthId <= model.Order.CreationDate.Month && x.AllocationId == allocationId);
                    model.Allocations.Add(
                        new OrderAllocation()
                            {
                                AllocationId = allocationId,
                                Name = validOrderAllocations.First(x => x.AllocationId == allocationId).Budgets_Allocations.DisplayName,
                                MonthId = model.Order.CreationDate.Month,
                                IsActive = true,
                                Amount = combineSplitted.Sum(x => x.Amount)
                            }
                    );

                    validOrderAllocations.RemoveAll(x => x.MonthId <= model.Order.CreationDate.Month && x.AllocationId == allocationId);
                }

                foreach (var allocation in validOrderAllocations)
                {
                    model.Allocations.Add(
                        new OrderAllocation()
                        {
                            AllocationId = allocation.AllocationId,
                            Name = allocation.Budgets_Allocations.Name,
                            MonthId = allocation.MonthId,
                            IsActive = true,
                            Amount = allocation.Amount
                        }
                    );
                }

                ViewBag.Allocations = userAllocations;

                int allAllocationsCount = model.Allocations.Count;
                model.Allocations = model.Allocations.Where(x => userAllocations.Any(a => a.Id == x.AllocationId)).ToList();
                int validAllocationsCount = model.Allocations.Count;

                ViewBag.ReAllocationRequired = allAllocationsCount > validAllocationsCount;
                ViewBag.BudgetYear = activeBudget.Year;
                ViewBag.ExistingItems = ItemsToString(model.Order.Orders_OrderToItem);
                return View(model);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(CreateOrderModel model, string itemsString)
        {
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);
            if (!ModelState.IsValid) return Error(Loc.Dic.error_invalid_form);

            // Initializing needed temporary variables
            bool wasReturnedToCreator;
            bool allowExeeding = true;
            List<Orders_OrderToAllocation> AllocationsToCreate = new List<Orders_OrderToAllocation>();

            Order orderFromDatabase;
            List<Orders_OrderToItem> itemsFromEditForm = new List<Orders_OrderToItem>();
            List<Orders_OrderToItem> itemsToDelete = new List<Orders_OrderToItem>();
            List<Orders_OrderToItem> itemsToCreate = new List<Orders_OrderToItem>();
            List<Orders_OrderToItem> itemsToUpdate = new List<Orders_OrderToItem>();

            decimal totalOrderPrice;
            decimal totalAllocation;
            List<Budgets_Allocations> orderAllocations = new List<Budgets_Allocations>();

            using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                orderFromDatabase = orderRep.GetEntity(model.Order.Id, "Supplier", "Orders_OrderToItem", "Orders_OrderToAllocation", "Users_ApprovalRoutes.Users_ApprovalStep");
            }

            if (orderFromDatabase == null) return Error(Loc.Dic.error_order_not_found);
            if (orderFromDatabase.UserId != CurrentUser.UserId) return Error(Loc.Dic.error_no_permission);

            if (orderFromDatabase.StatusId != (int)StatusType.Pending && orderFromDatabase.StatusId != (int)StatusType.PendingOrderCreator) return Error(Loc.Dic.error_order_edit_after_approval);
            wasReturnedToCreator = orderFromDatabase.StatusId == (int)StatusType.PendingOrderCreator;

            itemsFromEditForm = ItemsFromString(itemsString, model.Order.Id);
            if (itemsFromEditForm == null) return Error(Loc.Dic.error_invalid_form);
            if (itemsFromEditForm.Count == 0) return Error(Loc.Dic.error_order_has_no_items);
            if (itemsFromEditForm.Count == 0) return Error(Loc.Dic.error_order_has_no_items);

            totalOrderPrice = (decimal.Floor(itemsFromEditForm.Sum(x => x.SingleItemPrice * x.Quantity) * 1000) / 1000);

            if (model.Allocations == null || model.Allocations.Where(x => x.IsActive).Count() == 0) return Error(Loc.Dic.error_invalid_form);
            model.Allocations = model.Allocations.Where(x => x.IsActive).ToList();
            totalAllocation = (decimal.Floor(model.Allocations.Sum(x => x.Amount) * 1000) / 1000);

            if (totalOrderPrice != totalAllocation) return Error(Loc.Dic.error_order_insufficient_allocation);

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            using (AllocationRepository allocationsRep = new AllocationRepository(CurrentUser.CompanyId))
            using (BudgetsRepository budgetsRep = new BudgetsRepository(CurrentUser.CompanyId))
            {
                Budget currentBudget = budgetsRep.GetList().SingleOrDefault(x => x.CompanyId == CurrentUser.CompanyId && x.IsActive);
                if (currentBudget == null) return Error(Loc.Dic.error_database_error);
                model.Order.BudgetId = currentBudget.Id;

                int[] orderAllocationsIds = model.Allocations.Select(x => x.AllocationId).Distinct().ToArray();
                var allocationsData = allocationsRep.GetAllocationsData(orderAllocationsIds, StatusType.Pending);
                bool IsValidAllocations =
                    (allocationsData.Count == orderAllocationsIds.Length) &&
                    model.Allocations.All(x => (x.MonthId == null || (x.MonthId >= 1 && x.MonthId <= 12)) && x.Amount > 0);
                if (!IsValidAllocations) return Error(Loc.Dic.error_invalid_form);

                if (model.IsFutureOrder)
                {
                    foreach (var allocationData in allocationsData)
                    {
                        List<OrderAllocation> allocationMonths = model.Allocations.Where(x => x.AllocationId == allocationData.AllocationId).ToList();

                        foreach (var month in allocationMonths)
                        {
                            if (month.MonthId == DateTime.Now.Month)
                            {
                                OrderAllocation currentAllocation = model.Allocations.SingleOrDefault(x => x.AllocationId == allocationData.AllocationId);

                                var newAllocations = allocationsRep.GenerateOrderAllocations(allocationData, currentAllocation.Amount, allowExeeding, DateTime.Now.Month, orderFromDatabase.Id);
                                if (newAllocations == null) return Error(Loc.Dic.error_order_insufficient_allocation);

                                AllocationsToCreate.AddRange(newAllocations);
                            }
                            else
                            {
                                var monthData = allocationData.Months.SingleOrDefault(x => x.MonthId == month.MonthId);
                                if (!allowExeeding && month.Amount > monthData.RemainingAmount) return Error(Loc.Dic.error_order_insufficient_allocation);

                                Orders_OrderToAllocation newAllocation = new Orders_OrderToAllocation()
                                {
                                    AllocationId = allocationData.AllocationId,
                                    MonthId = month.MonthId.Value,
                                    Amount = month.Amount,
                                    OrderId = orderFromDatabase.Id
                                };

                                AllocationsToCreate.Add(newAllocation);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var allocationData in allocationsData)
                    {
                        OrderAllocation currentAllocation = model.Allocations.SingleOrDefault(x => x.AllocationId == allocationData.AllocationId);

                        var newAllocations = allocationsRep.GenerateOrderAllocations(allocationData, currentAllocation.Amount, allowExeeding, DateTime.Now.Month, orderFromDatabase.Id);
                        if (newAllocations == null) return Error(Loc.Dic.error_order_insufficient_allocation);

                        AllocationsToCreate.AddRange(newAllocations);
                    }
                }
            }

            using (OrderToAllocationRepository orderAllocationRep = new OrderToAllocationRepository())
            {
                foreach (var item in orderFromDatabase.Orders_OrderToAllocation)
                {
                    orderAllocationRep.Delete(item.Id);
                }

                foreach (var item in AllocationsToCreate)
                {
                    orderAllocationRep.Create(item);
                }
            }

            foreach (var newItem in itemsFromEditForm)
            {
                Orders_OrderToItem existingItem = orderFromDatabase.Orders_OrderToItem.SingleOrDefault(x => x.ItemId == newItem.ItemId);

                if (existingItem != null)
                {
                    if (
                        existingItem.Quantity != newItem.Quantity ||
                        existingItem.SingleItemPrice != newItem.SingleItemPrice
                        )
                    {
                        newItem.Id = existingItem.Id;
                        itemsToUpdate.Add(newItem);
                    }
                }
                else
                {
                    itemsToCreate.Add(newItem);
                }
            }

            foreach (var existingItem in orderFromDatabase.Orders_OrderToItem)
            {
                Orders_OrderToItem newItem = itemsFromEditForm.SingleOrDefault(x => x.ItemId == existingItem.ItemId);

                if (newItem == null)
                {
                    itemsToDelete.Add(existingItem);
                }
            }

            bool noErrors = true;

            using (OrderToItemRepository orderToItemRep = new OrderToItemRepository())
            {
                foreach (var item in itemsToCreate)
                {
                    if (!orderToItemRep.Create(item) && noErrors)
                        noErrors = false;
                }
                foreach (var item in itemsToUpdate)
                {
                    if (orderToItemRep.Update(item) == null && noErrors)
                        noErrors = false;
                }
                foreach (var item in itemsToDelete)
                {
                    if (!orderToItemRep.Delete(item.Id) && noErrors)
                        noErrors = false;
                }

                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    if (!CurrentUser.OrdersApprovalRouteId.HasValue)
                    {
                        model.Order.NextOrderApproverId = null;
                        model.Order.StatusId = (int)StatusType.ApprovedPendingInvoice;
                    }
                    else
                    {
                        Users_ApprovalRoutes orderRoute = orderFromDatabase.Users_ApprovalRoutes; //routesRep.GetEntity(CurrentUser.OrdersApprovalRouteId.Value, "Users_ApprovalStep");
                        Users_ApprovalStep firstStep = orderRoute.Users_ApprovalStep.OrderBy(x => x.StepNumber).FirstOrDefault();

                        if (firstStep != null)
                        {
                            model.Order.ApprovalRouteId = orderRoute.Id;
                            model.Order.ApprovalRouteStep = firstStep.StepNumber;
                            model.Order.NextOrderApproverId = firstStep.UserId;
                            model.Order.StatusId = (int)StatusType.Pending;
                        }
                        else
                        {
                            model.Order.ApprovalRouteId = null;
                            model.Order.ApprovalRouteStep = null;
                            model.Order.NextOrderApproverId = null;
                            model.Order.StatusId = (int)StatusType.ApprovedPendingInvoice;
                        }
                    }

                    model.Order.CompanyId = orderFromDatabase.CompanyId;
                    model.Order.CreationDate = orderFromDatabase.CreationDate;
                    model.Order.SupplierId = orderFromDatabase.SupplierId;
                    model.Order.UserId = orderFromDatabase.UserId;
                    model.Order.OrderNumber = orderFromDatabase.OrderNumber;
                    model.Order.InvoiceNumber = orderFromDatabase.InvoiceNumber;
                    model.Order.InvoiceDate = orderFromDatabase.InvoiceDate;
                    model.Order.LastStatusChangeDate = DateTime.Now;
                    model.Order.ValueDate = orderFromDatabase.ValueDate;
                    model.Order.WasAddedToInventory = orderFromDatabase.WasAddedToInventory;
                    model.Order.IsFutureOrder = model.IsFutureOrder;

                    model.Order.Price = totalOrderPrice;

                    ordersRep.Update(model.Order);

                    model.Order = ordersRep.GetEntity(model.Order.Id, "User", "User1");
                }
            }

            if (noErrors)
            {
                int? historyActionId = null;
                historyActionId = (int)HistoryActions.Edited;
                Orders_History orderHistory = new Orders_History();
                using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, model.Order.Id))
                    if (historyActionId.HasValue) ordersHistoryRep.Create(orderHistory, historyActionId.Value, model.NotesForApprover);

                if (wasReturnedToCreator)
                {
                    if (model.Order.NextOrderApproverId.HasValue)
                    {
                        SendNotifications.OrderPendingApproval(model.Order, Url);
                    }
                }
                
                return RedirectToAction("MyOrders");
            }
            else
                return Error(Loc.Dic.error_order_update_items_error);
        }

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);

            OrdersRepository.ExeedingOrderData model = new OrdersRepository.ExeedingOrderData();
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                StatusType minStatus = Authorized(RoleType.OrdersApprover) ? StatusType.ApprovedPendingInvoice : StatusType.Pending;
                model = ordersRep.GetOrderWithExeedingData(id, minStatus, "Orders_Statuses", "Supplier", "User", "Orders_OrderToItem.Orders_Items", "Orders_OrderToAllocation", "Orders_OrderToAllocation.Budgets_Allocations", "Budget", "User1");
            }

            if (model == null) return Error(Loc.Dic.error_order_not_found);
            if (model.OriginalOrder.UserId != CurrentUser.UserId) return Error(Loc.Dic.error_no_permission);
            if (model.OriginalOrder.StatusId != (int)StatusType.Pending && model.OriginalOrder.StatusId != (int)StatusType.PendingOrderCreator)
                return Error(Loc.Dic.error_order_delete_after_approval);

            ViewBag.OrderId = model.OriginalOrder.Id;
            return View(model);
        }

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id = 0)
        {
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);

            Order order;
            using (OrderToItemRepository orderToItemRep = new OrderToItemRepository())
            using (OrderToAllocationRepository orderToAllocationRep = new OrderToAllocationRepository())
            using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = orderRep.GetEntity(id);

                if (order == null) return Error(Loc.Dic.error_order_not_found);
                if (order.UserId != CurrentUser.UserId) return Error(Loc.Dic.error_no_permission);
                if (order.StatusId != (int)StatusType.Pending && order.StatusId != (int)StatusType.PendingOrderCreator)
                    return Error(Loc.Dic.error_order_delete_after_approval);

                if (!orderRep.Delete(order.Id)) return Error(Loc.Dic.error_orders_delete_error);
            }

            return RedirectToAction("MyOrders");
        }

        [OpenIdAuthorize]
        public ActionResult PendingInventory(int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.InventoryManager)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Order> orders;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                orders = ordersRep.GetList("Orders_Statuses", "Supplier", "User").Where(x => !x.WasAddedToInventory && x.StatusId > (int)StatusType.InvoiceScannedPendingOrderCreator);

                if (orders == null) return Error(Loc.Dic.error_orders_get_error);

                orders = Pagination(orders, page, sortby, order);

                return View(orders.ToList());
            }
        }

        [OpenIdAuthorize]
        [ChildActionOnly]
        public ActionResult PartialAddToInventory(AddToInventoryModel model)
        {
            return PartialView(model);
        }

        [OpenIdAuthorize]
        public ActionResult AddToInventory(int id = 0)
        {
            if (!Authorized(RoleType.InventoryManager)) return Error(Loc.Dic.error_no_permission);

            Order order;
            List<Location> locations = null;
            AddToInventoryModel model = new AddToInventoryModel();

            using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
            using (LocationsRepository locationsRep = new LocationsRepository(CurrentUser.CompanyId))
            {
                order = orderRep.GetEntity(id, "Supplier", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");
                if (order == null) return Error(Loc.Dic.error_order_not_found);
                if (order.StatusId < (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport) return Error(Loc.Dic.error_order_not_approved);

                locations = locationsRep.GetList().OrderBy(x => x.Name).ToList();
                if (locations == null || locations.Count == 0) return Error(Loc.Dic.error_no_locations_found);
            }

            model.OrderId = order.Id;
            model.OrderItems = order.Orders_OrderToItem.ToList();
            model.LocationsList = new SelectList(locations, "Id", "Name");
            return View(model);
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult AddToInventory(AddToInventoryModel model)
        {
            if (!Authorized(RoleType.InventoryManager)) return Error(Loc.Dic.error_no_permission);

            Order order;
            List<Inventory> createdItems = new List<Inventory>();
            List<Location> locations;
            bool noCreationErrors = true;

            using (InventoryRepository inventoryRep = new InventoryRepository(CurrentUser.CompanyId))
            using (LocationsRepository locationsRep = new LocationsRepository(CurrentUser.CompanyId))
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(model.OrderId, "Supplier", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");

                if (order == null) return Error(Loc.Dic.error_order_get_error);
                if (order.WasAddedToInventory) return Error(Loc.Dic.error_order_was_added_to_inventory);
                if (order.StatusId < (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport) return Error(Loc.Dic.error_invoice_not_scanned_and_approved);

                locations = locationsRep.GetList().ToList();
                if (locations == null || locations.Count == 0) return Error(Loc.Dic.error_no_locations_found);

                foreach (SplittedInventoryItem splitedItem in model.InventoryItems)
                {
                    if (!noCreationErrors) break;
                    if (!splitedItem.AddToInventory) continue;

                    int? itemId = splitedItem.ItemsToAdd[0].ItemId;
                    Orders_OrderToItem originalItem = order.Orders_OrderToItem.FirstOrDefault(x => x.Id == itemId);
                    bool isValidList = originalItem != null && splitedItem.ItemsToAdd.All(x => x.ItemId == itemId);

                    if (!isValidList) { noCreationErrors = false; break; }

                    if (splitedItem.ItemsToAdd.Count == 1)
                    {
                        Inventory listItem = splitedItem.ItemsToAdd[0];
                        if (!locations.Any(x => x.Id == listItem.LocationId)) return Error(Loc.Dic.error_invalid_form);

                        Inventory newItem = new Inventory()
                        {
                            AssignedTo = listItem.AssignedTo,
                            LocationId = listItem.LocationId,
                            Notes = listItem.Notes,
                            SerialNumber = listItem.SerialNumber,
                            Status = listItem.Status,
                            WarrentyPeriodStart = listItem.WarrentyPeriodStart,
                            WarrentyPeriodEnd = listItem.WarrentyPeriodEnd,
                            ItemId = originalItem.ItemId,
                            OrderId = order.Id,
                            CompanyId = CurrentUser.CompanyId,
                            IsOutOfInventory = false,
                            OriginalQuantity = originalItem.Quantity,
                            RemainingQuantity = originalItem.Quantity
                        };

                        if (!inventoryRep.Create(newItem)) { noCreationErrors = false; break; }
                        createdItems.Add(newItem);
                    }
                    else if (originalItem.Quantity == splitedItem.ItemsToAdd.Count)
                    {
                        foreach (var item in splitedItem.ItemsToAdd)
                        {
                            if (!locations.Any(x => x.Id == item.LocationId)) { noCreationErrors = false; break; }

                            item.ItemId = originalItem.ItemId;
                            item.OrderId = order.Id;
                            item.CompanyId = CurrentUser.CompanyId;
                            item.IsOutOfInventory = false;

                            if (!inventoryRep.Create(item)) { noCreationErrors = false; break; }
                            createdItems.Add(item);
                        }
                    }
                    else { noCreationErrors = false; break; }
                }

                if (!noCreationErrors)
                {
                    foreach (var item in createdItems)
                    {
                        inventoryRep.Delete(item.Id);
                    }

                    return Error(Loc.Dic.error_inventory_create_error);
                }

                order.WasAddedToInventory = true;
                order.LastStatusChangeDate = DateTime.Now;
                if (ordersRep.Update(order) == null) return Error(Loc.Dic.error_database_error);

                bool hasInventoryItems = model.InventoryItems.Any(x => x.AddToInventory);
                string notes = hasInventoryItems ? Loc.Dic.AddToInventory_with_inventory_items : Loc.Dic.AddToInventory_no_inventory_items;

                int? historyActionId = null;
                historyActionId = (int)HistoryActions.AddedToInventory;
                Orders_History orderHistory = new Orders_History();
                using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, order.Id))
                    if (historyActionId.HasValue) ordersHistoryRep.Create(orderHistory, historyActionId.Value, notes);

                return RedirectToAction("PendingInventory");
            }
        }

        [OpenIdAuthorize]
        public ActionResult OrdersToExport(bool isRecovery = false, int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);

            IEnumerable<Order> ordersToExport;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                ordersToExport = ordersRep.GetList("Orders_Statuses", "Supplier", "User");

                if (isRecovery)
                {
                    ordersToExport = ordersToExport.Where(x => x.StatusId > (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport);
                    ViewBag.isRecovery = true;
                }
                else
                {
                    ordersToExport = ordersToExport.Where(x => x.StatusId == (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport);
                    ViewBag.isRecovery = false;
                }

                ordersToExport = ordersToExport.ToList();

                if (ordersToExport == null) return Error(Loc.Dic.error_order_get_error);

                ordersToExport = Pagination(ordersToExport, page, sortby, order);
            }

            return View(ordersToExport.ToList());
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult OrdersToExport(List<int> selectedOrder = null)
        {
            if (!Authorized(RoleType.SystemManager)) return Error(Loc.Dic.error_no_permission);
            if (selectedOrder == null || selectedOrder.Count == 0) return Error(Loc.Dic.error_no_selected_orders);

            StringBuilder builder = new StringBuilder();

            List<Order> ordersToExport = new List<Order>();
            Company userCompany;

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            using (CompaniesRepository companiesRep = new CompaniesRepository())
            {
                ordersToExport = ordersRep.GetList("Orders_Statuses", "Supplier", "User")
                    .Where(x => selectedOrder.Contains(x.Id))
                    .ToList();

                userCompany = companiesRep.GetEntity(CurrentUser.CompanyId);

                if (ordersToExport == null) return Error(Loc.Dic.error_database_error);
                if (userCompany == null) return Error(Loc.Dic.error_database_error);

                if (String.IsNullOrEmpty(userCompany.ExternalCoinCode) || String.IsNullOrEmpty(userCompany.ExternalExpenseCode))
                    return Error(Loc.Dic.error_insufficient_company_info_for_export);

                int numberOfOrders = 0;
                builder.AppendLine(numberOfOrders.ToString().PadRight(180));

                foreach (var order in ordersToExport)
                {
                    decimal orderPrice;

                    if (order.Price.HasValue)
                    {
                        if (order.Price.Value > 999999999) return Error(String.Format("({0}: {1}) {2}", Loc.Dic.Order, order.OrderNumber, Loc.Dic.error_order_price_too_high));

                        orderPrice = order.Price.Value;
                    }
                    else
                    {
                        orderPrice = 0;
                    }

                    if (String.IsNullOrEmpty(order.Supplier.ExternalId))
                        return Error(String.Format("({0}: {1}) {2}", Loc.Dic.Supplier, order.Supplier.Name, Loc.Dic.error_insufficient_supplier_info_for_export));

                    if (String.IsNullOrEmpty(order.InvoiceNumber) || order.InvoiceDate == null)
                        return Error(String.Format("({0}: {1}) {2}", Loc.Dic.Order, order.OrderNumber, Loc.Dic.error_insufficient_order_info_for_export));

                    List<Orders_OrderToAllocation> orderAllocations = order.Orders_OrderToAllocation.ToList();
                    List<Budgets_Allocations> distinctOrderAllocations = orderAllocations.Select(x => x.Budgets_Allocations).Distinct().ToList();

                    if (!orderAllocations.Any()) return Error(String.Format("({0}: {1}) {2}", Loc.Dic.Order, order.OrderNumber, Loc.Dic.error_order_has_no_allocations));

                    foreach (var allocation in distinctOrderAllocations)
                    {
                        if (String.IsNullOrEmpty(allocation.ExternalId)) return Error(String.Format("({0}: {1}) {2}", Loc.Dic.BudgetAllocation, allocation.DisplayName, Loc.Dic.error_insufficient_allocation_info_for_export));

                        decimal allocationSum = orderAllocations.Where(x => x.AllocationId == allocation.Id).Sum(a => a.Amount);

                        builder.AppendLine(
                            String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}",
                            String.Empty.PadLeft(3),
                            order.InvoiceNumber.Substring(Math.Max(0, order.InvoiceNumber.Length - 5)).PadLeft(5),
                            order.InvoiceDate.Value.ToString("ddMMyy"),
                            String.Empty.PadLeft(5),
                            order.ValueDate.Value.ToString("ddMMyy"),
                            userCompany.ExternalCoinCode.PadLeft(3),
                            String.Empty.PadLeft(22),
                            allocation.ExternalId.ToString().PadLeft(8),
                            String.Empty.PadLeft(8),
                            String.Empty.PadLeft(8), //order.Supplier.ExternalId.ToString().PadLeft(8),
                            String.Empty.PadLeft(8),
                            allocationSum.ToString("0.00").PadLeft(12),
                            String.Empty.PadLeft(12),
                            String.Empty.PadLeft(12),
                            String.Empty.PadLeft(12),
                            String.Empty.PadLeft(12),
                            String.Empty.PadLeft(12),
                            String.Empty.PadLeft(12),
                            String.Empty.PadLeft(12)
                            )
                        );
                    }

                    builder.AppendLine(
                        String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}",
                        String.Empty.PadLeft(3),
                        order.InvoiceNumber.PadLeft(5),
                        order.InvoiceDate.Value.ToString("ddMMyy"),
                        String.Empty.PadLeft(5),
                        order.ValueDate.Value.ToString("ddMMyy"),
                        userCompany.ExternalCoinCode.PadLeft(3),
                        String.Empty.PadLeft(22),
                        String.Empty.PadLeft(8),
                        String.Empty.PadLeft(8),
                        order.Supplier.ExternalId.ToString().PadLeft(8),
                        String.Empty.PadLeft(8),
                        String.Empty.PadLeft(12),
                        String.Empty.PadLeft(12),
                        orderPrice.ToString("0.00").PadLeft(12),
                        String.Empty.PadLeft(12),
                        String.Empty.PadLeft(12),
                        String.Empty.PadLeft(12),
                        String.Empty.PadLeft(12),
                        String.Empty.PadLeft(12)
                        )
                    );

                    order.StatusId = (int)StatusType.InvoiceExportedToFile;
                    if (ordersRep.Update(order) == null) return Error(Loc.Dic.error_database_error);

                    int? historyActionId = null;
                    historyActionId = (int)HistoryActions.ExportedToFile;
                    Orders_History orderHistory = new Orders_History();
                    using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, order.Id))
                        if (historyActionId.HasValue) ordersHistoryRep.Create(orderHistory, historyActionId.Value);
                }

                //FileStream fileStream = new FileStream(//SystemFile.("", FileMode.Open());
                byte[] fileBytes = Encoding.UTF8.GetBytes(builder.ToString());
                string fileName = "MOVEIN.DAT";
                //Stream stream = new MemoryStream(fileBytes);

                SendNotifications.OrdersExported(CurrentUser, Url, ordersToExport.Count, fileBytes);

                Response.AppendHeader("Refresh", "1");
                Response.AppendHeader("Location", Url.Action("OrdersToExport", "Orders", null, "http"));

                return File(fileBytes, "text/plain", fileName);
            }
        }

        [OpenIdAuthorize]
        public ActionResult Search()
        {
            if (!Authorized(RoleType.OrdersWriter) && !Authorized(RoleType.OrdersViewer)) return Error(Loc.Dic.error_no_permission);
            if (!Authorized(RoleType.OrdersViewer)) ViewBag.UserId = CurrentUser.UserId;

            return View();
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Search(OrdersSearchValuesModel model)
        {
            if (!Authorized(RoleType.OrdersWriter) && !Authorized(RoleType.OrdersViewer)) return Error(Loc.Dic.error_no_permission);

            List<Order> matchingOrders;
            List<Order> TextMatchOrders = new List<Order>();

            ViewBag.UserId = model.UserId;
            ViewBag.StatusId = model.StatusId;
            ViewBag.SupplierId = model.SupplierId;
            ViewBag.HideUserField = model.HideUserField;
            ViewBag.HideStatusField = model.HideStatusField;
            ViewBag.HideSupplierField = model.HideSupplierField;

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            using (SuppliersRepository suppliersRep = new SuppliersRepository(CurrentUser.CompanyId))
            {
                IQueryable<Order> ordersQuery;

                ordersQuery = ordersRep.GetList("Company", "Orders_Statuses", "Supplier", "User").Where(x => x.CompanyId == CurrentUser.CompanyId);

                if (Authorized(RoleType.OrdersViewer))
                {
                    if (model.UserId.HasValue && model.UserId.Value != -1)
                        ordersQuery = ordersQuery.Where(x => x.UserId == model.UserId.Value);
                }
                else
                {
                    ordersQuery = ordersQuery.Where(x => x.UserId == CurrentUser.UserId);
                    ViewBag.UserId = CurrentUser.UserId;
                }

                if (model.BudgetId.HasValue && model.BudgetId.Value != -1)
                    ordersQuery = ordersQuery.Where(x => x.BudgetId == model.BudgetId.Value);

                if (model.OrderNumber.HasValue && model.OrderNumber.Value != -1)
                    ordersQuery = ordersQuery.Where(x => x.OrderNumber == model.OrderNumber.Value);

                if (model.SupplierId.HasValue && model.SupplierId.Value != -1)
                    ordersQuery = ordersQuery.Where(x => x.SupplierId == model.SupplierId.Value);

                if (model.StatusId.HasValue && model.StatusId.Value != -1)
                    ordersQuery = ordersQuery.Where(x => x.StatusId == model.StatusId.Value);

                if (model.AllocationId != null && model.AllocationId != "-1")
                    ordersQuery = ordersQuery.Where(x => x.Orders_OrderToAllocation.Any(oa => oa.Budgets_Allocations.ExternalId == model.AllocationId));

                if (model.PriceMin.HasValue && model.PriceMax.HasValue && model.PriceMax.Value < model.PriceMin.Value)
                    model.PriceMax = null;

                if (model.PriceMin.HasValue)
                    ordersQuery = ordersQuery.Where(x => x.Price >= model.PriceMin.Value);

                if (model.PriceMax.HasValue)
                    ordersQuery = ordersQuery.Where(x => x.Price <= model.PriceMax.Value);

                if (model.CreationMin.HasValue && model.CreationMax.HasValue && model.CreationMax.Value < model.CreationMin.Value)
                    model.CreationMax = null;

                if (model.CreationMin.HasValue)
                    ordersQuery = ordersQuery.Where(x => x.CreationDate >= model.CreationMin.Value);

                if (model.CreationMax.HasValue)
                    ordersQuery = ordersQuery.Where(x => x.CreationDate <= model.CreationMax.Value);

                matchingOrders = ordersQuery.ToList();
            }

            if (!String.IsNullOrEmpty(model.NoteText))
            {
                List<string> searchWords = model.NoteText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Trim()).ToList();
                foreach (var order in matchingOrders)
                {
                    foreach (var word in searchWords)
                    {
                        if (!String.IsNullOrEmpty(order.NotesForSupplier) && order.NotesForSupplier.Contains(word))
                        {
                            TextMatchOrders.Add(order);
                            break;
                        }
                    }
                }

                model.Matches = TextMatchOrders;
            }
            else
            {
                model.Matches = matchingOrders;
            }

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult List(IEnumerable<Order> orders, string baseUrl, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false, bool showUserName = true)
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

            if (CurrentUser == null)
            {
                ViewBag.CurrentUserId = 0;
                ViewBag.UserRoles = RoleType.None;
            }
            else
            {
                ViewBag.CurrentUserId = CurrentUser.UserId;
                ViewBag.UserRoles = (RoleType)CurrentUser.Roles;
            }

            return PartialView(orders);
        }

        [ChildActionOnly]
        public ActionResult SimpleList(IEnumerable<Order> orders, string baseUrl)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = false;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = null;
            ViewBag.Order = null;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 1;

            ViewBag.IsCheckBoxed = false;
            ViewBag.ShowUserName = true;

            ViewBag.UserRoles = (RoleType)CurrentUser.Roles;

            return PartialView("List", orders);
        }

        [ChildActionOnly]
        public ActionResult SelectorList(IEnumerable<Order> orders, string baseUrl)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = true;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = DEFAULT_SORT;
            ViewBag.Order = DEFAULT_DESC_ORDER;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 1;

            ViewBag.IsCheckBoxed = true;
            ViewBag.ShowUserName = true;

            ViewBag.UserRoles = (RoleType)CurrentUser.Roles;

            return PartialView("List", orders);
        }

        [ChildActionOnly]
        public ActionResult ListOrderItems(IEnumerable<Orders_OrderToItem> orderItems, string baseUrl, bool showCoinSign = false, string coinSign = null)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = false;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = null;
            ViewBag.Order = null;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 1;

            ViewBag.IsCheckBoxed = false;
            ViewBag.ShowUserName = true;
            ViewBag.ShowCoinSign = showCoinSign;
            ViewBag.CompanyCoinSign = coinSign ?? CurrentUser.CompanyCoinSign;

            return PartialView("ListOrderItems", orderItems);
        }

        [ChildActionOnly]
        public ActionResult ListOrderAllocations(IEnumerable<Orders_OrderToAllocation> orderAllocations, int budgetYear, bool isFutureOrder, string baseUrl, OrdersRepository.ExeedingOrderData exeedingData = null)
        {
            ViewBag.BaseUrl = baseUrl;
            ViewBag.IsOrdered = false;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = null;
            ViewBag.Order = null;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 1;

            ViewBag.IsCheckBoxed = false;
            ViewBag.ShowUserName = true;
            ViewBag.IsFutureOrder = isFutureOrder;
            ViewBag.BudgetYear = budgetYear;

            ViewBag.ExeedingData = exeedingData;

            return PartialView(orderAllocations);
        }

        [ChildActionOnly]
        public ActionResult PartialDetails(OrdersRepository.ExeedingOrderData model)
        {
            List<Orders_History> orderHistoryList = new List<Orders_History>();
            using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, model.OriginalOrder.Id))
                orderHistoryList = ordersHistoryRep.GetList("User", "Orders_History_Actions").OrderByDescending(x => x.CreationDate).ToList();
            
            ViewBag.orderHistoryList = orderHistoryList;
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult SearchForm(OrdersSearchValuesModel model, bool isExpanding, bool isCollapsed, int? userId = null, int? statusId = null, int? supplierId = null, bool hideUserField = false, bool hideStatusField = false, bool hideSupplierField = false)
        {
            if (model == null) model = new OrdersSearchValuesModel();

            using (UsersRepository usersRep = new UsersRepository(CurrentUser.CompanyId))
            using (BudgetsRepository budgetsRep = new BudgetsRepository(CurrentUser.CompanyId))
            using (SuppliersRepository suppliersRep = new SuppliersRepository(CurrentUser.CompanyId))
            using (OrderStatusesRepository statusesRep = new OrderStatusesRepository())
            using (AllocationRepository allocationsRep = new AllocationRepository(CurrentUser.CompanyId))
            {
                List<SelectListItemDB> usersAsSelectItems = new List<SelectListItemDB>() { new SelectListItemDB() { Id = -1, Name = Loc.Dic.AllUsersOption } };
                usersAsSelectItems.AddRange(usersRep.GetList().Select(x => new SelectListItemDB() { Id = x.Id, Name = x.FirstName + " " + x.LastName }));
                model.UsersList = new SelectList(usersAsSelectItems, "Id", "Name");

                List<SelectListItemDB> budgetsAsSelectItems = new List<SelectListItemDB>() { new SelectListItemDB() { Id = -1, Name = Loc.Dic.AllBudgetsOption } };
                budgetsAsSelectItems.AddRange(budgetsRep.GetList().AsEnumerable().Select(x => new SelectListItemDB() { Id = x.Id, Name = "(" + x.Year + ") " + x.Name }));
                model.BudgetsList = new SelectList(budgetsAsSelectItems, "Id", "Name");

                List<Supplier> suppliersSelectList = new List<Supplier>() { new Supplier() { Id = -1, Name = Loc.Dic.AllSuppliersOption } };
                suppliersSelectList.AddRange(suppliersRep.GetList().OrderByDescending(x => x.Name).ToList());
                model.SuppliersList = new SelectList(suppliersSelectList, "Id", "Name");

                List<Orders_Statuses> statusesSelectList = new List<Orders_Statuses>() { new Orders_Statuses() { Id = -1, Name = Loc.Dic.AllStatusesOption } };
                statusesSelectList.AddRange(statusesRep.GetList().ToList());
                model.StatusesList = new SelectList(statusesSelectList, "Id", "Name");

                List<SelectListStringItem> allocationsSelectList = new List<SelectListStringItem>() { new SelectListStringItem() { Id = "-1", Name = Loc.Dic.AllAllocationsOption } };
                allocationsSelectList.AddRange(allocationsRep.GetList().GroupBy(x => x.ExternalId).AsEnumerable().Select(x => new SelectListStringItem() { Id = x.First().ExternalId, Name = x.First().DisplayName }).ToList());
                model.AllocationsList = new SelectList(allocationsSelectList, "Id", "Name");
            }

            ViewBag.IsExpanding = isExpanding;
            ViewBag.IsCollapsed = isCollapsed;
            ViewBag.UserId = userId;
            ViewBag.StatusId = statusId;
            ViewBag.SupplierId = supplierId;
            ViewBag.HideUserField = hideUserField;
            ViewBag.HideStatusField = hideStatusField;
            ViewBag.HideSupplierField = hideSupplierField;
            return PartialView(model);
        }

        [OpenIdAuthorize]
        public ActionResult InvoiceApproval(int id = 0)
        {
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);

            Order order;
            using (OrdersRepository ordersRepository = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRepository.GetEntity(id);
            }

            if (order == null) return Error(Loc.Dic.error_order_not_found);
            if (order.UserId != CurrentUser.UserId) return Error(Loc.Dic.error_no_permission);

            ViewBag.orderId = id;
            return View();
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult InvoiceApproval(string selectedStatus, int orderId = 0)
        {
            if (!Authorized(RoleType.OrdersWriter)) return Error(Loc.Dic.error_no_permission);

            Order order;
            int? historyActionId = null;
            using (OrdersRepository ordersRepository = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRepository.GetEntity(orderId);

                if (order == null) return Error(Loc.Dic.error_order_not_found);
                if (order.UserId != CurrentUser.UserId) return Error(Loc.Dic.error_no_permission);

                if (selectedStatus == Loc.Dic.ApproveInvoce)
                {
                    order.StatusId = (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport;
                    order.LastStatusChangeDate = DateTime.Now;
                    historyActionId = (int)HistoryActions.InvoiceApproved;
                }
                if (selectedStatus == Loc.Dic.CancelOrder)
                {
                    order.StatusId = (int)StatusType.OrderCancelled;
                    order.LastStatusChangeDate = DateTime.Now;
                    historyActionId = (int)HistoryActions.Canceled;
                }

                Orders_History orderHistory = new Orders_History();
                using (OrdersHistoryRepository ordersHistoryRep = new OrdersHistoryRepository(CurrentUser.CompanyId, CurrentUser.UserId, order.Id))
                    if (historyActionId.HasValue) ordersHistoryRep.Create(orderHistory, historyActionId.Value);

                if (ordersRepository.Update(order) == null) return Error(Loc.Dic.error_database_error);
            }

            return RedirectToAction("MyOrders");
        }

        private IEnumerable<Order> Pagination(IEnumerable<Order> orders, int page = FIRST_PAGE, string sortby = DEFAULT_SORT, string order = DEFAULT_DESC_ORDER, bool showAll = false)
        {
            int numberOfItems = orders.Count();
            int numberOfPages = numberOfItems / ITEMS_PER_PAGE;
            if (numberOfItems % ITEMS_PER_PAGE != 0)
                numberOfPages++;

            if (page <= 0)
                page = FIRST_PAGE;
            if (page > numberOfPages)
                page = numberOfPages;

            if (sortby != NO_SORT_BY)
            {
                Func<Func<Order, dynamic>, IEnumerable<Order>> orderFunction;

                if (order == DEFAULT_DESC_ORDER)
                    orderFunction = x => orders.OrderByDescending(x);
                else
                    orderFunction = x => orders.OrderBy(x);

                switch (sortby)
                {
                    case "number":
                        orders = orderFunction(x => x.OrderNumber);
                        break;
                    case "creation":
                        orders = orderFunction(x => x.CreationDate);
                        break;
                    case "supplier":
                        orders = orderFunction(x => x.Supplier.Name);
                        break;
                    case "status":
                        orders = orderFunction(x => x.StatusId);
                        break;
                    case "price":
                        orders = orderFunction(x => x.Price);
                        break;
                    case "lastChange":
                        orders = orderFunction(x => x.LastStatusChangeDate);
                        break;
                    case "username":
                    default:
                        orders = orderFunction(x => x.User.FirstName + " " + x.User.LastName);
                        break;
                }
            }

            if (!showAll)
            {
                orders = orders
                    .Skip((page - 1) * ITEMS_PER_PAGE)
                    .Take(ITEMS_PER_PAGE)
                    .ToList();
            }

            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = page;
            ViewBag.NumberOfPages = numberOfPages;

            return orders;
        }

        private List<Orders_OrderToItem> ItemsFromString(string itemsString, int orderId)
        {
            List<Orders_OrderToItem> items = new List<Orders_OrderToItem>();

            string[] splitItems = itemsString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in splitItems)
            {
                int itemId = 0;
                decimal quantity = 0;
                decimal singleItemPrice = 0;

                string[] itemValues = item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (itemValues.Length != 3 ||
                    !int.TryParse(itemValues[0], out itemId) ||
                    !decimal.TryParse(itemValues[1], out quantity) ||
                    !decimal.TryParse(itemValues[2], out singleItemPrice))
                {
                    return null;
                }

                items.Add(
                    new Orders_OrderToItem()
                    {
                        OrderId = orderId,
                        ItemId = itemId,
                        Quantity = quantity,
                        SingleItemPrice = singleItemPrice
                    }
                );
            }

            return items;
        }

        private string ItemsToString(IEnumerable<Orders_OrderToItem> orderItems)
        {
            string existingItems = String.Empty;

            foreach (var item in orderItems)
            {
                existingItems += String.Format("{0},{1},{2},{3};", item.ItemId, item.Orders_Items.Title, item.Quantity, item.SingleItemPrice);
            }

            if (!String.IsNullOrEmpty(existingItems)) existingItems = existingItems.Remove(existingItems.Length - 1);

            return existingItems;
        }

        private UploadedFile GetUniqueFile(string folderName, int uniqueId)
        {
            UploadedFile file = new UploadedFile();

            string directoryPath = Server.MapPath("~/App_Data/Uploads/" + folderName);
            string PrefixPattern = String.Format("{0}_{1}*", CurrentUser.CompanyId, uniqueId);

            DirectoryInfo myDir = new DirectoryInfo(directoryPath);
            var files = myDir.GetFiles(PrefixPattern);

            if (files.Length == 0) return null;
            file.FullPath = files[0].FullName;

            string[] parts = file.FullPath.Split('.');
            if (!parts.Any()) return null;
            file.Extension = parts.Last();

            FileStream stream = System.IO.File.OpenRead(file.FullPath);
            file.Content = new byte[stream.Length];
            stream.Read(file.Content, 0, Convert.ToInt32(stream.Length));
            stream.Close();

            return file;
        }

        private void SaveUniqueFile(HttpPostedFileBase file, string folderName, int uniqueId)
        {
            string directoryPath = Server.MapPath("~/App_Data/Uploads/" + folderName);
            string PrefixPattern = String.Format("{0}_{1}.*", CurrentUser.CompanyId, uniqueId);

            DirectoryInfo myDir = new DirectoryInfo(directoryPath);
            var existingFiles = myDir.GetFiles(PrefixPattern);
            foreach (var existingFile in existingFiles)
            {
                existingFile.Delete();
            }

            string[] parts = file.FileName.Split('.');

            var fileName = String.Format("{0}_{1}.{2}", CurrentUser.CompanyId, uniqueId, parts.Last());
            var fullFilePath = Path.Combine(directoryPath, fileName);
            file.SaveAs(fullFilePath);
            file.InputStream.Close();
        }
    }
}