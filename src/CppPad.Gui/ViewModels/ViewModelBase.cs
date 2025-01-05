﻿#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#endregion

namespace CppPad.Gui.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string propertyName = null!
    )
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
