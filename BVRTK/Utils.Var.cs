using System.Collections.Immutable;

namespace BVRTK;

public partial class Utils
{
    
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
    
    public static ImmutableList<T> RemoveFromList<T>(ImmutableList<T> list, T value)
    {
        return list.Contains(value) ? list.Remove(value) : list;
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
        if(
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
}