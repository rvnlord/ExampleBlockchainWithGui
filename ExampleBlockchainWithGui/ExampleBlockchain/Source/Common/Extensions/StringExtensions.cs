using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using BlockchainApp.Properties;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using DomainParser.Library;
using HtmlAgilityPack;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Math;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class StringExtensions
    {
        #region String Extensions

        #region - Converters

        public static T DescriptionToEnum<T>([NotNull] this string descr) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an Enum");
            var enumVals = EnumUtils.GetValues<T>();
            foreach (var ev in enumVals)
                if ((ev as Enum).GetDescription() == descr)
                    return ev;
            throw new NullReferenceException("There is no Enum value that matches the description");
        }

        public static string HexToBase58(this string str)
        {
            return str.ToHexByteArray().ToBase58String();
        }

        public static byte[] ToUTF8ByteArray(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static byte[] ToHexByteArray(this string str)
        {
            byte[] bytes;
            if (string.IsNullOrEmpty(str))
                bytes = Constants.EmptyByteArray;
            else
            {
                var string_length = str.Length;
                var character_index = str.StartsWith("0x", StringComparison.Ordinal) ? 2 : 0;
                var number_of_characters = string_length - character_index;
                var add_leading_zero = false;

                if (0 != number_of_characters % 2)
                {
                    add_leading_zero = true;
                    number_of_characters += 1;
                }

                bytes = new byte[number_of_characters / 2];

                var write_index = 0;
                if (add_leading_zero)
                {
                    bytes[write_index++] = CharUtils.CharacterToByte(str[character_index], character_index);
                    character_index += 1;
                }

                for (var read_index = character_index; read_index < str.Length; read_index += 2)
                {
                    var upper = CharUtils.CharacterToByte(str[read_index], read_index, 4);
                    var lower = CharUtils.CharacterToByte(str[read_index + 1], read_index + 1);

                    bytes[write_index++] = (byte)(upper | lower);
                }
            }

            return bytes;
        }

        public static byte[] ToBase64ByteArray(this string str)
        {
            return Convert.FromBase64String(str);
        }

        public static int ToHexInt(this string str)
        {
            return int.Parse(str, System.Globalization.NumberStyles.HexNumber);
        }

        public static byte[] ToBase58ByteArray(this string encoded)
        {
            if (encoded == null)
                throw new ArgumentNullException(nameof(encoded));

            var result = new byte[0];
            if (encoded.Length == 0)
                return result;
            var bn = BigInteger.Zero;
            var i = 0;
            while (IsSpace(encoded[i]))
            {
                i++;
                if (i >= encoded.Length)
                    return result;
            }

            for (var y = i; y < encoded.Length; y++)
            {
                var p1 = Constants.PszBase58.IndexOf(encoded[y]);
                if (p1 == -1)
                {
                    while (IsSpace(encoded[y]))
                    {
                        y++;
                        if (y >= encoded.Length)
                            break;
                    }
                    if (y != encoded.Length)
                        throw new FormatException("Invalid base 58 string");
                    break;
                }
                var bnChar = BigInteger.ValueOf(p1);
                bn = bn.Multiply(Constants.Bn58);
                bn = bn.Add(bnChar);
            }

            var vchTmp = bn.ToByteArrayUnsigned();
            Array.Reverse(vchTmp);
            if (vchTmp.All(b => b == 0))
                vchTmp = new byte[0];

            if (vchTmp.Length >= 2 && vchTmp[vchTmp.Length - 1] == 0 && vchTmp[vchTmp.Length - 2] >= 0x80)
                vchTmp = vchTmp.SafeSubarray(0, vchTmp.Length - 1);

            var nLeadingZeros = 0;
            for (var y = i; y < encoded.Length && encoded[y] == Constants.PszBase58[0]; y++)
                nLeadingZeros++;

            result = new byte[nLeadingZeros + vchTmp.Length];
            Array.Copy(vchTmp.Reverse().ToArray(), 0, result, nLeadingZeros, vchTmp.Length);
            return result;
        }

        #endregion

        public static bool IsSpace(char c)
        {
            switch (c)
            {
            case ' ':
            case '\t':
            case '\n':
            case '\v':
            case '\f':
            case '\r':
            return true;
            }
            return false;
        }

        public static bool HasValueBetween(this string str, string start, string end)
        {
            return !string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end) &&
                   str.Contains(start) &&
                   str.Contains(end) &&
                   str.IndexOf(start, StringComparison.Ordinal) < str.IndexOf(end, StringComparison.Ordinal);
        }

        public static string Between(this string str, string start, string end)
        {
            return str.AfterFirst(start).BeforeLast(end);
        }

        public static string TakeUntil(this string str, string end)
        {
            return str.Split(new[] { end }, StringSplitOptions.None)[0];
        }

        public static string RemoveHTMLSymbols(this string str)
        {
            return str.Replace("&nbsp;", "")
                .Replace("&amp;", "");
        }

        public static bool IsNullWhiteSpaceOrDefault(this string str, string defVal)
        {
            return string.IsNullOrWhiteSpace(str) || str == defVal;
        }

        public static bool ContainsAny(this string str, params string[] strings)
        {
            if (strings == null || strings.Length == 0) return false;
            return strings.Any(str.Contains);
        }

        public static string Remove(this string str, string substring)
        {
            return str.Replace(substring, "");
        }

        public static string RemoveMany(this string str, params string[] substrings)
        {
            return substrings.Aggregate(str, (current, substring) => current.Remove(substring));
        }

        public static string[] Split(this string str, string separator, bool includeSeparator = false)
        {
            var split = str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            if (includeSeparator)
            {
                var splitWithSeparator = new string[split.Length + split.Length - 1];
                var j = 0;
                for (var i = 0; i < splitWithSeparator.Length; i++)
                {
                    if (i % 2 == 1)
                        splitWithSeparator[i] = separator;
                    else
                        splitWithSeparator[i] = split[j++];
                }
                split = splitWithSeparator;
            }
            return split;
        }

        public static string[] SplitByFirst(this string str, params string[] strings)
        {
            foreach (var s in strings)
                if (str.Contains(s))
                    return str.Split(s);
            return new[] { str };
        }

        public static IEnumerable<string> SplitInPartsOf(this string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException(@"Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string[] SameWords(this string str, string otherStr, bool casaeSensitive = false, string splitBy = " ", int minWordLength = 1)
        {
            if (casaeSensitive)
            {
                str = str.ToLower();
                otherStr = otherStr.ToLower();
            }

            var str1Arr = str.Split(splitBy);
            var str2Arr = otherStr.Split(splitBy);
            var intersection = str1Arr.Intersect(str2Arr).Where(w => w.Length >= minWordLength);
            return intersection.ToArray();
        }

        public static string[] SameWords(this string str, string[] otherStrings, bool casaeSensitive, string splitBy = " ", int minWordLength = 1)
        {
            var sameWords = new List<string>();

            foreach (var otherStr in otherStrings)
                sameWords.AddRange(str.SameWords(otherStr, casaeSensitive, splitBy, minWordLength));

            return sameWords.Distinct().ToArray();
        }

        public static string[] SameWords(this string str, params string[] otherStrings)
        {
            return str.SameWords(otherStrings, false, " ", 1);
        }

        public static bool HasSameWords(this string str, string otherStr, bool caseSensitive = false, string splitBy = " ", int minWordLength = 1)
        {
            return str.SameWords(otherStr, caseSensitive, splitBy, minWordLength).Any();
        }

        public static bool HasSameWords(this string str, string[] otherStrings, bool caseSensitive, string splitBy = " ", int minWordLength = 1)
        {
            return str.SameWords(otherStrings, caseSensitive, splitBy, minWordLength).Any();
        }

        public static bool HasSameWords(this string str, params string[] otherStrings)
        {
            return str.SameWords(otherStrings, false, " ", 1).Any();
        }

        public static double? TryToDouble(this string str)
        {
            str = str.Replace(',', '.');
            double value;
            var isParsable = double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            if (isParsable)
                return value;
            return null;
        }

        public static double ToDouble(this string str)
        {
            var parsedD = str.TryToDouble();
            if (parsedD == null)
                throw new InvalidCastException("Nie można sparsować wartości double");
            return (double)parsedD;
        }

        public static bool IsDouble(this string str)
        {
            return str.TryToDouble() != null;
        }

        public static bool StartsWithAny(this string str, params string[] strings)
        {
            return strings.Any(str.StartsWith);
        }

        public static bool EndsWithAny(this string str, params string[] strings)
        {
            return strings.Any(str.EndsWith);
        }

        public static bool ContainsAll(this string str, params string[] strings)
        {
            if (strings == null || strings.Length == 0) return false;
            return strings.All(str.Contains);
        }

        public static string RemoveWord(this string str, string word, string separator = " ")
        {
            return string.Join(separator, str.Split(separator).Where(w => w != word));
        }

        public static string RemoveWords(this string str, string[] words, string separator)
        {
            return words.Aggregate(str, (current, w) => current.RemoveWord(w));
        }

        public static string RemoveWords(this string str, params string[] words)
        {
            return str.RemoveWords(words, " ");
        }

        public static bool IsUri(this string str)
        {
            return Uri.TryCreate(str, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static string UriToDomain(this string str)
        {
            return DomainName.TryParse(new Uri(str).Host, out var completeDomain) ? completeDomain.SLD : "";
        }

        public static string Take(this string str, int n)
        {
            return new string(str.AsEnumerable().Take(n).ToArray());
        }

        public static string Skip(this string str, int n)
        {
            return new string(str.AsEnumerable().Skip(n).ToArray());
        }

        public static string TakeLast(this string str, int n)
        {
            return new string(str.AsEnumerable().TakeLast(n).ToArray());
        }

        public static string SkipLast(this string str, int n)
        {
            return new string(str.AsEnumerable().SkipLast(n).ToArray());
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string RemoveAt(this string str, params int[] positions)
        {
            var charsList = str.AsEnumerable().ToList();
            for (var i = charsList.Count - 1; i >= 0; i--)
                if (i.EqualsAny(positions))
                    charsList.RemoveAt(i);
            return new string(charsList.ToArray());
        }

        public static string ToUrlEncoded(this string str)
        {
            return Uri.EscapeDataString(str); // Uri.EscapeDataString HttpUtility.UrlPathEncode
        }

        public static string RegexReplace(this string str, string pattern, string replacement)
        {
            return Regex.Replace(str, pattern, replacement);
        }

        public static string ReplaceLast(this string str, string fromStr, string toStr)
        {
            var lastIndexOf = str.LastIndexOf(fromStr, StringComparison.Ordinal);
            if (lastIndexOf < 0)
                return str;

            var leading = str.Substring(0, lastIndexOf);
            var charsToEnd = str.Length - (lastIndexOf + fromStr.Length);
            var trailing = str.Substring(lastIndexOf + fromStr.Length, charsToEnd);

            return leading + toStr + trailing;
        }

        public static string RemoveLast(this string str, string fromStr)
        {
            return str.ReplaceLast(fromStr, string.Empty);
        }

        public static string ReplaceMany(this string str, IEnumerable<string> strEn, string replacement)
        {
            return strEn.Aggregate(str, (current, s) => current.Replace(s, replacement));
        }

        public static string TrimEnd(this string str, string end)
        {
            return str.EndsWith(end) ? str.Substring(0, str.Length - end.Length) : str;
        }

        public static bool ContainsOnlyDigits(this string str) => str.All(c => c >= '0' && c <= '9');

        public static string AfterFirst(this string str, string substring)
        {
            if (!string.IsNullOrEmpty(substring) && str.Contains(substring))
            {
                var split = str.Split(substring);
                if (str.StartsWith(substring))
                    split = new[] { "" }.Concat(split).ToArray();
                return split.Skip(1).JoinAsString(substring);
            }
            return str;
        }

        public static string BeforeFirst(this string str, string substring)
        {
            if (!string.IsNullOrEmpty(substring) && str.Contains(substring))
                return str.Split(substring).First();
            return str;
        }

        public static string AfterLast(this string str, string substring)
        {
            if (!string.IsNullOrEmpty(substring) && str.Contains(substring))
                return str.Split(substring).Last();
            return str;
        }

        public static string BeforeLast(this string str, string substring)
        {
            if (!string.IsNullOrEmpty(substring) && str.Contains(substring))
            {
                var split = str.Split(substring);
                if (str.EndsWith(substring))
                    split = split.Concat(new[] { "" }).ToArray();
                var l = split.Length;
                return split.Take(l - 1).JoinAsString(substring);
            }

            return str;
        }

        public static bool IsBefore(this string str, string word1, string word2)
        {
            var idx1 = str.IndexOf(word1, StringComparison.Ordinal);
            var idx2 = str.IndexOf(word2, StringComparison.Ordinal);

            if (idx1 < 0 || idx2 < 0 || word1 == word2)
                throw new Exception("Words are invalid");
            return idx1 <= idx2;
        }

        public static bool IsAfter(this string str, string word1, string word2)
        {
            var idx1 = str.IndexOf(word1, StringComparison.Ordinal);
            var idx2 = str.IndexOf(word2, StringComparison.Ordinal);

            if (idx1 < 0 || idx2 < 0 || word1 == word2)
                throw new Exception("Words are invalid");
            return idx1 > idx2;
        }

        public static string RemoveHexPrefix(this string value)
        {
            return value.Replace("0x", "");
        }

        public static string EnforceHexPrefix(this string str)
        {
            return str.StartsWith("0x") ? str : $"0x{str}";
        }

        public static bool IsNumber(this string str)
        {
            return str.All(char.IsDigit);
        }

        public static string SkipWhile(this string str, Func<char, bool> condition)
        {
            return new string(str.AsEnumerable().SkipWhile(condition).ToArray());
        }

        public static string TakeWhile(this string str, Func<char, bool> condition)
        {
            return new string(str.AsEnumerable().TakeWhile(condition).ToArray());
        }

        public static string SkipWhileDigit(this string str)
        {
            return new string(str.AsEnumerable().SkipWhile(char.IsDigit).ToArray());
        }

        public static string TakeWhileDigit(this string str)
        {
            return new string(str.AsEnumerable().TakeWhile(char.IsDigit).ToArray());
        }

        public static string TrimMultiline(this string str)
        {
            return str.Split(Environment.NewLine).Select(line => line.Trim()).JoinAsString().Remove(" ").Trim();
        }

        public static HtmlNode HtmlRoot(this string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        public static Dictionary<string, string> QueryStringToDictionary(this string queryString)
        {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AsEnumerable().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static bool EqIgnoreCase(this string str, string ostr)
        {
            return string.Equals(str, ostr, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqAnyIgnoreCase(this string str, params string[] os)
        {
            return os.Any(s => s.EqIgnoreCase(str));
        }

        public static bool EqAnyIgnoreCase(this string str, IEnumerable<string> os)
        {
            return os.Any(s => s.EqIgnoreCase(str));
        }

        public static JToken ToJToken(this string json)
        {
            JsonReader jReader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
            return JToken.Load(jReader);
        }

        public static JObject ToJObject(this string json)
        {
            JsonReader jReader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
            return JObject.Load(jReader);
        }

        public static JArray ToJArray(this string json)
        {
            JsonReader jReader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
            return JArray.Load(jReader);
        }

        public static string EnsureSuffix(this string str, string suffix)
        {
            if (!str.EndsWith(suffix))
                str += suffix;
            return str;
        }

        public static bool ContainsAny(this string str, IEnumerable<string> strings)
        {
            var lStr = str.ToLower();
            return strings.Select(s => s.ToLower()).Any(lStr.Contains);
        }

        public static bool ContainsAll(this string str, IEnumerable<string> strings)
        {
            var lStr = str.ToLower();
            return strings.Select(s => s.ToLower()).All(lStr.Contains);
        }

        public static string Repeat(this string str, int n)
        {
            return new string(str.AsEnumerable().Repeat(n).ToArray());
        }

        public static JToken JsonDeserialize(this string str)
        {
            if (!str.ContainsAny("{", "["))
                str = $"'{str}'";
            return JToken.Parse(str);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string RemoveWhiteSpace(this string str)
        {
            return str.RemoveMany("\r\n", " ");
        }

        public static bool IsIP(this string str)
        {
            return
                IPAddress.TryParse(str, out var address) &&
                address.AddressFamily.In(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6);
        }

        public static string ToIP(this string domainOrIp)
        {
            var ip = domainOrIp.IsIP() ? domainOrIp : Dns.GetHostAddresses(domainOrIp)[0].ToString();
            return ip.In("::1", "0.0.0.0") ? "127.0.0.1" : ip;
        }

        public static bool IsBase58(this string str)
        {
            foreach (var c in str)
                if (!Constants.PszBase58.Contains(c))
                    return false;
            return true;
        }

        public static string AddSpacesToPascalCase(this string text, bool wordsToLower = true, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var sb = new StringBuilder(text.Length * 2);
            sb.Append(text[0]);
            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if (text[i - 1] != ' ' && !char.IsUpper(text[i - 1]) || preserveAcronyms && char.IsUpper(text[i - 1]) && i < text.Length - 1 && !char.IsUpper(text[i + 1]))
                        sb.Append(' ');
                sb.Append(wordsToLower ? text[i].ToString().ToLower() : text[i].ToString());
            }
            return sb.ToString();
        }

        public static string Shuffle(this string str)
        {
            return new string(str.AsEnumerable().Shuffle().ToArray());
;        }

        #endregion

    }
}
