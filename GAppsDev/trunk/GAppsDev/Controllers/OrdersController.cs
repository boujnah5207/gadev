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
        public ActionResult DownloadOrderAsPdf(int id)
        {
            Order order = db.Orders.Single(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
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
                ViewBag.ItemId = new SelectList(itemsRep.GetList().ToList(), "Id", "Title");
                ViewBag.StatusId = new SelectList(statusRep.GetList().ToList(), "Id", "Name");
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
            Order order = db.Orders.Single(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", order.CompanyId);
            ViewBag.StatusId = new SelectList(db.Orders_Statuses, "Id", "Name", order.StatusId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", order.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", order.UserId);
            return View(order);
        }

        //
        // POST: /Orders/Edit/5

        [OpenIdAuthorize]
        [HttpPost]
        public ActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Attach(order);
                db.ObjectStateManager.ChangeObjectState(order, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "Id", "Name", order.CompanyId);
            ViewBag.StatusId = new SelectList(db.Orders_Statuses, "Id", "Name", order.StatusId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", order.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", order.UserId);
            return View(order);
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