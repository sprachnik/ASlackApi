using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SlackApi.Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly char[] _invalid = Path.GetInvalidFileNameChars();

        /// <summary>
        ///
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>

        /// <summary>
        /// Remove characters from a string if they are not valid for filenames.
        /// </summary>
        /// <param name="filename">The filename that may contain invalid filename characters.</param>
        /// <returns>The filename with all the invalid filename characters removed.</returns>
        public static string CleanFilename(string filename)
        {
            foreach (char c in _invalid)
            {
                filename = filename.Replace(c, '_');
            }

            return filename;
        }

        public static string ReplaceIfNewValueNotNull(this string source, string oldValue, string newValue)
        {
            if (!source.Contains(oldValue))
                return source;

            if (newValue is null)
                return source;

            return source.Replace(oldValue, newValue);
        }

        /// <summary>
        /// By default, pascalize converts strings to UpperCamelCase also removing underscores
        /// </summary>
        /// <remarks>
        /// Taken from https://github.com/Humanizr/Humanizer
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPascal(this string input)
        {
            return Regex.Replace(input, "(?:^|_)(.)", match => match.Groups[1].Value.ToUpper());
        }

        /// <summary>
        /// Same as Pascalize except that the first character is lower case
        /// </summary>
        /// <remarks>
        /// Taken from https://github.com/Humanizr/Humanizer
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCamel(this string input)
        {
            var word = input.ToPascal();
            return word.Substring(0, 1).ToLower() + word[1..];
        }

        public static string ListToString(this List<string> list)
        {
            return list.IsNullOrEmpty()
                ? string.Empty
                : list.Aggregate((x, y) => $"{x},{y}").TrimEnd(',');
        }

        public static string Append(this string input, string stringToAppend, bool ifIsTrue,
            bool nullOrWhitespaceCheck = true)
            => ifIsTrue ? Append(input, stringToAppend, nullOrWhitespaceCheck) : input;

        public static string Append(this string input, string stringToAppend, bool nullOrWhitespaceCheck = true)
            => !nullOrWhitespaceCheck || !string.IsNullOrWhiteSpace(stringToAppend) ? input + stringToAppend : input;

        public static bool FuzzySearch(this string haystack, string needle)
        {
            haystack = haystack.ToLower();
            needle = needle.ToLower();

            if (needle.Length > haystack.Length)
            {
                return false;
            }

            if (needle.Length == haystack.Length)
            {
                return needle == haystack;
            }

            int j = 0;

            for (int i = 0; i < haystack.Length && j < needle.Length; i++)
            {
                if (needle[j] == haystack[i])
                {
                    j++;
                }
            }

            return j == needle.Length;
        }

        public static T ToEnum<T>(this string enumString)
        {
            if (string.IsNullOrWhiteSpace(enumString))
                return default;

            return (T)Enum.Parse(typeof(T), enumString, true);
        }

        public static Stream ToStream(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static bool ToBoolOrDefault(this string input, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            var result = defaultValue;

            try
            {
                result = bool.Parse(input);
            }
            catch
            {
                // Ignore errors and return "defaultValue"
            }

            return result;
        }

        public static int ToIntOrDefault(this string input, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            var result = defaultValue;

            try
            {
                result = int.Parse(input);
            }
            catch
            {
                // Ignore errors and return "defaultValue"
            }

            return result;
        }

        public static int? ToNullableIntOrDefault(this string input, int? defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            var result = defaultValue;

            try
            {
                result = int.Parse(input);
            }
            catch
            {
                // Ignore errors and return "defaultValue"
            }

            return result;
        }

        public static string Replace(this string input, string[] replaceCollection, string newValue)
        {
            if (replaceCollection.IsNullOrEmpty() || string.IsNullOrWhiteSpace(input))
                return input;

            replaceCollection
                .ForEach(r => input = input.Replace(r, newValue));

            return input;
        }

        public static bool Contains(this string input, string[] contains)
        {
            if (contains.IsNullOrEmpty() || string.IsNullOrWhiteSpace(input))
                return false;

            var isAnyValueFound = false;

            contains
                .ForEach(r =>
                {
                    if (!isAnyValueFound)
                        isAnyValueFound = input.Contains(r);
                });

            return isAnyValueFound;
        }

        public static bool IsPropertyExist(dynamic obj, string name)
        {
            if (obj == null || string.IsNullOrWhiteSpace(name))
                return false;

            if (obj is ExpandoObject)
                return ((IDictionary<string, object>)obj).ContainsKey(name);

            return obj.GetType().GetProperty(name) != null;
        }

        public static string RemoveNonAlphaNumeric(this string input, string[] charactersToKeep = null)
        {
            var regex = "^a-zA-Z0-9_";

            if (!charactersToKeep.IsNullOrEmpty())
                regex += charactersToKeep.Aggregate((x, y) => x + y);

            return Regex.Replace(
                input, $"[{regex}]",
                string.Empty);
        }

        public static string EncodeStringToHMACSHA1(string input, byte[] key)
        {
            var hmac = new HMACSHA1(key);
            var bytes = Encoding.ASCII.GetBytes(input);
            var stream = new MemoryStream(bytes);
            return hmac.ComputeHash(stream).Aggregate("", (s, e) => s + string.Format("{0:x2}", e), s => s);
        }

        public static string GetEmail(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*",
                RegexOptions.IgnoreCase);

            var emailMatches = emailRegex.Matches(input);

            return emailMatches.IsNullOrEmpty() ? null : emailMatches.FirstOrDefault()?.Value;
        }

        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str[1..];

            return str.ToUpper();
        }

        public static string Truncate(this string value, int maxLength, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            if (value.Length <= maxLength)
                ellipsis = null;

            return value.Substring(0, Math.Min(value.Length, maxLength)) + ellipsis;
        }

        public static int GetNumberPart(this string value, int defaultValue = 0)
        {
            if (value == null)
                return defaultValue;

            var output = defaultValue;

            try
            {
                output = Regex.Replace(value, @"[^\d]", "").ToIntOrDefault();
            }
            catch
            {
            }

            return output;
        }

        public static bool ValidParameterMapping(this string input)
            => (!input.Contains("{") && !input.Contains("}"))
            || (input.Contains("{") && input.Contains("{{")
            && input.Contains("}") && input.Contains("}}"));

        public static string RemoveFileExtension(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            int index = input.LastIndexOf('.');
            return index == -1 ? input : input.Substring(0, index);
        }

        public static string RemoveQueryString(this string url)
            => !string.IsNullOrWhiteSpace(url) ? url.Split('?')[0] : url;
    }
}
