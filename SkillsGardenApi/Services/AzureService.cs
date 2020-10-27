using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public interface IAzureService
    {
        Task<string> saveImageToBlobStorage(FormFile file);
        bool deleteImageFromBlobStorage(string imageName);
        bool doesBlobExist(string imageName);
        string getBlobSas(string imageName);

    }

    public class AzureService : IAzureService
    {
        private CloudBlobContainer blobContainer;

        public AzureService() {
            // connection to storage account
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

            // create blob client
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            this.blobContainer = blobClient.GetContainerReference("images");
            this.blobContainer.CreateIfNotExists();
        }

        public async Task<string> saveImageToBlobStorage(FormFile file)
        {
            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            blobName = blobName.Replace("\"", "");

            var cloudBlockBlob = blobContainer.GetBlockBlobReference(blobName);

            cloudBlockBlob.Properties.ContentType = file.ContentType;
            cloudBlockBlob.Metadata.Add("origName", file.FileName);

            using (var fileStream = file.OpenReadStream())
            {
                await cloudBlockBlob.UploadFromStreamAsync(fileStream);
            }

            return blobName;
        }

        public bool deleteImageFromBlobStorage(string imageName)
        {
            //get imageblob by imagename
            var blob = blobContainer.GetBlockBlobReference(imageName);

            //delete image and return true if successful
            return blob.DeleteIfExists();
        }

        public bool doesBlobExist(string imageName)
        {
            // create reference for storage container
            CloudBlockBlob blob = this.blobContainer.GetBlockBlobReference(imageName);

            return blob.Exists();
        }

        public string getBlobSas(string imageName)
        {
            // create reference for storage container
            CloudBlockBlob blob = this.blobContainer.GetBlockBlobReference(imageName);

            // create access policy
            var sasPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.UtcNow.AddDays(-1),
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1)
            };
            var sasToken = blob.GetSharedAccessSignature(sasPolicy);

            // return url
            return new Uri(blob.Uri, sasToken).AbsoluteUri;
        }
    }
}
