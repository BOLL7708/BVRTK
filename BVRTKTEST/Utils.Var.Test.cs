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

        // Remove
        var listRemoveTruth = new List<string> { "one", "two", "four" }.ToImmutableList();
        var listRemove = Utils.RemoveFromList(list, "three");
        Assert.NotEqual(list, listRemove);
        Assert.Equal(listRemove, listRemoveTruth);
        
        // Replace
        var listReplaceTruth = new List<string> { "one", "2", "three", "four" }.ToImmutableList();
        var listReplace = Utils.ReplaceInList(list, "two", "2");
        Assert.NotEqual(list, listReplace);
        Assert.Equal(listReplace, listReplaceTruth);
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
}