using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Piscesco.Models;
using Microsoft.AspNetCore.Identity;
using Piscesco.Areas.Identity.Data;

namespace Piscesco.Controllers
{
    public class TablesController : Controller
    {
        private readonly UserManager<PiscescoUser> _userManager;
        private readonly SignInManager<PiscescoUser> _signInManager;

        public TablesController(UserManager<PiscescoUser> userManager, SignInManager<PiscescoUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // creating an Feedback table
        private CloudTable GetFeedbackTableInformation()
        {
            // 1.1 link with the appsettings.json
            // import Microsoft.Extensions.Configuration
            // import System.IO;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfiguration configure = builder.Build();

            //1.2 get the access connection string
            CloudStorageAccount accountDetails = CloudStorageAccount.Parse(configure["ConnectionStrings:BlobStorageConnection"]);

            //1.3 create client object to refer to the correct table
            CloudTableClient clientAgent = accountDetails.CreateCloudTableClient();
            CloudTable table = clientAgent.GetTableReference("feedback");

            return table;
        }

        public void CreateFeedbackTable()
        {
            CloudTable table = GetFeedbackTableInformation();
            table.CreateIfNotExistsAsync();
        }

        public ActionResult ViewFeedback(string id)
        {
            if (id != null)
            {
                ViewData["orderID"] = id;

                // proceed to get the related tablestorage entity value: feedback description if exist
                CloudTable table = GetFeedbackTableInformation();

                try
                {
                    TableOperation retrieveOperation = TableOperation.Retrieve<FeedbackEntity>(_userManager.GetUserId(User), id);
                    TableResult result = table.ExecuteAsync(retrieveOperation).Result; // getting the result along with ETag

                    if (result.Etag != null)
                    {
                        // getting the description back to the form
                        var feedbackItem = result.Result as FeedbackEntity;

                        return View(feedbackItem);
                    } else
                    {
                        return View();
                    }
                } 
                catch (Exception ex)
                {
                    // do nothing as the data might not inserted yet
                }

                return View();
            } else {
                TempData["Message"] = "Notice: Order ID undefined, please try again. ";
                return RedirectToAction("TransactionHistory", "Orders");
            }
        }

        [HttpPost]
        public ActionResult ProvideFeedback(string RowKey, string feedbackDescription)
        {
            CreateFeedbackTable();

            // check if the feedback already existed or not first
            CloudTable table = GetFeedbackTableInformation();

            FeedbackEntity feedback = new FeedbackEntity(_userManager.GetUserId(User), RowKey);
            feedback.FeedbackDate = DateTime.Now;
            feedback.FeedbackDescription = feedbackDescription;

            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<FeedbackEntity>(_userManager.GetUserId(User), RowKey);
                TableResult result = table.ExecuteAsync(retrieveOperation).Result; // getting the result along with ETag

                if (result.Etag != null)
                {
                    // if exist then go for the update process
                    feedback.ETag = "*";
                    TableOperation editOperation = TableOperation.Replace(feedback);
                    table.ExecuteAsync(editOperation);

                    TempData["Message"] = "Notice: Feedback updated.";
                }
                else
                {
                    // if not then proceed to insertion
                    try
                    {
                        TableOperation insertOperation = TableOperation.Insert(feedback); // insertion action
                        TableResult insertResult = table.ExecuteAsync(insertOperation).Result; // execute and obtain the result
                        TempData["Message"] = "Notice: Feedback added. ";
                    }
                    catch (Exception ex)
                    {
                        // ViewBag.result = 100; // give error code
                        // ViewBag.message = "Error: " + ex.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // the data might not inserted yet which is why will return an error during the retrieval, so proceed for insertion instead
                try
                {
                    TableOperation insertOperation = TableOperation.Insert(feedback); // insertion action
                    TableResult insertResult = table.ExecuteAsync(insertOperation).Result; // execute and obtain the result
                    TempData["Message"] = "Notice: Feedback added. ";
                }
                catch (Exception exr)
                {
                    // ViewBag.result = 100; // give error code
                    // ViewBag.message = "Error: " + exr.ToString();
                }
            }

            return RedirectToAction("ViewFeedback", new { id = RowKey });
        }

        public ActionResult DeleteFeedback(string RowKey)
        {
            CloudTable table = GetFeedbackTableInformation();

            FeedbackEntity deleteFeedback = new FeedbackEntity(_userManager.GetUserId(User), RowKey) { ETag = "*" };

            try
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteFeedback); // deletion action
                table.ExecuteAsync(deleteOperation); // execute and obtain the result
                
                TempData["Message"] = "Notice: Feedback for Order ID: " + RowKey + " removed. ";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Notice: Feedback unable to be removed, please try again. ";
                // ViewBag.result = 100; // give error code
                // ViewBag.message = "Error: " + ex.ToString();
            }

            return RedirectToAction("ViewFeedback", new { id = RowKey });
        }
    }
}