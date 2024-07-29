using System.Diagnostics;
using System.Windows.Input;

namespace Client.Commands;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;

    public AsyncRelayCommand(Func<Task> execute) : this(execute, null) { }

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object parameter) => await ExecuteAsync();

    private async Task ExecuteAsync()
    {
        try
        {
            await _execute();
        }
        catch (Exception ex)
        {
            // Log it somewhere
            Debug.WriteLine($"An error occurred while executing the command: {ex.Message}");
        }
    }
}