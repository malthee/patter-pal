namespace patter_pal.Util
{
    public static class TimeResolutionConstants
    {
        public const string DefaultTimeResolution = "h";

        public static readonly IDictionary<string, string> TimeResolutions = new Dictionary<string, string>
        {
            {"h", "Hour"},
            {"d", "Day"},
            {"m", "Month"}
        };
    }
}
