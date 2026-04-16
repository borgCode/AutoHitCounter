//

using System.Collections.Generic;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Views.Windows;

namespace AutoHitCounter.Utilities;

/// <summary>
/// Static helper class to show custom message boxes from anywhere in the application.
/// Delegates to a swappable <see cref="IMsgBox"/> implementation for testability.
/// </summary>
public static class MsgBox
{
    public static IMsgBox Service { get; set; } = new WpfMsgBox();

    public static void Show(string message, string title = "Message")
        => Service.Show(message, title);

    public static bool ShowOkCancel(string message, string title = "Message")
        => Service.ShowOkCancel(message, title);

    public static string ShowInput(string prompt, string defaultValue = "", string title = "Input")
        => Service.ShowInput(prompt, defaultValue, title);

    public static Dictionary<string, string>? ShowInputs(InputField[] fields, string title = "Input")
        => Service.ShowInputs(fields, title);

    public static bool ShowYesNo(string message, string title = "Message")
        => Service.ShowYesNo(message, title);

    public static bool? ShowYesNoCancel(string message, string title = "Message")
        => Service.ShowYesNoCancel(message, title);

    public static CustomMessageBoxResult ShowCustomButtons(string message, string title, CustomMessageBoxResult[] buttons)
        => Service.ShowCustomButtons(message, title, buttons);
}

internal class WpfMsgBox : IMsgBox
{
    public void Show(string message, string title)
    {
        var box = new CustomMessageBox(message, title, showYesNo: false, showCancel: false);
        box.ShowDialog();
    }

    public bool ShowOkCancel(string message, string title)
    {
        var box = new CustomMessageBox(message, title, showYesNo: false, showCancel: true);
        box.ShowDialog();
        return box.Result ?? false;
    }

    public string ShowInput(string prompt, string defaultValue, string title)
    {
        var box = new InputBox(prompt, defaultValue, title);
        box.ShowDialog();
        return box.Result ? box.InputValue : null;
    }

    public Dictionary<string, string> ShowInputs(InputField[] fields, string title)
    {
        var box = new InputBox(fields, title);
        box.ShowDialog();
        return box.Result ? box.GetValues() : null;
    }

    public bool ShowYesNo(string message, string title)
    {
        var box = new CustomMessageBox(message, title, showYesNo: true, showCancel: false);
        box.ShowDialog();
        return box.Result ?? false;
    }

    public bool? ShowYesNoCancel(string message, string title)
    {
        var box = new CustomMessageBox(message, title, showYesNo: true, showCancel: true);
        box.ShowDialog();
        return box.Result;
    }

    public CustomMessageBoxResult ShowCustomButtons(string message, string title, CustomMessageBoxResult[] buttons)
    {
        var box = new CustomMessageBox(message, title, buttons);
        box.ShowDialog();
        return box.ResultValue;
    }
}
