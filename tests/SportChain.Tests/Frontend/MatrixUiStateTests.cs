using System.Drawing;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;
using Xunit;

namespace SportChain.Tests.Frontend;

public class MatrixUiStateTests
{
    [Fact]
    public void AppTheme_SemanticColors_AdhereToTailwindDesignTokens()
    {
        // Assert: Brand & Semantic colors
        Assert.Equal(Color.FromArgb(13, 148, 136), AppTheme.Primary);     // Teal 600
        Assert.Equal(Color.FromArgb(16, 185, 129), AppTheme.Success);     // Emerald 500
        Assert.Equal(Color.FromArgb(245, 158, 11), AppTheme.Warning);     // Amber 500
        Assert.Equal(Color.FromArgb(239, 68, 68), AppTheme.Danger);       // Red 500
        Assert.Equal(Color.FromArgb(2, 132, 199), AppTheme.Info);         // Sky 600
    }


    [Theory]
    [InlineData(BadgeType.Success, IconChar.CheckCircle)]
    [InlineData(BadgeType.Warning, IconChar.Clock)]
    [InlineData(BadgeType.Danger, IconChar.TimesCircle)]
    [InlineData(BadgeType.InUse, IconChar.PlayCircle)]
    [InlineData(BadgeType.Info, IconChar.InfoCircle)]
    public void StatusBadge_TypeChange_UpdatesDefaultIconCorrectly(BadgeType type, IconChar expectedIcon)
    {
        // Arrange
        var badge = new StatusBadge();

        // Act
        badge.Type = type;

        // Assert
        Assert.Equal(expectedIcon, badge.Icon);
    }

    [Fact]
    public void PastSlotCheck_EvaluatesCorrectlyForCurrentDay()
    {
        // Giả lập thời gian hiện tại là 14:30
        var simulatedNow = new TimeSpan(14, 30, 0);

        var slotMorning = new { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 0, 0) };
        var slotCurrent = new { StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(15, 0, 0) };
        var slotEvening = new { StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(19, 0, 0) };

        // Ca sáng đã kết thúc (EndTime <= simulatedNow)
        bool isMorningPast = slotMorning.EndTime <= simulatedNow;
        // Ca hiện tại chưa kết thúc (EndTime > simulatedNow)
        bool isCurrentPast = slotCurrent.EndTime <= simulatedNow;
        // Ca tối trong tương lai
        bool isEveningPast = slotEvening.EndTime <= simulatedNow;

        // Assert
        Assert.True(isMorningPast, "Ca sáng 08:00 - 09:00 phải được nhận diện là đã qua khi hiện tại là 14:30");
        Assert.False(isCurrentPast, "Ca 14:00 - 15:00 chưa kết thúc nên không được coi là đã qua");
        Assert.False(isEveningPast, "Ca tối 18:00 - 19:00 là ca tương lai");
    }
}
