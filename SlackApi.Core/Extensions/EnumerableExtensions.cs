using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlackApi.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Union<T>(this IEnumerable<T> source, T item)
        {
            return source.Union(Enumerable.Repeat(item, 1));
        }

        /// <summary>
        /// Wraps this object instance into an IEnumerable&lt;T&gt;
        /// consisting of a single item.
        /// </summary>
        /// <typeparam name="T"> Type of the object. </typeparam>
        /// <param name="item"> The instance that will be wrapped. </param>
        /// <returns> An IEnumerable&lt;T&gt; consisting of a single item. </returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static T[] YieldArray<T>(this T item)
            => new[] { item };

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }

            return string.Empty;
        }

        public static IEnumerable<T> DropLast<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return InternalDropLast(source);
        }

        private static IEnumerable<T> InternalDropLast<T>(IEnumerable<T> source)
        {
            T buffer = default;
            bool buffered = false;

            foreach (T x in source)
            {
                if (buffered)
                    yield return buffer;

                buffer = x;
                buffered = true;
            }
        }

        public static string[] RemoveAll(this string[] items, string[] itemsToRemove)
        {
            if (items != null)
            {
                var listOfItems = items.ToList();
                listOfItems.RemoveAll(i => itemsToRemove.Contains(i));
                return listOfItems.ToArray();
            }
            return null;
        }

        public static string[] AddRange(this string[] items, string[] itemsToAdd)
        {
            if (items != null)
            {
                var listOfItems = items.ToList();
                listOfItems.AddRange(itemsToAdd);
                return listOfItems.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Enumerates the enumerable and invokes a callback action on each item.
        /// </summary>
        /// <typeparam name="T">The generic type of the <see cref="IEnumerable"/>.</typeparam>
        /// <param name="instance">The instance of <see cref="IEnumerable"/> to extend.</param>
        /// <param name="foreachCallback">The callback action to invoke for each item.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> instance, Action<T> foreachCallback)
        {
            if (foreachCallback != null && !instance.IsNullOrEmpty())
            {
                foreach (var item in instance)
                    foreachCallback(item);
            }

            return instance;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> instance, Action<T, int> foreachCallback)
        {
            if (foreachCallback != null && !instance.IsNullOrEmpty())
            {
                foreach (var item in instance.Select((value, index) => new { index, value }))
                {
                    foreachCallback(item.value, item.index);
                }
            }

            return instance;
        }

        // http://stackoverflow.com/questions/4597472/check-ienumerablet-for-items-having-duplicate-properties
        public static bool ContainsDuplicates<T>(this IEnumerable<T> list)
            => !list.All(new HashSet<T>().Add);

        public static IEnumerable<T> GetDuplicates<T>(this IEnumerable<T> source)
        {
            HashSet<T> itemsSeen = new();
            HashSet<T> itemsYielded = new();

            foreach (T item in source)
            {
                if (!itemsSeen.Add(item))
                {
                    if (itemsYielded.Add(item))
                    {
                        yield return item;
                    }
                }
            }
        }

        public static void AddIfNotNull<T>(this IList<T> source, T value)
        {
            if (value != null)
                source.Add(value);
        }

        /// <summary>
        /// Returns the index of the first element in the sequence
        /// that satisfies a condition.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IEnumerable{T}"/> that contains
        /// the elements to apply the predicate to.
        /// </param>
        /// <param name="predicate">
        /// A function to test each element for a condition.
        /// </param>
        /// <returns>
        /// The zero-based index position of the first element of <paramref name="source"/>
        /// for which <paramref name="predicate"/> returns <see langword="true"/>;
        /// or -1 if <paramref name="source"/> is empty
        /// or no element satisfies the condition.
        /// </returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            int i = 0;

            foreach (TSource element in source)
            {
                if (predicate(element))
                    return i;

                i++;
            }

            return -1;
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }

        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> source) =>
             source == null || !source.Any();

        public static bool IsNullOrEmpty(this string[] source) =>
             source == null || !source.Any();

        public static bool IsNullOrEmpty<T, TX>(this Dictionary<T, TX> dict) =>
             dict == null || !dict.Any();

        public static int CountOrNullIsZero<TSource>(this IEnumerable<TSource> source)
        {
            var enumerable = source as TSource[] ?? source.ToArray();
            return !enumerable.Any() ? 0 : enumerable.Length;
        }

        public static StringBuilder ToCsvString(this IEnumerable<string> source)
        {
            var list = source.ToList();
            var values = new StringBuilder();
            values.AppendFormat("{0}", list[0]);
            for (int i = 1; i < list.Count; i++)
                values.AppendFormat(", {0}", list[i]);
            return values;
        }

        public static List<List<T>> SplitList<T>(this IEnumerable<T> source, int size = 2000)
        {
            return source
                .Select((v, i) => new { Index = i, Value = v })
                .GroupBy(g => g.Index / size)
                .Select(g => g.Select(v => v.Value).ToList())
                .ToList();
        }

        public static IEnumerable<T> EnumToEnumerable<T>(this Enum enumType)
        {
            return Enum.GetValues(enumType.GetType()).Cast<T>();
        }

        public static T? DeserializeBytes<T>(this byte[] arr)
        {
            var arrString = Encoding.UTF8.GetString(arr);

            if (arrString is null)
                return default;

            return JsonSerializer.Deserialize<T>(arrString);
        }

        public static string[] RemoveItem(this string[] array, string itemToRemove)
        {
            if (array.IsNullOrEmpty())
                return array;

            var list = array.ToList();
            list.Remove(itemToRemove);
            array = list.ToArray();

            return array;
        }

        public static IEnumerable<T> ToEnumFlags<T>(this long? binary)
            where T : struct, IConvertible
        {
            var outputType = typeof(T);
            if (!outputType.IsEnum) throw new Exception($"Expected type {outputType.Name} is not an Enum");

            if (!binary.HasValue) return null;

            var retVal = new List<T>();
            foreach (var flagToCheck in Enum.GetValues(outputType).Cast<T>())
            {
                if ((binary.Value & Convert.ToInt64(flagToCheck)) != 0)
                    retVal.Add(flagToCheck);
            }
            return retVal;
        }

        public static long GetHashLong<T>(this HashSet<T> set)
        {
            var setString = JsonSerializer.Serialize(set);
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(setString));
            return Math.Abs(BitConverter.ToInt64(hash));
        }

        public static List<List<T>> GetGroupedBatches<T, TBatch>(this List<T> list,
            Func<T, TBatch> filter,
            int batchSize = 10)
        {
            var skip = 0;
            var batches = new List<List<T>>();
            var groupedList = list
                    .GroupBy(filter)
                    .ToDictionary(g => g.Key, g => g.ToList());

            while (true)
            {
                var batch = groupedList
                    .Skip(skip)
                    .Take(batchSize)
                    .SelectMany(g => g.Value).ToList();

                if (!batch.Any())
                    break;

                batches.Add(batch);
                skip += batchSize;
            }

            return batches;
        }

        public static async Task<Dictionary<List<T>, TResponse>> BatchAsync<T, TResponse>(this List<T> list,
            Func<List<T>, TResponse> func,
            int batchSize = 10,
            bool isSingleThreaded = true)
        {
            var resultsDict = new Dictionary<List<T>, TResponse>();

            if (list.IsNullOrEmpty())
                return resultsDict;

            var skip = 0;
            var batchNo = 1;

            while (true)
            {
                var batch = list
                    .Skip(skip)
                    .Take(batchSize)
                    .ToList();

                if (!batch.Any())
                    break;

                if (isSingleThreaded)
                {
                    var response = await Task.Run(() => func(batch));
                    resultsDict.Add(batch, response);
                }
                else
                {
                    resultsDict.Add(batch, func(batch));
                }

                batchNo++;
                skip += batchSize;
            }

            return resultsDict;
        }

        public static Dictionary<IEnumerable<KeyValuePair<TBatch, List<T>>>, TResponse> Batch<T, TBatch, TResponse>(this IList<T> list,
            Func<T, TBatch> filter,
            Func<IList<T>, TResponse> func,
            int batchSize = 10)
        {
            if (list.IsNullOrEmpty())
                return new Dictionary<IEnumerable<KeyValuePair<TBatch, List<T>>>, TResponse>();

            var skip = 0;
            var resultsDict = new Dictionary<IEnumerable<KeyValuePair<TBatch, List<T>>>, TResponse>();
            var batchNo = 1;

            var wholeList = list
                   .GroupBy(filter)
                   .ToDictionary(g => g.Key, g => g.ToList());

            while (true)
            {
                var groupedBatch = wholeList
                    .Skip(skip)
                    .Take(batchSize);

                var batch = groupedBatch.SelectMany(g => g.Value).ToList();

                if (!batch.Any())
                    break;

                resultsDict.Add(groupedBatch, func(batch));

                if (wholeList.Count == groupedBatch.Count())
                    break;

                batchNo++;
                skip += batchSize;
            }

            return resultsDict;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<T> ToEnumFlags<T>(this long binary)
            where T : struct, IConvertible
        {
            return ((long?)binary).ToEnumFlags<T>();
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static byte[] ToArray(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using MemoryStream ms = new();
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }

        public static byte[] GetFileBytes(this IFormFile file)
        {
            if (file.Length > 0)
            {
                using var ms = new MemoryStream();
                file.CopyTo(ms);
                var fileBytes = ms.ToArray();
                return fileBytes;
            }

            return Array.Empty<byte>();
        }
    }
}
