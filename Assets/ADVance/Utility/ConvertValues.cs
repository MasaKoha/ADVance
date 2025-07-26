namespace ADVance.Utility
{
    public static class ConvertValues
    {
        public static short ToShort(this string value, short defaultValue = 0)
        {
            return short.TryParse(value, out var result) ? result : defaultValue;
        }

        public static ushort ToUShort(this string value, ushort defaultValue = 0)
        {
            return ushort.TryParse(value, out var result) ? result : defaultValue;
        }

        public static int ToInt(this string value, int defaultValue = 0)
        {
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public static uint ToUInt(this string value, uint defaultValue = 0)
        {
            return uint.TryParse(value, out var result) ? result : defaultValue;
        }

        public static long ToLong(this string value, long defaultValue = 0L)
        {
            return long.TryParse(value, out var result) ? result : defaultValue;
        }

        public static ulong ToULong(this string value, ulong defaultValue = 0UL)
        {
            return ulong.TryParse(value, out var result) ? result : defaultValue;
        }

        public static float ToFloat(this string value, float defaultValue = 0f)
        {
            return float.TryParse(value, out var result) ? result : defaultValue;
        }

        public static double ToDouble(this string value, double defaultValue = 0d)
        {
            return double.TryParse(value, out var result) ? result : defaultValue;
        }

        public static bool ToBool(this string value, bool defaultValue = false)
        {
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }
    }
}