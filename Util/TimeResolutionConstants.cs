namespace patter_pal.Util
{
    public static class TimeResolutionConstants
    {
        public const string DefaultTimeResolution = "i";

        public static readonly IDictionary<string, string> TimeResolutions = new Dictionary<string, string>
        {
            {"i", "Minute"},
            {"h", "Hour"},
            {"d", "Day"},
            {"m", "Month"}
        };
    }
}
