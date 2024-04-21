using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Memory;

namespace F1_23_Trainer;

public class Cheats(MainWindow mainWindow)
{
    public readonly Mem Mem = new();
    private UIntPtr _fuelConsumptionAddress;
    public UIntPtr FuelConsumptionDetourAddress;
    private UIntPtr _gripRateAddress;
    public UIntPtr GripRateDetourAddress;
    private UIntPtr _engTempAddress;
    public UIntPtr EngTempDetourAddress;
    private UIntPtr _brakeTempAddress;
    public UIntPtr BrakeTempDetourAddress;
    private UIntPtr _lapTime1Address;
    public UIntPtr LapTime1DetourAddress;
    private UIntPtr _lapTime2Address;
    public UIntPtr LapTime2DetourAddress;
    private UIntPtr _lapTime3Address;
    public UIntPtr LapTime3DetourAddress;
    private UIntPtr _tireTemp1Address;
    public UIntPtr TireTemp1DetourAddress;
    private UIntPtr _tireTemp2Address;
    public UIntPtr TireTemp2DetourAddress;
    private UIntPtr _resourcePointsAddress;
    public UIntPtr ResourcePointsDetourAddress;
    private UIntPtr _enginePowerAddress;
    public UIntPtr EnginePowerDetourAddress;
    private UIntPtr _engineWearAddress;
    public UIntPtr EngineWearDetourAddress;
    private UIntPtr _tireWearAddress;
    public UIntPtr TireWearDetourAddress;
    private UIntPtr _ersDrainRateAddress;
    public UIntPtr ErsDrainRateDetourAddress;
    private UIntPtr _driverMoneyAddress;
    public UIntPtr DriverMoneyDetourAddress;
    private UIntPtr _instUpgradeAddress;
    public UIntPtr InstUpgradeDetourAddress;
    private UIntPtr _teamMoneyAddress;
    public UIntPtr TeamMoneyDetourAddress;
    private UIntPtr _teamAcclaimAddress;
    public UIntPtr TeamAcclaimDetourAddress;
    private UIntPtr _playerAcclaimAddress;
    public UIntPtr PlayerAcclaimDetourAddress;
    private UIntPtr _noCollisionDamageAddress;
    public UIntPtr NoCollisionDamageDetourAddress;
    private UIntPtr _velocityAddress;
    public UIntPtr VelocityDetourAddress;
    
    public void SetupAttach()
    {
        SetToggleSwitchState(false);
        
        Process.EnterDebugMode();
        if (Mem.OpenProcess(MainWindow.ProcessName) == Mem.OpenProcessResults.Success)
        {
            HandleOpenGame();
            SetupExit();
        }
        Process.LeaveDebugMode();

        var watcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
        watcher.EventArrived += (_, e) =>
        {
            if (mainWindow.ViewModel.Attached)
            {
                return;
            }

            var name = e.NewEvent.Properties["ProcessName"].Value.ToString()?.ToLower();
            if (name == null || !name.Equals(MainWindow.ProcessName, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            Process.EnterDebugMode();
            var result = Mem.OpenProcess(MainWindow.ProcessName);
            Process.LeaveDebugMode();
            if (result != Mem.OpenProcessResults.Success) return;
            HandleOpenGame();
            SetupExit();
        };

        watcher.Start();
    }

    private void SetupExit()
    {
        Mem.MProc.Process.EnableRaisingEvents = true;
        Mem.MProc.Process.Exited += (_, _) => HandleCloseGame();
    }

    private void HandleOpenGame()
    {
        if (mainWindow.ViewModel.Attached)
        {
            return;
        }

        mainWindow.Dispatcher.Invoke(() =>
        {
            mainWindow.GameStatusLabel.Foreground = Brushes.Green;
            mainWindow.GameStatusLabel.Text = "On";
            mainWindow.ProcessIdLabel.Text = Mem.MProc.ProcessId.ToString();
        });

        SetToggleSwitchState(true);
        mainWindow.ViewModel.Attached = true;
    }

    private void HandleCloseGame()
    {
        if (!mainWindow.ViewModel.Attached)
        {
            return;
        }

        mainWindow.Dispatcher.Invoke(() =>
        {
            mainWindow.GameStatusLabel.Foreground = Brushes.Red;
            mainWindow.GameStatusLabel.Text = "Off";
            mainWindow.ProcessIdLabel.Text = "0";
        });

        mainWindow.ViewModel.Attached = false;
        ResetTrainer();
    }

    private void ResetTrainer()
    {
        var fields = GetType().GetFields().Where(f => f.FieldType == typeof(UIntPtr));
        foreach (var field in fields)
        {
            field.SetValue(this, UIntPtr.Zero);
        }
        SetToggleSwitchState(false);
    }

    private void SetToggleSwitchState(bool enable)
    {
        mainWindow.Dispatcher.Invoke(() =>
        {
            ToggleSwitch[] toggleSwitches =
            [
                mainWindow.EngWearSwitch,
                mainWindow.TireWearSwitch,
                mainWindow.InstUpgSwitch,
                mainWindow.ErsSwitch,
                mainWindow.FuelSwitch,
                mainWindow.GripSwitch,
                mainWindow.EngPowerSwitch,
                mainWindow.EngTempSwitch,
                mainWindow.BrakeTempSwitch,
                mainWindow.TireTempSwitch,
                mainWindow.LapTimeSwitch,
                mainWindow.ResourcePointsSwitch,
                mainWindow.AccelSwitch,
                mainWindow.BrakeSwitch,
                mainWindow.JumpSwitch,
                mainWindow.DriverMoneySwitch,
                mainWindow.TeamMoneySwitch,
                mainWindow.NoDamageSwitch,
                mainWindow.PlayerAcclaimSwitch,
                mainWindow.TeamAcclaimSwitch,
                mainWindow.FreezeAiSwitch,
            ];

            foreach (var toggleSwitch in toggleSwitches)
            {
                if (!enable)
                {
                    toggleSwitch.IsOn = enable;
                }
                toggleSwitch.IsEnabled = enable;
            }
        });
    }

    public void TrainerClose()
    {
        if (!mainWindow.ViewModel.Attached)
        {
            return;
        }
        
        if (FuelConsumptionDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_fuelConsumptionAddress, new byte[] { 0xC5, 0xF2, 0x5C, 0xC0, 0xC5, 0xFA, 0x11, 0x86, 0x00, 0x01, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, FuelConsumptionDetourAddress, 0, Imps.MemRelease);
        }

        if (GripRateDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_gripRateAddress, new byte[] { 0xC5, 0xFA, 0x10, 0x8F, 0x90, 0x00, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, GripRateDetourAddress, 0, Imps.MemRelease);
        }

        if (EngTempDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_engTempAddress, new byte[] { 0xC5, 0xFA, 0x11, 0x86, 0x30, 0x25, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, EngTempDetourAddress, 0, Imps.MemRelease);
        }

        if (BrakeTempDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_brakeTempAddress, new byte[] { 0xC5, 0xFA, 0x11, 0x46, 0x08 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, BrakeTempDetourAddress, 0, Imps.MemRelease);
        }

        if (LapTime1DetourAddress > 0)
        {
            Mem.WriteArrayMemory(_lapTime1Address, new byte[] { 0x89, 0x59, 0x48, 0x4C, 0x8B, 0x77, 0x50 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, LapTime1DetourAddress, 0, Imps.MemRelease);
        }

        if (LapTime2DetourAddress > 0)
        {
            Mem.WriteArrayMemory(_lapTime2Address, new byte[] { 0xC5, 0xF2, 0x58, 0xC8, 0xC5, 0xF9, 0x7E, 0xCA });
            Imps.VirtualFreeEx(Mem.MProc.Handle, LapTime2DetourAddress, 0, Imps.MemRelease);
        }

        if (LapTime3DetourAddress > 0)
        {
            Mem.WriteArrayMemory(_lapTime3Address, new byte[] { 0x89, 0x8C, 0x06, 0xB0, 0x00, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, LapTime3DetourAddress, 0, Imps.MemRelease);
        }

        if (TireTemp1DetourAddress > 0)
        {
            Mem.WriteArrayMemory(_tireTemp1Address, new byte[] { 0xC5, 0xFA, 0x11, 0x66, 0x68 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, TireTemp1DetourAddress, 0, Imps.MemRelease);
        }

        if (TireTemp2DetourAddress > 0)
        {
            Mem.WriteArrayMemory(_tireTemp2Address, new byte[] { 0xC5, 0xFA, 0x11, 0x46, 0x64 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, TireTemp2DetourAddress, 0, Imps.MemRelease);
        }

        if (ResourcePointsDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_resourcePointsAddress, new byte[] { 0x48, 0x8B, 0x00, 0x8B, 0x68, 0x04 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, ResourcePointsDetourAddress, 0, Imps.MemRelease);
        }

        if (EnginePowerDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_enginePowerAddress, new byte[] { 0xC5, 0xFA, 0x59, 0x83, 0x7C, 0x25, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, EnginePowerDetourAddress, 0, Imps.MemRelease);
        }

        if (EngineWearDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_engineWearAddress, new byte[] { 0xC5, 0xFA, 0x58, 0x8E, 0x40, 0x25, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, EngineWearDetourAddress, 0, Imps.MemRelease);
        }

        if (TireWearDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_tireWearAddress, new byte[] { 0xC5, 0xFA, 0x58, 0x46, 0x78 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, TireWearDetourAddress, 0, Imps.MemRelease);
        }

        if (ErsDrainRateDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_ersDrainRateAddress, new byte[] { 0xC4, 0xC1, 0x4A, 0x5C, 0xC1 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, ErsDrainRateDetourAddress, 0, Imps.MemRelease);
        }

        if (DriverMoneyDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_driverMoneyAddress, new byte[] { 0x89, 0x86, 0x98, 0x04, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, DriverMoneyDetourAddress, 0, Imps.MemRelease);
        }

        if (InstUpgradeDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_instUpgradeAddress, new byte[] { 0x48, 0x8B, 0x83, 0x98, 0x00, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, InstUpgradeDetourAddress, 0, Imps.MemRelease);
        }

        if (TeamMoneyDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_teamMoneyAddress, new byte[] { 0x41, 0x3B, 0x85, 0x38, 0x01, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, TeamMoneyDetourAddress, 0, Imps.MemRelease);
        }

        if (TeamAcclaimDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_teamAcclaimAddress, new byte[] { 0xC4, 0xC1, 0x7A, 0x10, 0x47, 0x38 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, TeamAcclaimDetourAddress, 0, Imps.MemRelease);
        }

        if (PlayerAcclaimDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_playerAcclaimAddress, new byte[] { 0xC5, 0xFA, 0x10, 0x41, 0x30 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, PlayerAcclaimDetourAddress, 0, Imps.MemRelease);
        }

        if (NoCollisionDamageDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_noCollisionDamageAddress, new byte[] { 0xC5, 0xFA, 0x10, 0x42, 0x50 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, NoCollisionDamageDetourAddress, 0, Imps.MemRelease);
        }

        if (VelocityDetourAddress > 0)
        {
            Mem.WriteArrayMemory(_velocityAddress, new byte[] { 0xC5, 0xF8, 0x29, 0x80, 0x20, 0x02, 0x00, 0x00 });
            Imps.VirtualFreeEx(Mem.MProc.Handle, VelocityDetourAddress, 0, Imps.MemRelease);
        }
        
        Imports.CloseHandle(Mem.MProc.Handle);
    }

    private async Task<nuint> SmartAobScan(string search, UIntPtr? start = null, UIntPtr? end = null)
    {
        var handle = Mem.MProc.Handle;
        var minRange = (long)Mem.MProc.Process.MainModule!.BaseAddress;
        var maxRange = minRange + Mem.MProc.Process.MainModule!.ModuleMemorySize;

        if (start != null)
        {
            minRange = (long)start;
        }

        if (end != null)
        {
            maxRange = (long)end;
        }

        var scanStartAddr = minRange;
        var address = (UIntPtr)minRange;

        try
        {
            Imps.GetSystemInfo(out var info);
            while (address < (ulong)maxRange)
            {
                Imps.Native_VirtualQueryEx(handle, address, out Imps.MemoryBasicInformation64 memInfo, info.PageSize);
                if (address == memInfo.BaseAddress + memInfo.RegionSize)
                {
                    break;
                }

                var scanEndAddr = (long)memInfo.BaseAddress + (long)memInfo.RegionSize;

                nuint retAddress;
                if (scanEndAddr - scanStartAddr > 500000000)
                {
                    retAddress = await ScanRange(search, scanStartAddr, scanEndAddr);
                }
                else
                {
                    retAddress = (await Mem.AoBScan(scanStartAddr, scanEndAddr, search, false, false, true, false)).FirstOrDefault();
                }

                if (retAddress != 0)
                {
                    return retAddress;
                }

                scanStartAddr = scanEndAddr;
                address = memInfo.BaseAddress + checked((UIntPtr)memInfo.RegionSize);
            }
        }
        catch
        {
            // ignored
        }

        return 0;
    }

    private async Task<nuint> ScanRange(string search, long startAddr, long endAddr)
    {
        var end = startAddr + (endAddr - startAddr) / 2;
        var retAddress = (await Mem.AoBScan(startAddr, end, search, false, false, true, false)).FirstOrDefault();
        return retAddress;
    }

    public async Task CheatFuelConsumption()
    {
        _fuelConsumptionAddress = 0;
        FuelConsumptionDetourAddress = 0;

        const string sig = "C5 F2 ? C0 C5 ? 11 86 ? ? ? ? 0F B6 ? ? ? ? ? 8D 0C";
        _fuelConsumptionAddress = await SmartAobScan(sig);

        if (_fuelConsumptionAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x24, 0x00, 0x00, 0x00, 0x01, 0x75, 0x11, 0x83, 0xBB, 0xF0, 0x00, 0x00, 0x00, 0x01, 0x74,
                0x08, 0xC5, 0xFA, 0x59, 0x05, 0x12, 0x00, 0x00, 0x00, 0xC5, 0xF2, 0x5C, 0xC0, 0xC5, 0xFA, 0x11, 0x86,
                0x00, 0x01, 0x00, 0x00
            };

            FuelConsumptionDetourAddress = Mem.CreateDetour(_fuelConsumptionAddress, asm, 12);
            return;
        }
        
        ShowError("Fuel Consumption", sig);
    }
    
    public async Task CheatGripRate()
    {
        _gripRateAddress = 0;
        GripRateDetourAddress = 0;

        const string sig = "C5 FA ? 8F 90 ? ? ? ? 0D ? ? ? ? 70 ? F2 ? 8F C0 00 00 00 C5 FA 10 90 ? ? ? ? C5 FA ? 98 F4";
        _gripRateAddress = await SmartAobScan(sig);

        if (_gripRateAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x2A, 0x00, 0x00, 0x00, 0x01, 0x75, 0x1B, 0x83, 0xB8, 0xF0, 0x00, 0x00, 0x00, 0x01, 0x74,
                0x12, 0xC5, 0xFA, 0x59, 0x05, 0x18, 0x00, 0x00, 0x00, 0xC4, 0xA1, 0x7A, 0x11, 0x84, 0xB6, 0xD0, 0x05,
                0x00, 0x00, 0xC5, 0xFA, 0x10, 0x8F, 0x90, 0x00, 0x00, 0x00
            };

            GripRateDetourAddress = Mem.CreateDetour(_gripRateAddress, asm, 8);
            return;
        }
        
        ShowError("Grip Rate", sig);
    }
    
    public async Task CheatEngineTemp()
    {
        _engTempAddress = 0;
        EngTempDetourAddress = 0;

        const string sig = "C5 FA ? 86 30 25 ? ? ? ? 78";
        _engTempAddress = await SmartAobScan(sig);

        if (_engTempAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x25, 0x00, 0x00, 0x00, 0x01, 0x75, 0x16, 0xC5, 0xFA, 0x10, 0x05, 0x1C, 0x00, 0x00, 0x00,
                0x68, 0x33, 0x93, 0x88, 0x43, 0xC5, 0xFA, 0x58, 0x04, 0x24, 0x48, 0x83, 0xC4, 0x08, 0xC5, 0xFA, 0x11,
                0x86, 0x30, 0x25, 0x00, 0x00
            };

            EngTempDetourAddress = Mem.CreateDetour(_engTempAddress, asm, 8);
            return;
        }
        
        ShowError("Engine Temp", sig);
    }
    
    public async Task CheatBrakeTemp()
    {
        _brakeTempAddress = 0;
        BrakeTempDetourAddress = 0;

        const string sig = "C5 FA ? 46 08 ? F8 28 74";
        _brakeTempAddress = await SmartAobScan(sig);

        if (_brakeTempAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x22, 0x00, 0x00, 0x00, 0x01, 0x75, 0x16, 0xC5, 0xFA, 0x10, 0x05, 0x19, 0x00, 0x00, 0x00,
                0x68, 0x33, 0x93, 0x88, 0x43, 0xC5, 0xFA, 0x58, 0x04, 0x24, 0x48, 0x83, 0xC4, 0x08, 0xC5, 0xFA, 0x11,
                0x46, 0x08
            };

            BrakeTempDetourAddress = Mem.CreateDetour(_brakeTempAddress, asm, 5);
            return;
        }
        
        ShowError("Brake Temp", sig);
    }
    
    public async Task CheatLapTime()
    {
        _lapTime1Address = 0;
        LapTime1DetourAddress = 0;
        _lapTime2Address = 0;
        LapTime2DetourAddress = 0;
        _lapTime3Address = 0;
        LapTime3DetourAddress = 0;

        const string lapTime1Sig = "89 59 ? 4C 8B ? ? 4C 3B";
        _lapTime1Address = await SmartAobScan(lapTime1Sig);

        if (_lapTime1Address > 0)
        {
            var asm = new byte[]
            {
                0x4C, 0x8D, 0xB4, 0x10, 0xE8, 0x00, 0x00, 0x00, 0x4C, 0x89, 0x35, 0x0C, 0x00, 0x00, 0x00, 0x89, 0x59,
                0x48, 0x4C, 0x8B, 0x77, 0x50
            };

            LapTime1DetourAddress = Mem.CreateDetour(_lapTime1Address, asm, 7);
        }
        else
        {
            ShowError("Lap Time", lapTime1Sig);
            return;
        }        
        
        const string lapTime2Sig = "C5 F2 ? C8 ? ? ? CA ? ? 41 89";
        _lapTime2Address = await SmartAobScan(lapTime2Sig);

        var cmpPtrBytes = BitConverter.GetBytes(LapTime1DetourAddress + 0x1B);
        if (_lapTime2Address > 0)
        {
            var asm = new byte[]
            {
                0x68, 0x00, 0x00, 0x00, 0x3F, 0x80, 0x3D, 0x35, 0x00, 0x00, 0x00, 0x01, 0x75, 0x22, 0xC5, 0xF8, 0x2F,
                0x0C, 0x24, 0x76, 0x1B, 0x57, 0x4A, 0x8D, 0x94, 0x28, 0xE8, 0x00, 0x00, 0x00, 0x48, 0xBF,
                cmpPtrBytes[0], cmpPtrBytes[1], cmpPtrBytes[2], cmpPtrBytes[3], cmpPtrBytes[4], cmpPtrBytes[5],
                cmpPtrBytes[6], cmpPtrBytes[7], 0x48, 0x39, 0x17, 0x5F, 0x75, 0x02, 0xEB, 0x04, 0xC5, 0xF2, 0x58, 0xC8,
                0x48, 0x83, 0xC4, 0x08, 0xC5, 0xF9, 0x7E, 0xCA
            };

            LapTime2DetourAddress = Mem.CreateDetour(_lapTime2Address, asm, 8);
        }
        else
        {
            ShowError("Lap Time", lapTime2Sig);
            return;
        }        
        
        const string lapTime3Sig = "89 8C ? ? ? ? ? EB ? 8B 86";
        _lapTime3Address = await SmartAobScan(lapTime3Sig);

        if (_lapTime3Address > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x2B, 0x00, 0x00, 0x00, 0x01, 0x75, 0x1D, 0x50, 0x53, 0x48, 0x8D, 0x9C, 0x30, 0xB8, 0x00,
                0x00, 0x00, 0x48, 0xB8, cmpPtrBytes[0], cmpPtrBytes[1], cmpPtrBytes[2], cmpPtrBytes[3], cmpPtrBytes[4],
                cmpPtrBytes[5], cmpPtrBytes[6], cmpPtrBytes[7], 0x48, 0x39, 0x18, 0x5B, 0x58, 0x75, 0x02, 0xEB, 0x07,
                0x89, 0x8C, 0x30, 0xB0, 0x00, 0x00, 0x00
            };

            LapTime3DetourAddress = Mem.CreateDetour(_lapTime3Address, asm, 7);
            return;
        }
        
        ShowError("Lap Time", lapTime3Sig);
    }

    public async Task CheatTireTemp()
    {
        _tireTemp1Address = 0;
        TireTemp1DetourAddress = 0;
        _tireTemp2Address = 0;
        TireTemp2DetourAddress = 0;

        const string temp1Sig = "C5 FA ? 66 68 ? ? 10 25";
        _tireTemp1Address = await SmartAobScan(temp1Sig);

        if (_tireTemp1Address > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x22, 0x00, 0x00, 0x00, 0x01, 0x75, 0x16, 0xC5, 0xFA, 0x10, 0x25, 0x19, 0x00, 0x00, 0x00,
                0x68, 0x33, 0x93, 0x88, 0x43, 0xC5, 0xDA, 0x58, 0x24, 0x24, 0x48, 0x83, 0xC4, 0x08, 0xC5, 0xFA, 0x11,
                0x66, 0x68
            };

            TireTemp1DetourAddress = Mem.CreateDetour(_tireTemp1Address, asm, 5);
        }
        else
        {
            ShowError("Tire Temp", temp1Sig);
            return;
        }        
        
        const string temp2Sig = "C5 FA ? 46 64 C5 F8 ? 86 80";
        _tireTemp2Address = await SmartAobScan(temp2Sig);

        if (_tireTemp2Address > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x22, 0x00, 0x00, 0x00, 0x01, 0x75, 0x16, 0xC5, 0xFA, 0x10, 0x05, 0x19, 0x00, 0x00, 0x00,
                0x68, 0x33, 0x93, 0x88, 0x43, 0xC5, 0xFA, 0x58, 0x04, 0x24, 0x48, 0x83, 0xC4, 0x08, 0xC5, 0xFA, 0x11,
                0x46, 0x64
            };

            TireTemp2DetourAddress = Mem.CreateDetour(_tireTemp2Address, asm, 5);
            return;
        }
        
        ShowError("Tire Time", temp2Sig);
    }
    
    public async Task CheatResourcePoints()
    {
        _resourcePointsAddress = 0;
        ResourcePointsDetourAddress = 0;

        const string sig = "48 8B ? 8B 68 ? 89 2E";
        _resourcePointsAddress = await SmartAobScan(sig);

        if (_resourcePointsAddress > 0)
        {
            var asm = new byte[]
            {
                0x48, 0x8B, 0x00, 0x80, 0x3D, 0x13, 0x00, 0x00, 0x00, 0x01, 0x75, 0x09, 0x8B, 0x2D, 0x0C, 0x00, 0x00,
                0x00, 0x89, 0x68, 0x04, 0x8B, 0x68, 0x04
            };

            ResourcePointsDetourAddress = Mem.CreateDetour(_resourcePointsAddress, asm, 6);
            return;
        }
        
        ShowError("Resource Points", sig);
    }
    
    public async Task CheatEnginePower()
    {
        _enginePowerAddress = 0;
        EnginePowerDetourAddress = 0;

        const string sig = "C5 FA ? 83 7C 25 00";
        _enginePowerAddress = await SmartAobScan(sig);

        if (_enginePowerAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x24, 0x00, 0x00, 0x00, 0x01, 0x75, 0x15, 0x48, 0x8B, 0x4B, 0x08, 0x83, 0xB9, 0xF0, 0x00,
                0x00, 0x00, 0x01, 0x74, 0x08, 0xC5, 0xFA, 0x59, 0x05, 0x0E, 0x00, 0x00, 0x00, 0xC5, 0xFA, 0x59, 0x83,
                0x7C, 0x25, 0x00, 0x00
            };

            EnginePowerDetourAddress = Mem.CreateDetour(_enginePowerAddress, asm, 8);
            return;
        }
        
        ShowError("Engine Power", sig);
    }
    
    public async Task CheatEngineWear()
    {
        _engineWearAddress = 0;
        EngineWearDetourAddress = 0;

        const string sig = "C5 FA ? 8E 40 ? 00 00 C5 F8 ? C0 C5";
        _engineWearAddress = await SmartAobScan(sig);

        if (_engineWearAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x5F, 0x00, 0x00, 0x00, 0x01, 0x75, 0x50, 0xC7, 0x86, 0x2C, 0x07, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xC7, 0x86, 0x40, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x86, 0x44, 0x25, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x86, 0x48, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x86,
                0x4C, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x86, 0x50, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0xC7, 0x86, 0x54, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x86, 0xC4, 0x9B, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0xC5, 0xFA, 0x58, 0x8E, 0x40, 0x25, 0x00, 0x00
            };

            EngineWearDetourAddress = Mem.CreateDetour(_engineWearAddress, asm, 8);
            return;
        }
        
        ShowError("Engine Wear", sig);
    }

    public async Task CheatTireWear()
    {
        _tireWearAddress = 0;
        TireWearDetourAddress = 0;

        const string sig = "C5 FA ? 46 78 ? FA 5F";
        _tireWearAddress = await SmartAobScan(sig);

        if (_tireWearAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x20, 0x00, 0x00, 0x00, 0x01, 0x75, 0x14, 0x48, 0x8B, 0x46, 0x08, 0x83, 0xB8, 0xF0, 0x00,
                0x00, 0x00, 0x01, 0x74, 0x07, 0xC7, 0x46, 0x78, 0x00, 0x00, 0x00, 0x00, 0xC5, 0xFA, 0x58, 0x46, 0x78
            };

            TireWearDetourAddress = Mem.CreateDetour(_tireWearAddress, asm, 5);
            return;
        }
        
        ShowError("Tire Wear", sig);
    }
    
    public async Task CheatErsDrainRate()
    {
        _ersDrainRateAddress = 0;
        ErsDrainRateDetourAddress = 0;

        const string sig = "C4 C1 ? ? C1 C5 ? 58 C0 C5 ? 58 86 BC";
        _ersDrainRateAddress = await SmartAobScan(sig);

        if (_ersDrainRateAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x1D, 0x00, 0x00, 0x00, 0x01, 0x75, 0x11, 0x83, 0xB8, 0xF0, 0x00, 0x00, 0x00, 0x01, 0x74,
                0x08, 0xC5, 0x32, 0x59, 0x0D, 0x0B, 0x00, 0x00, 0x00, 0xC4, 0xC1, 0x4A, 0x5C, 0xC1
            };

            ErsDrainRateDetourAddress = Mem.CreateDetour(_ersDrainRateAddress, asm, 5);
            return;
        }
        
        ShowError("ERS Drain Rate", sig);
    }
    
    public async Task CheatDriverMoney()
    {
        _driverMoneyAddress = 0;
        DriverMoneyDetourAddress = 0;

        const string sig = "89 86 ? ? ? ? 48 89 ? E8 ? ? ? ? 49 8B";
        _driverMoneyAddress = await SmartAobScan(sig);

        if (_driverMoneyAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x19, 0x00, 0x00, 0x00, 0x01, 0x75, 0x0C, 0x8B, 0x05, 0x12, 0x00, 0x00, 0x00, 0x48, 0x8D,
                0x49, 0xB8, 0x89, 0x01, 0x89, 0x86, 0x98, 0x04, 0x00, 0x00
            };

            DriverMoneyDetourAddress = Mem.CreateDetour(_driverMoneyAddress, asm, 6);
            return;
        }
        
        ShowError("Driver Money", sig);
    }
    
    public async Task CheatInstantUpgrade()
    {
        _instUpgradeAddress = 0;
        InstUpgradeDetourAddress = 0;

        const string sig = "48 8B ? ? ? ? ? 80 78 F8 ? 75 ? 48 89";
        _instUpgradeAddress = await SmartAobScan(sig);

        if (_instUpgradeAddress > 0)
        {
            var asm = new byte[]
            {
                0x48, 0x8B, 0x83, 0x98, 0x00, 0x00, 0x00, 0x80, 0x3D, 0x0B, 0x00, 0x00, 0x00, 0x01, 0x75, 0x04, 0xC6,
                0x40, 0xF8, 0x03
            };

            InstUpgradeDetourAddress = Mem.CreateDetour(_instUpgradeAddress, asm, 7);
            return;
        }
        
        ShowError("Instant Upgrade", sig);
    }
    
    public async Task CheatTeamMoney()
    {
        _teamMoneyAddress = 0;
        TeamMoneyDetourAddress = 0;

        const string sig = "41 3B ? ? ? ? ? 0F 9D ? ? ? ? ? ? 4D 8B";
        _teamMoneyAddress = await SmartAobScan(sig);

        if (_teamMoneyAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x1B, 0x00, 0x00, 0x00, 0x01, 0x75, 0x0D, 0x8B, 0x05, 0x14, 0x00, 0x00, 0x00, 0x4C, 0x8D,
                0x71, 0xB8, 0x41, 0x89, 0x06, 0x41, 0x3B, 0x85, 0x38, 0x01, 0x00, 0x00
            };

            TeamMoneyDetourAddress = Mem.CreateDetour(_teamMoneyAddress, asm, 7);
            return;
        }
        
        ShowError("Team Money", sig);
    }
    
    public async Task CheatTeamAcclaim()
    {
        _teamAcclaimAddress = 0;
        TeamAcclaimDetourAddress = 0;

        const string sig = "C4 C1 ? ? 47 38 ? C1 7A 5F ? 08 C4 C1 7A 5D ? 0C ? 89 C1";
        _teamAcclaimAddress = await SmartAobScan(sig);

        if (_teamAcclaimAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x23, 0x00, 0x00, 0x00, 0x01, 0x75, 0x0E, 0xC5, 0xFA, 0x10, 0x05, 0x1A, 0x00, 0x00, 0x00,
                0xC4, 0xC1, 0x7A, 0x11, 0x47, 0x38, 0xC4, 0xC1, 0x7A, 0x10, 0x47, 0x38, 0xC5, 0xFA, 0x11, 0x05, 0x0A,
                0x00, 0x00, 0x00
            };

            TeamAcclaimDetourAddress = Mem.CreateDetour(_teamAcclaimAddress, asm, 6);
            return;
        }
        
        ShowError("Team Acclaim", sig);
    }
    
    public async Task CheatPlayerAcclaim()
    {
        _playerAcclaimAddress = 0;
        PlayerAcclaimDetourAddress = 0;

        const string sig = "C5 FA ? 41 30 ? FA 5F 41 08 ? FA 5D 71";
        _playerAcclaimAddress = await SmartAobScan(sig);

        if (_playerAcclaimAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x21, 0x00, 0x00, 0x00, 0x01, 0x75, 0x0D, 0xC5, 0xFA, 0x10, 0x05, 0x18, 0x00, 0x00, 0x00,
                0xC5, 0xFA, 0x11, 0x41, 0x30, 0xC5, 0xFA, 0x10, 0x41, 0x30, 0xC5, 0xFA, 0x11, 0x05, 0x0A, 0x00, 0x00,
                0x00
            };

            PlayerAcclaimDetourAddress = Mem.CreateDetour(_playerAcclaimAddress, asm, 5);
            return;
        }
        
        ShowError("Player Acclaim", sig);
    }
    
    public async Task CheatNoCollisionDamage()
    {
        _noCollisionDamageAddress = 0;
        NoCollisionDamageDetourAddress = 0;

        const string sig = "C5 FA ? 42 ? C5 FA ? 0D";
        _noCollisionDamageAddress = await SmartAobScan(sig);

        if (_noCollisionDamageAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0x1C, 0x00, 0x00, 0x00, 0x01, 0x75, 0x10, 0x83, 0xB9, 0xF0, 0x00, 0x00, 0x00, 0x01, 0x74,
                0x07, 0xC7, 0x42, 0x50, 0x00, 0x00, 0x00, 0x00, 0xC5, 0xFA, 0x10, 0x42, 0x50
            };

            NoCollisionDamageDetourAddress = Mem.CreateDetour(_noCollisionDamageAddress, asm, 5);
            return;
        }
        
        ShowError("Player Acclaim", sig);
    }
    
    public async Task CheatVelocity()
    {
        _velocityAddress = 0;
        VelocityDetourAddress = 0;

        const string sig = "C5 F8 ? 80 20 ? 00 00 48 8B ? ? 48 8B ? ? ? ? ? 48 8B ? ? 45 31 ? 48 83 BA C0 01 00 00";
        _velocityAddress = await SmartAobScan(sig);

        if (_velocityAddress > 0)
        {
            var asm = new byte[]
            {
                0x80, 0x3D, 0xFD, 0x00, 0x00, 0x00, 0x01, 0x75, 0x29, 0x41, 0x83, 0xBF, 0xF0, 0x00, 0x00, 0x00, 0x01,
                0x74, 0x1F, 0xC5, 0xFA, 0x10, 0x88, 0x34, 0x02, 0x00, 0x00, 0xC5, 0xF2, 0x58, 0x0D, 0xE2, 0x00, 0x00,
                0x00, 0xC5, 0xFA, 0x11, 0x88, 0x34, 0x02, 0x00, 0x00, 0xC6, 0x05, 0xD2, 0x00, 0x00, 0x00, 0x00, 0x80,
                0x3D, 0xD0, 0x00, 0x00, 0x00, 0x01, 0x75, 0x41, 0x41, 0x83, 0xBF, 0xF0, 0x00, 0x00, 0x00, 0x01, 0x74,
                0x37, 0xC5, 0xFA, 0x10, 0x88, 0x30, 0x02, 0x00, 0x00, 0xC5, 0xF2, 0x59, 0x0D, 0xB5, 0x00, 0x00, 0x00,
                0xC5, 0xFA, 0x11, 0x88, 0x30, 0x02, 0x00, 0x00, 0xC5, 0xFA, 0x10, 0x88, 0x38, 0x02, 0x00, 0x00, 0xC5,
                0xF2, 0x59, 0x0D, 0x9D, 0x00, 0x00, 0x00, 0xC5, 0xFA, 0x11, 0x88, 0x38, 0x02, 0x00, 0x00, 0xC6, 0x05,
                0x8D, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3D, 0x8B, 0x00, 0x00, 0x00, 0x01, 0x75, 0x41, 0x41, 0x83, 0xBF,
                0xF0, 0x00, 0x00, 0x00, 0x01, 0x74, 0x37, 0xC5, 0xFA, 0x10, 0x88, 0x30, 0x02, 0x00, 0x00, 0xC5, 0xF2,
                0x59, 0x0D, 0x70, 0x00, 0x00, 0x00, 0xC5, 0xFA, 0x11, 0x88, 0x30, 0x02, 0x00, 0x00, 0xC5, 0xFA, 0x10,
                0x88, 0x38, 0x02, 0x00, 0x00, 0xC5, 0xF2, 0x59, 0x0D, 0x58, 0x00, 0x00, 0x00, 0xC5, 0xFA, 0x11, 0x88,
                0x38, 0x02, 0x00, 0x00, 0xC6, 0x05, 0x48, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3D, 0x46, 0x00, 0x00, 0x00,
                0x01, 0x75, 0x28, 0x41, 0x83, 0xBF, 0xF0, 0x00, 0x00, 0x00, 0x01, 0x75, 0x1E, 0xC7, 0x80, 0x30, 0x02,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x80, 0x34, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7,
                0x80, 0x38, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC5, 0xF8, 0x29, 0x80, 0x20, 0x02, 0x00, 0x00
            };

            VelocityDetourAddress = Mem.CreateDetour(_velocityAddress, asm, 8);
            return;
        }
        
        ShowError("Velocity", sig);
    }

    private static void ShowError(string feature, string sig)
    {
        MessageBox.Show(
            $"Address for this feature wasn't found!\nPlease try to activate the cheat again or try to restart the game and the trainer.\n\nIf this error still occur, please (Press Ctrl+C) to copy, and make an issue on the github repository.\n\nFeature: {feature}\nSignature: {sig}\n\nTrainer Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error
        );
    }
}