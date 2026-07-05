using XTerm;
using XTerm.Options;

namespace XTerm.Tests;

public class PrivateCsiPrefixTests
{
    private static Terminal CreateTerminal(int cols = 40, int rows = 4)
    {
        var options = new TerminalOptions { Cols = cols, Rows = rows };
        return new Terminal(options);
    }

    [Fact]
    public void ModifyOtherKeys_IsNotTreatedAsSgr()
    {
        // xterm modifyOtherKeys (CSI > 4 ; 2 m) shares the final byte with SGR;
        // it must not enable underline/dim.
        var terminal = CreateTerminal();

        terminal.Write("\x1b[>4;2mhello");

        var line = terminal.Buffer.GetLine(0)!;
        Assert.False(line[0].Attributes.IsUnderline());
        Assert.False(line[0].Attributes.IsDim());
        Assert.Equal("hello", line.TranslateToString(true));
    }

    [Fact]
    public void KittyKeyboardSequences_DoNotRestoreCursor()
    {
        var terminal = CreateTerminal();

        terminal.Write("\x1b[2;5Hx\x1b[s");
        terminal.Write("\x1b[3;1H\x1b[>1u\x1b[<u\x1b[?uy");

        // The cursor must stay where CUP put it; kitty push/pop/query must not
        // act as CSI u (restore cursor).
        Assert.Equal("y", terminal.Buffer.GetLine(2)!.TranslateToString(true));
    }

    [Fact]
    public void PlainSgrUnderline_StillWorks()
    {
        var terminal = CreateTerminal();

        terminal.Write("\x1b[4mu");

        Assert.True(terminal.Buffer.GetLine(0)![0].Attributes.IsUnderline());
    }

    [Fact]
    public void DecPrivateModes_StillWork()
    {
        var terminal = CreateTerminal();

        terminal.Write("\x1b[?25l");
        Assert.False(terminal.CursorVisible);

        terminal.Write("\x1b[?25h");
        Assert.True(terminal.CursorVisible);
    }
}
