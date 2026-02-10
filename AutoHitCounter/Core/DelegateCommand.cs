using System;
using System.Windows.Input;

namespace AutoHitCounter.Core;

public class DelegateCommand : ICommand
{
    private readonly Action<object>? _execute;
    private readonly Action? _executeWithoutParam;
    private readonly Predicate<object>? _canExecute;
    private readonly Func<bool>? _canExecuteWithoutParam;

    public DelegateCommand(Action<object> execute, Predicate<object>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public DelegateCommand(Action execute, Func<bool>? canExecute = null)
    {
        _executeWithoutParam = execute;
        _canExecuteWithoutParam = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public bool CanExecute(object? parameter) =>
        _canExecuteWithoutParam?.Invoke() ?? _canExecute?.Invoke(parameter ?? new object()) ?? true;

    public void Execute(object? parameter)
    {
        _execute?.Invoke(parameter ?? new object());
        _executeWithoutParam?.Invoke();
    }
}

internal class DelegateCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?>? _canExecute;

    public DelegateCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChange() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter is T t ? t : default) ?? true;

    public void Execute(object? parameter)
    {
        if (parameter is T t)
            _execute(t);
        else
            _execute(default);
    }
}