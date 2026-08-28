using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WPF_App.Infrastructure;

public abstract class ObservableObject : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;

		field = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		return true;
	}
}

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
	private readonly Func<bool> canExecute = canExecute ?? (() => true);
	public event EventHandler? CanExecuteChanged;
	public bool CanExecute(object? parameter) => canExecute();
	public void Execute(object? parameter) => execute();
	public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}