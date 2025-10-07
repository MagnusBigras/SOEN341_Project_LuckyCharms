using Lucky_Charm_Event_track.Enums;
using System;
using System.Collections.Generic;

namespace Lucky_Charm_Event_track.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string PhoneNumber {get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime AccountCreationDate { get; set; }
        public AccountTypes AccountType { get; set; }
        public List<Ticket> Tickets { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }

    }
}
