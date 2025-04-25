namespace virtual_store.settingclasses
{
    public class TimeHelper
    {
          public static DateTime GetCurrentPakistanTime()
    {
            DateTime utcNow = DateTime.UtcNow;

            // Define the Pakistan Standard Time zone
            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            // Convert the UTC time to Pakistan Standard Time
            DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, pakistanTimeZone);
            return pakistanTime;
        }
    }
}
