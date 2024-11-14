namespace AonFreelancing.Utilities;

public class StringOperations
{
    public static string GetTimeAgo(DateTime createdAt)
    {
        var timeSpan = DateTime.UtcNow - createdAt;
        return timeSpan.TotalMinutes switch
        {
            < 1 => "just now",
            < 60 => $"{timeSpan.Minutes} minute(s) ago",
            < 1440 => $"{timeSpan.Hours} hour(s) ago",   // 1440 minutes in a day
            < 10080 => $"{timeSpan.Days} day(s) ago",    // 10080 minutes in a week
            < 43200 => $"{timeSpan.Days / 7} week(s) ago",   // 43200 minutes in a month (approx. 30 days)
            < 525600 => $"{timeSpan.Days / 30} month(s) ago", // 525600 minutes in a year
            _ => $"{timeSpan.Days / 365} year(s) ago"
        };
    }
}