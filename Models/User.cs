using System;
using System.Collections.Generic;

namespace CalendarSync.SystemCalendar.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        public virtual long Id {get; set;}
        public virtual Guid? ProfilePictureId { get; set; }

        public virtual bool ShouldChangePasswordOnNextLogin { get; set; }

        public DateTime? SignInTokenExpireTimeUtc { get; set; }

        public string SignInToken { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }

        public string GoogleAuthenticatorKey { get; set; }


        public virtual Guid? UniqueId { get; set; }



    }
}