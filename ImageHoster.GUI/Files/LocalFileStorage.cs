using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ImageHoster.GUI.Files
{
    public class LocalFileStorage : IFileService
    {
        private string root;

        public LocalFileStorage(string root)
        {
            this.root = root;
        }

        public void Setup()
        {

        }

        public void Upload(string path, Stream data, string contentType, Size thumbnailSize)
        {
            string fullpath = Path.Combine(root, path);

            MemoryStream memorystream;

            if (data is MemoryStream)
                memorystream = data as MemoryStream;
            else
            {
                memorystream = new MemoryStream();
                data.CopyTo(memorystream);
            }

            File.WriteAllBytes(fullpath, memorystream.ToArray());

            Image image = Image.FromStream(data);
            Image thumb = image.GetThumbnailImage(thumbnailSize.Width, thumbnailSize.Height, () => true, IntPtr.Zero);
            thumb.Save(fullpath + "t");
        }

        public async Task<ApplicationFile> Get(string filename, string thumb)
        {
            string path = Path.Combine(root, filename + thumb);
            var stream = File.OpenRead(path);
            var memory = new MemoryStream();
            await stream.CopyToAsync(memory);

            string mime = System.Web.MimeMapping.GetMimeMapping(filename);

            return new ApplicationFile() { Data = memory, ContentType = mime };
        }

        public void Delete(string filename)
        {
            File.Delete(Path.Combine(root, filename));
        }
    }
}