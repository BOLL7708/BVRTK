using System.Collections.Immutable;

namespace BVRTK;

public partial class Utils
{
    // TODO: GET STUFF FROM THE GENERATOR AND USE THESE UTILITY CLASSES IN THE GENERATOR INSTEAD!
    // TODO: ALSO GET STUFF FOR DICTIONARIES FROM KeyboardSimulatorPages!!!

    #region list

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

    public static ImmutableList<T> ReplaceInList<T>(ImmutableList<T> list, int index, T value, bool ignoreDuplicate = false)
    {
        if (index < 0 || index >= list.Count) return list;

        if (ignoreDuplicate && list.Contains(value)) // No need to add it again
        {
            return RemoveFromList(list, index); // Because it would have been replaced
        }

        return list.SetItem(index, value);
    }

    public static ImmutableList<T> RemoveFromList<T>(ImmutableList<T> list, T value)
    {
        return list.Remove(value);
    }

    public static ImmutableList<T> RemoveFromList<T>(ImmutableList<T> list, int index)
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

    #region Dictionary

    public static ImmutableSortedDictionary<string, T[]> ReplaceInDictionaryArray<T>(
        ImmutableSortedDictionary<string, T[]> dictionary,
        string key, T currentItem, T newItem, bool ignoreDuplicate = false
    )
    {
        dictionary.TryGetValue(key, out var items);
        var list = (items ?? []).ToList();

        var index = list.IndexOf(currentItem);
        if (index < 0 || EqualityComparer<T>.Default.Equals(currentItem, newItem)) return dictionary;

        if (ignoreDuplicate && list.Contains(newItem)) // No need to add it again
        {
            return RemoveFromDictionaryArray(dictionary, key, currentItem); // Because it would have been replaced.
        }

        list[index] = newItem;
        return dictionary.SetItem(key, [.. list]);
    }

    public static ImmutableSortedDictionary<string, T[]> ReplaceInDictionaryArray<T>(
        ImmutableSortedDictionary<string, T[]> dictionary,
        string key, int index, T item, bool ignoreDuplicate = false
    )
    {
        dictionary.TryGetValue(key, out var items);
        var list = (items ?? []).ToList();

        if (index < 0 || index >= list.Count) return dictionary;

        if (ignoreDuplicate && list.Contains(item)) // No need to add it again
        {
            return RemoveFromDictionaryArray(dictionary, key, index); // Because it would have been replaced
        }

        list[index] = item;
        return dictionary.SetItem(key, [.. list]);
    }

    public static ImmutableSortedDictionary<string, T[]> RemoveFromDictionaryArray<T>(
        ImmutableSortedDictionary<string, T[]> dictionary, string key, T item
    )
    {
        dictionary.TryGetValue(key, out var items);
        if (items == null || !items.Contains(item)) return dictionary;

        var list = items.ToList();
        list.Remove(item);
        return dictionary.SetItem(key, [.. list]);
    }

    public static ImmutableSortedDictionary<string, T[]> RemoveFromDictionaryArray<T>(
        ImmutableSortedDictionary<string, T[]> dictionary, string key, int index
    )
    {
        dictionary.TryGetValue(key, out var items);
        if (items == null || index >= items.Length) return dictionary;

        var list = items.ToList();
        list.RemoveAt(index);
        return dictionary.SetItem(key, [.. list]);
    }

    public static ImmutableSortedDictionary<string, T[]> AddToDictionaryArray<T>(
        ImmutableSortedDictionary<string, T[]> dictionary, string key, T newItem, bool ignoreDuplicate = false
    )
    {
        dictionary.TryGetValue(key, out var items);
        if (items != null && ignoreDuplicate && items.Contains(newItem)) return dictionary;

        var array = items ?? [];
        return dictionary.SetItem(key, [.. array, newItem]);
    }


    public static ImmutableSortedDictionary<string, T[]> RearrangeDictionaryArray<T>(ImmutableSortedDictionary<string, T[]> dictionary, string key, int currentIndex, int newIndex)
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
        return dictionary.SetItem(key, [.. list]);
    }

    #endregion
}