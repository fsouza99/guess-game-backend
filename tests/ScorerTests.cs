using App.Applications;
using System.IO;
using System.Text.Json;
using Xunit;

namespace App.Tests;

public class ScorerTests
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
    public void Evaluate_Test(int inputReference)
    {
        JsonDocument guess = InputHelper.GetGuessJsonDoc(inputReference);
        JsonDocument reference = InputHelper.GetReferenceJsonDoc(inputReference);
        JsonDocument rules = InputHelper.GetRulesJsonDoc(inputReference);
        int exp_score = InputHelper.GetScore(inputReference);

        int real_score = GuessScorer.Evaluate(guess, reference, rules);

        Assert.Equal(exp_score, real_score);
    }
}

