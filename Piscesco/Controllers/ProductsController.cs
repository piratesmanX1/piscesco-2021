using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Piscesco.Areas.Identity.Data;
using Piscesco.Controllers;
using Piscesco.Data;
using Piscesco.Models;

namespace Piscesco.Views.Products
{
    public class ProductsController : Controller
    {
        private readonly PiscescoModelContext _context;
        private readonly UserManager<PiscescoUser> _userManager;
        private static int _stallID;

        public ProductsController(PiscescoModelContext context, UserManager<PiscescoUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id.HasValue)
            {
                //Debug.WriteLine(id);
                _stallID = (int)id;
            }
            var products = from p in _context.Product select p;
            products = products.Where(item => item.StallID.Equals(_stallID));


            return View(products);
        }

        // GET: Products
        public async Task<IActionResult> StallCatchSetup(int? id)
        {
            if (id.HasValue)
            {
                //Debug.WriteLine(id);
                _stallID = (int)id;
            }
            var products = from p in _context.Product select p;
            products = products.Where(item => item.StallID.Equals(_stallID));

            var featuredProducts = from p in _context.FeaturedProduct select p;
            featuredProducts = featuredProducts.Where(item => item.StallID.Equals(_stallID));

            List<Product> featuredProductsList = new List<Product>();
            foreach (var featuredItem in featuredProducts)
            {
                foreach (var productItem in products)
                {
                    if (featuredItem.ProductID.Equals(productItem.ProductID))
                    {
                        featuredProductsList.Add(productItem);
                    }
                }
            }

            //var featuredProducts = from p in _context.Product
            //                       join fp in _context.FeaturedProduct on p.ProductID equals fp.ProductID into result
            //                       where
            //                       p.StallID == _stallID 
            //                       select result;


            ViewData["Products"] = products;
            ViewData["FeaturedProducts"] = featuredProductsList;


            return View();
        }

        // GET: Products
        public async Task<IActionResult> BrowseProducts(int? id, string ProductName)
        {
            if (id == null)
            {
                return NotFound();
            }
            _stallID = (int)id;
            var products = from p in _context.Product select p;
            products = products.Where(item => item.StallID.Equals(_stallID));

            // to check whether its there or not
            if (!string.IsNullOrEmpty(ProductName))
            {
                // s stands for database variable, ProductName is the table column
                products = products.Where(p => p.ProductName.Contains(ProductName));
            }

            var featuredProducts = from p in _context.FeaturedProduct select p;
            featuredProducts = featuredProducts.Where(item => item.StallID.Equals(_stallID));

            List<Product> featuredProductsList = new List<Product>();
            foreach (var featuredItem in featuredProducts)
            {
                foreach (var productItem in products)
                {
                    if (featuredItem.ProductID.Equals(productItem.ProductID))
                    {
                        featuredProductsList.Add(productItem);
                    }
                }
            }
            var stallInformation = from s in _context.Stall select s;
            stallInformation = stallInformation.Where(item => item.StallID.Equals(_stallID));

            ViewData["Stall"] = stallInformation.First();
            ViewData["FeaturedProducts"] = featuredProductsList;

            // Default: this is to display the entire page/data on load, we will change it to only show after filter
            // return View(await _context.Product.ToListAsync());
            // return View(products);
            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> BrowseStalls(String StallName)
        {
            // technically just like SQL query
            var stall = from m in _context.Stall
                        select m; // selecting the product/data from the context product

            // to check whether its there or not
            if (!string.IsNullOrEmpty(StallName))
            {
                // s stands for database variable, ProductName is the table column
                stall = stall.Where(s => s.StallName.Contains(StallName));
            }

            var products = from p in _context.Product select p; // selecting all product to show within the stall card

            ViewData["Products"] = products;

            // Default: this is to display the entire page/data on load, we will change it to only show after filter
            // return View(await _context.Product.ToListAsync());
            return View(await stall.ToListAsync());
        }



        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }



        // GET: Products/Create
        public IActionResult Create()
        {
            Product p = new Product();
            p.StallID = _stallID;
            return View(p);
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductID,StallID,ProductName,ProductDescription,Price,ProductUnit,Stock,ProductImage")] Product product, IFormFile files)
        {
            if (ModelState.IsValid)
            {
                //If stall does no exist
                var stall = await _context.Stall.FindAsync(_stallID);
                if (stall == null)
                {
                    return NotFound();
                }

                //Set stall ID
                product.StallID = _stallID;

                //Image upload
                Guid imageUUID = Guid.NewGuid();
                string imageUUIDString = imageUUID.ToString();
                product.ProductImage = imageUUIDString;
                BlobsController bc = new BlobsController();
                bc.UploadImage(files, imageUUIDString);

                _context.Add(product);



                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,StallID,ProductName,ProductDescription,Price,ProductUnit,Stock,ProductImage")] Product product, IFormFile files)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (files != null)
                    {
                        Guid imageUUID = Guid.NewGuid();
                        string imageUUIDString = imageUUID.ToString();
                        product.ProductImage = imageUUIDString;
                        BlobsController bc = new BlobsController();
                        bc.UploadImage(files, imageUUIDString);

                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
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
            return View(product);
        }


        [HttpPost]
        [ActionName("AddFeatured")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeatured(int id, [Bind("FeaturedProductID,StallID,ProductID")] FeaturedProduct fp)
        {
            if (ModelState.IsValid)
            {
                //If stall does no exist
                var stall = await _context.Stall.FindAsync(_stallID);
                if (stall == null)
                {
                    return NotFound();
                }

                //Set stall ID
                fp.StallID = _stallID;
                fp.ProductID = id;
                _context.Add(fp);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(StallCatchSetup));
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("RemoveFeatured")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFeatured(int id)
        {
            //var product = await _context.Product.FindAsync(id);
            var featuredProducts = from p in _context.FeaturedProduct
                                   select p;
            FeaturedProduct selectedProduct = new FeaturedProduct();
            foreach(var fp in featuredProducts)
            {
                if (fp.ProductID.Equals(id) && fp.StallID.Equals(_stallID))
                {
                    selectedProduct = fp;
                }
            }
            //var delete = await _context.FeaturedProduct.FindAsync(selectedProduct.FeaturedProductID);
            _context.FeaturedProduct.Remove(selectedProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(StallCatchSetup));
        }

        // GET: Products/AddToCart/5
        public async Task<IActionResult> AddToCartPage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            // technically just like SQL query
            var order = from m in _context.Order
                        select m; // selecting the product/data from the context product

            // check if the user have the existing product within the ongoing order or not
            order = order.Where(item => item.ProductID.Equals((int)id));
            order = order.Where(item => item.OwnerID == (_userManager.GetUserId(User)));
            order = (order.Where(item => item.Status == "Pending"));

            if (order.Any())
            {
                ViewData["Order"] = order.First();
            }

            ViewData["Product"] = product;

            return View();
        }

        [HttpPost]
        [ActionName("AddToCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([Bind("OrderID,StallID,ProductID,ProductName,ProductQuantity,TotalPrice,Status,TransactionDate")] Order order)
        {
            if (ModelState.IsValid)
            {
                //If product does no exist
                var product = await _context.Product.FindAsync(order.ProductID);
                if (product == null)
                {
                    return NotFound();
                }
                int productStock = product.Stock - order.ProductQuantity;

                //Setup order
                order.Status = "Pending";
                if (productStock < 0)
                {
                    TempData["Message"] = "Invalid quantity, the product: " + order.ProductName + " does not have that much quantity in stock";
                    return RedirectToAction("AddToCartPage", new { id = order.ProductID });
                }

                // product.Stock = productStock;

                // check if user ongoing cart have the existing product or not
                var orderitem = from m in _context.Order
                                select m; // selecting the product/data from the context product
                orderitem = orderitem.Where(item => item.ProductID.Equals(order.ProductID));
                orderitem = orderitem.Where(item => item.OwnerID == (_userManager.GetUserId(User)));
                orderitem = (orderitem.Where(item => item.Status == "Pending"));

                if (orderitem.Any())
                {
                    // if it existed then we update instead
                    var itemcontent = orderitem.First();
                    itemcontent.ProductQuantity = order.ProductQuantity;

                    _context.Update(itemcontent);
                    
                    TempData["Message"] = "Product : " + order.ProductName + " quantity updated.";

                    await _context.SaveChangesAsync();
                }
                else
                {
                    // if not then add to cart instead
                    order.OwnerID = _userManager.GetUserId(User);
                    order.TotalPrice = product.Price * order.ProductQuantity;

                    // _context.Update(product);
                    _context.Add(order);

                    TempData["Message"] = "Product : " + order.ProductName + " added into cart.";

                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("AddToCartPage", new { id = order.ProductID });
        }

        [HttpPost]
        [ActionName("RemoveFromCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart (Guid id)
        {
            //If product does not exist
            var cartitem = await _context.Order.FindAsync(id);
            if (cartitem == null)
            {
                // return NotFound();

                TempData["Message"] = "Notice: The selected product already removed from the cart.";
                return RedirectToAction("CartList");
            }

            // if the cart item exist, then proceed to deletion
            _context.Order.Remove(cartitem);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Notice: Product " + cartitem.ProductName + " removed from the cart.";

            return RedirectToAction("CartList", "Orders");
        }

        [HttpPost]
        [ActionName("CheckoutCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutCart()
        {
            // check if the cart items existed
            // check if user ongoing cart have the existing product or not
            var orderitem = from m in _context.Order
                            select m; // selecting the product/data from the context product
            orderitem = orderitem.Where(item => item.OwnerID == (_userManager.GetUserId(User)));
            orderitem = (orderitem.Where(item => item.Status == "Pending"));

            if (orderitem.Any())
            {
                // if it existed then we proceed to check the product quantity whether it exceeds the stock quantity or not
                bool valid = false;
                foreach (var cartItem in orderitem)
                {
                    var product = from p in _context.Product select p;
                    product = product.Where(item => item.ProductID == (cartItem.ProductID));
                    var productcontent = product.First();

                    if (productcontent.Stock >= cartItem.ProductQuantity)
                    {
                        valid = true;
                    } else
                    {
                        valid = false;
                        if (productcontent.Stock == 0)
                        {
                            TempData["Message"] = "Notice: Product " + cartItem.ProductName + " is no longer available, please remove from your cart to proceed checkout.";
                        } else
                        {
                            TempData["Message"] = "Notice: Product " + cartItem.ProductName + " exceeds the available quantity of " + productcontent.Stock + ". Please edit and try again.";
                        }

                        return RedirectToAction("CartList", "Orders");
                    }
                }

                if (valid == true)
                {
                    // if it didnt exceed, then update the status and reduce the stock from the product accordingly
                    foreach (var cartItem in orderitem)
                    {
                        var product = from p in _context.Product select p;
                        product = product.Where(item => item.ProductID == (cartItem.ProductID));
                        var productcontent = product.First();

                        var productQuantity = productcontent.Stock - cartItem.ProductQuantity;
                        productcontent.Stock = productQuantity;

                        cartItem.Status = "Purchased";
                        cartItem.TransactionDate = DateTime.Now;

                        _context.Order.Update(cartItem);
                        _context.Product.Update(productcontent);
                    }

                    TempData["Message"] = "Notice: Cart checked out, you may check it via the transaction history.";

                    await _context.SaveChangesAsync();
                }
            } else
            {
                TempData["Message"] = "Notice: Cart item does not exist, please at least add one item into cart and try again.";
            }

            return RedirectToAction("CartList", "Orders");
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductID == id);
        }
    }
}
