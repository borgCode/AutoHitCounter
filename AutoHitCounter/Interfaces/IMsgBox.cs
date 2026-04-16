//

using System.Collections.Generic;
using AutoHitCounter.Views.Windows;

namespace AutoHitCounter.Interfaces;

public interface IMsgBox
{
    void Show(string message, string title = "Message");
    bool ShowOkCancel(string message, string title = "Message");
    string ShowInput(string prompt, string defaultValue = "", string title = "Input");
    Dictionary<string, string> ShowInputs(InputField[] fields, string title = "Input");
    bool ShowYesNo(string message, string title = "Message");
    bool? ShowYesNoCancel(string message, string title = "Message");
    CustomMessageBoxResult ShowCustomButtons(string message, string title, CustomMessageBoxResult[] buttons);
}
