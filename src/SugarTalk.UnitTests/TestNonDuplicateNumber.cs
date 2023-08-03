using Xunit;
using Shouldly;

namespace SugarTalk.UnitTests;

public class Tests
{
    [Fact]
    public void TestNonDuplicateNumber()
    {
        var meetingNumbers = new List<string> { "1", "2", "3" };

        var availableNumbers = Enumerable
            .Range(0, 10)
            .Select(num => num.ToString())
            .Except(meetingNumbers).ToList();

        availableNumbers.Count.ShouldBe(7);
        availableNumbers.Any(x => meetingNumbers.Contains(x)).ShouldBeFalse();
    }
}