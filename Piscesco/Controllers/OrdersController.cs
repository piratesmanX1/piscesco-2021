using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage.Table;
using Piscesco.Areas.Identity.Data;
using Piscesco.Data;
using Piscesco.Models;
using Piscesco.Controllers;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;

namespace Piscesco.Controllers
{
    public class OrdersController : Controller
    {
        private readonly PiscescoModelContext _context;
        private readonly UserManager<PiscescoUser> _userManager;
        private static int _stallID;
        private static int _userID;
        private static DateTime _startingDate;
        private static DateTime _endDate;

        public OrdersController(PiscescoModelContext context, UserManager<PiscescoUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart Items
        public async Task<IActionResult> CartList()
        {
            var products = from p in _context.Product select p;
            ViewData["Products"] = products;

            var stalls = from s in _context.Stall select s;
            ViewData["Stalls"] = stalls;

            var orderList = await _context.Order.Where(orderItem => orderItem.OwnerID.Equals(_userManager.GetUserId(User)) && orderItem.Status == "Pending").ToListAsync();

            return View(orderList);
        }

        // GET: Transaction History
        public async Task<IActionResult> TransactionHistory()
        {
            var products = from p in _context.Product select p;

            ViewData["Products"] = products;

            var orderList = await _context.Order.Where(orderItem => orderItem.OwnerID.Equals(_userManager.GetUserId(User)) && orderItem.Status == "Purchased").ToListAsync();
             
            return View(orderList);
        }

        // GET: Stall all orders
        public async Task<IActionResult> OrderByStall(int? id)
        {

            if (id.HasValue)
            {
                _stallID = (int)id;
            }
            var products = from p in _context.Product select p;
            products = products.Where(item => item.StallID.Equals(_stallID));
            
            ViewData["Products"] = products;

            var orderList = await _context.Order.Where(orderItem => orderItem.StallID == _stallID && orderItem.Status == "Purchased").ToListAsync();

            return View(orderList);
        }

        // GET: Stall orders by date
        public async Task<IActionResult> OrderByDate(string start, string end)
        {

            if (!String.IsNullOrEmpty(start))
            {
                _startingDate = Convert.ToDateTime(start);
                _endDate = Convert.ToDateTime(end);
            }

            var products = from p in _context.Product select p;
            products = products.Where(item => item.StallID.Equals(_stallID));

            ViewData["Products"] = products;

            //var orderList = await _context.Order.Where(orderItem => orderItem.StallID == _stallID && orderItem.Status == "Pending").ToListAsync();
            List<Order> orderList;
            if (start.Equals(end))
            {
                orderList = _context.Order.
                FromSqlRaw("SELECT * FROM [Order] WHERE [Status] = 'Purchased' AND TransactionDate >= " + "'" + _startingDate.ToString("yyyy-MM-dd") + "'" + "AND TransactionDate <= '" + _startingDate.AddDays(1).AddTicks(-1) + "'")
                .ToList();
            }
            else
            {
                orderList = _context.Order.
                FromSqlRaw("SELECT * FROM [Order] WHERE [Status] = 'Purchased' AND TransactionDate >= " + "'" + _startingDate.ToString("yyyy-MM-dd") + "'" + "AND TransactionDate <= '" + _endDate.AddDays(1).AddTicks(-1) + "'")
                .ToList();
            }

            ViewData["StartDate"] = _startingDate.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = _endDate.ToString("yyyy-MM-dd");

            return View(orderList);
        }

        //Get partial view
        public PartialViewResult SearchByDate(string start, string end)
        {
            if (!String.IsNullOrEmpty(start))
            {
                _startingDate = Convert.ToDateTime(start);
                _endDate = Convert.ToDateTime(end);
            }

            var products = from p in _context.Product select p;
            products = products.Where(item => item.StallID.Equals(_stallID));

            ViewData["Products"] = products;

            List<Order> orderList;
            if (start.Equals(end))
            {
                orderList = _context.Order.
                FromSqlRaw("SELECT * FROM [Order] WHERE [Status] = 'Purchased' AND TransactionDate >= " + "'" + _startingDate.ToString("yyyy-MM-dd") + "'" + "AND TransactionDate <= '" + _startingDate.AddDays(1).AddTicks(-1) + "'")
                .ToList();
            }
            else
            {
                orderList = _context.Order.
                FromSqlRaw("SELECT * FROM [Order] WHERE [Status] = 'Purchased' AND TransactionDate >= " + "'" + _startingDate.ToString("yyyy-MM-dd") + "'" + "AND TransactionDate <= '" + _endDate.AddDays(1).AddTicks(-1) + "'")
                .ToList();
            }
            ViewData["StartDate"] = _startingDate.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = _endDate.ToString("yyyy-MM-dd");

            return PartialView("_OrderPartialView",orderList);
        }


        public async Task<IActionResult> StallList()
        {
            var loginSession = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var stalls = from s in _context.Stall select s;
            stalls = stalls.Where(item => item.OwnerID.Equals(loginSession.Id));

            return View(await stalls.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(Guid? id, int? pid)
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
            if(pid == null)
            {
                return NotFound();
            }
            var product = await _context.Product.FindAsync(pid);
            ViewData["Product"] = product;

            // proceed to get the related tablestorage entity value: feedback description if exist
            // 1.1 link with the appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfiguration configure = builder.Build();

            //1.2 get the access connection string
            CloudStorageAccount accountDetails = CloudStorageAccount.Parse(configure["ConnectionStrings:BlobStorageConnection"]);

            //1.3 create client object to refer to the correct table
            CloudTableClient clientAgent = accountDetails.CreateCloudTableClient();
            CloudTable table = clientAgent.GetTableReference("feedback");

            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<FeedbackEntity>(order.OwnerID, id.ToString());
                TableResult result = table.ExecuteAsync(retrieveOperation).Result; // getting the result along with ETag

                if (result.Etag != null)
                {
                    // getting the description back to the form
                    var feedbackItem = result.Result as FeedbackEntity;

                    ViewData["Feedback"] = feedbackItem;
                }
                else
                {
                    ViewData["Feedback"] = null;
                }
            }
            catch (Exception ex)
            {
                ViewData["Feedback"] = null;
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderID,StallID,ProductID,ProductName,ProductQuantity,TotalPrice,Status,TransactionDate")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.OrderID = Guid.NewGuid();
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
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

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("OrderID,StallID,ProductID,ProductName,ProductQuantity,TotalPrice,Status,TransactionDate")] Order order)
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
        public async Task<IActionResult> Delete(Guid? id)
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
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var order = await _context.Order.FindAsync(id);
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }






        private bool OrderExists(Guid id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }
    }
}
