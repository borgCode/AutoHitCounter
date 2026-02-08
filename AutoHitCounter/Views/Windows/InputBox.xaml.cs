// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Views.Windows;

public partial class InputBox : Window
{
    public bool Result { get; private set; }
    
    private readonly List<TextBox> _textBoxes = new();
    private readonly string[] _keys;

    public InputBox(InputField[] fields, string title = "Input")
    {
        InitializeComponent();
        TitleText.Text = title;
        _keys = fields.Select(f => f.Key).ToArray();

        foreach (var field in fields)
        {
            var label = new TextBlock
            {
                Text = field.Prompt,
                Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#F1F1F1")!,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var textBox = new TextBox
            {
                Text = field.DefaultValue,
                Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#3E3E42")!,
                Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#F1F1F1")!,
                BorderBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#555555")!,
                Padding = new Thickness(5, 3, 5, 3),
                Margin = new Thickness(0, 0, 0, 12)
            };
            textBox.KeyDown += InputTextBox_KeyDown;

            _textBoxes.Add(textBox);
            InputsPanel.Children.Add(label);
            InputsPanel.Children.Add(textBox);
        }

        Loaded += (s, e) =>
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            User32.SetTopmost(hwnd);
            
            if (_textBoxes.Count > 0)
            {
                _textBoxes[0].Focus();
                _textBoxes[0].SelectAll();
            }

            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Closing += (sender, args) => Close();
            }
        };
    }
    
    public InputBox(string prompt, string defaultValue = "", string title = "Input")
        : this(new[] { new InputField("value", prompt, defaultValue) }, title)
    {
    }

    public Dictionary<string, string> GetValues()
    {
        var result = new Dictionary<string, string>();
        for (int i = 0; i < _keys.Length; i++)
        {
            result[_keys[i]] = _textBoxes[i].Text;
        }
        return result;
    }
    
    public string InputValue => _textBoxes.Count > 0 ? _textBoxes[0].Text : string.Empty;

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var currentBox = sender as TextBox;
            int index = _textBoxes.IndexOf(currentBox!);
            
            if (index < _textBoxes.Count - 1)
            {
                _textBoxes[index + 1].Focus();
                _textBoxes[index + 1].SelectAll();
            }
            else
            {
                Result = true;
                Close();
            }
        }
        else if (e.Key == Key.Escape)
        {
            Result = false;
            Close();
        }
    }
}

public record InputField(string Key, string Prompt, string DefaultValue = "")
{
    public string Key { get; } = Key;
    public string Prompt { get; } = Prompt;
    public string DefaultValue { get; } = DefaultValue;
}