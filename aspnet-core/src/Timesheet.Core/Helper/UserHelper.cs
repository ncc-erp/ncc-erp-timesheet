using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Helper
{
    public class UserHelper
    {
        public static string GetKomuUserName(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                return null;
            }
            int index = emailAddress.IndexOf("@");
            if (index < 0)
            {
                return emailAddress;
            }
            return emailAddress.Substring(0, index).ToLower();

        }
    }
}
