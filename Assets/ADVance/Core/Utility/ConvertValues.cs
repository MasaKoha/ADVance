namespace ADVance.Utility
{
    public static class ConvertValues
    {
        public static short ToShort(this string value, short defaultValue = 0)
        {
            return short.TryParse(value, out var result) ? result : defaultValue;
        }

        public static short ToShort(this object value, short defaultValue = 0)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is short result ? result : value.ToString().ToShort(defaultValue);
        }

        public static ushort ToUShort(this string value, ushort defaultValue = 0)
        {
            return ushort.TryParse(value, out var result) ? result : defaultValue;
        }

        public static ushort ToUShort(this object value, ushort defaultValue = 0)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is ushort result ? result : value.ToString().ToUShort(defaultValue);
        }

        public static int ToInt(this string value, int defaultValue = 0)
        {
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public static int ToInt(this object value, int defaultValue = 0)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is int result ? result : value.ToString().ToInt(defaultValue);
        }

        public static uint ToUInt(this string value, uint defaultValue = 0)
        {
            return uint.TryParse(value, out var result) ? result : defaultValue;
        }

        public static uint ToUInt(this object value, uint defaultValue = 0)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is uint result ? result : value.ToString().ToUInt(defaultValue);
        }

        public static long ToLong(this string value, long defaultValue = 0L)
        {
            return long.TryParse(value, out var result) ? result : defaultValue;
        }

        public static long ToLong(this object value, long defaultValue = 0L)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is long result ? result : value.ToString().ToLong(defaultValue);
        }

        public static ulong ToULong(this string value, ulong defaultValue = 0UL)
        {
            return ulong.TryParse(value, out var result) ? result : defaultValue;
        }

        public static ulong ToULong(this object value, ulong defaultValue = 0UL)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is ulong result ? result : value.ToString().ToULong(defaultValue);
        }

        public static float ToFloat(this string value, float defaultValue = 0f)
        {
            return float.TryParse(value, out var result) ? result : defaultValue;
        }

        public static float ToFloat(this object value, float defaultValue = 0f)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is float result ? result : value.ToString().ToFloat(defaultValue);
        }

        public static double ToDouble(this string value, double defaultValue = 0d)
        {
            return double.TryParse(value, out var result) ? result : defaultValue;
        }

        public static double ToDouble(this object value, double defaultValue = 0d)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is double result ? result : value.ToString().ToDouble(defaultValue);
        }

        public static bool ToBool(this string value, bool defaultValue = false)
        {
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        public static bool ToBool(this object value, bool defaultValue = false)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return value is bool result ? result : value.ToString().ToBool(defaultValue);
        }
    }
}