using System.Linq;
using EPiServer.Data;
using Geta.ImageOptimization.Models;

namespace Geta.ImageOptimization.Interfaces
{
    public interface IImageLogRepository
    {
        ImageLogEntry GetLogEntry(Identity id);

        ImageLogEntry GetLogEntry(string imageUrl);

        IQueryable<ImageLogEntry> GetAllLogEntries();

        Identity Save(ImageLogEntry imageLogEntry);
    }
}