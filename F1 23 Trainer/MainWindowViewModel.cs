using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace F1_23_Trainer;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private bool _attached;

    [ObservableProperty] private bool _hotkeysVisible;

    [ObservableProperty] private double _hotkeysOpacity;

    [ObservableProperty] private double _hotkeysBackgroundOpacity;
    
    [ObservableProperty] private string _accelHotkeyString = null!;
    
    [ObservableProperty] private string _brakeHotkeyString = null!;
    
    [ObservableProperty] private string _jumpHotkeyString = null!;

    public static string SteralizeHokeyString(GlobalHotkey hotkey)
    {
        return hotkey.Modifier == ModifierKeys.None ? $"[{hotkey.Key}]" : $"[{hotkey.Modifier} + {hotkey.Key}]";
    }
}