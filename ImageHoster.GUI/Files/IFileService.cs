using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.GUI.Files
{
    public interface IFileService
    {
        void Setup();
        void Upload(string path, Stream data, string contentType, Size thumbnailSize);
        Task<ApplicationFile> Get(string filename, string thumb);
        void Delete(string filename);
    }

    public class ApplicationFile
    {
        public MemoryStream Data { get; set; }
        public string ContentType { get; set; }
    }
}
