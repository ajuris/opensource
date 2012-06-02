using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.DirectoryServices;
using System.Web.Security;
using EPiServer.Security;

namespace Geta.Security
{
    /// <summary>
    ///   ASP.NET 2.0 Role provider for Microsofts Active Directory.
    /// </summary>
    /// <remarks>
    ///   This role provider is designed to work together with the default Systwm.Web.Security.ActiveDirectoryMembershipProvider, although there are no hard dependencies.
    /// </remarks>
    public class ActiveDirectoryRoleProvider : RoleProvider
    {
        private const string _memberOfAttribute = "memberOf";
        private const string _membersAttribute = "member";
        private string _applicationName;
        private DirectoryDataFactory _factory;
        private string _roleNameAttribute;
        private string _userNameAttribute;

        /// <summary>
        ///   Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <value> </value>
        /// <returns> The name of the application to store and retrieve role information for. </returns>
        public override string ApplicationName
        {
            get { return this._applicationName; }
            set { this._applicationName = value; }
        }

        /// <summary>
        ///   Initializes the provider.
        /// </summary>
        /// <param name="name"> The friendly name of the provider. </param>
        /// <param name="config"> A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider. </param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call
        ///   <see
        ///     cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see>
        ///   on a provider after the provider has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (String.IsNullOrEmpty(name))
            {
                name = "ActiveDirectoryRoleProvider";
            }

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Active Directory Role provider");
            }

            TryGetDestructive(config, "applicationName", out this._applicationName);

            if (!TryGetDestructive(config, "attributeMapUsername", out this._userNameAttribute))
            {
                this._userNameAttribute = "userPrincipalName";
            }

            if (!TryGetDestructive(config, "attributeMapRolename", out this._roleNameAttribute))
            {
                // Possible values are sAMAccountName, displayName or cn
                this._roleNameAttribute = "sAMAccountName";
            }

            this._factory = new DirectoryDataFactory(config);
            this._factory.AddPropertyToLoad(this._userNameAttribute);
            this._factory.AddPropertyToLoad(this._roleNameAttribute);
            this._factory.AddPropertyToLoad(_memberOfAttribute);
            this._factory.AddPropertyToLoad(_membersAttribute);

            base.Initialize(name, config);

            if (config.Count > 0)
            {
                throw new ProviderException("Illegal configuration element " + config.AllKeys[0]);
            }
        }

        /// <summary>
        ///   Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName"> The role to search in. </param>
        /// <param name="usernameToMatch"> The user name to search for. </param>
        /// <returns> A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role. </returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            DirectoryData role = GetRole(roleName);
            if (role == null)
            {
                throw new ProviderException(String.Format("The role {0} does not exist.", roleName));
            }

            ICollection<DirectoryData> matchingUsers =
                this._factory.FindAll(String.Format("(&({0}={1})(objectClass=user))", this._userNameAttribute, usernameToMatch),
                                      SearchScope.Subtree,
                                      this._userNameAttribute);

            if (matchingUsers == null)
            {
                return new string[0];
            }

            var userList = new List<string>();
            foreach (DirectoryData user in matchingUsers)
            {
                var rolesForUser = new List<string>(user[_memberOfAttribute]);
                if (rolesForUser.Contains(role.DistinguishedName))
                {
                    userList.Add(user.GetFirstPropertyValue(this._userNameAttribute));
                }
            }

            return userList.ToArray();
        }

        /// <summary>
        ///   Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns> A string array containing the names of all the roles stored in the data source for the configured applicationName. </returns>
        public override string[] GetAllRoles()
        {
            ICollection<DirectoryData> allRoles = this._factory.FindAll("(objectClass=group)", SearchScope.Subtree, this._roleNameAttribute);
            if (allRoles == null)
            {
                return new string[0];
            }

            var roleList = new List<string>();
            foreach (DirectoryData ds in allRoles)
            {
                DirectoryData role = this._factory.GetEntry(ds.DistinguishedName);
                if (role == null)
                {
                    continue;
                }
                roleList.Add(role[this._roleNameAttribute][0]);
            }

            return roleList.ToArray();
        }

        /// <summary>
        ///   Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username"> The user to return a list of roles for. </param>
        /// <returns> A string array containing the names of all the roles that the specified user is in for the configured applicationName. </returns>
        public override string[] GetRolesForUser(string username)
        {
            DirectoryData userResult = GetUser(username);

            if (userResult == null)
            {
                return new string[0];
            }

            var roleList = GetRolesForUserRecursive(userResult);
            return roleList.ToArray();
        }

        /// <summary>
        ///   Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName"> The name of the role to get the list of users for. </param>
        /// <returns> A string array containing the names of all the users who are members of the specified role for the configured applicationName. </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            // GetRole is used rather than RoleExists to avoid "getting" the role twice, should it exist.
            DirectoryData roleData = GetRole(roleName);
            if (roleData == null)
            {
                throw new ProviderException(String.Format("The role {0} does not exist.", roleName));
            }

            var userList = new List<string>();
            string[] members;
            if (roleData.TryGetValue(_membersAttribute, out members))
            {
                foreach (string userDistinguishedName in members)
                {
                    DirectoryData userEntry = this._factory.GetEntry(userDistinguishedName);
                    if (userEntry.SchemaClassName != "user")
                    {
                        continue;
                    }

                    // Only show users that are within our root context.
                    if (this._factory.IsWithinSubtree(userDistinguishedName))
                    {
                        userList.Add(userEntry.GetFirstPropertyValue(this._userNameAttribute));
                    }
                }
            }

            return userList.ToArray();
        }

        /// <summary>
        ///   Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username"> The user name to search for. </param>
        /// <param name="roleName"> The role to search in. </param>
        /// <returns> true if the specified user is in the specified role for the configured applicationName; otherwise, false. </returns>
        public override bool IsUserInRole(string userName, string roleName)
        {
            DirectoryData dd = GetUser(userName);
            if (dd == null)
            {
                throw new ProviderException(String.Format("User {0} does not exist.", userName));
            }

            return IsUserInRoleRecursive(dd.DistinguishedName, GetRole(roleName));
        }

        /// <summary>
        ///   Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName"> The name of the role to search for in the data source. </param>
        /// <returns> true if the role name already exists in the data source for the configured applicationName; otherwise, false. </returns>
        public override bool RoleExists(string roleName)
        {
            return GetRole(roleName) != null;
        }

        #region Not supported methods

        /// <summary>
        ///   Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames"> A string array of user names to be added to the specified roles. </param>
        /// <param name="roleNames"> A string array of the role names to add the specified user names to. </param>
        /// <remarks>
        ///   This method is not supported in this provider.
        /// </remarks>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName"> The name of the role to create. </param>
        /// <remarks>
        ///   This method is not supported in this provider.
        /// </remarks>
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName"> The name of the role to delete. </param>
        /// <param name="throwOnPopulatedRole"> If true, throw an exception if roleName has one or more members and do not delete roleName. </param>
        /// <returns> true if the role was successfully deleted; otherwise, false. </returns>
        /// <remarks>
        ///   This method is not supported in this provider.
        /// </remarks>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames"> A string array of user names to be removed from the specified roles. </param>
        /// <param name="roleNames"> A string array of role names to remove the specified user names from. </param>
        /// <remarks>
        ///   This method is not supported in this provider.
        /// </remarks>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private helper methods

        protected string NormalizeUserName(string userName)
        {
            int bsIndex = userName.IndexOf('\\');
            if (bsIndex > 0)
            {
                userName = userName.Substring(bsIndex + 1);
            }

            return userName;
        }

        private DirectoryData GetUser(string userName)
        {
            return
                this._factory.FindOne(String.Format("(&({0}={1})(objectClass=user))", this._userNameAttribute, NormalizeUserName(userName)),
                                      SearchScope.Subtree);
        }

        private DirectoryData GetRole(string roleName)
        {
            return this._factory.FindOne(String.Format("(&({0}={1})(objectClass=group))", this._roleNameAttribute, roleName),
                                         SearchScope.Subtree);
        }

        private bool IsUserInRoleRecursive(string distinguishedUserName, DirectoryData roleEntry)
        {
            if (roleEntry == null)
            {
                return false;
            }

            string[] members;
            if (!roleEntry.TryGetValue(_membersAttribute, out members))
            {
                // The membership attribute does not exists for this item, cannot be a member here
                return false;
            }

            foreach (string distinguishedMemberName in members)
            {
                if (String.Compare(distinguishedMemberName, distinguishedUserName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                DirectoryData member = this._factory.GetEntry(distinguishedMemberName);
                if (member == null)
                {
                    continue;
                }

                if (member.SchemaClassName == "group")
                {
                    return IsUserInRoleRecursive(distinguishedUserName, member);
                }
            }

            return false;
        }

        private List<string> GetRolesForUserRecursive(DirectoryData entry)
        {
            var subtree = new List<string>();
            string[] propertyValue;
            if (entry.TryGetValue(_memberOfAttribute, out propertyValue))
            {
                foreach (string role in propertyValue)
                {
                    DirectoryData roleEntry = this._factory.GetEntry(role);
                    if (roleEntry == null || roleEntry.SchemaClassName != "group")
                    {
                        continue;
                    }
                    string[] roleName;
                    if (roleEntry.TryGetValue(this._roleNameAttribute, out roleName))
                    {
                        subtree.Add(roleName[0]);
                    }
                    subtree.AddRange(GetRolesForUserRecursive(roleEntry));
                }
            }

            return subtree;
        }

        private bool TryGetDestructive(NameValueCollection config, string name, out string value)
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

        #endregion
    }
}
