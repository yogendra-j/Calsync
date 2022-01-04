
namespace CalendarSync.SystemCalendar.Dto
{
    public class OutlookTokenInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; }

        public string Name { get; set; }

        public string Tokeninfo { get; set; }

        public long UserId { get; set; }

        public virtual string RefreshToken { get; set; }
    }
}
