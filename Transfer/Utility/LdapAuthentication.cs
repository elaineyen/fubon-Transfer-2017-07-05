using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace Transfer.Utility
{
    public static class LdapAuthentication
    {
        public static bool isAuthenticatrd(string path, string domain, string userId,string pwd)
        {
            try
            {
                string domainAndUsername;
                if (!domain.IsNullOrWhiteSpace())
                    domainAndUsername = string.Concat(domain, "\\", userId);
                else
                    domainAndUsername = userId;
                using (DirectoryEntry entry = new DirectoryEntry(
                    path, domainAndUsername, pwd, AuthenticationTypes.None))
                {
                    Object obj = entry.NativeObject;
                    DirectorySearcher search = new DirectorySearcher(entry);
                    search.Filter = "(SAMAccountName=" + userId + ")";
                    search.PropertiesToLoad.Add("cn");
                    SearchResult result = search.FindOne();
                    var cn = result.Properties["cn"][0];

                    return (result != null);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}