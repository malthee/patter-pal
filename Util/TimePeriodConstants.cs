namespace patter_pal.Util
{
    public static class TimePeriodConstants
    {
        public const string DefaultTimePeriod = "d-3";

        public static readonly IDictionary<string, string> TimePeriods = new Dictionary<string, string>
        {
            {"d-1", "Today"},
            {"d-3", "Last 3 Days"},
            //{"w-1", "This Week"},
            {"d-7", "Last 7 Days"},
            //{"w-2", "Last 2 Weeks"},
            //{"m-1", "This Month"},
            {"d-30", "Last 30 Days"},
            {"j-1", "Last Year"},
        };

    }
}
