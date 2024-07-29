using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Helpers;

/// <summary>
/// Domain object that notify about property changes.
/// </summary>
public class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Set property value.
    /// </summary>
    /// <typeparam name="T">Property type</typeparam>
    /// <param name="reference">Current property value</param>
    /// <param name="value">New property value</param>
    /// <param name="propertyName">Property name</param>
    /// <returns>
    /// Returns true if new value different from old value(reference), otherwise returns false.
    /// </returns>
    public bool SetProperty<T>(ref T reference, T value, [CallerMemberName] in string propertyName = default!)
    {
        if (Equals(reference, value))
            return false;

        reference = value;

        OnPropertyChanged(propertyName);

        return true;
    }

    /// <summary>
    /// Occurs when property changed.
    /// </summary>
    /// <param name="propertyName">Property name</param>
    protected void OnPropertyChanged([CallerMemberName] in string propertyName = default)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}