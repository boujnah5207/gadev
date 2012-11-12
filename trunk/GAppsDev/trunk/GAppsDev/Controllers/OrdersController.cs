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
using GAppsDev.OpenIdService;
using Mvc4.OpenId.Sample.Security;
using GAppsDev.Models;
using Rotativa;
using BaseLibraries;
using System.IO;
using GAppsDev.Models.Search;
using System.Globalization;
using System.Text;

namespace GAppsDev.Controllers
{
    public class OrdersController : BaseController
    {
        const int ITEMS_PER_PAGE = 10;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string DEFAULT_DESC_ORDER = "DESC";

        private Entities db = new Entities();

        [OpenIdAuthorize]
        public ActionResult Home()
        {
            return View();
        }

        //
        // GET: /Orders/

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = NO_SORT_BY, string order = DEFAULT_DESC_ORDER)
        {
            if (!Authorized(RoleType.OrdersViewer))
                return Error(Errors.NO_PERMISSION);

                IEnumerable<Order> orders;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                orders = ordersRep.GetList("Orders_Statuses", "Supplier", "User");

                    if (orders != null)
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
                            case "username":
                            default:
                                orders = orderFunction(x => x.User.FirstName + " " + x.User.LastName);
                                break;
                            }
                        }

                        orders = orders
                            .Skip((page - 1) * ITEMS_PER_PAGE)
                            .Take(ITEMS_PER_PAGE)
                            .ToList();

                        ViewBag.Sortby = sortby;
                        ViewBag.Order = order;
                        ViewBag.CurrPage = page;
                        ViewBag.NumberOfPages = numberOfPages;

                        return View(orders.ToList());
                    }
                    else
                    {
                        return Error(Errors.ORDERS_GET_ERROR);
                    }
                }
            }

        //
        // GET: /Orders/Details/5

        [OpenIdAuthorize]
        public ActionResult MyOrders(int page = FIRST_PAGE, string sortby = NO_SORT_BY, string order = DEFAULT_DESC_ORDER)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                IEnumerable<Order> orders;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    orders = ordersRep.GetList("Orders_Statuses", "Supplier", "User").Where(x => x.UserId == CurrentUser.UserId);

                    if (orders != null)
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
                            }
                        }

                        orders = orders
                            .Skip((page - 1) * ITEMS_PER_PAGE)
                            .Take(ITEMS_PER_PAGE)
                            .ToList();

                        ViewBag.Sortby = sortby;
                        ViewBag.Order = order;
                        ViewBag.CurrPage = page;
                        ViewBag.NumberOfPages = numberOfPages;

                        ViewBag.UserId = CurrentUser.UserId;
                        return View(orders.ToList());

                    }
                    else
                    {
                        return Error(Errors.ORDERS_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult PendingOrders(int page = FIRST_PAGE, string sortby = NO_SORT_BY, string order = DEFAULT_DESC_ORDER)
        {
            if (Authorized(RoleType.OrdersApprover))
            {
                IEnumerable<Order> orders;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    orders = ordersRep.GetList("Orders_Statuses", "Supplier", "User")
                        .Where(x =>
                            x.CompanyId == CurrentUser.CompanyId &&
                            x.NextOrderApproverId == CurrentUser.UserId &&
                            x.StatusId == (int)StatusType.Pending
                            );

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
                            orderFunction = x => orders.OrderBy(x);
                        else
                            orderFunction = x => orders.OrderByDescending(x);

                        switch (sortby)
                        {
                            case "username":
                            default:
                                orders = orderFunction(x => x.User.FirstName + " " + x.User.LastName);
                                break;
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
                        }
                    }

                    orders = orders
                        .Skip((page - 1) * ITEMS_PER_PAGE)
                        .Take(ITEMS_PER_PAGE)
                        .ToList();

                    ViewBag.Sortby = sortby;
                    ViewBag.Order = order;
                    ViewBag.CurrPage = page;
                    ViewBag.NumberOfPages = numberOfPages;

                    return View(orders.ToList());
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult ModifyStatus(int id = 0)
        {
            if (!Authorized(RoleType.OrdersApprover))
                return Error(Errors.NO_PERMISSION);

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                OrderModel orderModel = new OrderModel();
                    orderModel.Order = ordersRep.GetEntity(id);

                if (orderModel.Order == null)
                    return Error(Errors.ORDER_GET_ERROR);

                if (orderModel.Order.CompanyId != CurrentUser.CompanyId || orderModel.Order.NextOrderApproverId != CurrentUser.UserId)
                    return Error(Errors.NO_PERMISSION);

                            orderModel.OrderToItem = orderModel.Order.Orders_OrderToItem.ToList();

                if (orderModel.OrderToItem == null)
                    return Error(Errors.DATABASE_ERROR);

                                return View(orderModel);
                            }
                }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult ModifyStatus(OrderModel modifiedOrder, string selectedStatus)
        {
            if (Authorized(RoleType.OrdersApprover))
            {
                Budgets_ExpensesToIncomes budgetAllocation;
                decimal? totalUsedAllocation;

                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                using (BudgetsExpensesToIncomesRepository allocationsRep = new BudgetsExpensesToIncomesRepository())
                {
                    Order orderFromDB = ordersRep.GetEntity(modifiedOrder.Order.Id);

                    if (orderFromDB != null)
                    {
                        if (orderFromDB.CompanyId == CurrentUser.CompanyId && orderFromDB.NextOrderApproverId == CurrentUser.UserId)
                        {
                            if (selectedStatus == Loc.Dic.ApproveOrder)
                            {
                                budgetAllocation = allocationsRep.GetEntity(orderFromDB.BudgetAllocationId.Value);

                                if (budgetAllocation != null)
                                {
                                    totalUsedAllocation = ordersRep.GetList()
                                            .Where(o => o.BudgetAllocationId == orderFromDB.BudgetAllocationId && o.StatusId >= (int)StatusType.ApprovedPendingInvoice)
                                            .Sum(x => x.Price);

                                    if ((totalUsedAllocation ?? 0) + orderFromDB.Price <= budgetAllocation.Amount)
                                    {
                                        if (CurrentUser.OrdersApproverId.HasValue)
                                        {
                                            orderFromDB.NextOrderApproverId = CurrentUser.OrdersApproverId.Value;
                                            orderFromDB.StatusId = (int)StatusType.PartiallyApproved;
                                        }
                                        else
                                        {
                                            orderFromDB.StatusId = (int)StatusType.ApprovedPendingInvoice;
                                            orderFromDB.NextOrderApproverId = null;
                                        }
                                    }
                                    else
                                    {
                                        return Error(Errors.ORDER_INSUFFICIENT_ALLOCATION);
                                    }
                                }
                                else
                                {
                                    return Error(Errors.ALLOCATIONS_GET_ERROR);
                                }
                            }
                            else if (selectedStatus == Loc.Dic.DeclineOrder)
                            {
                                orderFromDB.StatusId = (int)StatusType.Declined;
                            }
                            else if (selectedStatus == Loc.Dic.SendBackToUser)
                            {
                                orderFromDB.StatusId = (int)StatusType.PendingOrderCreator;
                            }

                            orderFromDB.OrderApproverNotes = modifiedOrder.Order.OrderApproverNotes;
                            if (ordersRep.Update(orderFromDB) != null)
                            {
                                EmailMethods emailMethods = new EmailMethods("NOREPLY@pqdev.com", "מערכת הזמנות", "noreply50100200");
                                emailMethods.sendGoogleEmail(orderFromDB.User.Email, orderFromDB.User.FirstName, "עדכון סטטוס הזמנה", "סטטוס הזמנה מספר " + orderFromDB.Id + " שונה ל " + selectedStatus + "Http://gappsdev.pqdev.com/Orders/MyOrders");

                                return RedirectToAction("PendingOrders");
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
                    else
                    {
                        return Error(Errors.ORDER_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult UploadInvoiceFile(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Order order;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    order = ordersRep.GetEntity(id);
                }

                if (order != null)
                {
                    if (order.CompanyId == CurrentUser.CompanyId)
                    {
                        if (order.StatusId == (int)StatusType.ApprovedPendingInvoice)
                        {
                            ViewBag.OrderID = id;
                            return View();
                        }
                        else if (order.StatusId < (int)StatusType.ApprovedPendingInvoice)
                        {
                            return Error(Errors.ORDER_NOT_APPROVED);
                        }
                        else
                        {
                            return Error(Errors.ORDER_ALREADY_HAS_INVOICE);
                        }
                    }
                    else
                    {
                        return Error(Errors.ORDER_GET_ERROR);
                    }
                }
                else
                {
                    return Error(Errors.ORDER_NOT_FOUND);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        // This action handles the form POST and the upload
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult UploadInvoiceFile(HttpPostedFileBase file, AddToInventoryModel model)
        {
            if (Authorized(RoleType.SystemManager))
            {
                if (file != null && file.ContentLength > 0)
                {
                    Order order;
                    List<Inventory> createdItems = new List<Inventory>();
                    List<Location> locations;
                    bool noCreationErrors = true;

                    using (InventoryRepository inventoryRep = new InventoryRepository())
                    using (LocationsRepository locationsRep = new LocationsRepository())
                    using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                    {
                        order = ordersRep.GetEntity(model.OrderId, "Supplier", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");

                        if (order != null)
                        {
                            if (order.CompanyId == CurrentUser.CompanyId)
                            {
                                if (order.StatusId == (int)StatusType.ApprovedPendingInvoice)
                                {
                                    locations = locationsRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();

                                    if (locations != null)
                                    {
                                        foreach (SplittedInventoryItem splitedItem in model.InventoryItems)
                                        {
                                            if (!noCreationErrors)
                                                break;

                                            if (!splitedItem.AddToInventory)
                                                continue;

                                            int itemId = splitedItem.ItemsToAdd[0].ItemId;
                                            Orders_OrderToItem originalItem = order.Orders_OrderToItem.FirstOrDefault(x => x.Id == itemId);
                                            bool isValidList = originalItem != null && splitedItem.ItemsToAdd.All(x => x.ItemId == itemId);

                                            if (isValidList)
                                            {
                                                if (splitedItem.ItemsToAdd.Count == 1)
                                                {
                                                    Inventory listItem = splitedItem.ItemsToAdd[0];

                                                    if (locations.Any(x => x.Id == listItem.LocationId))
                                                    {
                                                        for (int i = 0; i < originalItem.Quantity; i++)
                                                        {
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
                                                            };

                                                            if (inventoryRep.Create(newItem))
                                                            {
                                                                createdItems.Add(newItem);
                                                            }
                                                            else
                                                            {
                                                                noCreationErrors = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (
                                                    originalItem.Quantity == splitedItem.ItemsToAdd.Count
                                                    )
                                                {
                                                    foreach (var item in splitedItem.ItemsToAdd)
                                                    {
                                                        if (locations.Any(x => x.Id == item.LocationId))
                                                        {
                                                            item.ItemId = originalItem.ItemId;
                                                            item.OrderId = order.Id;
                                                            item.CompanyId = CurrentUser.CompanyId;
                                                            item.IsOutOfInventory = false;

                                                            if (inventoryRep.Create(item))
                                                            {
                                                                createdItems.Add(item);
                                                            }
                                                            else
                                                            {
                                                                noCreationErrors = false;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            noCreationErrors = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    noCreationErrors = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                noCreationErrors = false;
                                                break;
                                            }
                                        }

                                        if (noCreationErrors)
                                        {
                                            var fileName = CurrentUser.CompanyId.ToString() + "_" + model.OrderId.ToString() + ".pdf";
                                            var path = Path.Combine(Server.MapPath("~/App_Data/Uploads/Invoices"), fileName);
                                            file.SaveAs(path);

                                            order.StatusId = (int)StatusType.InvoiceScannedPendingOrderCreator;
                                            ordersRep.Update(order);

                                            return RedirectToAction("Index");
                                        }
                                        else
                                        {
                                            foreach (var item in createdItems)
                                            {
                                                inventoryRep.Delete(item.Id);
                                            }

                                            return Error(Errors.INVENTORY_CREATE_ERROR);
                                        }
                                    }
                                    else
                                    {
                                        return Error(Errors.DATABASE_ERROR);
                                    }
                                }
                                else if (order.StatusId < (int)StatusType.ApprovedPendingInvoice)
                                {
                                    return Error(Errors.ORDER_NOT_APPROVED);
                                }
                                else
                                {
                                    return Error(Errors.ORDER_ALREADY_HAS_INVOICE);
                                }
                            }
                            else
                            {
                                return Error(Errors.NO_PERMISSION);
                            }
                        }
                        else
                        {
                            return Error(Errors.ORDER_GET_ERROR);
                        }
                    }
                }
                else
                {
                    return Error(Errors.INVALID_FORM);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult UploadReceiptFile(int id = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Order order;
                using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    order = orderRep.GetEntity(id);

                    if (order != null)
                    {
                        if (order.CompanyId == CurrentUser.CompanyId)
                        {
                            if (order.StatusId == (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                            {
                                ViewBag.OrderId = id;
                                return View();
                            }
                            else if (order.StatusId < (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                            {
                                return Error(Loc.Dic.Error_InvoiceNotApproved);
                            }
                            else //if (order.StatusId > (int)StatusType.InvoiceApprovedByOrderCreatorPendingReceipt)
                            {
                                return Error(Loc.Dic.Error_OrderAlreadyHasReceipt);
                            }
                        }
                        else
                        {
                            return Error(Loc.Dic.Error_NoPermission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.Error_OrderNotFound);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.Error_NoPermission);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult UploadReceiptFile(HttpPostedFileBase file, int orderId = 0)
        {
            if (Authorized(RoleType.SystemManager))
            {
                Order order;
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    order = ordersRep.GetEntity(orderId);

                    if (order != null)
                    {
                        if (order.CompanyId == CurrentUser.CompanyId)
                        {
                            if (order.StatusId == (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                            {
                                order.StatusId = (int)StatusType.ReceiptScanned;

                                if (ordersRep.Update(order) != null)
                                    return RedirectToAction("Index");
                                else
                                    return Error(Loc.Dic.Error_DatabaseError);
                            }
                            else if (order.StatusId < (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                            {
                                return Error(Loc.Dic.Error_InvoiceNotApproved);
                            }
                            else //if (order.StatusId > (int)StatusType.InvoiceApprovedByOrderCreatorPendingReceipt)
                            {
                                return Error(Loc.Dic.Error_OrderAlreadyHasReceipt);
                            }
                        }
                        else
                        {
                            return Error(Loc.Dic.Error_NoPermission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.Error_OrderNotFound);
                    }
                }
            }
            else
            {
                return Error(Loc.Dic.Error_NoPermission);
            }
        }

        [OpenIdAuthorize]
        public ActionResult DownloadInvoice(int id = 0)
        {
            Order order;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id);

                if (order != null)
                {
                    if (order.CompanyId == CurrentUser.CompanyId)
                    {
                        if (Authorized(RoleType.OrdersViewer) || order.UserId == CurrentUser.UserId)
                        {
                            if (order.StatusId >= (int)StatusType.InvoiceScannedPendingOrderCreator)
                            {
                                string fileName = CurrentUser.CompanyId.ToString() + "_" + id.ToString() + ".pdf";
                                string path = Path.Combine(Server.MapPath("~/App_Data/Uploads/Invoices"), fileName);

                                if (System.IO.File.Exists(path))
                                {
                                    FileStream stream = System.IO.File.OpenRead(path);

                                    byte[] contents = new byte[stream.Length];
                                    stream.Read(contents, 0, Convert.ToInt32(stream.Length));
                                    stream.Close();

                                    Response.AddHeader("Content-Disposition", "inline; filename=test.pdf");
                                    return File(contents, "application/pdf");
                                }
                                else
                                {
                                    return Error(Loc.Dic.Error_InvoiceFileNotFound);
                                }
                            }
                            else
                            {
                                return Error(Loc.Dic.Error_InvoiceNotScanned);
                            }
                        }
                        else
                        {
                            return Error(Loc.Dic.Error_NoPermission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.Error_NoPermission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.Error_OrderNotFound);
                }
            }
        }

        [OpenIdAuthorize]
        public ActionResult DownloadReceipt(int id = 0)
        {
            Order order;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id);

                if (order != null)
                {
                    if (order.CompanyId == CurrentUser.CompanyId)
                    {
                        if (Authorized(RoleType.OrdersViewer) || order.UserId == CurrentUser.UserId)
                        {
                            if (order.StatusId >= (int)StatusType.InvoiceScannedPendingOrderCreator)
                            {
                                string fileName = CurrentUser.CompanyId.ToString() + "_" + id.ToString() + ".pdf";
                                string path = Path.Combine(Server.MapPath("~/App_Data/Uploads/Receipts"), fileName);

                                if (System.IO.File.Exists(path))
                                {
                                    FileStream stream = System.IO.File.OpenRead(path);
                                    byte[] contents = new byte[stream.Length];
                                    stream.Read(contents, 0, Convert.ToInt32(stream.Length));
                                    stream.Close();

                                    Response.AddHeader("Content-Disposition", "inline; filename=test.pdf");
                                    return File(contents, "application/pdf");
                                }
                                else
                                {
                                    return Error(Loc.Dic.Error_ReceiptFileNotFound);
                                }
                            }
                            else
                            {
                                return Error(Loc.Dic.Error_InvoiceNotScanned);
                            }
                        }
                        else
                        {
                            return Error(Loc.Dic.Error_NoPermission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.Error_NoPermission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.Error_OrderNotFound);
                }
            }
        }

        //[OpenIdAuthorize]
        public ActionResult PrintOrderToScreen(int id = 0, string languageCode = "he")
        {
            CultureInfo ci = new CultureInfo(languageCode);
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture =
            CultureInfo.CreateSpecificCulture(ci.Name);

            PrintOrderModel model = new PrintOrderModel();

            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            using (OrderToItemRepository orderItemsRep = new OrderToItemRepository())
            {
                model.Order = ordersRep.GetEntity(id, "User", "Company", "Supplier");
                model.Items = orderItemsRep.GetList("Orders_Items").Where(x => x.OrderId == id).ToList();
            }

            if (model.Order == null)
            {
                return HttpNotFound();
            }

            ViewBag.LanguageCode = languageCode;
            return View(model);
        }

        [OpenIdAuthorize]
        public ActionResult DownloadOrderAsPdf(int id = 0)
        {
            string cookieName = OpenIdMembershipService.LOGIN_COOKIE_NAME;
            HttpCookie cookie = Request.Cookies[cookieName];
            Dictionary<string, string> cookies = new Dictionary<string, string>();
            cookies.Add(cookieName, cookie.Value);

            Order order = db.Orders.Single(o => o.Id == id);
            return new ActionAsPdf("PrintOrderToScreen", new { id = id, languageCode = CurrentUser.LanguageCode }) { FileName = "Invoice.pdf" };
        }

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            Order order;
            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
            {
                order = ordersRep.GetEntity(id, "Orders_Statuses", "Supplier", "User");

                if (order != null)
                {
                    if (order.CompanyId == CurrentUser.CompanyId)
                    {
                        if (Authorized(RoleType.OrdersViewer) || order.UserId == CurrentUser.UserId)
                        {
                            OrderModel orderModel = new OrderModel()
                            {
                                Order = order,
                                OrderToItem = order.Orders_OrderToItem.ToList()
                            };

                            return View(orderModel);
                        }
                        else
                        {
                            return Error(Loc.Dic.Error_NoPermission);
                        }
                    }
                    else
                    {
                        return Error(Loc.Dic.Error_NoPermission);
                    }
                }
                else
                {
                    return Error(Loc.Dic.Error_OrderNotFound);
                }
            }
        }

        //
        // GET: /Orders/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                using (SuppliersRepository suppliersRep = new SuppliersRepository())
                using (BudgetsUsersToPermissionsRepository budgetsUsersToPermissionsRepository = new BudgetsUsersToPermissionsRepository())
                using (BudgetsPermissionsToAllocationRepository budgetsPermissionsToAllocationRepository = new BudgetsPermissionsToAllocationRepository())
                {
                    ViewBag.SupplierId = new SelectList(suppliersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList(), "Id", "Name");

                    List<SelectListItemDB> allocationsSelectList = new List<SelectListItemDB>();
                    List<Budgets_ExpensesToIncomes> allocations = new List<Budgets_ExpensesToIncomes>();
                    List<Budgets_UsersToPermissions> permissions = budgetsUsersToPermissionsRepository.GetList().Where(x => x.UserId == CurrentUser.UserId).ToList();

                    foreach (var permission in permissions)
                    {
                        allocations.AddRange(
                            budgetsPermissionsToAllocationRepository.GetList()
                                .Where(x => x.BudgetsPermissionsId == permission.Budgets_Permissions.Id)
                                .Select(x => x.Budgets_ExpensesToIncomes)
                                .ToList()
                                );
                    }

                    allocationsSelectList = allocations
                        .Distinct()
                        .Select(a => new { Id = a.Id, Name = a.Amount + ": " + a.Budgets_Incomes.CustomName + "-->" + a.Budgets_Expenses.CustomName })
                        .AsEnumerable()
                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() })
                        .ToList();

                    ViewBag.BudgetAllocationId = new SelectList(allocationsSelectList, "Id", "Name");
                }

                return View();
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Orders/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Order order, string itemsString)
        {
            if (ModelState.IsValid)
            {
                if (Authorized(RoleType.OrdersWriter))
                {
                    List<Orders_OrderToItem> ItemsList = ItemsFromString(itemsString, 0);
                    decimal totalOrderPrice;
                    decimal? totalUsedAllocation;

                    Budgets_ExpensesToIncomes budgetAllocation;

                    order.UserId = CurrentUser.UserId;
                    order.CompanyId = CurrentUser.CompanyId;
                    order.CreationDate = DateTime.Now;
                    order.StatusId = (int)StatusType.Pending;
                    order.OrderApproverNotes = String.Empty;
                    order.Price = ItemsList.Sum(item => item.SingleItemPrice * item.Quantity);
                    order.NextOrderApproverId = CurrentUser.OrdersApproverId;

                    if (ItemsList != null || !order.BudgetAllocationId.HasValue)
                    {
                        if (ItemsList.Count > 0)
                            totalOrderPrice = ItemsList.Sum(x => x.SingleItemPrice * x.Quantity);
                        else
                            return Error(Errors.ORDER_HAS_NO_ITEMS);

                        bool wasOrderCreated;
                        using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                        using (BudgetsExpensesToIncomesRepository allocationsRep = new BudgetsExpensesToIncomesRepository())
                        {
                            budgetAllocation = allocationsRep.GetEntity(order.BudgetAllocationId.Value);

                            if (budgetAllocation != null)
                            {
                                totalUsedAllocation = ordersRep.GetList()
                                        .Where(o => o.BudgetAllocationId == order.BudgetAllocationId && o.StatusId >= (int)StatusType.ApprovedPendingInvoice)
                                        .Sum(x => (decimal?)x.Price);

                                if ((totalUsedAllocation ?? 0) + totalOrderPrice <= budgetAllocation.Amount)
                                {
                                    int? lastOrderNumber = ordersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Select(x => (int?)x.OrderNumber).Max();

                                    if (lastOrderNumber.HasValue)
                                        order.OrderNumber = lastOrderNumber.Value + 1;
                                    else
                                        order.OrderNumber = 1;

                                    wasOrderCreated = ordersRep.Create(order);
                                }
                                else
                                {
                                    return Error(Errors.ORDER_INSUFFICIENT_ALLOCATION);
                                }
                            }
                            else
                            {
                                return Error(Errors.ALLOCATIONS_GET_ERROR);
                            }
                        }

                        if (wasOrderCreated)
                        {
                            foreach (var item in ItemsList)
                            {
                                item.OrderId = order.Id;
                            }

                            bool noItemErrors = true;
                            List<Orders_OrderToItem> createdItems = new List<Orders_OrderToItem>();
                            using (OrderToItemRepository orderToItemRep = new OrderToItemRepository())
                            {
                                foreach (Orders_OrderToItem item in ItemsList)
                                {
                                    if (orderToItemRep.Create(item))
                                        createdItems.Add(item);
                                    else
                                    {
                                        noItemErrors = false;
                                        break;
                                    }
                                }
                            }

                            if (noItemErrors)
                            {
                                return RedirectToAction("MyOrders");
                            }
                            else
                            {
                                using (OrderToItemRepository orderToItemRep = new OrderToItemRepository())
                                {
                                    foreach (Orders_OrderToItem item in createdItems)
                                    {
                                        orderToItemRep.Delete(item.Id);
                                    }
                                }

                                using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                                {
                                    orderRep.Delete(order.Id);
                                }

                                return Error(Errors.ORDERS_CREATE_ERROR);
                            }
                        }
                        else
                        {
                            return Error(Errors.ORDERS_CREATE_ERROR);
                        }
                    }
                    else
                    {
                        return Error(Errors.INVALID_FORM);
                    }
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
        // GET: /Orders/Edit/5

        [OpenIdAuthorize]
        public ActionResult Edit(int id = 0)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                Order order;
                using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                using (BudgetsUsersToPermissionsRepository budgetsUsersToPermissionsRepository = new BudgetsUsersToPermissionsRepository())
                using (BudgetsPermissionsToAllocationRepository budgetsPermissionsToAllocationRepository = new BudgetsPermissionsToAllocationRepository())
                {
                    order = orderRep.GetEntity(id, "Supplier", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");

                    List<SelectListItemDB> allocationsSelectList = new List<SelectListItemDB>();
                    List<Budgets_ExpensesToIncomes> allocations = new List<Budgets_ExpensesToIncomes>();
                    List<Budgets_UsersToPermissions> permissions = budgetsUsersToPermissionsRepository.GetList().Where(x => x.UserId == CurrentUser.UserId).ToList();

                    foreach (var permission in permissions)
                    {
                        allocations.AddRange(
                            budgetsPermissionsToAllocationRepository.GetList()
                                .Where(x => x.BudgetsPermissionsId == permission.Budgets_Permissions.Id)
                                .Select(x => x.Budgets_ExpensesToIncomes)
                                .ToList()
                                );
                    }

                    allocationsSelectList = allocations
                        .Distinct()
                        .Select(a => new { Id = a.Id, Name = a.Amount + ": " + a.Budgets_Incomes.CustomName + "-->" + a.Budgets_Expenses.CustomName })
                        .AsEnumerable()
                        .Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() })
                        .ToList();

                    ViewBag.BudgetAllocationId = new SelectList(allocationsSelectList, "Id", "Name");
                }

                if (order.UserId == CurrentUser.UserId)
                {
                    if (order != null)
                    {
                        string existingItems = "";
                        foreach (var item in order.Orders_OrderToItem)
                        {
                            existingItems += String.Format("{0},{1},{2},{3};", item.ItemId, item.Orders_Items.Title, item.Quantity, item.SingleItemPrice);
                        }

                        if (!String.IsNullOrEmpty(existingItems))
                            existingItems = existingItems.Remove(existingItems.Length - 1);

                        ViewBag.ExistingItems = existingItems;

                        return View(order);
                    }
                    else
                    {
                        return Error(Errors.ORDER_NOT_FOUND);
                    }
                }
                else
                {
                    return Error(Errors.NO_PERMISSION);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Orders/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Order order, string itemsString)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                if (ModelState.IsValid)
                {
                    Order orderFromDatabase;
                    List<Orders_OrderToItem> itemsFromEditForm = new List<Orders_OrderToItem>();
                    List<Orders_OrderToItem> itemsToDelete = new List<Orders_OrderToItem>();
                    List<Orders_OrderToItem> itemsToCreate = new List<Orders_OrderToItem>();
                    List<Orders_OrderToItem> itemsToUpdate = new List<Orders_OrderToItem>();

                    using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                    {
                        orderFromDatabase = orderRep.GetEntity(order.Id, "Supplier", "Orders_OrderToItem");
                    }

                    if (orderFromDatabase != null)
                    {
                        if (orderFromDatabase.UserId == CurrentUser.UserId)
                        {
                            if (orderFromDatabase.StatusId == (int)StatusType.Pending || orderFromDatabase.Id == (int)StatusType.PendingOrderCreator)
                            {
                                itemsFromEditForm = ItemsFromString(itemsString, order.Id);
                                if (itemsFromEditForm != null)
                                {
                                    if (itemsFromEditForm.Count == 0)
                                        return Error(Errors.ORDER_HAS_NO_ITEMS);

                                    decimal? totalUsedAllocation;
                                    decimal totalOrderPrice = itemsFromEditForm.Sum(x => x.SingleItemPrice * x.Quantity);
                                    Budgets_ExpensesToIncomes budgetAllocation;

                                    using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                                    using (BudgetsExpensesToIncomesRepository allocationsRep = new BudgetsExpensesToIncomesRepository())
                                    {
                                        budgetAllocation = allocationsRep.GetEntity(order.BudgetAllocationId.Value);

                                        if (budgetAllocation != null)
                                        {
                                            totalUsedAllocation = ordersRep.GetList()
                                                    .Where(o => o.BudgetAllocationId == order.BudgetAllocationId && o.StatusId >= (int)StatusType.ApprovedPendingInvoice)
                                                    .Sum(x => (decimal?)x.Price);

                                            if ((totalUsedAllocation ?? 0) + totalOrderPrice > budgetAllocation.Amount)
                                                return Error(Errors.ORDER_INSUFFICIENT_ALLOCATION);
                                        }
                                        else
                                        {
                                            return Error(Errors.DATABASE_ERROR);
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

                                        if (order.Notes != orderFromDatabase.Notes || order.BudgetAllocationId != orderFromDatabase.BudgetAllocationId)
                                        {
                                            using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                                            {
                                                order.CompanyId = orderFromDatabase.CompanyId;
                                                order.CreationDate = orderFromDatabase.CreationDate;
                                                order.StatusId = orderFromDatabase.StatusId;
                                                order.SupplierId = orderFromDatabase.SupplierId;
                                                order.UserId = orderFromDatabase.UserId;

                                                order.Price = ordersRep.GetEntity(order.Id).Orders_OrderToItem.Sum(item => item.SingleItemPrice * item.Quantity);

                                                ordersRep.Update(order);
                                            }
                                        }
                                    }

                                    if (noErrors)
                                        return RedirectToAction("MyOrders");
                                    else
                                        return Error(Errors.ORDER_UPDATE_ITEMS_ERROR);
                                }
                                else
                                {
                                    return Error(Errors.INVALID_FORM);
                                }
                            }
                            else
                            {
                                return Error(Errors.ORDER_EDIT_AFTER_APPROVAL);
                            }
                        }
                        else
                        {
                            return Error(Errors.NO_PERMISSION);
                        }
                    }
                    else
                    {
                        return Error(Errors.ORDER_NOT_FOUND);
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
        // GET: /Orders/Delete/5

        [OpenIdAuthorize]
        public ActionResult Delete(int id = 0)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                OrderModel model = new OrderModel();

                using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    model.Order = orderRep.GetEntity(id, "Supplier", "Company", "User", "Orders_Statuses", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");
                    model.OrderToItem = model.Order.Orders_OrderToItem.ToList();
                }

                if (model.Order != null)
                {
                    if (model.Order.UserId == CurrentUser.UserId)
                    {
                        if (model.Order.StatusId == (int)StatusType.Pending || model.Order.StatusId == (int)StatusType.PendingOrderCreator)
                        {
                            return View(model);
                        }
                        else
                        {
                            return Error(Errors.ORDER_DELETE_AFTER_APPROVAL);
                        }
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.ORDER_NOT_FOUND);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        //
        // POST: /Orders/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Authorized(RoleType.OrdersWriter))
            {
                Order order;
                using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    order = orderRep.GetEntity(id, "Supplier", "Orders_OrderToItem");
                }

                if (order != null)
                {
                    if (order.UserId == CurrentUser.UserId)
                    {
                        if (order.StatusId == (int)StatusType.Pending || order.StatusId == (int)StatusType.PendingOrderCreator)
                        {
                            bool noItemErrors = true;
                            using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                            using (OrderToItemRepository orderToItemRep = new OrderToItemRepository())
                            {
                                foreach (var item in order.Orders_OrderToItem)
                                {
                                    if (!orderToItemRep.Delete(item.Id))
                                        noItemErrors = false;
                                }

                                if (noItemErrors)
                                {
                                    if (orderRep.Delete(order.Id))
                                    {
                                        return RedirectToAction("MyOrders");
                                    }
                                    else
                                    {
                                        return Error(Errors.ORDERS_DELETE_ERROR);
                                    }
                                }
                                else
                                {
                                    return Error(Errors.ORDERS_DELETE_ITEMS_ERROR);
                                }
                            }
                        }
                        else
                        {
                            return Error(Errors.ORDER_DELETE_AFTER_APPROVAL);
                        }
                    }
                    else
                    {
                        return Error(Errors.NO_PERMISSION);
                    }
                }
                else
                {
                    return Error(Errors.ORDER_NOT_FOUND);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        [ChildActionOnly]
        public ActionResult AddToInventory(int id = 0)
        {
            if (Authorized(RoleType.OrdersApprover))
            {
                AddToInventoryModel model = new AddToInventoryModel();
                Order order;
                using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    order = orderRep.GetEntity(id, "Supplier", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");
                }

                if (order != null)
                {
                    if (order.CompanyId == CurrentUser.CompanyId)
                    {
                        List<Location> locations = null;
                        using (LocationsRepository locationsRep = new LocationsRepository())
                        using (OrderToItemRepository orderToItemRep = new OrderToItemRepository())
                        {
                            locations = locationsRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();
                            model.OrderItems = orderToItemRep.GetList("Orders_Items").Where(x => x.OrderId == order.Id).ToList();
                        }

                        if (model.OrderItems != null && locations != null)
                        {
                            model.OrderId = order.Id;
                            model.LocationsList = new SelectList(locations, "Id", "Name");
                            return PartialView(model);
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
                else
                {
                    return Error(Errors.ORDER_NOT_FOUND);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult AddToInventory(AddToInventoryModel model)
        {
            /*
            if (Authorized(RoleType.OrdersApprover))
            {
                List<Inventory> createdItems = new List<Inventory>();
                bool noCreationErrors = true;

                Order order;
                List<Location> locations;

                using (OrdersRepository orderRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    order = orderRep.GetEntity(model.OrderId, "Supplier", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");
                }

                if (order != null)
                {
                    if (order.CompanyId == CurrentUser.CompanyId)
                    {
                        using (InventoryRepository inventoryRep = new InventoryRepository())
                        using (LocationsRepository locationsRep = new LocationsRepository())
                        {
                            locations = locationsRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList();

                            if (locations != null)
                            {
                                if (model.InventoryItems.Count > 0 && model.InventoryItems.All(x => x.ItemsToAdd.Count > 0))
                                {
                                    foreach (SplittedInventoryItem splitedItem in model.InventoryItems)
                                    {
                                        if (!noCreationErrors)
                                            break;

                                        int itemId = splitedItem.ItemsToAdd[0].ItemId;
                                        Orders_OrderToItem originalItem = order.Orders_OrderToItem.FirstOrDefault(x => x.Id == itemId);
                                        bool isValidForm = originalItem != null && splitedItem.ItemsToAdd.All(x => x.ItemId == itemId);

                                        if (isValidForm)
                                        {
                                            if (splitedItem.ItemsToAdd.Count == 1)
                                            {
                                                if (locations.Any(x => x.Id == splitedItem.ItemsToAdd[0].LocationId))
                                                {
                                                    for (int i = 0; i < originalItem.Quantity; i++)
                                                    {
                                                        Inventory item = new Inventory()
                                                        {
                                                            AssignedTo = splitedItem[0].AssignedTo,
                                                            LocationId = splitedItem[0].LocationId,
                                                            Notes = splitedItem[0].Notes,
                                                            SerialNumber = splitedItem[0].SerialNumber,
                                                            Status = splitedItem[0].Status,
                                                            WarrentyPeriodStart = splitedItem[0].WarrentyPeriodStart,
                                                            WarrentyPeriodEnd = splitedItem[0].WarrentyPeriodEnd,
                                                            ItemId = originalItem.ItemId,
                                                            OrderId = order.Id,
                                                            CompanyId = CurrentUser.CompanyId,
                                                            IsOutOfInventory = false,
                                                        };

                                                        if (inventoryRep.Create(item))
                                                        {
                                                            createdItems.Add(item);
                                                        }
                                                        else
                                                        {
                                                            noCreationErrors = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else if (
                                                originalItem.Quantity == splitedItem.Count
                                                )
                                            {
                                                foreach (var item in splitedItem)
                                                {
                                                    if (locations.Any(x => x.Id == item.LocationId))
                                                    {
                                                        item.ItemId = originalItem.ItemId;
                                                        item.OrderId = order.Id;
                                                        item.CompanyId = CurrentUser.CompanyId;
                                                        item.IsOutOfInventory = false;

                                                        if (inventoryRep.Create(item))
                                                        {
                                                            createdItems.Add(item);
                                                        }
                                                        else
                                                        {
                                                            noCreationErrors = false;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        noCreationErrors = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                noCreationErrors = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            noCreationErrors = false;
                                            break;
                                        }
                                    }

                                    if (noCreationErrors)
                                    {
                                        return RedirectToAction("Index");
                                    }
                                    else
                                    {
                                        foreach (var item in createdItems)
                                        {
                                            inventoryRep.Delete(item.Id);
                                        }

                                        return Error(Errors.INVENTORY_CREATE_ERROR);
                                    }
                                }
                                else
                                {
                                    return Error(Errors.INVALID_FORM);
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
                else
                {
                    return Error(Errors.ORDER_NOT_FOUND);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
            */
            return View();
        }

        [OpenIdAuthorize]
        public ActionResult Search()
        {
            if (Authorized(RoleType.OrdersWriter) || Authorized(RoleType.OrdersViewer))
            {
                if (!Authorized(RoleType.OrdersViewer))
                {
                    ViewBag.UserId = CurrentUser.UserId;
                }

                return View();
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult OrdersToExport()
        {
            if (Authorized(RoleType.SystemManager))
            {
                List<Order> ordersToExport;

                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    ordersToExport = ordersRep.GetList("Orders_Statuses", "Supplier", "User")
                        .Where(x => x.CompanyId == CurrentUser.CompanyId && x.StatusId == (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                        .ToList();

                }

                if (ordersToExport != null)
                {
                    return View(ordersToExport);
                }
                else
                {
                    return Error(Errors.ORDER_GET_ERROR);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult OrdersToExport(int[] selectedOrder)
        {
            if (Authorized(RoleType.SystemManager))
            {
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

                    if (ordersToExport != null)
                    {
                        if (userCompany == null)
                            return Error(Errors.DATABASE_ERROR);

                        if (String.IsNullOrEmpty(userCompany.ExternalCoinCode) || String.IsNullOrEmpty(userCompany.ExternalExpenseCode))
                            return Error("Insufficient company data for export");

                        int numberOfOrders = ordersToExport.Count > 999 ? 0 : ordersToExport.Count;

                        builder.AppendLine(
                            numberOfOrders.ToString().PadRight(180)
                            );

                        foreach (var order in ordersToExport)
                        {
                            DateTime paymentDate;
                            decimal orderPrice;

                            if (order.Price.HasValue)
                            {
                                if (order.Price.Value > 999999999)
                                    return Error("Price is too high");
                                else
                                    orderPrice = order.Price.Value;
                            }
                            else
                            {
                                orderPrice = 0;
                            }

                            if (order.Price > 999999999)
                                return Error("Price is too high");

                            if (String.IsNullOrEmpty(order.Supplier.ExternalId))
                                return Error("Insufficient supplier data for export");

                            if (String.IsNullOrEmpty(order.Budgets_ExpensesToIncomes.ExternalId))
                                return Error("Insufficient allocation data for export");

                            if (String.IsNullOrEmpty(order.InvoiceNumber) || order.InvoiceDate == null)
                                return Error("Insufficient order data for export");

                            string orderNotes = order.Notes == null ? String.Empty : order.Notes;

                            if (order.Orders_OrderToAllocation.Count > 0)
                            {
                                int paymentMonthId = order.Orders_OrderToAllocation.Max(month => month.Id);
                                paymentDate = new DateTime(order.Budgets_ExpensesToIncomes.Budget.Year, paymentMonthId, 1);
                            }
                            else
                            {
                                paymentDate = new DateTime(order.Budgets_ExpensesToIncomes.Budget.Year, order.CreationDate.Month, 1);
                            }

                            builder.AppendLine(
                                String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}",
                                userCompany.ExternalExpenseCode.PadLeft(3),
                                order.InvoiceNumber.PadLeft(5),
                                order.InvoiceDate.Value.ToString("ddMMyy"),
                                String.Empty.PadLeft(5),
                                paymentDate.ToString("ddMMyy"),
                                userCompany.ExternalCoinCode.PadLeft(3),
                                String.Empty.PadLeft(22),
                                order.Budgets_ExpensesToIncomes.ExternalId.ToString().PadLeft(8),
                                String.Empty.PadLeft(8),
                                order.Supplier.ExternalId.ToString().PadLeft(8),
                                String.Empty.PadLeft(8),
                                orderPrice.ToString("0.00").PadLeft(12),
                                String.Empty.PadLeft(12),
                                orderPrice.ToString("0.00").PadLeft(12),
                                String.Empty.PadLeft(12),
                                String.Empty.PadLeft(12),
                                String.Empty.PadLeft(12),
                                String.Empty.PadLeft(12),
                                String.Empty.PadLeft(12)
                                )
                                );
                        }
                        return File(Encoding.UTF8.GetBytes(builder.ToString()),
                             "text/plain",
                             "MOVEIN.DAT");
                    }
                    else
                    {
                        return Error(Errors.ORDER_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        public ActionResult ExportAll()
        {
            if (Authorized(RoleType.SystemManager))
            {
                StringBuilder builder = new StringBuilder();

                List<Order> ordersToExport;
                Company userCompany;

                using (CompaniesRepository companiesRep = new CompaniesRepository())
                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                {
                    ordersToExport = ordersRep.GetList()
                        .Where(x => x.CompanyId == CurrentUser.CompanyId && x.StatusId == (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport)
                        .ToList();

                    userCompany = companiesRep.GetEntity(CurrentUser.CompanyId);

                    if (ordersToExport != null)
                    {
                        if (userCompany == null)
                            return Error(Errors.DATABASE_ERROR);

                        if (String.IsNullOrEmpty(userCompany.ExternalCoinCode) || String.IsNullOrEmpty(userCompany.ExternalExpenseCode))
                            return Error("Insufficient company data for export");

                        int numberOfOrders = ordersToExport.Count > 999 ? 0 : ordersToExport.Count;

                        builder.AppendLine(
                            numberOfOrders.ToString().PadRight(180)
                            );

                        foreach (var order in ordersToExport)
                        {
                            DateTime paymentDate;
                            decimal orderPrice;

                            if (order.Price.HasValue)
                            {
                                if (order.Price.Value > 999999999)
                                return Error("Price is too high");
                                else
                                    orderPrice = order.Price.Value;
                            }
                            else
                            {
                                orderPrice = 0;
                            }

                            if (String.IsNullOrEmpty(order.Supplier.ExternalId))
                                return Error("Insufficient supplier data for export");

                            if (String.IsNullOrEmpty(order.Budgets_ExpensesToIncomes.ExternalId))
                                return Error("Insufficient allocation data for export");

                            if (String.IsNullOrEmpty(order.InvoiceNumber) || order.InvoiceDate == null)
                                return Error("Insufficient order data for export");

                            string orderNotes = order.Notes == null ? String.Empty : order.Notes;

                            if (order.Orders_OrderToAllocation.Count > 0)
                            {
                                int paymentMonthId = order.Orders_OrderToAllocation.Max(month => month.Id);
                                paymentDate = new DateTime(order.Budgets_ExpensesToIncomes.Budget.Year, paymentMonthId, 1);
                            }
                            else
                            {
                                paymentDate = new DateTime(order.Budgets_ExpensesToIncomes.Budget.Year, order.CreationDate.Month, 1);
                            }

                            builder.AppendLine(
                                String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}",
                                userCompany.ExternalExpenseCode.PadLeft(3),
                                order.InvoiceNumber.PadLeft(5),
                                order.InvoiceDate.Value.ToString("ddMMyy"),
                                String.Empty.PadLeft(5),
                                paymentDate.ToString("ddMMyy"),
                                userCompany.ExternalCoinCode.PadLeft(3),
                                String.Empty.PadLeft(22),
                                order.Budgets_ExpensesToIncomes.ExternalId.ToString().PadLeft(8),
                                String.Empty.PadLeft(8),
                                order.Supplier.ExternalId.ToString().PadLeft(8),
                                String.Empty.PadLeft(8),
                                orderPrice.ToString("0.00").PadLeft(12),
                                String.Empty.PadLeft(12),
                                orderPrice.ToString("0.00").PadLeft(12),
                                String.Empty.PadLeft(12),
                                String.Empty.PadLeft(12),
                                String.Empty.PadLeft(12),
                                String.Empty.PadLeft(12),
                                String.Empty.PadLeft(12)
                                )
                                );
                        }

                        return File(Encoding.Default.GetBytes(builder.ToString()),
                             "text/plain",
                             "MOVEIN.DAT");
                    }
                    else
                    {
                        return Error(Errors.ORDER_GET_ERROR);
                    }
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Search(OrdersSearchValuesModel model)
        {
            if (Authorized(RoleType.OrdersWriter) || Authorized(RoleType.OrdersViewer))
            {
                List<Order> matchingOrders;
                List<Order> TextMatchOrders = new List<Order>();

                ViewBag.UserId = model.UserId;
                ViewBag.StatusId = model.StatusId;
                ViewBag.SupplierId = model.SupplierId;
                ViewBag.HideUserField = model.HideUserField;
                ViewBag.HideStatusField = model.HideStatusField;
                ViewBag.HideSupplierField = model.HideSupplierField;

                using (OrdersRepository ordersRep = new OrdersRepository(CurrentUser.CompanyId))
                using (UsersRepository usersRep = new UsersRepository())
                using (SuppliersRepository suppliersRep = new SuppliersRepository())
                using (OrderStatusesRepository statusesRep = new OrderStatusesRepository())
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
                        ordersQuery = ordersQuery.Where(x => x.Budgets_ExpensesToIncomes.BudgetId == model.BudgetId.Value);

                    if (model.OrderNumber.HasValue && model.OrderNumber.Value != -1)
                        ordersQuery = ordersQuery.Where(x => x.OrderNumber == model.OrderNumber.Value);

                    if (model.SupplierId.HasValue && model.SupplierId.Value != -1)
                        ordersQuery = ordersQuery.Where(x => x.SupplierId == model.SupplierId.Value);

                    if (model.StatusId.HasValue && model.StatusId.Value != -1)
                        ordersQuery = ordersQuery.Where(x => x.StatusId == model.StatusId.Value);

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
                            if (!String.IsNullOrEmpty(order.Notes) && order.Notes.Contains(word))
                            {
                                TextMatchOrders.Add(order);
                                break;
                            }

                            if (!String.IsNullOrEmpty(order.OrderApproverNotes) && order.OrderApproverNotes.Contains(word))
                            {
                                TextMatchOrders.Add(order);
                                break;
                            }
                        }
                    }

                    model.Matches = TextMatchOrders;
                    return View(model);
                }
                else
                {
                    model.Matches = matchingOrders;
                    return View(model);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
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
                ViewBag.UserRoles = RoleType.None;
            }
            else
            {
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
            ViewBag.IsOrdered = false;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = null;
            ViewBag.Order = null;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 1;

            ViewBag.IsCheckBoxed = true;
            ViewBag.ShowUserName = true;

            ViewBag.UserRoles = (RoleType)CurrentUser.Roles;

            return PartialView("List", orders);
        }

        [ChildActionOnly]
        public ActionResult SearchForm(OrdersSearchValuesModel model, bool isExpanding, bool isCollapsed, int? userId = null, int? statusId = null, int? supplierId = null, bool hideUserField = false, bool hideStatusField = false, bool hideSupplierField = false)
        {
            using (UsersRepository usersRep = new UsersRepository())
            using (BudgetsRepository budgetsRep = new BudgetsRepository())
            using (SuppliersRepository suppliersRep = new SuppliersRepository())
            using (OrderStatusesRepository statusesRep = new OrderStatusesRepository())
            {
                List<SelectListItemDB> usersAsSelectItems = new List<SelectListItemDB>() { new SelectListItemDB() { Id = -1, Name = "כל המזמינים" } };
                usersAsSelectItems.AddRange(usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Select(x => new SelectListItemDB() { Id = x.Id, Name = x.FirstName + " " + x.LastName }));
                model.UsersList = new SelectList(usersAsSelectItems, "Id", "Name");

                List<SelectListItemDB> budgetsAsSelectItems = new List<SelectListItemDB>() { new SelectListItemDB() { Id = -1, Name = "כל התקציבים" } };
                budgetsAsSelectItems.AddRange(budgetsRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Select(x => new { Id = x.Id, Name = x.Year }).AsEnumerable().Select(x => new SelectListItemDB() { Id = x.Id, Name = x.Name.ToString() }));
                model.BudgetsList = new SelectList(budgetsAsSelectItems, "Id", "Name");

                List<Supplier> suppliersSelectList = new List<Supplier>() { new Supplier() { Id = -1, Name = "כל הספקים" } };
                suppliersSelectList.AddRange(suppliersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList());
                model.SuppliersList = new SelectList(suppliersSelectList, "Id", "Name");

                List<Orders_Statuses> statusesSelectList = new List<Orders_Statuses>() { new Orders_Statuses() { Id = -1, Name = "כל הסטאטוסים" } };
                statusesSelectList.AddRange(statusesRep.GetList().ToList());
                model.StatusesList = new SelectList(statusesSelectList, "Id", "Name");
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
            ViewBag.orderId = id;
            return View();
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult InvoiceApproval(string selectedStatus, int orderId = 0)
        {
            using (OrdersRepository ordersRepository = new OrdersRepository(CurrentUser.CompanyId))
            {
                Order order = ordersRepository.GetEntity(orderId);
                if (selectedStatus == Loc.Dic.ApproveInvoce)
                {
                    order.StatusId = (int)StatusType.InvoiceApprovedByOrderCreatorPendingFileExport;
                }
                if (selectedStatus == Loc.Dic.CancelOrder)
                {
                    order.StatusId = (int)StatusType.OrderCancelled;
                }
                ordersRepository.Update(order);
            }
            return RedirectToAction("Index");
        }


        private List<Orders_OrderToItem> ItemsFromString(string itemsString, int orderId)
        {
            List<Orders_OrderToItem> items = new List<Orders_OrderToItem>();

            string[] splitItems = itemsString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in splitItems)
            {
                bool isValidItem;
                int itemId = 0;
                int quantity = 0;
                int singleItemPrice = 0;

                string[] itemValues = item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (itemValues.Length == 3)
                {
                    if (
                        int.TryParse(itemValues[0], out itemId) &&
                        int.TryParse(itemValues[1], out quantity) &&
                        int.TryParse(itemValues[2], out singleItemPrice)
                        )
                    {
                        isValidItem = true;
                    }
                    else
                    {
                        isValidItem = false;
                    }
                }
                else
                {
                    isValidItem = false;
                }

                if (isValidItem)
                {
                    Orders_OrderToItem newItem = new Orders_OrderToItem()
                    {
                        OrderId = orderId,
                        ItemId = itemId,
                        Quantity = quantity,
                        SingleItemPrice = singleItemPrice
                    };

                    items.Add(newItem);
                }
                else
                {
                    return null;
                }
            }

            return items;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}