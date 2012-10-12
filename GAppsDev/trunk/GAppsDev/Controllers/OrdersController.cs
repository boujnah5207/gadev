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
using GAppsDev.OpenIdService;
using Mvc4.OpenId.Sample.Security;
using GAppsDev.Models;
using Rotativa;

namespace GAppsDev.Controllers
{
    public class OrdersController : BaseController
    {
        private const int WAITING_FOR_APPROVAL_STATUS = 1;
        private Entities db = new Entities();

        //
        // GET: /Orders/



        [OpenIdAuthorize]
        public ActionResult Index()
        {
            using (OrdersRepository ordersRep = new OrdersRepository())
            {
                return View(ordersRep.GetList("Company", "Orders_Statuses", "Supplier", "User").ToList());
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
                orderModel.OrderToITem = db.Orders_OrderToItem.Where(x => x.OrderId == id).ToList();
                return View(orderModel);
            }
            else return Error(Errors.NO_PERMISSION);
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
            return new ViewAsPdf("PrintOrderToScreen", order) { FileName = "Invoice.pdf"};
            //return new ViewAsPdf("PrintOrderToScreen", new { id = id }) { FileName = "Invoice.pdf", Cookies = cookies };
        }

        [OpenIdAuthorize]
        public ActionResult Details(int id = 0)
        {
            OrderModel orderModel = new OrderModel();
            using (OrdersRepository ordersRepository = new OrdersRepository())
            {
                orderModel.Order = db.Orders.Single(o => o.Id == id);
                orderModel.OrderToITem = db.Orders_OrderToItem.Where(x => x.OrderId == id).ToList();
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
            using (OrderStatusesRepository statusRep = new OrderStatusesRepository())
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
                    List<Orders_OrderToItem> ItemsList = new List<Orders_OrderToItem>();

                    order.UserId = CurrentUser.UserId;
                    order.CompanyId = CurrentUser.CompanyId;
                    order.CreationDate = DateTime.Now;
                    order.StatusId = WAITING_FOR_APPROVAL_STATUS;
                    order.OrderApproverNotes = String.Empty;

                    bool wasOrderCreated;
                    using (OrdersRepository orderRep = new OrdersRepository())
                    {
                        wasOrderCreated = orderRep.Create(order);
                    }

                    if (wasOrderCreated)
                    {
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
                                    OrderId = order.Id,
                                    ItemId = itemId,
                                    Quantity = quantity,
                                    SingleItemPrice = singleItemPrice
                                };

                                ItemsList.Add(newItem);
                            }
                            else
                            {
                                using (OrdersRepository orderRep = new OrdersRepository())
                                {
                                    orderRep.Delete(order.Id);
                                }
                                return Error(Errors.INVALID_FORM);
                            }
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
                                    noItemErrors = false;
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

            /*
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", order.CompanyId);
            ViewBag.StatusId = new SelectList(db.Orders_Statuses, "Id", "Name", order.StatusId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", order.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", order.UserId);
            return View(order);
            */
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
                                        OrderId = orderFromDatabase.Id,
                                        ItemId = itemId,
                                        Quantity = quantity,
                                        SingleItemPrice = singleItemPrice
                                    };

                                    itemsFromEditForm.Add(newItem);
                                }
                                else
                                {
                                    return Error(Errors.INVALID_FORM);
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
                                        //existingItem.Quantity = newItem.Quantity;
                                        //existingItem.SingleItemPrice = newItem.SingleItemPrice;
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

                            using(OrderToItemRepository orderToItemRep = new OrderToItemRepository())
                            {
                                foreach (var item in itemsToCreate)
                                {
                                    orderToItemRep.Create(item);
                                }
                                foreach (var item in itemsToUpdate)
                                {
                                    orderToItemRep.Update(item);
                                }
                                foreach (var item in itemsToDelete)
                                {
                                    orderToItemRep.Delete(item.Id);
                                }
                            }

                            return RedirectToAction("MyOrders");
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
            Order order = db.Orders.Single(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        //
        // POST: /Orders/Delete/5

        [OpenIdAuthorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Single(o => o.Id == id);
            db.Orders.DeleteObject(order);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}