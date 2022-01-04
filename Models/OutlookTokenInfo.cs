﻿using CalendarSync.SystemCalendar.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalendarSync.SystemCalendar.Dto
{
    public class OutlookTokenInfo
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }

        public virtual string Email { get; set; }

        public virtual string Name { get; set; }

        public virtual string Tokeninfo { get; set; }

        public virtual string RefreshToken { get; set; }
        public virtual DateTime? LastRefreshedTime { get; set; }

        public virtual long? UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }
        public string DeltaToken { get; set; }
    }
}