using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Caching;
using System.Xml;
using System.Xml.Serialization;

namespace Geta.CacheManager.Admin
{
    internal class Common
    {
        internal static MemoryStream BinarySerialize(object objectToSerialize)
        {
            MemoryStream serializationStream = new MemoryStream();
            new BinaryFormatter().Serialize(serializationStream, objectToSerialize);
            return serializationStream;
        }

        public static void ClearCache()
        {
            HttpContext current = HttpContext.Current;
            Cache cache = current != null ? current.Cache : HttpRuntime.Cache;
            ArrayList list = GetCacheList();
            foreach (object obj2 in list)
            {
                cache.Remove(obj2.ToString());
            }
        }

        public static ArrayList GetCacheList()
        {
            HttpContext current = HttpContext.Current;
            Cache cache = current != null ? current.Cache : HttpRuntime.Cache;
            var list = new ArrayList();
            IDictionaryEnumerator enumerator = cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Key.ToString());
            }
            return list;
        }

        public static bool DeleteCacheItem(string item)
        {
            try
            {
                HttpContext current = HttpContext.Current;
                Cache cache = current != null ? current.Cache : HttpRuntime.Cache;
                cache.Remove(item);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool IsNullorEmpty(string text)
        {
            return ((text == null) || (text.Trim() == string.Empty));
        }

        internal static StringWriter XmlSerialize(object objectToSerialize)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(objectToSerialize.GetType());
                StringWriter textWriter = new StringWriter();
                XmlQualifiedName[] namespaces = new XmlQualifiedName[] { new XmlQualifiedName() };
                serializer.Serialize(textWriter, objectToSerialize, new XmlSerializerNamespaces(namespaces));
                return textWriter;
            }
            catch (InvalidOperationException)
            {
            }
            catch (SerializationException)
            {
            }
            catch (Exception exception)
            {
                string message = exception.Message;
            }
            return null;
        }
    }
}