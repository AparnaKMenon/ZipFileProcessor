using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipFileProcessor.Services
{
    internal interface IZipProcessingService
    {
        public Task ProcessZipFilesAsync();
    }
}
