using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ImageHoster.GUI.Files
{
    public class AzureFileStorage : IFileService
    {
        private string ConnectionStringName { get; set; }
        private CloudStorageAccount Account { get; set; }
        private CloudBlobContainer Container { get; set; }

        private string containerName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cstringname">Name of the connection string in the Web.Config</param>
        public AzureFileStorage(string cstringname, string containername)
        {
            ConnectionStringName = cstringname;
            containerName = containername;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Setup()
        {
            Account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorage"]);
            CloudBlobClient client = Account.CreateCloudBlobClient();
            Container = client.GetContainerReference(containerName);
            Container.CreateIfNotExistsAsync();
        }

        public void Upload(string path, Stream data, string contentType, Size thumbnailSize)
        {
            ICloudBlob blob = Container.GetBlockBlobReference(path);
            blob.UploadFromStream(data);
            blob.Properties.ContentType = contentType;
            blob.SetProperties();
            blob = null;
            data.Position = 0;

            Image image = Image.FromStream(data);
            Image thumb = image.GetThumbnailImage(thumbnailSize.Width, thumbnailSize.Height, () => true, IntPtr.Zero);
            MemoryStream thumbdata = new MemoryStream();
            thumb.Save(thumbdata, ImageFormat.Jpeg);
            thumbdata.Flush();
            string thumbnailpath = Path.GetFileNameWithoutExtension(path) + "t" + Path.GetExtension(path);
            thumbdata.Position = 0;

            ICloudBlob thumbblob = Container.GetBlockBlobReference(thumbnailpath);
            thumbblob.UploadFromStream(thumbdata);
            thumbblob.Properties.ContentType = "image/jpg";
            thumbblob.SetProperties();
        }

        public async Task<ApplicationFile> Get(string filename, string thumb)
        {
            string path = Path.GetFileNameWithoutExtension(filename) + thumb + Path.GetExtension(filename);
            ICloudBlob blob = Container.GetBlockBlobReference(path);
            var target = new MemoryStream();
            await blob.DownloadRangeToStreamAsync(target, null, null);

            return new ApplicationFile() { ContentType = blob.Properties.ContentType, Data = target };
        }

        public async void Delete(string filename)
        {
            ICloudBlob blob = await Container.GetBlobReferenceFromServerAsync(filename);
            string thumbnailpath = Path.GetFileNameWithoutExtension(filename) + "t" + Path.GetExtension(filename);
            ICloudBlob thumbblob = await Container.GetBlobReferenceFromServerAsync(thumbnailpath);

            await blob.DeleteAsync();
            await thumbblob.DeleteAsync();
        }
    }
}