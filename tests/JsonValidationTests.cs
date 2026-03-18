using App.Applications;
using System.IO;
using System.Text.Json;
using Xunit;

namespace App.Tests;

public class JsonValidationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void DataTemplate_Test(int inputReference)
    {
        JsonDocument temp = InputHelper.GetBadDataTempJsonDoc(inputReference);
        bool check_result = JsonDataChecker.DataTemplate(temp);
        Assert.False(check_result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void DataOnTemplate_Test(int inputReference)
    {
        JsonDocument data = InputHelper.GetBadDataJsonDoc(inputReference);
        JsonDocument temp = InputHelper.GetGoodDataTempJsonDoc(inputReference);
        bool check_result = JsonDataChecker.DataOnTemplate(data, temp);
        Assert.False(check_result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void ScoringRulesOnTemplate_Test(int inputReference)
    {
        JsonDocument data = InputHelper.GetBadRulesJsonDoc(inputReference);
        JsonDocument temp = InputHelper.GetGoodDataTempJsonDoc(inputReference);
        bool check_result = JsonDataChecker.ScoringRulesOnTemplate(data, temp);
        Assert.False(check_result);
    }
}

