using System;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Geta.ImageOptimization.Models
{
    [EPiServerDataStore(AutomaticallyRemapStore = true, AutomaticallyCreateStore = true)]
    public class ImageLogEntry
    {
        public Identity Id { get; set; }

        public string VirtualPath { get; set; }

        [EPiServerDataIndex]
        public string ImageUrl { get; set; }

        public DateTime Modified { get; set; }

        public decimal PercentSaved { get; set; }

        public int OriginalSize { get; set; }

        public int OptimizedSize { get; set; }

        public bool IsOptimized { get; set; }
    }
}