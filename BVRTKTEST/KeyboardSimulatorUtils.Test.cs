using BVRTK.Components.KeyboardSimulator;
using Xunit;

namespace BVRTKTEST;

public class KeyboardSimulatorUtilsTest
{
    [Fact]
    public void ParseGroup()
    {
        var group1 = KeyboardSimulatorUtils.ParseGroup("002|1 TestName");
        Assert.Equal(2, group1.Order);
        Assert.True(group1.Enabled);
        Assert.Equal("TestName", group1.Name);
        
        var group2 = KeyboardSimulatorUtils.ParseGroup("004|0 TestName2");
        Assert.Equal(4, group2.Order);
        Assert.False(group2.Enabled);
        Assert.Equal("TestName2", group2.Name);
    } 
}