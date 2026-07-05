using XTerm.Input;

namespace XTerm.Tests;

public class KittyKeyboardProtocolTests
{
    private static Terminal CreateTerminal(int cols = 40, int rows = 4)
    {
        var options = new XTerm.Options.TerminalOptions { Cols = cols, Rows = rows };
        return new Terminal(options);
    }

    [Fact]
    public void KittyPush_SetsCurrentFlags()
    {
        var terminal = CreateTerminal();

        terminal.Write("\u001b[>1u");

        Assert.Equal(1, terminal.KittyFlags);
    }

    [Fact]
    public void KittyPop_RestoresPreviousFlags()
    {
        var terminal = CreateTerminal();

        terminal.Write("\u001b[>1u");
        terminal.Write("\u001b[<u");

        Assert.Equal(0, terminal.KittyFlags);
    }

    [Fact]
    public void KittySet_ReplacesCurrentFlags()
    {
        var terminal = CreateTerminal();

        terminal.Write("\u001b[=5;1u");

        Assert.Equal(5, terminal.KittyFlags);
    }

    [Fact]
    public void KittyQuery_ReportsCurrentFlags()
    {
        var terminal = CreateTerminal();
        string? response = null;
        terminal.DataReceived += (_, e) => response = e.Data;

        terminal.Write("\u001b[>5u");
        terminal.Write("\u001b[?u");

        Assert.Equal("\u001b[?5u", response);
    }

    [Fact]
    public void ModifyOtherKeys_SetsAndResetsLevel()
    {
        var terminal = CreateTerminal();

        terminal.Write("\u001b[>4;2m");
        Assert.Equal(2, terminal.ModifyOtherKeysLevel);

        terminal.Write("\u001b[>4;0m");
        Assert.Equal(0, terminal.ModifyOtherKeysLevel);
    }

    [Fact]
    public void Reset_ClearsKeyboardProtocolState()
    {
        var terminal = CreateTerminal();

        terminal.Write("\u001b[>1u");
        terminal.Write("\u001b[>4;2m");
        terminal.Reset();

        Assert.Equal(0, terminal.KittyFlags);
        Assert.Equal(0, terminal.ModifyOtherKeysLevel);
    }

    [Fact]
    public void ShiftEnter_IsLegacyCarriageReturn_WhenNoProtocolActive()
    {
        var terminal = CreateTerminal();

        Assert.Equal("\r", terminal.GenerateKeyInput(Key.Enter, KeyModifiers.Shift));
    }

    [Fact]
    public void ShiftEnter_IsKittyEncoded_WhenDisambiguateActive()
    {
        var terminal = CreateTerminal();
        terminal.Write("\u001b[>1u");

        Assert.Equal("\u001b[13;2u", terminal.GenerateKeyInput(Key.Enter, KeyModifiers.Shift));
        // The disambiguate flag leaves unmodified Enter as its legacy carriage return.
        Assert.Equal("\r", terminal.GenerateKeyInput(Key.Enter));
    }

    [Fact]
    public void CmdEnter_EncodesSuperModifier_UnderKitty()
    {
        var terminal = CreateTerminal();
        terminal.Write("\u001b[>1u");

        // Super adds 8 to the modifier, so Cmd/Win+Enter is modifier 9.
        Assert.Equal("\u001b[13;9u", terminal.GenerateKeyInput(Key.Enter, KeyModifiers.Super));
    }

    [Fact]
    public void ShiftEnter_IsModifyOtherKeysEncoded_WhenLevelSet()
    {
        var terminal = CreateTerminal();
        terminal.Write("\u001b[>4;2m");

        Assert.Equal("\u001b[27;2;13~", terminal.GenerateKeyInput(Key.Enter, KeyModifiers.Shift));
    }

    [Fact]
    public void ShiftTabAndBackspace_AreProtocolEncoded_UnderKitty()
    {
        var terminal = CreateTerminal();
        terminal.Write("\u001b[>1u");

        Assert.Equal("\u001b[9;2u", terminal.GenerateKeyInput(Key.Tab, KeyModifiers.Shift));
        Assert.Equal("\u001b[127;5u", terminal.GenerateKeyInput(Key.Backspace, KeyModifiers.Control));
    }

    [Fact]
    public void KittyPush_EvictsOldest_WhenStackIsFull()
    {
        var terminal = CreateTerminal();

        for (int i = 0; i < 16; i++)
        {
            terminal.Write("\u001b[>1u");
        }

        // The 17th push must still take effect (oldest evicted), not be ignored.
        terminal.Write("\u001b[>2u");

        Assert.Equal(2, terminal.KittyFlags);
    }

    [Fact]
    public void KittySet_DefaultsModeToSet_WhenModeFieldIsEmpty()
    {
        var terminal = CreateTerminal();

        terminal.Write("\u001b[=5;u");

        Assert.Equal(5, terminal.KittyFlags);
    }
}
