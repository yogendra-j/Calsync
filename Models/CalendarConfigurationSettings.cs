namespace CalendarSyncPOC.Models
{
    public class CalendarConfigurationSettings
    {
        public virtual long Id { get; set; }
        public virtual string GoogleEventsEndpoint { get; set; }
        public virtual string GoogleTokenEndpoint { get; set; }
        public virtual string GoogleClientId { get; set; }
        public virtual string GoogleClientSecret { get; set; }
        public virtual string GoogleApiKey { get; set; }
        public virtual string GoogleScope { get; set; }
        public virtual string GoogleRedirectURL { get; set; }
        public virtual string OutlookEventEndPoint { get; set; }
        public virtual string OutlookAuthorizationEndpoint { get; set; }
        public virtual string OutlookTokenEndpoint { get; set; }
        public virtual string OutlookClientId { get; set; }
        public virtual string OutlookClientSecret { get; set; }
        public virtual string OutlookScope { get; set; }
        public virtual string OutlookRedirectURL { get; set; }
    }
}
