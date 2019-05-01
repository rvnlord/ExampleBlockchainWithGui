using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BlockchainApp.Properties;
using BlockchainApp.Source.Common.Utils;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Math;
using static BlockchainApp.Source.Common.Constants;
using static BlockchainApp.Source.Common.Utils.UtilClasses.JsonSerialization.CustomJsonSerializer;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToStringN(this object o)
        {
            string strO = null;
            try { strO = o.ToString(); } catch (Exception) { }
            return string.IsNullOrWhiteSpace(strO) ? null : strO;
        }

        public static T ToEnum<T>(this object value) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an Enum");
            return (T)Enum.Parse(typeof(T), value.ToString().RemoveMany(" ", "-"), true);
        }

        public static T? ToEnumOrNull<T>(this object value) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an Enum");
            T? parsedEnum = null;
            try
            {
                parsedEnum = (T)Enum.Parse(typeof(T), value.ToString().RemoveMany(" ", "-"), true);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentException || ex is OverflowException) { }

            return parsedEnum;
        }

        public static int? ToIntN(this object obj)
        {
            if (obj == null) return null;
            if (obj is bool) return Convert.ToInt32(obj);
            if (obj.GetType().IsEnum) return (int)obj;
            return int.TryParse(obj.ToDoubleN()?.Round().ToString().BeforeFirst("."), NumberStyles.Any, Culture, out var val) ? val : (int?)null;
        }

        public static int ToInt(this object obj)
        {
            var intN = obj.ToIntN();
            if (intN != null) return (int)intN;
            throw new ArgumentNullException(nameof(obj));
        }

        public static uint? ToUIntN(this object obj)
        {
            if (obj == null) return null;
            if (obj is bool) return Convert.ToUInt32(obj);
            if (obj.GetType().IsEnum) return (uint)obj;
            return UInt32.TryParse(obj.ToDoubleN()?.Round().ToString().BeforeFirst("."), NumberStyles.Any, Culture, out var val) ? val : (uint?)null;
        }

        public static uint ToUInt(this object obj)
        {
            var uintN = obj.ToUIntN();
            if (uintN != null) return (uint)uintN;
            throw new ArgumentNullException(nameof(obj));
        }

        public static long? ToLongN(this object obj)
        {
            if (obj == null) return null;
            if (obj is bool) return Convert.ToInt64(obj);
            if (obj.GetType().IsEnum) return (long)obj;
            return Int64.TryParse(obj.ToDoubleN()?.Round().ToString().BeforeFirst("."), NumberStyles.Any, Culture, out long val) ? val : (long?)null;
        }

        public static long ToLong(this object obj)
        {
            var longN = obj.ToLongN();
            if (longN != null) return (long)longN;
            throw new ArgumentNullException(nameof(obj));
        }

        public static double? ToDoubleN(this object obj)
        {
            if (obj == null) return null;
            if (obj is bool) return Convert.ToDouble(obj);

            var strD = obj.ToString().Replace(",", ".");
            var isNegative = strD.StartsWith("-");
            if (isNegative || strD.StartsWith("+"))
                strD = strD.Skip(1);

            var parsedVal = double.TryParse(strD, NumberStyles.Any, Culture, out double tmpvalue) ? tmpvalue : (double?)null;
            if (isNegative)
                parsedVal = -parsedVal;
            return parsedVal;
        }

        public static double ToDouble([NotNull] this object obj)
        {
            var doubleN = obj.ToDoubleN();
            if (doubleN != null) return (double)doubleN;
            throw new ArgumentNullException(nameof(obj));
        }

        public static decimal? ToDecimalN(this object obj)
        {
            if (obj == null) return null;
            if (obj is bool) return Convert.ToDecimal(obj);
            return Decimal.TryParse(obj.ToString().Replace(",", "."), NumberStyles.Any, Culture, out decimal tmpvalue) ? tmpvalue : (decimal?)null;
        }

        public static decimal ToDecimal([NotNull] this object obj)
        {
            var decimalN = obj.ToDecimalN();
            if (decimalN != null) return (decimal)decimalN;
            throw new ArgumentNullException(nameof(obj));
        }

        public static DateTime? ToDateTimeN(this object obj)
        {
            return DateTime.TryParse(obj?.ToString(), out DateTime tmpvalue) ? tmpvalue : (DateTime?)null;
        }

        public static DateTime ToDateTime(this object obj)
        {
            var DateTimeN = obj.ToDateTimeN();
            if (DateTimeN != null) return (DateTime)DateTimeN;
            throw new ArgumentNullException(nameof(obj));
        }

        public static DateTime ToDateTimeExact(this object obj, string format)
        {
            return DateTime.ParseExact(obj?.ToString(), format, CultureInfo.InvariantCulture);
        }

        public static bool? ToBoolN(this object obj)
        {
            if (obj == null) return null;
            if (obj is bool) return (bool)obj;
            if (obj.ToIntN() != null) return Convert.ToBoolean(obj.ToInt());
            return Boolean.TryParse(obj.ToString(), out bool tmpvalue) ? tmpvalue : (bool?)null;
        }

        public static bool ToBool(this object obj)
        {
            var boolN = obj.ToBoolN();
            if (boolN != null) return (bool)boolN;
            throw new ArgumentNullException(nameof(obj));
        }

        public static BigInteger ToBigIntN(this object obj)
        {
            if (obj == null) return null;
            if (obj is bool) return new BigInteger(Convert.ToInt32(obj).ToString());
            try
            {
                var isNegative = false;
                var strBi = obj.ToString();
                if (strBi.StartsWith("-"))
                {
                    strBi = strBi.Skip(1);
                    isNegative = true;
                }

                if (strBi.StartsWith("0x"))
                    strBi = strBi.Skip(2);
                var radix = 10;
                if (strBi.ContainsAny("ABCDEFabcdef".Select(c => c.ToString()).ToArray()))
                    radix = 16;

                var bi = new BigInteger(strBi.Replace(".", ",").TakeUntil(","), radix);
                return isNegative ? bi.Negate() : bi;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static BigInteger ToBigInt(this object obj)
        {
            var bigintN = obj.ToBigIntN();
            if (bigintN != null) return bigintN;
            throw new ArgumentNullException(nameof(obj));
        }

        public static ExtendedTime ToExtendedTimeN(this object o, string format = null, TimeZoneKind tz = TimeZoneKind.UTC)
        {
            if (string.IsNullOrWhiteSpace(o?.ToString()))
                return null;

            var parsedDateTime = format != null
                ? DateTime.ParseExact(o.ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
                : DateTime.Parse(o.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

            return parsedDateTime.ToExtendedTime(tz);
        }

        public static ExtendedTime ToExtendedTime(this object o, string format = null, TimeZoneKind tz = TimeZoneKind.UTC)
        {
            var extTime = o.ToExtendedTimeN(format, tz);
            if (extTime == null)
                throw new InvalidCastException(nameof(o));
            return extTime;
        }

        public static T GetProperty<T>(this object src, string propName)
        {
            return (T)src.GetType().GetProperty(propName)?.GetValue(src, null);
        }

        public static void SetProperty<T>(this object src, string propName, T propValue)
        {
            src.GetType().GetProperty(propName)?.SetValue(src, propValue);
        }

        public static T GetField<T>(this object src, string fieldName)
        {
            return (T)src.GetType().GetField(fieldName)?.GetValue(src);
        }

        public static void SetField<T>(this object src, string fieldName, T fieldValue)
        {
            src.GetType().GetField(fieldName)?.SetValue(src, fieldValue);
        }

        public static void AddEventHandlers(this object o, string eventName, List<Delegate> ehs)
        {
            EventUtils.AddEventHandlers(o, eventName, ehs);
        }

        public static List<Delegate> RemoveEventHandlers(this object o, string eventName)
        {
            return EventUtils.RemoveEventHandlers(o, eventName);
        }

        public static string JsonSerialize(this object o)
        {
            return o is JToken jt ? jt.ToString() : JsonConvert.SerializeObject(o, Formatting.Indented, Serializer.Converters.ToArray());
        }

        public static JToken ToJToken(this object o)
        {
            return JToken.FromObject(o, Serializer);
        }

        public static T CastTo<T>(this object o) => (T)o;

        public static dynamic CastToReflected(this object o, Type type)
        {
            var methodInfo = typeof(ObjectExtensions).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
            var genericArguments = new[] { type };
            var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
            return genericMethodInfo?.Invoke(null, new[] { o });
        }
    }
}
