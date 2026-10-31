using System.Collections.Immutable;
using System.Globalization;

namespace BVRTK;

public partial class Utils
{
    #region Immutable LIst

    public static ImmutableList<T> ReplaceInList<T>(ImmutableList<T> list, T currentValue, T newValue, bool ignoreDuplicate = false)
    {
        var index = list.IndexOf(currentValue);
        if (index < 0 || EqualityComparer<T>.Default.Equals(currentValue, newValue)) return list;

        if (ignoreDuplicate && list.Contains(newValue)) // No need to add it again
        {
            return RemoveFromList(list, currentValue); // Because it would have been replaced.
        }

        return list.SetItem(index, newValue);
    }

    public static ImmutableList<T> ReplaceInListAt<T>(ImmutableList<T> list, int index, T value, bool ignoreDuplicate = false)
    {
        if (index < 0 || index >= list.Count) return list;

        if (ignoreDuplicate && list.Contains(value)) // No need to add it again
        {
            return RemoveFromListAt(list, index); // Because it would have been replaced
        }

        return list.SetItem(index, value);
    }

    public static ImmutableList<T> RemoveFromList<T>(ImmutableList<T> list, T value)
    {
        return list.Remove(value);
    }

    public static ImmutableList<T> RemoveFromListAt<T>(ImmutableList<T> list, int index)
    {
        return index >= 0 && index < list.Count ? list.RemoveAt(index) : list;
    }

    public static ImmutableList<T> AddToList<T>(ImmutableList<T> list, T value, bool ignoreDuplicate = false)
    {
        if (ignoreDuplicate && list.Contains(value)) return list;
        return list.Add(value);
    }


    /// <summary>
    /// Will move an item in an immutable list, returns the resulting list.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="currentIndex">Move this</param>
    /// <param name="newIndex">To here</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ImmutableList<T> RearrangeList<T>(ImmutableList<T> list, int currentIndex, int newIndex)
    {
        if (
            currentIndex == newIndex
            || currentIndex < 0
            || currentIndex >= list.Count
            || newIndex < 0
            || newIndex >= list.Count
        ) return list;

        var item = list[currentIndex];
        return list.RemoveAt(currentIndex).Insert(newIndex, item);
    }

    #endregion

    #region Immutable Dictionary

    public static TDic ReplaceInDictionaryArray<TDic, TKey, TVal>(
        TDic dictionary, TKey key, TVal currentItem, TVal newItem, bool ignoreDuplicate = false
    ) where TDic : IImmutableDictionary<TKey, TVal[]>
    {
        dictionary.TryGetValue(key, out var items);
        var list = (items ?? []).ToList();

        var index = list.IndexOf(currentItem);
        if (index < 0 || EqualityComparer<TVal>.Default.Equals(currentItem, newItem)) return dictionary;

        if (ignoreDuplicate && list.Contains(newItem)) // No need to add it again
        {
            return RemoveFromDictionaryArray(dictionary, key, currentItem); // Because it would have been replaced.
        }

        list[index] = newItem;
        return (TDic)dictionary.SetItem(key, [.. list]);
    }

    public static TDic ReplaceInDictionaryArrayAt<TDic, TKey, TVal>(
        TDic dictionary, TKey key, int index, TVal item, bool ignoreDuplicate = false
    ) where TDic : IImmutableDictionary<TKey, TVal[]>
    {
        dictionary.TryGetValue(key, out var items);
        var list = (items ?? []).ToList();

        if (index < 0 || index >= list.Count) return dictionary;

        if (ignoreDuplicate && list.Contains(item)) // No need to add it again
        {
            return RemoveFromDictionaryArrayAt<TDic, TKey, TVal>(dictionary, key, index); // Because it would have been replaced
        }

        list[index] = item;
        return (TDic)dictionary.SetItem(key, [.. list]);
    }

    public static TDic RemoveFromDictionaryArray<TDic, TKey, TVal>(
        TDic dictionary, TKey key, TVal item
    ) where TDic : IImmutableDictionary<TKey, TVal[]>
    {
        dictionary.TryGetValue(key, out var items);
        if (items == null || !items.Contains(item)) return dictionary;

        var list = items.ToList();
        list.Remove(item);
        return (TDic)dictionary.SetItem(key, [.. list]);
    }

    public static TDic RemoveFromDictionaryArrayAt<TDic, TKey, TVal>(
        TDic dictionary, TKey key, int index
    ) where TDic : IImmutableDictionary<TKey, TVal[]>
    {
        dictionary.TryGetValue(key, out var items);
        if (items == null || index < 0 || index >= items.Length) return dictionary;

        var list = items.ToList();
        list.RemoveAt(index);
        return (TDic)dictionary.SetItem(key, [.. list]);
    }

    public static TDic AddToDictionaryArray<TDic, TKey, TVal>(
        TDic dictionary, TKey key, TVal newItem, bool ignoreDuplicate = false
    ) where TDic : IImmutableDictionary<TKey, TVal[]>
    {
        dictionary.TryGetValue(key, out var items);
        if (items != null && ignoreDuplicate && items.Contains(newItem)) return dictionary;

        var array = items ?? [];
        return (TDic)dictionary.SetItem(key, [.. array, newItem]);
    }

    /// <summary>
    /// Move a value from one index to another. 
    /// </summary>
    /// <param name="dictionary">A dicitonary with array values.</param>
    /// <param name="key">The dictionary key for the array to rearrange.</param>
    /// <param name="currentIndex">The array index of the value we should move.</param>
    /// <param name="newIndex">New array index the value will be moved to.</param>
    /// <typeparam name="TDic">The immutable dictionary type, sorted or not.</typeparam>
    /// <typeparam name="TKey">The key type in the dictionary.</typeparam>
    /// <typeparam name="TVal">The value type in the dicitonary.</typeparam>
    /// <returns></returns>
    public static TDic RearrangeDictionaryArray<TDic, TKey, TVal>(
        TDic dictionary, TKey key, int currentIndex, int newIndex
    ) where TDic : IImmutableDictionary<TKey, TVal[]>
    {
        dictionary.TryGetValue(key, out var items);
        var list = (items ?? []).ToList();

        if (
            currentIndex == newIndex
            || currentIndex < 0
            || currentIndex >= list.Count
            || newIndex < 0
            || newIndex >= list.Count
        ) return dictionary;

        var item = list[currentIndex];
        list.RemoveAt(currentIndex);
        list.Insert(newIndex, item);
        return (TDic)dictionary.SetItem(key, [.. list]);
    }
    
    public static TDic RenameKeyInDictionary<TDic, TKey, TVal>(
        TDic dictionary, TKey oldKey, TKey newKey
    ) where TDic : IImmutableDictionary<TKey, TVal>
    {
        dictionary.TryGetValue(oldKey, out var item);
        if (item == null) return dictionary;
        return (TDic)dictionary.Remove(oldKey).Add(newKey, item);
    }
    
    #endregion

    #region Immutable Sorted String Keyed Array Dictionary

    public static string GetZeroPaddedInt(int value, int max)
    {
        var format = $"D{max.ToString(CultureInfo.InvariantCulture).Length}";
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Only works on sorted dictionaries with string keys in the shape of "serial|whatever else". The pipe is important.
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="divider"></param>
    /// <typeparam name="TVal"></typeparam>
    /// <returns></returns>
    public static ImmutableSortedDictionary<string, TVal> ReserializeSortedDictionary<TVal>(ImmutableSortedDictionary<string, TVal> dictionary, char divider = '|')
    {
        return ReserializeList([.. dictionary], divider).ToImmutableSortedDictionary();
    }

    private static List<KeyValuePair<string, TVal>> ReserializeList<TVal>(List<KeyValuePair<string, TVal>> list, char divider = '|')
    {
        var serial = 1; // We start on 1 so new values can be inserted at 0 and still end up at the top after resorting.
        var max = list.Count;
        var entries = new List<KeyValuePair<string, TVal>>(list.Count);
        foreach (var (key, value) in list)
        {
            var firstPipeIndex = key.IndexOf(divider);
            var rest = firstPipeIndex >= 0 ? key[firstPipeIndex..] : string.Empty;
            entries.Add(KeyValuePair.Create(GetZeroPaddedInt(serial, max) + rest, value));
            serial++;
        }

        return entries;
    }

    public static ImmutableSortedDictionary<string, TVal> RearrangeSortedDictionary<TVal>(ImmutableSortedDictionary<string, TVal> dictionary, string key, int move, char divider = '|')
    {
        var list = dictionary.ToList();
        var index = list.FindIndex(kvp=>kvp.Key.Equals(key));
        if (index < 0) return dictionary;

        var destination = index + move;
        if (destination < 0) destination = 0;
        if (destination >= dictionary.Count) destination = dictionary.Count - 1;

        var item = list[index];
        list.RemoveAt(index);
        list.Insert(destination, item);
        
        return ReserializeList(list, divider).ToImmutableSortedDictionary();
    }
    
    #endregion
}