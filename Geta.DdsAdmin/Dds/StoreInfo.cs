using System.Collections.Generic;
using EPiServer.Data.Dynamic;

namespace Geta.DdsAdmin.Dds
{
    public class StoreInfo
    {
        public string Name { get; set; }
        public int Rows { get; set; }
        public IEnumerable<PropertyMap> Columns { get; set; }
    }
}
