using XTerm;
using XTerm.Options;

namespace XTerm.Tests;

public class CharsetDesignationTests
{
    private static Terminal CreateTerminal(int cols = 80, int rows = 24)
    {
        var options = new TerminalOptions { Cols = cols, Rows = rows };
        return new Terminal(options);
    }

    [Fact]
    public void ConsecutiveDesignations_DoNotLeakCollectBytes()
    {
        // zsh/tcell emit "ESC(B ESC)0" (enacs) at startup. The ')' sequence must
        // designate G1, not re-designate G0 with a stale '(' from the previous
        // sequence, otherwise all subsequent ASCII renders as DEC line-drawing.
        var terminal = CreateTerminal();

        terminal.Write("\x1b(B\x1b)0main");

        Assert.Equal("main", terminal.Buffer.GetLine(0)!.TranslateToString(true));
    }

    [Fact]
    public void DecGraphicsOnG0_StillTranslates()
    {
        var terminal = CreateTerminal();

        terminal.Write("\x1b(0qqq");

        Assert.Equal("───", terminal.Buffer.GetLine(0)!.TranslateToString(true));
    }

    [Fact]
    public void ShiftOutShiftIn_SwitchBetweenG0AndG1()
    {
        var terminal = CreateTerminal();

        terminal.Write("\x1b)0\x0eq\x0fq");

        Assert.Equal("─q", terminal.Buffer.GetLine(0)!.TranslateToString(true));
    }
}
