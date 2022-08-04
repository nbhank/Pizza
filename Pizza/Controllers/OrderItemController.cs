using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pizza.Models;

namespace Pizza.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly PizzaContext _context;

        public OrderItemController(PizzaContext context)
        {
            _context = context;
        }

        // GET: OrderItem
        public async Task<IActionResult> Index( Int32? orderId, string customerName)
        {
            if (orderId!=null)
            {
                HttpContext.Session.SetString("orderId", orderId.ToString());
                HttpContext.Session.SetString("customerName", customerName);

            }
            else if (HttpContext.Session.GetString("orderId")!=null)
            {
                orderId = Convert.ToInt32(HttpContext.Session.GetString("orderId"));
            }
            else
            {
                TempData["message"] = "Please select an dorder to find its items";
                return Redirect("/Order");
            }

            var pizzaContext = _context.OrderItem
                .Where(a=>a.OrderId==orderId)
                .OrderByDescending(a=>a.OrderItemId)
                .Include(o => o.Item).Include(o => o.Order);
            double total = 0;
            foreach (var item in pizzaContext)
            {
                total += item.Price;
            }
            ViewBag.total =total.ToString("c2");
            return View(await pizzaContext.ToListAsync());
        }

        // GET: OrderItem/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItem
                .Include(o => o.Item)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.OrderItemId == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // GET: OrderItem/Create
        public IActionResult Create()
        {
            ViewData["ItemId"] = new SelectList(_context.Item.OrderBy(a => a.Name), "ItemId", "Name");
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "OrderId");
            return View();
        }

        // POST: OrderItem/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( OrderItem orderItem)
        {
            var item = _context.Item.FirstOrDefault(a => a.ItemId == orderItem.ItemId);
            
            if (item==null)
            {
                ModelState.AddModelError("itemId", "missing ItemId");
            }
            else
            {
                orderItem.Price = item.BaseCost * orderItem.Quantity;
                if (item.CostFactorForToppings!=0)
                {
                    HttpContext.Session.SetString("factor", item.CostFactorForToppings.ToString());
                }
                else if (item.IsPizzaTopping)
                {
                    orderItem.Price *= Convert.ToDouble(HttpContext.Session.GetString("factor"));
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ItemId"] = new SelectList(_context.Item.OrderBy(a=>a.Name), "ItemId", "Name", orderItem.ItemId);
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "OrderId", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrderItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItem.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }
            ViewData["ItemId"] = new SelectList(_context.Item, "ItemId", "Name", orderItem.ItemId);
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "OrderId", orderItem.OrderId);
            return View(orderItem);
        }

        // POST: OrderItem/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderItemId,OrderId,ItemId,Quantity,Price,Comment")] OrderItem orderItem)
        {
            if (id != orderItem.OrderItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemExists(orderItem.OrderItemId))
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
            ViewData["ItemId"] = new SelectList(_context.Item, "ItemId", "Name", orderItem.ItemId);
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "OrderId", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrderItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItem
                .Include(o => o.Item)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.OrderItemId == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: OrderItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItem.FindAsync(id);
            _context.OrderItem.Remove(orderItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItem.Any(e => e.OrderItemId == id);
        }
    }
}
