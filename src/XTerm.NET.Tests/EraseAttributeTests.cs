using XTerm;
using XTerm.Options;

namespace XTerm.Tests;

public class EraseAttributeTests
{
    private static Terminal CreateTerminal(int cols = 40, int rows = 4)
    {
        var options = new TerminalOptions { Cols = cols, Rows = rows };
        return new Terminal(options);
    }

    private static int CountUnderlinedCells(Terminal terminal, int row)
    {
        var line = terminal.Buffer.GetLine(row)!;
        int count = 0;
        for (int x = 0; x < line.Length; x++)
        {
            if (line[x].Attributes.IsUnderline())
                count++;
        }
        return count;
    }

    [Fact]
    public void EraseInLine_DoesNotSpreadUnderlineIntoBlanks()
    {
        // Erased cells take only the background color (BCE, as in xterm);
        // text attributes such as underline must not bleed into blanks.
        var terminal = CreateTerminal();

        terminal.Write("\x1b[4munder\x1b[m plain\x1b[4m\x1b[K");

        Assert.Equal(5, CountUnderlinedCells(terminal, 0));
    }

    [Fact]
    public void EraseInDisplay_DoesNotSpreadUnderlineIntoBlanks()
    {
        var terminal = CreateTerminal();

        terminal.Write("\x1b[4mX\x1b[2J");

        Assert.Equal(0, CountUnderlinedCells(terminal, 0));
    }

    [Fact]
    public void EraseInLine_KeepsBackgroundColor()
    {
        var terminal = CreateTerminal();

        terminal.Write("\x1b[44m\x1b[K");

        var line = terminal.Buffer.GetLine(0)!;
        Assert.Equal(4, line[10].Attributes.GetBgColor());
    }
}
