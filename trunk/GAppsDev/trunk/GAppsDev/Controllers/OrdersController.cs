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

namespace GAppsDev.Controllers
{
    public class OrdersController : BaseController
    {
        private const int WAITING_FOR_APPROVAL_STATUS = 1;
        private const int APPROVED_WAITING_FOR_CREATOR_UPLOAD_INVOICE = 3;
        private const int WAITING_FOR_CREATOR_REPLAY_STATUS = 7;

        const int ITEMS_PER_PAGE = 7;
        const int FIRST_PAGE = 1;
        const string NO_SORT_BY = "None";
        const string ASCENDING = "ASC";

        private Entities db = new Entities();

        //
        // GET: /Orders/

        [OpenIdAuthorize]
        public ActionResult Index(int page = FIRST_PAGE, string sortby = NO_SORT_BY, string order = ASCENDING)
        {
            if (Authorized(RoleType.Employee))
            {
                IEnumerable<Order> orders;
                using (OrdersRepository ordersRep = new OrdersRepository())
                using (UsersRepository usersRep = new UsersRepository())
                using (SuppliersRepository suppliersRep = new SuppliersRepository())
                using (OrderStatusesRepository statusesRep = new OrderStatusesRepository())
                {
                    orders = ordersRep.GetList("Company", "Orders_Statuses", "Supplier", "User").Where(x => x.CompanyId == CurrentUser.CompanyId);
                    List<SelectListItemFromDB> usersAsSelectItems = usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.FirstName + " " + x.LastName }).ToList();
                    ViewBag.UsersList = new SelectList(usersAsSelectItems, "Id", "Name");
                    ViewBag.SuppliersList = new SelectList(suppliersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList(), "Id", "Name");
                    ViewBag.StatusesList = new SelectList(statusesRep.GetList().ToList(), "Id", "Name");
                    
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

                        if (order == ASCENDING)
                            orderFunction = x => orders.OrderBy(x);
                        else
                            orderFunction = x => orders.OrderByDescending(x);

                        switch (sortby)
                        {
                            case "username":
                            default:
                                orders = orderFunction(x => x.User.FirstName + " " + x.User.LastName);
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
                                orders = orderFunction(x => x.Orders_OrderToItem.Sum(item => item.SingleItemPrice));
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

        //
        // GET: /Orders/Details/5

        [OpenIdAuthorize]
        public ActionResult MyOrders()
        {
            if (Authorized(RoleType.Employee))
            {
                using (OrdersRepository ordersRep = new OrdersRepository())
                {
                    ViewBag.ApprovedStatus = APPROVED_WAITING_FOR_CREATOR_UPLOAD_INVOICE;
                    return View(ordersRep.GetList("Orders_Statuses", "Supplier").Where(x => x.UserId == CurrentUser.UserId).ToList());
                }
            }
            else return Error(Errors.NO_PERMISSION);
        }

        [OpenIdAuthorize]
        public ActionResult PendingOrders()
        {
            if (Authorized(RoleType.OrdersApprover))
            {
                using (OrdersRepository ordersRep = new OrdersRepository())
                {
                    return View(ordersRep.GetList("Supplier", "User").Where(x => x.Orders_Statuses.Id == (int)StatusType.Pending).ToList());
                }
            }
            else return Error(Errors.NO_PERMISSION);
        }

        [OpenIdAuthorize]
        public ActionResult ModifyStatus(int id = 0)
        {
            if (Authorized(RoleType.OrdersApprover))
            {
                OrderModel orderModel = new OrderModel();
                orderModel.Order = db.Orders.Single(o => o.Id == id);
                orderModel.OrderToItem = db.Orders_OrderToItem.Where(x => x.OrderId == id).ToList();
                return View(orderModel);
            }
            else return Error(Errors.NO_PERMISSION);
        }


        // This action renders the form
        [OpenIdAuthorize]
        public ActionResult UploadSingleFile()
        {
            return View();
        }

        // This action handles the form POST and the upload
        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult UploadSingleFile(HttpPostedFileBase file)
        {
            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                // extract only the fielname
                var fileName = Path.GetFileName(file.FileName);
                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(path);
            }
            // redirect back to the index action to show the form once again
            return RedirectToAction("Index");
        }

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult ModifyStatus(OrderModel modifiedOrder, string selectedStatus)
        {
            if (Authorized(RoleType.OrdersApprover))
            {
                using (OrdersRepository ordersRepository = new OrdersRepository())
                {
                    Order order = ordersRepository.GetEntity(modifiedOrder.Order.Id);
                    order.OrderApproverNotes = modifiedOrder.Order.OrderApproverNotes;
                    if (selectedStatus == "אשר הזמנה") order.StatusId = (int)StatusType.ApprovedPendingInvoice;
                    if (selectedStatus == "דחה הזמנה") order.StatusId = (int)StatusType.Declined;
                    if (selectedStatus == "החזר למשתמש") order.StatusId = (int)StatusType.PendingOrderCreator;
                    ordersRepository.Update(order);
                    EmailMethods emailMethods = new EmailMethods("NOREPLY@pqdev.com", "מערכת הזמנות", "noreply50100200");
                    emailMethods.sendGoogleEmail(CurrentUser.Email, CurrentUser.FullName, "עדכון סטטוס הזמנה", "סטטוס הזמנה מספר " + order.Id + " שונה ל " + order.Orders_Statuses.Name);
                    return RedirectToAction("PendingOrders");
                }
            }
            else return Error(Errors.NO_PERMISSION);
        }

        [OpenIdAuthorize]
        public ActionResult PrintOrderToScreen(int id)
        {
            if (Authorized(RoleType.Employee))
            {
            Order order = db.Orders.Single(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
            else return Error(Errors.NO_PERMISSION);
        }

        [OpenIdAuthorize]
        public ActionResult DownloadOrderAsPdf(int id)
        {
            string cookieName = OpenIdMembershipService.LOGIN_COOKIE_NAME;
            HttpCookie cookie = Request.Cookies[cookieName];
            Dictionary<string, string> cookies = new Dictionary<string, string>();
            cookies.Add(cookieName, cookie.Value);

            Order order = db.Orders.Single(o => o.Id == id);
            return new ViewAsPdf("PrintOrderToScreen", order) { FileName = "Invoice.pdf" };
            //return new ViewAsPdf("PrintOrderToScreen", new { id = id }) { FileName = "Invoice.pdf", Cookies = cookies };
        }

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            OrderModel orderModel = new OrderModel();
            using (OrdersRepository ordersRepository = new OrdersRepository())
            {
            orderModel.Order = db.Orders.Single(o => o.Id == id);
            orderModel.OrderToItem = db.Orders_OrderToItem.Where(x => x.OrderId == id).ToList();
            }
            if (orderModel.Order == null)
            {
                return HttpNotFound();
            }
            return View(orderModel);
        }

        //
        // GET: /Orders/Create

        [OpenIdAuthorize]
        public ActionResult Create()
        {
            using (OrderItemsRepository itemsRep = new OrderItemsRepository())
            using (SuppliersRepository suppliersRep = new SuppliersRepository())
            {
                ViewBag.SupplierId = new SelectList(suppliersRep.GetList().ToList(), "Id", "Name");
            }

            return View();
        }

        //
        // POST: /Orders/Create

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Create(Order order, string itemsString)
        {
            if (ModelState.IsValid)
            {
                if (Authorized(RoleType.Employee))
                {
                    List<Orders_OrderToItem> ItemsList = ItemsFromString(itemsString, 0);

                    order.UserId = CurrentUser.UserId;
                    order.CompanyId = CurrentUser.CompanyId;
                    order.CreationDate = DateTime.Now;
                    order.StatusId = WAITING_FOR_APPROVAL_STATUS;
                    order.OrderApproverNotes = String.Empty;
                    order.Price = ItemsList.Sum(item => item.SingleItemPrice * item.Quantity);

                    bool wasOrderCreated;
                    using (OrdersRepository orderRep = new OrdersRepository())
                    {
                        wasOrderCreated = orderRep.Create(order);
                    }

                    if (wasOrderCreated)
                    {
                        if (ItemsList != null)
                            {
                            if (ItemsList.Count == 0)
                                return Error(Errors.ORDER_HAS_NO_ITEMS);

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
                            return RedirectToAction("MyOrders");
                        else
                        {
                            using (OrderToItemRepository orderToItemRep = new OrderToItemRepository())
                            {
                                foreach (Orders_OrderToItem item in createdItems)
                                {
                                    orderToItemRep.Delete(item.Id);
                                }
                            }

                            using (OrdersRepository orderRep = new OrdersRepository())
                            {
                                orderRep.Delete(order.Id);
                            }

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
                        return Error(Errors.ORDERS_CREATE_ERROR);
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
            if (Authorized(RoleType.Employee))
            {
                Order order;
                using (OrdersRepository orderRep = new OrdersRepository())
                {
                    order = orderRep.GetEntity(id, "Supplier", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");
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
            if (Authorized(RoleType.Employee))
            {
                if (ModelState.IsValid)
                {
                    Order orderFromDatabase;
                    List<Orders_OrderToItem> itemsFromEditForm = new List<Orders_OrderToItem>();
                    List<Orders_OrderToItem> itemsToDelete = new List<Orders_OrderToItem>();
                    List<Orders_OrderToItem> itemsToCreate = new List<Orders_OrderToItem>();
                    List<Orders_OrderToItem> itemsToUpdate = new List<Orders_OrderToItem>();

                    using (OrdersRepository orderRep = new OrdersRepository())
                    {
                        orderFromDatabase = orderRep.GetEntity(order.Id, "Supplier", "Orders_OrderToItem");
                    }

                    if (orderFromDatabase != null)
                    {
                        if (orderFromDatabase.UserId == CurrentUser.UserId)
                        {
                            if (orderFromDatabase.StatusId == WAITING_FOR_APPROVAL_STATUS || orderFromDatabase.Id == WAITING_FOR_CREATOR_REPLAY_STATUS)
                            {
                                itemsFromEditForm = ItemsFromString(itemsString, order.Id);
                                if (itemsFromEditForm != null)
                                {
                                    if (itemsFromEditForm.Count == 0)
                                        return Error(Errors.ORDER_HAS_NO_ITEMS);

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

                                        if (order.Notes != orderFromDatabase.Notes)
                                        {
                                            using (OrdersRepository ordersRep = new OrdersRepository())
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
            if (Authorized(RoleType.Employee))
            {
                OrderModel model = new OrderModel();

                using (OrdersRepository orderRep = new OrdersRepository())
                {
                    model.Order = orderRep.GetEntity(id, "Supplier", "Company", "User", "Orders_Statuses", "Orders_OrderToItem", "Orders_OrderToItem.Orders_Items");
                    model.OrderToItem = model.Order.Orders_OrderToItem.ToList();
                }

                if (model.Order != null)
                {
                    if (model.Order.UserId == CurrentUser.UserId)
                    {
                        if (model.Order.StatusId == WAITING_FOR_APPROVAL_STATUS || model.Order.StatusId == WAITING_FOR_CREATOR_REPLAY_STATUS)
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
            if (Authorized(RoleType.Employee))
            {
                Order order;
                using (OrdersRepository orderRep = new OrdersRepository())
                {
                    order = orderRep.GetEntity(id, "Supplier", "Orders_OrderToItem");
                }

                if (order != null)
                {
                    if (order.UserId == CurrentUser.UserId)
                    {
                        if (order.StatusId == WAITING_FOR_APPROVAL_STATUS || order.StatusId == WAITING_FOR_CREATOR_REPLAY_STATUS)
                        {
                        bool noItemErrors = true;
                        using (OrdersRepository orderRep = new OrdersRepository())
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
        [HttpPost]
        public ActionResult UploadInvoice(int id = 0)
        {
            if (Authorized(RoleType.Employee))
            {
                Order order;
                using (OrdersRepository orderRep = new OrdersRepository())
                {
                    order = orderRep.GetEntity(id);
                }

                if (order != null)
                {
                    if (order.UserId == CurrentUser.UserId)
                    {
                        if (order.StatusId == 3)
                        {
                            // logic goes here
                            return View();
                        }
                        else if (order.StatusId < 3)
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
        public ActionResult Search(OrdersSearchModel model)
        {
            if (Authorized(RoleType.Employee))
            {
                List<Order> matchingOrders;
                List<Order> TextMatchOrders = new List<Order>();

                using (OrdersRepository ordersRep = new OrdersRepository())
                using (UsersRepository usersRep = new UsersRepository())
                using (SuppliersRepository suppliersRep = new SuppliersRepository())
                using (OrderStatusesRepository statusesRep = new OrderStatusesRepository())
                {
                    IQueryable<Order> ordersQuery;

                    ordersQuery = ordersRep.GetList("Company", "Orders_Statuses", "Supplier", "User").Where(x => x.CompanyId == CurrentUser.CompanyId);
                    List<SelectListItemFromDB> usersAsSelectItems = usersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).Select(x => new SelectListItemFromDB() { Id = x.Id, Name = x.FirstName + " " + x.LastName }).ToList();
                    ViewBag.UsersList = new SelectList(usersAsSelectItems, "Id", "Name");
                    ViewBag.SuppliersList = new SelectList(suppliersRep.GetList().Where(x => x.CompanyId == CurrentUser.CompanyId).ToList(), "Id", "Name");
                    ViewBag.StatusesList = new SelectList(statusesRep.GetList().ToList(), "Id", "Name");

                    if (model.UserId.HasValue)
                        ordersQuery = ordersQuery.Where(x => x.UserId == model.UserId.Value);

                    if (model.SupplierId.HasValue)
                        ordersQuery = ordersQuery.Where(x => x.SupplierId == model.SupplierId.Value);

                    if (model.StatusId.HasValue)
                        ordersQuery = ordersQuery.Where(x => x.StatusId == model.StatusId.Value);

                    if (model.PriceMin.HasValue && model.PriceMax.HasValue && model.PriceMax.Value <= model.PriceMin.Value)
                        model.PriceMax = null;

                    if (model.PriceMin.HasValue)
                        ordersQuery = ordersQuery.Where(x => x.Price >= model.PriceMin.Value);

                    if (model.PriceMax.HasValue)
                        ordersQuery = ordersQuery.Where(x => x.Price <= model.PriceMax.Value);

                    if (model.CreationMin.HasValue && model.CreationMax.HasValue && model.CreationMax.Value <= model.CreationMin.Value)
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

                    return View(TextMatchOrders);
                }
                else
                {
                    return View(matchingOrders);
                }
            }
            else
            {
                return Error(Errors.NO_PERMISSION);
            }
        }

        [ChildActionOnly]
        public ActionResult List(IEnumerable<Order> orders, bool isOrdered, bool isPaged, string sortby, string order, int currPage, int numberOfPages, bool isCheckBoxed = false)
        {
            ViewBag.IsOrdered = isOrdered;
            ViewBag.IsPaged = isPaged;
            ViewBag.Sortby = sortby;
            ViewBag.Order = order;
            ViewBag.CurrPage = currPage;
            ViewBag.NumberOfPages = numberOfPages;
            ViewBag.IsCheckBoxed = isCheckBoxed;

            return PartialView(orders);
        }

        [ChildActionOnly]
        public ActionResult SimpleList(IEnumerable<Order> orders)
        {
            ViewBag.IsOrdered = false;
            ViewBag.IsPaged = false;
            ViewBag.Sortby = null;
            ViewBag.Order = null;
            ViewBag.CurrPage = 1;
            ViewBag.NumberOfPages = 1;
            ViewBag.IsCheckBoxed = false;

            return PartialView("List", orders);
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