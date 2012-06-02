using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.DirectoryServices;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using EPiServer.Security;

namespace Geta.Security
{
    public class DirectoryDataFactory
    {
        // Used to create unique cache keys
        private const string RootCacheKey = "EPiServer:DirectoryServiceRoot";
        private const string CacheKeyEntryPrefix = "EPiServer:DirectoryServiceEntry:";
        private const string CacheKeyFindOnePrefix = "EPiServer:DirectoryServiceFindOne:";
        private const string CacheKeyFindAllPrefix = "EPiServer:DirectoryServiceFindAll:";

        // The attributes / properties to use for extracting Distinguished name and object class from Active Directory data
        private const string DistingushedNameAttribute = "distinguishedName";
        private const string ObjectClassAttribute = "objectClass";
        private readonly TimeSpan _cacheTimeout;
        private string _baseConnectionString;
        private AuthenticationTypes _connectionProtection;

        // Instance variables
        private string _connectionString;
        private string _password;
        private List<string> _propertiesToLoad;
        private string _rootDistinguishedName;
        private string _username;

        /// <summary>
        ///   Initializes a new instance of the <see cref="DirectoryDataFactory" /> class.
        /// </summary>
        /// <param name="connectionString"> The connection string. </param>
        /// <param name="username"> The username. </param>
        /// <param name="password"> The password. </param>
        /// <param name="connectionProtection"> The connection protection. </param>
        /// <param name="absoluteCacheTimeout"> The absolute cache timeout. </param>
        /// <remarks>
        ///   Create and initialize an instance of this class for accessing Active Directory data.
        /// </remarks>
        public DirectoryDataFactory(string connectionString,
                                    string username,
                                    string password,
                                    AuthenticationTypes connectionProtection,
                                    TimeSpan absoluteCacheTimeout)
        {
            this._connectionString = connectionString;
            this._username = username;
            this._password = password;
            this._connectionProtection = connectionProtection;
            this._cacheTimeout = absoluteCacheTimeout;
            Initialize();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DirectoryDataFactory" /> class.
        /// </summary>
        /// <param name="config"> The configuration data to use. </param>
        /// <remarks>
        ///   This constructor is intended to be called from within a providers Initialize method. It will read its settings from the config parameter and initialize this instance. Required settings in config are: connectionStringName = name of the connection string to use for connecting to the Active Directory server. connectionUsername = username for the connection. connectionPassword = password for the connection. Optional settings in config are: connectionProtection = level of protection and security for the Active Directory connection. None or Secure (default). cacheTimeout = how long cached Active Directory entries stys in the cache. Default is 10 minutes.
        /// </remarks>
        public DirectoryDataFactory(NameValueCollection config)
        {
            string connectionStringName;
            if (!TryGetDestructive(config, "connectionStringName", out connectionStringName))
            {
                throw new ProviderException("Required attribute connectionStringName not supplied.");
            }

            var settings = WebConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null)
            {
                throw new ProviderException(String.Format("Connection string {0} not found.", connectionStringName));
            }

            this._connectionString = settings.ConnectionString;
            if (String.IsNullOrEmpty(this._connectionString))
            {
                throw new ProviderException(String.Format("Connection string {0} is empty.", connectionStringName));
            }

            if (!TryGetDestructive(config, "connectionUsername", out this._username))
            {
                throw new ProviderException("Required attribute connectionUsername not supplied.");
            }

            if (!TryGetDestructive(config, "connectionPassword", out this._password))
            {
                throw new ProviderException("Required attribute connectionPassword not supplied.");
            }

            this._connectionProtection = AuthenticationTypes.Secure;
            string connectionProtection;
            if (TryGetDestructive(config, "connectionProtection", out connectionProtection))
            {
                try
                {
                    this._connectionProtection = (AuthenticationTypes) Enum.Parse(typeof (AuthenticationTypes), connectionProtection, true);
                }
                catch (ArgumentException)
                {
                    throw new ProviderException(
                        String.Format("Attribute connectionProtection has illegal value {0}, supported values are {1}.",
                                      connectionProtection,
                                      String.Join(", ", Enum.GetNames(typeof (AuthenticationTypes)))));
                }
            }

            string cacheTimeSpan;
            if (TryGetDestructive(config, "cacheTimeout", out cacheTimeSpan))
            {
                if (!TimeSpan.TryParse(cacheTimeSpan, out this._cacheTimeout))
                {
                    throw new ProviderException(
                        String.Format("Attribute cacheTimeout has illegal value {0}, should be formatted as \"hours:minutes:seconds\"",
                                      cacheTimeSpan));
                }
            }
            else
            {
                this._cacheTimeout = new TimeSpan(0, 10, 0);
            }

            Initialize();
        }

        /// <summary>
        ///   Gets or sets the connection string.
        /// </summary>
        /// <value> The connection string. </value>
        /// <remarks>
        ///   The string for connecting to the Active Directory server. It should be in the format LDAP://dns.name.for.machine or LDAP://dns.name.for.active.directory.domain
        /// </remarks>
        public string ConnectionString
        {
            get { return this._connectionString; }
            protected set { this._connectionString = value; }
        }

        /// <summary>
        ///   Gets or sets the user name.
        /// </summary>
        /// <value> The user name. </value>
        /// <remarks>
        ///   The user name to use for connecting to the Active Directory domain as defined by ConnectionString.
        /// </remarks>
        public string Username
        {
            get { return this._username; }
            protected set { this._username = value; }
        }

        /// <summary>
        ///   Gets or sets the password.
        /// </summary>
        /// <value> The password. </value>
        /// <remarks>
        ///   The password used, together with the Username, to connect to the Active Directory domain.
        /// </remarks>
        public string Password
        {
            get { return this._password; }
            protected set { this._password = value; }
        }

        /// <summary>
        ///   Gets or sets the connection protection.
        /// </summary>
        /// <value> The connection protection. </value>
        /// <remarks>
        ///   Defines the type and level of protection for the Active Directory communications.
        /// </remarks>
        public AuthenticationTypes ConnectionProtection
        {
            get { return this._connectionProtection; }
            protected set { this._connectionProtection = value; }
        }

        /// <summary>
        ///   Gets or sets the distinguished name of the root for Active Directory searches.
        /// </summary>
        /// <value> The name of the root. </value>
        public string RootDistinguishedName
        {
            get { return this._rootDistinguishedName; }
            protected set { this._rootDistinguishedName = value; }
        }

        /// <summary>
        ///   Determines whether the specified distinguished name is part of the Active Directory tree used for searches.
        /// </summary>
        /// <param name="distinguishedName"> Distinguished name of an Active Directory entry. </param>
        /// <returns> <c>true</c> if the specified distinguished name is within subtree; otherwise, <c>false</c> . </returns>
        public bool IsWithinSubtree(string distinguishedName)
        {
            return distinguishedName.EndsWith(this._rootDistinguishedName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///   Adds a property to load on access to Active Directory.
        /// </summary>
        /// <param name="propertyName"> Name of the property. </param>
        /// <remarks>
        ///   This class keeps a list of property names to query for whenever an Active Directory entry is retrieved. By calling this method you can expand this list with custom properties.
        /// </remarks>
        public void AddPropertyToLoad(string propertyName)
        {
            if (this._propertiesToLoad.Contains(propertyName))
            {
                return;
            }
            this._propertiesToLoad.Add(propertyName);
            ClearCache();
        }

        /// <summary>
        ///   Clears the Active Directory cache.
        /// </summary>
        public void ClearCache()
        {
            HttpRuntime.Cache.Remove(RootCacheKey);
        }

        /// <summary>
        ///   Gets a specific Active Directory entry as a DirectoryData object.
        /// </summary>
        /// <param name="distinguishedName"> Distinguished name of the entry. </param>
        /// <returns> A DirectoryData instance containing the properties defined by PropertiesToLoad. </returns>
        /// <remarks>
        ///   This method uses caching to speed up the operation, if the DirectoryData object is found in the cache, no communication with the Active Directory domain will take place.
        /// </remarks>
        public DirectoryData GetEntry(string distinguishedName)
        {
            var cacheKey = CacheKeyEntryPrefix + distinguishedName;
            var dd = (DirectoryData) HttpRuntime.Cache[cacheKey];
            if (dd != null)
            {
                return dd;
            }

            try
            {
                using (var entry = CreateDirectoryEntry(distinguishedName))
                {
                    dd = CreateDirectoryDataFromDirectoryEntry(entry);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("GetEntry failed on distinguishedName = \"{0}\"", distinguishedName), ex);
            }

            if (null == dd)
            {
                return null;
            }

            StoreInCache(cacheKey, dd);
            return dd;
        }

        /// <summary>
        ///   Find on Active Directory entry based on search filter.
        /// </summary>
        /// <param name="filter"> The filter, which is a LDAP query. </param>
        /// <param name="scope"> The scope at which to search. </param>
        /// <returns> A DirectoryData instance with the entry, or null if the search did not find an entry. </returns>
        public DirectoryData FindOne(string filter, SearchScope scope)
        {
            var cacheKey = CacheKeyFindOnePrefix + filter + scope;
            var dd = (DirectoryData) HttpRuntime.Cache[cacheKey];
            if (dd != null)
            {
                return dd;
            }

            using (var userSearcher = new DirectorySearcher(CreateDirectoryEntry(), filter, this._propertiesToLoad.ToArray(), scope))
            {
                dd = CreateDirectoryDataFromSearchResult(userSearcher.FindOne());

                if (dd == null)
                {
                    return null;
                }
            }

            StoreInCache(cacheKey, dd);
            return dd;
        }

        public ICollection<DirectoryData> FindAll(string filter, SearchScope scope)
        {
            return FindAll(filter, scope, null);
        }

        public ICollection<DirectoryData> FindAll(string filter, SearchScope scope, string sortByProperty)
        {
            var cacheKey = CacheKeyFindAllPrefix + filter + scope;
            var results = (ICollection<DirectoryData>) HttpRuntime.Cache[cacheKey];
            if (results != null)
            {
                return results;
            }

            using (var userSearcher = new DirectorySearcher(CreateDirectoryEntry(), filter, this._propertiesToLoad.ToArray(), scope))
            {
                using (var searchResults = userSearcher.FindAll())
                {
                    var c = searchResults.Count;
                    if (searchResults == null)
                    {
                        // TODO Should handle search misses in the cache as well
                        return null;
                    }

                    if (sortByProperty == null)
                    {
                        results = new List<DirectoryData>(searchResults.Count);
                        foreach (SearchResult sr in searchResults)
                        {
                            results.Add(CreateDirectoryDataFromSearchResult(sr));
                        }
                    }
                    else
                    {
                        var sortedResults = new SortedList<string, DirectoryData>(searchResults.Count);
                        foreach (SearchResult sr in searchResults)
                        {
                            var dd = CreateDirectoryDataFromSearchResult(sr);
                            sortedResults.Add(dd.GetFirstPropertyValue(sortByProperty), dd);
                        }

                        results = sortedResults.Values;
                    }
                }
            }

            StoreInCache(cacheKey, results);
            return results;
        }

        protected DirectoryEntry CreateDirectoryEntry()
        {
            return new DirectoryEntry(this._connectionString, this._username, this._password, this._connectionProtection);
        }

        protected DirectoryEntry CreateDirectoryEntry(string rootDistinguishedName)
        {
            if (!IsWithinSubtree(rootDistinguishedName))
            {
                return null;
            }

            return new DirectoryEntry(this._baseConnectionString + EscapeCharacters(rootDistinguishedName),
                                      this._username,
                                      this._password,
                                      this._connectionProtection);
        }

        protected string EscapeCharacters(string s)
        {
            var charactersToEscape = new[] {'/'};
            int start = s.IndexOfAny(charactersToEscape);

            if (start == -1)
                return s;

            var builder = new StringBuilder(s.Substring(0, start), s.Length + 6);

            for (int i = start; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '/':
                        builder.Append(@"\/");
                        break;

                    default:
                        builder.Append(s[i]);
                        break;
                }
            }

            return builder.ToString();
        }

        protected DirectoryData CreateDirectoryDataFromDirectoryEntry(DirectoryEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            var properties = new Dictionary<string, string[]>(this._propertiesToLoad.Count);
            foreach (string propName in this._propertiesToLoad)
            {
                if (entry.Properties.Contains(propName))
                {
                    PropertyValueCollection propVal = entry.Properties[propName];
                    var values = new string[propVal.Count];
                    for (int i = 0; i < propVal.Count; i++)
                    {
                        values[i] = propVal[i].ToString();
                    }
                    properties.Add(propName, values);
                }
            }

            return new DirectoryData(DistinguishedName(properties), entry.SchemaClassName, properties);
        }

        protected DirectoryData CreateDirectoryDataFromSearchResult(SearchResult result)
        {
            if (result == null)
            {
                return null;
            }

            var properties = new Dictionary<string, string[]>(this._propertiesToLoad.Count);
            foreach (string propName in this._propertiesToLoad)
            {
                if (result.Properties.Contains(propName))
                {
                    var propVal = result.Properties[propName];
                    var values = new string[propVal.Count];
                    for (int i = 0; i < propVal.Count; i++)
                    {
                        values[i] = propVal[i].ToString();
                    }
                    properties.Add(propName, values);
                }
            }

            return new DirectoryData(DistinguishedName(properties), SchemaClassName(properties), properties);
        }

        protected string DistinguishedName(Dictionary<string, string[]> properties)
        {
            return properties[DistingushedNameAttribute][0];
        }

        protected string SchemaClassName(Dictionary<string, string[]> properties)
        {
            // Get the most specific object class
            string objectClass = String.Empty;
            string[] objectClassList = properties[ObjectClassAttribute];
            if (objectClassList != null)
            {
                objectClass = objectClassList[objectClassList.Length - 1];
            }

            return objectClass;
        }

        protected void StoreInCache(string cacheKey, object data)
        {
            // Make sure that we have a cache root to have all other items dependent on
            if (HttpRuntime.Cache[RootCacheKey] == null)
            {
                HttpRuntime.Cache.Insert(RootCacheKey, new Object());
            }

            // Store item in cache with dependency on root cache key
            HttpRuntime.Cache.Insert(cacheKey,
                                     data,
                                     new CacheDependency(null, new[] {RootCacheKey}),
                                     DateTime.Now.Add(this._cacheTimeout),
                                     Cache.NoSlidingExpiration);
        }

        protected bool TryGetDestructive(NameValueCollection config, string name, out string value)
        {
            value = config[name];
            if (value != null)
            {
                config.Remove(name);
            }

            if (String.IsNullOrEmpty(value))
            {
                value = null;
                return false;
            }

            return true;
        }

        /// <summary>
        ///   Final initialization steps for a newly created instance.
        /// </summary>
        private void Initialize()
        {
            this._propertiesToLoad = new List<string>(5) {DistingushedNameAttribute, ObjectClassAttribute};

            // Construct the base connection string, which should be a string that you can append a distinguished name to
            // If you have connectionString "ldap://mydomain.company.com", baseConnectionString will be "ldap://mydomain.company.com"
            // "LDAP://mydomain.company.com/CN=Some,DC=company,DC=com" -> "LDAP://mydomain.company.com/"
            int hostStart = this._connectionString.IndexOf("://");
            if (hostStart < 0)
            {
                throw new ProviderException(String.Format("Protocol specification missing from connection string {0}",
                                                          this._connectionString));
            }

            int hostEnd = this._connectionString.IndexOf("/", hostStart + 3, StringComparison.Ordinal);
            if (hostEnd < 0)
            {
                this._baseConnectionString = this._connectionString + "/";
            }
            else if (hostEnd + 1 < this._connectionString.Length)
            {
                this._baseConnectionString = this._connectionString.Remove(hostEnd + 1);
            }
            else
            {
                this._baseConnectionString = this._connectionString;
            }

            using (DirectoryEntry entry = CreateDirectoryEntry())
            {
                this._rootDistinguishedName = entry.Properties[DistingushedNameAttribute][0].ToString();
            }
        }
    }
}
