using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace F1_23_Trainer;

public sealed partial class MainWindow
{
    public static string ProcessName => "F1_23.exe";
    public static string GameName => "F1 23";
    public static string GameVersion => "v1.20.1082038";

    private GlobalHotkey _currentHotkey = null!;
    private readonly GlobalHotkey _accelHotkey;
    private readonly GlobalHotkey _brakeHotkey;
    private readonly GlobalHotkey _jumpHotkey;

    private readonly Cheats _cheats;
    public MainWindowViewModel ViewModel { get; }
    
    public MainWindow()
    {
        ViewModel = new MainWindowViewModel();
        DataContext = this;

        InitializeComponent();
        _cheats = new Cheats(this);
        _cheats.SetupAttach();
        
        _accelHotkey = new GlobalHotkey(ModifierKeys.None, Key.None, AccelHackCallback);
        _brakeHotkey = new GlobalHotkey(ModifierKeys.None, Key.None, BrakeHackCallback);
        _jumpHotkey = new GlobalHotkey(ModifierKeys.None, Key.None, JumpHackCallback);
        
        HotkeysManager.AddHotkey(_accelHotkey);
        HotkeysManager.AddHotkey(_brakeHotkey);
        HotkeysManager.AddHotkey(_jumpHotkey);

        SteralizeHotkeyStrings();
    }

    private void SteralizeHotkeyStrings()
    {
        ViewModel.AccelHotkeyString = MainWindowViewModel.SteralizeHokeyString(_accelHotkey);
        ViewModel.BrakeHotkeyString = MainWindowViewModel.SteralizeHokeyString(_brakeHotkey);
        ViewModel.JumpHotkeyString = MainWindowViewModel.SteralizeHokeyString(_jumpHotkey);
    }
    
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || ViewModel.HotkeysVisible) return;
        Cursor = Cursors.SizeAll;
        DragMove();
        Cursor = Cursors.Arrow;
    }

    private void Minimize_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || ViewModel.HotkeysVisible) return;
        SystemCommands.MinimizeWindow(this);
    }

    private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || ViewModel.HotkeysVisible) return;
        SystemCommands.CloseWindow(this);
        _cheats.TrainerClose();
        Application.Current.Shutdown();
    }

    private void RectangleOpenLink_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Rectangle rectangle)
        {
            return;
        }

        if (e.ChangedButton != MouseButton.Left || ViewModel.HotkeysVisible) return;
        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = rectangle.ToolTip as string
        };
        Process.Start(processStartInfo);
    }

    private async void OpenHotkeysPrompt()
    {
        HotKeyBox.HotKey = null;
        ViewModel.HotkeysVisible = true;

        if (!HotKeyBox.Focus())
        {
            HotKeyBox.Focus();
        }

        while (ViewModel is { HotkeysOpacity: < 1, HotkeysBackgroundOpacity: < 0.5 })
        {
            ViewModel.HotkeysOpacity += 0.05;
            ViewModel.HotkeysBackgroundOpacity = ViewModel.HotkeysOpacity / 2;
            await Task.Delay(10);
        }
    }

    private async void CloseHotkeysPrompt()
    {
        while (ViewModel is { HotkeysOpacity: > 0, HotkeysBackgroundOpacity: > 0 })
        {
            ViewModel.HotkeysOpacity -= 0.05;
            ViewModel.HotkeysBackgroundOpacity = ViewModel.HotkeysOpacity / 2;
            await Task.Delay(10);
        }

        ViewModel.HotkeysVisible = false;
    }

    private void HotKeyBox_OnHotKeyChanged(object sender, RoutedPropertyChangedEventArgs<HotKey?> e)
    {
        if (e.NewValue == null)
        {
            return;
        }

        if (HotkeysManager.DoesTheSameHotkeyExist(_currentHotkey, e.NewValue.Key, e.NewValue.ModifierKeys))
        {
            MessageBox.Show("This hotkey is already registered!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            HotKeyBox.HotKey = null;
        }
        else
        {
            _currentHotkey.Key = e.NewValue.Key;
            _currentHotkey.Modifier = e.NewValue.ModifierKeys;
            CloseHotkeysPrompt();
            SteralizeHotkeyStrings();
        }
    }

    private async void FuelSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.FuelConsumptionDetourAddress == 0)
        {
            await _cheats.CheatFuelConsumption();
        }
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.FuelConsumptionDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= FuelSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += FuelSwitch_OnToggled;
            return;
        }

        _cheats.Mem.WriteMemory(_cheats.FuelConsumptionDetourAddress + 0x2B, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.FuelConsumptionDetourAddress + 0x2C, Convert.ToSingle(FuelSlider.Value));
    }

    private void FuelSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { FuelConsumptionDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.FuelConsumptionDetourAddress + 0x2C, Convert.ToSingle(e.NewValue));
    }

    private async void GripSwitch_OnToggled(object sender, RoutedEventArgs e)
    {        
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.GripRateDetourAddress == 0)
        {
            await _cheats.CheatGripRate();
        }
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.GripRateDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= GripSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += GripSwitch_OnToggled;
            return;
        }

        _cheats.Mem.WriteMemory(_cheats.GripRateDetourAddress + 0x31, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.GripRateDetourAddress + 0x32, Convert.ToSingle(GripSlider.Value));
    }

    private void GripSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { GripRateDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.GripRateDetourAddress + 0x32, Convert.ToSingle(e.NewValue));
    }

    private async void EngTempSwitch_OnToggled(object sender, RoutedEventArgs e)
    {        
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.EngTempDetourAddress == 0)
        {
            await _cheats.CheatEngineTemp();
        }
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.EngTempDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= EngTempSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += EngTempSwitch_OnToggled;
            return;
        }

        _cheats.Mem.WriteMemory(_cheats.EngTempDetourAddress + 0x2C, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.EngTempDetourAddress + 0x2D, Convert.ToSingle(EngTempSlider.Value));
    }

    private void EngTempSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { EngTempDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.EngTempDetourAddress + 0x2D, Convert.ToSingle(e.NewValue));
    }

    private async void BrakeTempSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.BrakeTempDetourAddress == 0)
        {
            await _cheats.CheatBrakeTemp();
        }
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.BrakeTempDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= BrakeTempSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += BrakeTempSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.BrakeTempDetourAddress + 0x29, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.BrakeTempDetourAddress + 0x2A, Convert.ToSingle(BrakeTempSlider.Value));
    }

    private void BrakeTempSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { BrakeTempDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.BrakeTempDetourAddress + 0x2A, Convert.ToSingle(e.NewValue));
    }

    private async void LapTimeSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.LapTime1DetourAddress == 0 ||
            _cheats.LapTime2DetourAddress == 0 ||
            _cheats.LapTime3DetourAddress == 0)
        {
            await _cheats.CheatLapTime();
        }
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.LapTime1DetourAddress == 0 ||
            _cheats.LapTime2DetourAddress == 0 ||
            _cheats.LapTime3DetourAddress == 0)
        {
            toggleSwitch.Toggled -= LapTimeSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += LapTimeSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.LapTime2DetourAddress + 0x41, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.LapTime3DetourAddress + 0x32, toggleSwitch.IsOn ? (byte)1 : (byte)0);
    }

    private async void TireTempSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.TireTemp1DetourAddress == 0 || _cheats.TireTemp2DetourAddress == 0)
        {
            await _cheats.CheatTireTemp();
        }

        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.TireTemp1DetourAddress == 0 || _cheats.TireTemp2DetourAddress == 0)
        {
            toggleSwitch.Toggled -= TireTempSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += TireTempSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.TireTemp1DetourAddress + 0x29, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.TireTemp1DetourAddress + 0x2A, Convert.ToSingle(TireTempSlider.Value));
        _cheats.Mem.WriteMemory(_cheats.TireTemp2DetourAddress + 0x29, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.TireTemp2DetourAddress + 0x2A, Convert.ToSingle(TireTempSlider.Value));
    }

    private void TireTempSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { TireTemp1DetourAddress: > 0, TireTemp2DetourAddress: > 0 })
        {
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.TireTemp1DetourAddress + 0x2A, Convert.ToSingle(e.NewValue));
        _cheats.Mem.WriteMemory(_cheats.TireTemp2DetourAddress + 0x2A, Convert.ToSingle(e.NewValue));
    }

    private async void ResourcePointsSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.ResourcePointsDetourAddress == 0)
        {
            await _cheats.CheatResourcePoints();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.ResourcePointsDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= ResourcePointsSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += ResourcePointsSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.ResourcePointsDetourAddress + 0x1D, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.ResourcePointsDetourAddress + 0x1E, Convert.ToInt32(ResourcePointsNum.Value));
    }

    private void ResourcePointsNum_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
    {
        if (_cheats is not { ResourcePointsDetourAddress: > 0}) return;
        _cheats.Mem.WriteMemory(_cheats.ResourcePointsDetourAddress + 0x1E, Convert.ToInt32(e.NewValue));
    }

    private void AccelMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _currentHotkey = _accelHotkey;
        OpenHotkeysPrompt();
    }
    
    private void BrakeMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _currentHotkey = _brakeHotkey;
        OpenHotkeysPrompt();
    }

    private void JumpMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _currentHotkey = _jumpHotkey;
        OpenHotkeysPrompt();
    }

    private async void EngPowerSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.EnginePowerDetourAddress == 0)
        {
            await _cheats.CheatEnginePower();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.EnginePowerDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= EngPowerSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += EngPowerSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.EnginePowerDetourAddress + 0x2B, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.EnginePowerDetourAddress + 0x2C, Convert.ToSingle(EngPowerSlider.Value));
    }

    private void EngPowerSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { EnginePowerDetourAddress: > 0}) return;
        _cheats.Mem.WriteMemory(_cheats.EnginePowerDetourAddress + 0x2C, Convert.ToSingle(e.NewValue));
    }

    private async void EngWearSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.EngineWearDetourAddress == 0)
        {
            await _cheats.CheatEngineWear();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.EngineWearDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= EngWearSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += EngWearSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.EngineWearDetourAddress + 0x66, toggleSwitch.IsOn ? (byte)1 : (byte)0);
    }

    private async void TireWearSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.TireWearDetourAddress == 0)
        {
            await _cheats.CheatTireWear();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.TireWearDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= TireWearSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += TireWearSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.TireWearDetourAddress + 0x27, toggleSwitch.IsOn ? (byte)1 : (byte)0);
    }

    private async void ErsSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.ErsDrainRateDetourAddress == 0)
        {
            await _cheats.CheatErsDrainRate();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.ErsDrainRateDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= TireWearSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += TireWearSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.ErsDrainRateDetourAddress + 0x24, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.ErsDrainRateDetourAddress + 0x25, Convert.ToSingle(ErsSlider.Value));
    }

    private void ErsSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { ErsDrainRateDetourAddress: > 0}) return;
        _cheats.Mem.WriteMemory(_cheats.ErsDrainRateDetourAddress + 0x25, Convert.ToSingle(e.NewValue));
    }

    private async void DriverMoneySwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.DriverMoneyDetourAddress == 0)
        {
            await _cheats.CheatDriverMoney();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.DriverMoneyDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= DriverMoneySwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += DriverMoneySwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.DriverMoneyDetourAddress + 0x20, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.DriverMoneyDetourAddress + 0x21, Convert.ToInt32(DriverMoneyNum.Value));
    }

    private void DriverMoneyNum_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
    {
        if (_cheats is not { DriverMoneyDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.DriverMoneyDetourAddress + 0x21, Convert.ToInt32(e.NewValue));
    }

    private async void InstUpgSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.InstUpgradeDetourAddress == 0)
        {
            await _cheats.CheatInstantUpgrade();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.InstUpgradeDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= InstUpgSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += InstUpgSwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.InstUpgradeDetourAddress + 0x19, toggleSwitch.IsOn ? (byte)1 : (byte)0);
    }

    private async void TeamMoneySwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        if (_cheats.TeamMoneyDetourAddress == 0)
        {
            await _cheats.CheatTeamMoney();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.TeamMoneyDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= TeamMoneySwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += TeamMoneySwitch_OnToggled;
            return;
        }
        
        _cheats.Mem.WriteMemory(_cheats.TeamMoneyDetourAddress + 0x22, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.TeamMoneyDetourAddress + 0x23, Convert.ToInt32(DriverMoneyNum.Value));
    }

    private void TeamMoneyNum_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
    {
        if (_cheats is not { TeamMoneyDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.TeamMoneyDetourAddress + 0x23, Convert.ToInt32(e.NewValue));
    }

    private async void TeamAcclaimPull_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || !ViewModel.Attached)
        {
            return;
        }

        TeamAcclaimSwitch.IsEnabled = false;
        button.IsEnabled = false;
        
        if (_cheats.TeamAcclaimDetourAddress == 0)
        {
            await _cheats.CheatTeamAcclaim();
        }
        
        if (ViewModel.Attached)
        {
            TeamAcclaimSwitch.IsEnabled = true;
            button.IsEnabled = true;
        }

        if (_cheats.TeamAcclaimDetourAddress <= 0)
        {
            return;
        }

        TeamAcclaimNum.Value = _cheats.Mem.ReadMemory<float>(_cheats.TeamAcclaimDetourAddress + 0x2F);
    }

    private void TeamAcclaimNum_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
    {
        if (_cheats is not { TeamAcclaimDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.TeamAcclaimDetourAddress + 0x2B, Convert.ToSingle(e.NewValue));
    }

    private async void TeamAcclaimSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        TeamAcclaimPull.IsEnabled = false;
        toggleSwitch.IsEnabled = false;
        
        if (_cheats.TeamAcclaimDetourAddress == 0)
        {
            await _cheats.CheatTeamAcclaim();
        }
        
        if (ViewModel.Attached)
        {
            TeamAcclaimPull.IsEnabled = true;
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.TeamAcclaimDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= TeamAcclaimSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += TeamAcclaimSwitch_OnToggled;
            return;
        }

        _cheats.Mem.WriteMemory(_cheats.TeamAcclaimDetourAddress + 0x2A, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.TeamAcclaimDetourAddress + 0x2B, Convert.ToSingle(TeamAcclaimNum.Value));
    }

    private async void PlayerAcclaim_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || !ViewModel.Attached)
        {
            return;
        }

        PlayerAcclaimSwitch.IsEnabled = false;
        button.IsEnabled = false;
        
        if (_cheats.PlayerAcclaimDetourAddress == 0)
        {
            await _cheats.CheatPlayerAcclaim();
        }
        
        if (ViewModel.Attached)
        {
            PlayerAcclaimSwitch.IsEnabled = true;
            button.IsEnabled = true;
        }

        if (_cheats.PlayerAcclaimDetourAddress <= 0)
        {
            return;
        }

        PlayerAcclaimNum.Value = _cheats.Mem.ReadMemory<float>(_cheats.PlayerAcclaimDetourAddress + 0x2D);
    }

    private void PlayerAcclaimNum_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
    {
        if (_cheats is not { PlayerAcclaimDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.PlayerAcclaimDetourAddress + 0x29, Convert.ToSingle(e.NewValue));
    }

    private async void PlayerAcclaimSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        PlayerAcclaimPull.IsEnabled = false;
        toggleSwitch.IsEnabled = false;
        
        if (_cheats.PlayerAcclaimDetourAddress == 0)
        {
            await _cheats.CheatPlayerAcclaim();
        }
        
        if (ViewModel.Attached)
        {
            PlayerAcclaimPull.IsEnabled = true;
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.PlayerAcclaimDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= PlayerAcclaimSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += PlayerAcclaimSwitch_OnToggled;
            return;
        }

        _cheats.Mem.WriteMemory(_cheats.PlayerAcclaimDetourAddress + 0x28, toggleSwitch.IsOn ? (byte)1 : (byte)0);
        _cheats.Mem.WriteMemory(_cheats.PlayerAcclaimDetourAddress + 0x29, Convert.ToSingle(PlayerAcclaimNum.Value));
    }

    private async void NoDamageSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        
        if (_cheats.NoCollisionDamageDetourAddress == 0)
        {
            await _cheats.CheatNoCollisionDamage();
        }
        
        if (ViewModel.Attached)
        {
            toggleSwitch.IsEnabled = true;
        }

        if (_cheats.NoCollisionDamageDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= NoDamageSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += NoDamageSwitch_OnToggled;
            return;
        }

        _cheats.Mem.WriteMemory(_cheats.NoCollisionDamageDetourAddress + 0x23, toggleSwitch.IsOn ? (byte)1 : (byte)0);
    }

    private async void FreezeAiSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        JumpSwitch.IsEnabled = false;
        AccelSwitch.IsEnabled = false;
        BrakeSwitch.IsEnabled = false;
        FreezeAiSwitch.IsEnabled = false;
        
        if (_cheats.VelocityDetourAddress == 0)
        {
            await _cheats.CheatVelocity();
        }
        
        if (ViewModel.Attached)
        {
            JumpSwitch.IsEnabled = true;
            AccelSwitch.IsEnabled = true;
            BrakeSwitch.IsEnabled = true;
            FreezeAiSwitch.IsEnabled = true;
        }

        if (_cheats.VelocityDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= FreezeAiSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += FreezeAiSwitch_OnToggled;
            return;
        }

        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x113, toggleSwitch.IsOn ? (byte)1 : (byte)0);
    }

    private async void JumpSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        JumpSwitch.IsEnabled = false;
        AccelSwitch.IsEnabled = false;
        BrakeSwitch.IsEnabled = false;
        FreezeAiSwitch.IsEnabled = false;
        
        if (_cheats.VelocityDetourAddress == 0)
        {
            await _cheats.CheatVelocity();
        }
        
        if (ViewModel.Attached)
        {
            JumpSwitch.IsEnabled = true;
            AccelSwitch.IsEnabled = true;
            BrakeSwitch.IsEnabled = true;
            FreezeAiSwitch.IsEnabled = true;
        }

        if (_cheats.VelocityDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= JumpSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += JumpSwitch_OnToggled;
            return;
        }

        _jumpHotkey.CanExecute = toggleSwitch.IsOn;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x105, Convert.ToSingle(JumpSlider.Value * 2));
    }

    private void JumpHackCallback()
    {
        if (_cheats is not { VelocityDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x104, (byte)1);
    }

    private async void BrakeSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        JumpSwitch.IsEnabled = false;
        AccelSwitch.IsEnabled = false;
        BrakeSwitch.IsEnabled = false;
        FreezeAiSwitch.IsEnabled = false;
        
        if (_cheats.VelocityDetourAddress == 0)
        {
            await _cheats.CheatVelocity();
        }
        
        if (ViewModel.Attached)
        {
            JumpSwitch.IsEnabled = true;
            AccelSwitch.IsEnabled = true;
            BrakeSwitch.IsEnabled = true;
            FreezeAiSwitch.IsEnabled = true;
        }

        if (_cheats.VelocityDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= BrakeSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += BrakeSwitch_OnToggled;
            return;
        }

        _brakeHotkey.CanExecute = toggleSwitch.IsOn;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x10A, 1f - Convert.ToSingle(BrakeSlider.Value / 2));
    }

    private void BrakeHackCallback()
    {
        if (_cheats is not { VelocityDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x109, (byte)1);
    }

    private async void AccelSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggleSwitch || !ViewModel.Attached)
        {
            return;
        }

        JumpSwitch.IsEnabled = false;
        AccelSwitch.IsEnabled = false;
        BrakeSwitch.IsEnabled = false;
        FreezeAiSwitch.IsEnabled = false;
        
        if (_cheats.VelocityDetourAddress == 0)
        {
            await _cheats.CheatVelocity();
        }
        
        if (ViewModel.Attached)
        {
            JumpSwitch.IsEnabled = true;
            AccelSwitch.IsEnabled = true;
            BrakeSwitch.IsEnabled = true;
            FreezeAiSwitch.IsEnabled = true;
        }

        if (_cheats.VelocityDetourAddress <= 0)
        {
            toggleSwitch.Toggled -= AccelSwitch_OnToggled;
            toggleSwitch.IsOn = false;
            toggleSwitch.Toggled += AccelSwitch_OnToggled;
            return;
        }

        _accelHotkey.CanExecute = toggleSwitch.IsOn;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x10F, 1f + Convert.ToSingle(AccelSlider.Value / 2));
    }

    private void AccelHackCallback()
    {
        if (_cheats is not { VelocityDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x10E, (byte)1);
    }

    private void AccelSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { VelocityDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x10F, 1f + Convert.ToSingle(AccelSlider.Value / 2));
    }

    private void BrakeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { VelocityDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x10A, Convert.ToSingle(BrakeSlider.Value / 10));
    }

    private void JumpSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_cheats is not { VelocityDetourAddress: > 0 }) return;
        _cheats.Mem.WriteMemory(_cheats.VelocityDetourAddress + 0x105, Convert.ToSingle(JumpSlider.Value * 3));
    }
}