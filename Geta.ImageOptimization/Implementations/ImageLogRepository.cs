using System;
using System.Linq;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using Geta.ImageOptimization.Interfaces;
using Geta.ImageOptimization.Models;

namespace Geta.ImageOptimization.Implementations
{
    public class ImageLogRepository : IImageLogRepository
    {
        private static DynamicDataStore Store
        {
            get
            {
                return typeof(ImageLogEntry).GetStore();
            }
        }

        public ImageLogEntry GetLogEntry(Identity id)
        {
            return Store.Load<ImageLogEntry>(id);
        }

        public ImageLogEntry GetLogEntry(string imageUrl)
        {
            return Store.Find<ImageLogEntry>("ImageUrl", imageUrl).FirstOrDefault();
        }

        public IQueryable<ImageLogEntry> GetAllLogEntries()
        {
            return Store.Items<ImageLogEntry>();
        }

        public Identity Save(ImageLogEntry imageLogEntry)
        {
            imageLogEntry.Modified = DateTime.Now;

            if (imageLogEntry.Id != null)
            {
                return Store.Save(imageLogEntry, imageLogEntry.Id);
            }
            else
            {
                return Store.Save(imageLogEntry);
            }
        }
    }
}