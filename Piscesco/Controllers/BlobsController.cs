using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Piscesco.Controllers
{
    public class BlobsController
    {

        //Get container
        private CloudBlobContainer GetBlobContainerInformation()
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

            //1.3 create client object to refer to the correct container
            CloudBlobClient clientAgent = accountDetails.CreateCloudBlobClient();
            CloudBlobContainer container = clientAgent.GetContainerReference("imageassets");

            return container;
        }

        public void CreateContainer()
        {
            CloudBlobContainer container = GetBlobContainerInformation();
            container.CreateIfNotExistsAsync();

        }

        public bool UploadImage(IFormFile files, string imageUUID)
        {
            CloudBlobContainer container = GetBlobContainerInformation();
            container.CreateIfNotExistsAsync();

            CloudBlockBlob blobitem = null;
            //blobitem = container.GetBlockBlobReference(imageUUID);
            string message = null;
            try { 
            

            blobitem = container.GetBlockBlobReference(imageUUID);
            var stream = files.OpenReadStream();
            blobitem.UploadFromStreamAsync(stream).Wait();
            message += "The file of " + blobitem.Name + "is now uploaded";
            return true;
                
            }
            catch (Exception ex)
            {
                    message = message + "The file of " + blobitem.Name + "could not be uploaded";
                    message = message + ex.ToString();
                return false;
            }
        }



        public Uri GetImage(string imageName)
        {
            CloudBlobContainer container = this.GetBlobContainerInformation();

            try
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(imageName);
                return blob.Uri;
            }
            catch (Exception ex)
            {

                return null;
            }
            
            

            
        }
    
    
    
    
    }


}





