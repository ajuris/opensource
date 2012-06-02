using System.Web.Security;

namespace Geta.Security
{
    public class WinActiveDirectoryMembershipProvider : ActiveDirectoryMembershipProvider
    {
        protected string Normalize(string userName)
        {
            int bsIndex = userName.IndexOf('\\');
            return bsIndex > 0 ? userName.Substring(bsIndex + 1) : userName;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return base.GetUser(Normalize(username), userIsOnline);
        }
    }
}
