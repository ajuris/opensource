using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using log4net;

namespace Geta.DdsAdmin.Dds
{
    public class Store
    {
        private static ILog logger = LogManager.GetLogger(typeof(Store));

        /// <summary>
        /// create new item
        /// </summary>
        /// <param name="storeInfo">entity metadata</param>
        /// <param name="storeName">store type name</param>
        /// <param name="values">key value pairs of columns and their respective values</param>
        /// <returns>newly created item identity</returns>
        public PropertyBag Create(StoreInfo storeInfo, string storeName, Dictionary<string, string> values)
        {
            try
            {
                var store = DynamicDataStoreFactory.Instance.GetStore(storeName);
                var item = new PropertyBag { Id = Identity.NewIdentity() };
                foreach (var value in values)
                {
                    if (value.Key == "Id")
                    {
                        // skip id when creating new record
                        continue;
                    }

                    var meta = storeInfo.Columns.Where(si => si.PropertyName == value.Key).First();

                    if (meta is CollectionPropertyMap)
                    {
                        throw new NotImplementedException(string.Format("Saving CollectionPropertyMap field name = {0} is not yet implemented!", value.Key));
                    }

                    item[value.Key] = value.Value;
                }

                store.Save(item);

                return item;
            }
            catch (NotImplementedException ex)
            {
                logger.Error(ex);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Create row failed", ex);
                return null;
            }
        }

        /// <summary>
        /// delete item
        /// </summary>
        /// <param name="storeName">store type name</param>
        /// <param name="id">Identity</param>
        /// <returns>true if successfully deleted</returns>
        public bool Delete(string storeName, Identity id)
        {
            try
            {
                var store = DynamicDataStoreFactory.Instance.GetStore(storeName);
                var query = store.ItemsAsPropertyBag().Where(items => items.Id.Equals(id));
                var item = query.First();

                store.Delete(item);
                return true;
            }
            catch(Exception ex)
            {
                logger.Error("Delete failed", ex);
                return false;
            }
        }

        public IEnumerable<StoreInfo> Explore()
        {
            foreach(var info in Scout().ToList())
            {
                info.Rows = DynamicDataStoreFactory.Instance.GetStore(info.Name).Items().Count(); // select n+1
                yield return info;
            }
        }

        /// <summary>
        /// update item column value
        /// </summary>
        /// <param name="storeName">store type name</param>
        /// <param name="id">Identity</param>
        /// <param name="columnId">column index</param>
        /// <param name="columnName">column name</param>
        /// <param name="value">column value</param>
        /// <returns>true if successfully updated</returns>
        public bool Update(string storeName, Identity id, int columnId, string columnName, object value)
        {
            try
            {
                var store = DynamicDataStoreFactory.Instance.GetStore(storeName);
                var query = store.ItemsAsPropertyBag().Where(items => items.Id.Equals(id));
                var item = query.First();

                item[columnName] = value;
                store.Save(item);
                return true;
            }
            catch(Exception ex)
            {
                logger.Error("Update cell failed", ex);
                return false;
            }
        }

        private IEnumerable<StoreInfo> Scout()
        {
            foreach(StoreDefinition sd in StoreDefinition.GetAll())
            {
                yield return new StoreInfo {Name = sd.StoreName, Columns = sd.ActiveMappings};
            }
        }
    }
}
