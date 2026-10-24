using System.Collections.Immutable;
using Xunit;
using Utils = BVRTK.Utils;

namespace BVRTKTEST;

public class UtilsVarTest
{
    #region List

    [Fact]
    public void EditList()
    {
        var list = new List<string> { "one", "two", "three", "four" }.ToImmutableList();

        // Add
        var listAddTruth = new List<string> { "one", "two", "three", "four", "five" }.ToImmutableList();
        var listAdd = Utils.AddToList(list, "five");
        Assert.NotEqual(list, listAdd);
        Assert.Equal(listAdd, listAddTruth);

        var listAddDupeTruth = new List<string> { "one", "two", "three", "four", "four" }.ToImmutableList();
        var listAddDupe = Utils.AddToList(list, "four");
        Assert.NotEqual(list, listAddDupe);
        Assert.Equal(listAddDupe, listAddDupeTruth);

        var listAddNoDupe = Utils.AddToList(list, "four", true);
        Assert.Equal(list, listAddNoDupe);

        // Remove
        var listRemoveTruth = new List<string> { "one", "two", "four" }.ToImmutableList();
        var listRemove = Utils.RemoveFromList(list, "three");
        Assert.NotEqual(list, listRemove);
        Assert.Equal(listRemove, listRemoveTruth);
        var listRemove2 = Utils.RemoveFromList(list, 2);
        Assert.NotEqual(list, listRemove2);
        Assert.Equal(listRemove2, listRemoveTruth);
        
        var listRemove3 = Utils.RemoveFromList(list, "five");
        Assert.Equal(list, listRemove3);
        var listRemove4 = Utils.RemoveFromList(list, 100);
        Assert.Equal(list, listRemove4);

        // Replace
        var listReplaceTruth = new List<string> { "one", "2", "three", "four" }.ToImmutableList();
        var listReplace = Utils.ReplaceInList(list, "two", "2");
        Assert.NotEqual(list, listReplace);
        Assert.Equal(listReplace, listReplaceTruth);
        var listReplace2 = Utils.ReplaceInList(list, 1, "2");
        Assert.NotEqual(list, listReplace2);
        Assert.Equal(listReplace2, listReplaceTruth);
        
        var listReplace3 = Utils.ReplaceInList(list, "five", "2");
        Assert.Equal(list, listReplace3);
        var listReplace4 = Utils.ReplaceInList(list, 100, "2");
        Assert.Equal(list, listReplace4);
    }

    [Fact]
    public void RearrangeList()
    {
        var list = new List<string> { "one", "two", "three", "four", "five", "six" }.ToImmutableList();

        // Move one down
        var listOneDownTruth = new List<string> { "one", "three", "two", "four", "five", "six" }.ToImmutableList();
        var listOneDown = Utils.RearrangeList(list, 2, 1);
        Assert.NotEqual(list, listOneDown);
        Assert.Equal(listOneDown, listOneDownTruth);

        // Move one up
        var listOneUpTruth = new List<string> { "one", "two", "three", "five", "four", "six" }.ToImmutableList();
        var listOneUp = Utils.RearrangeList(list, 4, 3);
        Assert.NotEqual(list, listOneUp);
        Assert.Equal(listOneUp, listOneUpTruth);

        // Move to end
        var listEndTruth = new List<string> { "one", "three", "four", "five", "six", "two" }.ToImmutableList();
        var listEnd = Utils.RearrangeList(list, 1, 5);
        Assert.NotEqual(list, listEnd);
        Assert.Equal(listEnd, listEndTruth);

        // Move to start
        var listStartTruth = new List<string> { "five", "one", "two", "three", "four", "six" }.ToImmutableList();
        var listStart = Utils.RearrangeList(list, 4, 0);
        Assert.NotEqual(list, listStart);
        Assert.Equal(listStart, listStartTruth);

        // Move down
        var listDownTruth = new List<string> { "one", "five", "two", "three", "four", "six" }.ToImmutableList();
        var listDown = Utils.RearrangeList(list, 4, 1);
        Assert.NotEqual(list, listDown);
        Assert.Equal(listDown, listDownTruth);

        // Move up
        var listUpTruth = new List<string> { "one", "three", "four", "five", "two", "six" }.ToImmutableList();
        var listUp = Utils.RearrangeList(list, 1, 4);
        Assert.NotEqual(list, listUp);
        Assert.Equal(listUp, listUpTruth);
    }

    #endregion

    #region Dictionary

    [Fact]
    public void EditDictionary()
    {
        var dic = new SortedDictionary<string, string[]> { { "one", ["two"] }, { "three", ["four"] } }.ToImmutableSortedDictionary();

        // Add
        var dicAddTruth = new SortedDictionary<string, string[]> { { "one", ["two", "five"] }, { "three", ["four"] } }.ToImmutableSortedDictionary();
        var dicAdd = Utils.AddDictionaryArrayItem<string>(dic, "one", "five");
        Assert.NotEqual(dic, dicAdd);
        Assert.Equal(dicAdd, dicAddTruth);
        var dicAddNewTruth = new SortedDictionary<string, string[]> { { "one", ["two"] }, { "three", ["four"] }, { "five", ["six"] } }.ToImmutableSortedDictionary();
        var dicAddNew = Utils.AddDictionaryArrayItem<string>(dic, "five", "six");
        Assert.NotEqual(dic, dicAddNew);
        Assert.Equal(dicAddNew, dicAddNewTruth);
        
        var dicAddDupeTruth = new SortedDictionary<string, string[]> { { "one", ["two", "two"] }, { "three", ["four"] } }.ToImmutableSortedDictionary();
        var dicAddDupe = Utils.AddDictionaryArrayItem<string>(dic, "one", "two");
        Assert.NotEqual(dic, dicAddDupe);
        Assert.Equal(dicAddDupe, dicAddDupeTruth);
        
        var dicAddNoDupe = Utils.AddDictionaryArrayItem<string>(dic, "three", "four", true);
        Assert.Equal(dic, dicAddNoDupe);
    #endregion
}