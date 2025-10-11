using Lucky_Charm_Event_track.Enums;
using Lucky_Charm_Event_track.Models;
using System;


namespace Lucky_Charm_Event_track.Services
{
    public class SessionManager
    {
        public UserAccount CurrentLoggedInUser { get; set; }
        public bool IsAdmin { get; set; }
        public Event CurrentEvent { get; set; }
        public string CurrentPage { get; set; }
        public DateTime SessionStart { get; set; }

        public void InitializeSession(UserAccount userAccount, string current_page)
        {
            CurrentLoggedInUser = userAccount;
            if (userAccount.AccountType.Equals(AccountTypes.Administrator))
            {
                IsAdmin = true;
            }
            else
            {
                IsAdmin = false;
            }
            CurrentEvent = null;
            CurrentPage = current_page;
            SessionStart = DateTime.Now;
        }
        public void ClearSession()
        {
            CurrentLoggedInUser = null;
            IsAdmin = false;
            CurrentEvent = null;
            CurrentPage = null;
            SessionStart = DateTime.Now;
        }

    }
}
