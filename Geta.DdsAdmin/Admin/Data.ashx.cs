using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.Security;
using Geta.DdsAdmin.Dds;
using log4net;
using Newtonsoft.Json;

namespace Geta.DdsAdmin.Admin
{
    public class Data : IHttpHandler
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DdsAdmin));

        public Data()
        {
            Explorer = new Store();
        }

        public Store Explorer { get; set; }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            logger.Info("Started request");
            try
            {
                if (!PrincipalInfo.HasAdminAccess)
                {
                    context.Response.StatusCode = 401;
                    context.Response.End();
                    return;
                }

                // read mandatory parameters
                string operation = context.Request.QueryString["operation"];
                logger.InfoFormat("Operation:{0}", operation);
                string store = context.Request.QueryString["store"];
                logger.InfoFormat("Store:{0}", operation);

                // common header
                context.Response.Clear();
                context.Response.ClearContent();
                context.Response.ContentType = "application/json; charset=UTF-8";

                // specific operations
                if (operation == "read")
                {
                    int displayStart = Convert.ToInt32(context.Request.QueryString["iDisplayStart"]);
                    int displayLength = Convert.ToInt32(context.Request.QueryString["iDisplayLength"]);
                    int echo = Convert.ToInt32(context.Request.QueryString["sEcho"]);
                    string search = context.Request.QueryString["sSearch"];

                    Read(context, echo, displayStart, store, displayLength, search);
                }
                else if (operation == "update")
                {
                    int columnId = Convert.ToInt32(context.Request.Form["columnId"]);
                    string columnName = context.Request.Form["columnName"];
                    string id = context.Request.Form["id"];
                    string value = context.Request.Form["value"];

                    Update(context, columnId, value, id, columnName, store);
                }
                else if (operation == "delete")
                {
                    string id = context.Request.Form["id"];
                    Delete(context, store, id);
                }
                else if (operation == "create")
                {
                    var values = new Dictionary<string, string>();
                    foreach (var item in context.Request.Form.AllKeys)
                    {
                        values.Add(item.Substring(5), context.Request.Form[item]);
                    }

                    Create(context, values, store);
                }
                else
                {
                    logger.ErrorFormat("Operation:{0} is not implemented!", operation);
                    throw new NotImplementedException(string.Format("Operation:{0} is not implemented!", operation));
                }

                context.Response.End();
            }
            catch (NotImplementedException ex)
            {
                context.Response.StatusCode = 501;
                context.Response.Write(JsonConvert.SerializeObject(ex.Message));
                logger.Error(ex);
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured:", ex);
                throw;
            }

            logger.Info("Finished request");
        }

        /// <summary>
        /// Create new item
        /// </summary>
        /// <param name="context">
        /// context
        /// </param>
        /// <param name="values">
        /// key-value pairs of store type
        /// </param>
        /// <param name="store">
        /// store type name to be used
        /// </param>
        private void Create(HttpContext context, Dictionary<string, string> values, string store)
        {
            logger.Debug("Create started");
            StoreInfo storeInfo = Explorer.Explore().First(s => s.Name == store);

            var newItem = Explorer.Create(storeInfo, store, values);

            if (newItem != null)
            {
                context.Response.Write(newItem.Id.ToString());
            }
            else
            {
                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject("Could not create row!"));
                logger.Debug("Create failed - Could not create row!");
            }

            logger.Debug("Create finished");
        }

        /// <summary>
        /// delete item by id
        /// </summary>
        /// <param name="context">
        /// context
        /// </param>
        /// <param name="store">
        /// store type name to be used
        /// </param>
        /// <param name="id">
        /// Identity Id in string form
        /// </param>
        private void Delete(HttpContext context, string store, string id)
        {
            logger.Debug("Delete started");
            Identity identity = null;
            if (Identity.TryParse(id, out identity) && identity != null)
            {
                if (Explorer.Delete(store, identity))
                {
                    context.Response.Write("ok");
                }
                else
                {
                    context.Response.StatusCode = 500;
                    context.Response.Write(JsonConvert.SerializeObject("Could not delete row!"));
                }
            }
            else
            {
                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject("Could not interpret Id!"));
            }

            logger.Debug("Delete finished");
        }

        private List<List<string>> GetData(StoreInfo storeInfo, IEnumerable<PropertyBag> data)
        {
            var stringData = new List<List<string>>();
            foreach (var row in data)
            {
                var item = new List<string> {row.Id.ToString()};
                foreach (var column in storeInfo.Columns)
                {
                    item.Add(row[column.PropertyName] == null ? null : row[column.PropertyName].ToString());
                }

                stringData.Add(item);
            }

            return stringData;
        }

        /// <summary>
        /// listing/paging/searching of items
        /// </summary>
        /// <param name="context">
        /// context
        /// </param>
        /// <param name="echo">
        /// used by datatables
        /// </param>
        /// <param name="displayStart">
        /// page start index
        /// </param>
        /// <param name="store">
        /// store type name to be used
        /// </param>
        /// <param name="displayLength">
        /// page size
        /// </param>
        /// <param name="search">
        /// full text search criteria
        /// </param>
        private void Read(HttpContext context, 
                          int echo, 
                          int displayStart, 
                          string store, 
                          int displayLength, 
                          string search)
        {
            logger.Debug("Read started");
            int totalCount = 0;

            StoreInfo storeInfo = Explorer.Explore().First(s => s.Name == store);

            int sortCol = Convert.ToInt32(context.Request.QueryString["iSortCol_0"]);
            string sortDir = context.Request.QueryString["sSortDir_0"];

            // TODO: we cannot order here due to fact that this is Propertybag, if we could it would be great performance boost
            // var orderBy = sortCol == 0 ? "Id" : storeInfo.Columns.ToList()[sortCol - 1].PropertyName;
            var query = DynamicDataStoreFactory.Instance.GetStore(store).ItemsAsPropertyBag(); // .OrderBy(orderBy);

            var data = sortCol == 0 && string.IsNullOrEmpty(search)
                           ? (sortDir == "asc"
                                  ? query.OrderBy(r => r.Id).Skip(displayStart).Take(displayLength).ToList()
                                  : query.OrderByDescending(r => r.Id).Skip(displayStart).Take(displayLength).ToList())
                           : query.ToList();

            var stringData = new List<List<string>>();
            if (sortCol == 0 && string.IsNullOrEmpty(search))
            {
                // no sorting and no filtering, use fast code then
                stringData = GetData(storeInfo, data);
                totalCount = query.Count();
            }
            else
            {
                totalCount = SortAndFilterData(displayStart, displayLength, sortCol, sortDir, search, storeInfo, data, ref stringData);
            }

            var result = new
                             {
                                 sEcho = echo, 
                                 iTotalRecords = totalCount, 
                                 iTotalDisplayRecords = totalCount, 
                                 aaData = stringData
                             };

            context.Response.Write(JsonConvert.SerializeObject(result));
            logger.Debug("Read finished");
        }

        private int SortAndFilterData(int displayStart, 
                                      int displayLength, 
                                      int sortCol, 
                                      string sortDir, 
                                      string search, 
                                      StoreInfo storeInfo, 
                                      IEnumerable<PropertyBag> data, 
                                      ref List<List<string>> stringData)
        {
            foreach (var row in data)
            {
                bool containsSearchCriteria = string.IsNullOrEmpty(search);

                var item = new List<string> {row.Id.ToString()};
                if (!containsSearchCriteria && row.Id.ToString().Contains(search))
                {
                    containsSearchCriteria = true;
                }

                foreach (var column in storeInfo.Columns)
                {
                    if (row[column.PropertyName] != null)
                    {
                        var value = row[column.PropertyName].ToString();
                        item.Add(value);
                        if (!containsSearchCriteria && value.Contains(search))
                        {
                            containsSearchCriteria = true;
                        }
                    }
                    else
                    {
                        item.Add(null);
                    }
                }

                if (containsSearchCriteria)
                {
                    stringData.Add(item);
                }
            }

            int totalCount = stringData.Count();

            // reorder stringData and skip and take just needed
            if (sortDir == "asc")
            {
                stringData = (from sd in stringData orderby sd[sortCol] ascending select sd).Skip(displayStart).Take(displayLength).ToList();
            }
            else
            {
                stringData =
                    (from sd in stringData orderby sd[sortCol] descending select sd).Skip(displayStart).Take(displayLength).ToList();
            }

            return totalCount;
        }

        /// <summary>
        /// update item column
        /// </summary>
        /// <param name="context">
        /// context
        /// </param>
        /// <param name="columnId">
        /// column index
        /// </param>
        /// <param name="value">
        /// value as string
        /// </param>
        /// <param name="id">
        /// item Identity id as string
        /// </param>
        /// <param name="columnName">
        /// column name
        /// </param>
        /// <param name="store">
        /// store type name to be used
        /// </param>
        private void Update(HttpContext context, int columnId, string value, string id, string columnName, string store)
        {
            logger.DebugFormat("Update finished");
            StoreInfo storeInfo = Explorer.Explore().First(s => s.Name == store);
            var column = storeInfo.Columns.Where(item => item.PropertyName == columnName).First();

            Identity identity = null;
            if (Identity.TryParse(id, out identity) && identity != null)
            {
                if (Explorer.Update(store, identity, columnId, columnName, value))
                {
                    context.Response.Write(JsonConvert.SerializeObject(value));
                }
                else
                {
                    context.Response.StatusCode = 500;
                    context.Response.Write(JsonConvert.SerializeObject("Could not save cell!"));
                    logger.Error("Update failed - Could not save cell!");
                }
            }
            else
            {
                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject("Could not interpret Id!"));
                logger.Error("Update failed - Could not interpret Id!");
            }

            logger.Debug("Update finished");
        }
    }
}
