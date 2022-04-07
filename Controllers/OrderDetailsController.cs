using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ramirez_Mackenzie_HW5.DAL;
using Ramirez_Mackenzie_HW5.Models;

namespace Ramirez_Mackenzie_HW5.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // GET:
        // ORDERDETAILS
        public async Task<IActionResult> Index()
        {
            return View(await _context.OrderDetail.ToListAsync());
        }

        // GET:
        // ORDERDETAILS/DETAILS/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetail
                .FirstOrDefaultAsync(m => m.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // GET:
        // ORDERDETAILS/CREATE
        public IActionResult Create(int orderID)
        {
            //create a new instance of the RegistrationDetail class
            OrderDetail od = new OrderDetail();

            //find the registration that should be associated with this registration
            Order dbOrder = _context.Order.Find(orderID);

            //set the new registration detail's registration equal to the registration you just found
            od.Order = dbOrder;

            //populate the ViewBag with a list of existing courses
            ViewBag.AllProducts = GetAllProducts();

            //pass the newly created registration detail to the view
            return View(od);
        }

        // POST:
        // ORDERDETAILS/CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Order, OrderNotes, Quantity")] OrderDetail orderDetail, int SelectedProduct)
        {
            // IF FALSE - DO THIS
            if (ModelState.IsValid == false)
            {
                ViewBag.AllProducts = GetAllProducts();
                return View(orderDetail);
            }

            // FIND PRODUCT ASSOCIATED W THIS ORDER
            Product dbProduct = _context.Products.Find(SelectedProduct);

            //set the order detail's course to be equal to the one we just found
            orderDetail.Product = dbProduct;

            // FIND ORDER IN DATABASE
            Order dbOrder = _context.Order.Find(orderDetail.Order.OrderID);

            // SET ORDER ON ORDER DETAIL OF ORDER WE JUST FOUND
            orderDetail.Order = dbOrder;

            // SET ORDER DETAILS PRICE TO PRODUCT PRICE
            //this will allow us to to store the price that the user paid
            orderDetail.ProductPrice = dbProduct.Price;

            // todo
            // CALCULATE EXTENDED PRICE FOR ORDER DETAIL
            //orderDetail.ExtendedPrice = orderDetail.Quantity * orderDetail.ProductPrice;

            // ADD ORDER DETAIL TO DATBASE
            _context.Add(orderDetail);
            await _context.SaveChangesAsync();

            // SEND USER TO DETAILS PAGE
            return RedirectToAction("Details", "Orders", new { id = orderDetail.Order.OrderID });
        }


        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetail.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderDetailID,Quantity,ProductPrice")] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(orderDetail.OrderDetailID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(orderDetail);
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetail
                .FirstOrDefaultAsync(m => m.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderDetail = await _context.OrderDetail.FindAsync(id);
            _context.OrderDetail.Remove(orderDetail);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // PRIVATE
        // ORDERDETAIL EXISTS
        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetail.Any(e => e.OrderDetailID == id);
        }

        // PRIVATE
        // GET ALL PRODUCTS
        private SelectList GetAllProducts()
        {

            //create a list for all the courses
            List<Product> allProducts = _context.Products.ToList();

            //use the constructor on select list to create a new select list with the options
            SelectList slAllProducts = new SelectList(allProducts, nameof(Product.ProductID), nameof(Product.Name));

            return slAllProducts;

        }

    }
}
