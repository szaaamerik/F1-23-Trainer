﻿using System.Windows.Input;

namespace F1_23_Trainer;

// ReSharper disable once ClassNeverInstantiated.Global
public class GlobalHotkey(ModifierKeys modifier, Key key, Action callbackMethod, bool canExecute = false)
{
    public ModifierKeys Modifier { get; set; } = modifier;
    public Key Key { get; set; } = key;
    public Action Callback { get; set; } = callbackMethod;
    public bool CanExecute { get; set; } = canExecute;
}