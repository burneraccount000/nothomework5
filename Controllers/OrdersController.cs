using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Ramirez_Mackenzie_HW5.DAL;
using Ramirez_Mackenzie_HW5.Utilities;
using Ramirez_Mackenzie_HW5.Models;

namespace Ramirez_Mackenzie_HW5.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET:
        // ORDERS INDEX
        public IActionResult Index()
        {

            // IF ADMIN - VIEW ALL
            List<Order> Orders = new List<Order>();
            if (User.IsInRole("Admin"))
            {
                Orders = _context.Order.Include(o => o.OrderDetails).ToList();
            }

            // CUSTOMER
            else
            {
                Orders = _context.Order
                                .Include(r => r.OrderDetails)
                                .Where(r => r.User.UserName == User.Identity.Name)
                                .ToList();
            }

            return View(Orders);
        }

        // GET:
        // ORDER/DETAILS/5
        public async Task<IActionResult> Details(int? id)
        {
            // USER DID NOT SPECIFY AN ORDER
            if (id == null)
            {
                return View("Error", new String[] { "Please specify an order to view!" });
            }

            // FIND ORDER IN DB
            Order order = await _context.Order
                                              .Include(r => r.OrderDetails)
                                              .ThenInclude(r => r.Product)
                                              .Include(r => r.User)
                                              .FirstOrDefaultAsync(m => m.OrderID == id);

            // NO ORDER FOUND
            if (order == null)
            {
                return View("Error", new String[] { "This order was not found!" });
            }

            // CUSTOMER
            if (User.IsInRole("Customer") && order.User.UserName != User.Identity.Name)
            {
                return View("Error", new String[] { "This is not your order!  Don't be such a snoop!" });
            }

            // SEND USER TO DETAILS PAGE
            return View(order);
        }


        // GET
        // ORDERS/CREATE
        [Authorize(Roles = "Customer")]
        public IActionResult Create()
        {
            return View();
        }


        // POST:
        // ORDERS/CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([Bind("OrderNotes")] Order order)
        {
            //Find the next registration number from the utilities class
            order.OrderNumber = OrderNumberGenerator.GetNextOrderNumber(_context);

            //Set the date of this order
            order.OrderDate = DateTime.Now;

            //Associate the registration with the logged-in customer
            order.User = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            //make sure all properties are valid
            if (ModelState.IsValid == false)
            {
                return View(order);
            }

            //if code gets this far, add the registration to the database
            _context.Add(order);
            await _context.SaveChangesAsync();

            //send the user on to the action that will allow them to 
            //create a registration detail.  Be sure to pass along the RegistrationID
            //that you created when you added the registration to the database above
            return RedirectToAction("Create", "OrderDetails", new { orderID = order.OrderID });
        }


        // GET:
        // ORDER/EDIT/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST:
        // ORDER/EDIT/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,OrderNumber,OrderDate,OrderNotes")] Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID))
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
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }
    }
}
