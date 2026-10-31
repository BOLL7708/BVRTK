using System.Collections.Immutable;

namespace BVRTK;

public static class VarExtensions
{
    extension<TVal>(ImmutableSortedDictionary<string, TVal> dictionary)
    {
        public ImmutableSortedDictionary<string, TVal> ReserializeKeys() => Utils.ReserializeSortedDictionary(dictionary);
        public ImmutableSortedDictionary<string, TVal> RenameKey(string oldKey, string newKey) => Utils.RenameKeyInDictionary<ImmutableSortedDictionary<string, TVal>, string, TVal>(dictionary, oldKey, newKey);
        public ImmutableSortedDictionary<string, TVal> RearrangeKey(string key, int move) => Utils.RearrangeSortedDictionary(dictionary, key, move);
        public ImmutableSortedDictionary<string, TVal> RemoveKey(string key) => dictionary.Remove(key).ReserializeKeys();
    }
}