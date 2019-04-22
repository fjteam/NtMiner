﻿using System;
using System.Text.RegularExpressions;

namespace NTMiner.Core.Gpus.Impl {
    public class NVIDIAOverClock : IOverClock {
        public NVIDIAOverClock() {
        }

        public void SetCoreClock(int gpuIndex, int value) {
            value = 1000 * value;
            if (gpuIndex == NTMinerRoot.GpuAllId) {
                foreach (var gpu in NTMinerRoot.Current.GpuSet) {
                    if (gpu.Index == NTMinerRoot.GpuAllId) {
                        continue;
                    }
                    Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} gclk:{value}");
                }
            }
            else {
                Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpuIndex} gclk:{value}");
            }
        }

        public void SetMemoryClock(int gpuIndex, int value) {
            value = 1000 * value;
            if (gpuIndex == NTMinerRoot.GpuAllId) {
                foreach (var gpu in NTMinerRoot.Current.GpuSet) {
                    if (gpu.Index == NTMinerRoot.GpuAllId) {
                        continue;
                    }
                    Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} mclk:{value}");
                }
            }
            else {
                Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpuIndex} mclk:{value}");
            }
        }

        public void SetPowerCapacity(int gpuIndex, int value) {
            if (value == 0) {
                return;
            }
            if (gpuIndex == NTMinerRoot.GpuAllId) {
                foreach (var gpu in NTMinerRoot.Current.GpuSet) {
                    if (gpu.Index == NTMinerRoot.GpuAllId) {
                        continue;
                    }
                    Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} pcap:{value}");
                }
            }
            else {
                Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpuIndex} pcap:{value}");
            }
        }

        public void SetThermCapacity(int gpuIndex, int value) {
            if (value == 0) {
                return;
            }
            if (gpuIndex == NTMinerRoot.GpuAllId) {
                foreach (var gpu in NTMinerRoot.Current.GpuSet) {
                    if (gpu.Index == NTMinerRoot.GpuAllId) {
                        continue;
                    }
                    Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} tcap:{value}");
                }
            }
            else {
                Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpuIndex} tcap:{value}");
            }
        }

        public void SetCool(int gpuIndex, int value) {
            if (value == 0) {
                return;
            }
            if (gpuIndex == NTMinerRoot.GpuAllId) {
                foreach (var gpu in NTMinerRoot.Current.GpuSet) {
                    if (gpu.Index == NTMinerRoot.GpuAllId) {
                        continue;
                    }
                    Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} cool:{value}");
                }
            }
            else {
                Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpuIndex} cool:{value}");
            }
        }

        public void RefreshGpuState(int gpuIndex) {
            if (gpuIndex == NTMinerRoot.GpuAllId) {
                foreach (var gpu in NTMinerRoot.Current.GpuSet) {
                    if (gpu.Index == NTMinerRoot.GpuAllId) {
                        continue;
                    }
                    RefreshGpuState(gpu);
                }
            }
            else {
                if (NTMinerRoot.Current.GpuSet.TryGetGpu(gpuIndex, out IGpu gpu)) {
                    RefreshGpuState(gpu);
                }
            }
        }

        public static void RefreshGpuState(IGpu gpu) {
            const string coreClockDeltaMinMaxPattern = @"c\[0\]\.freqDelta     = (\d+) kHz \[(-?\d+) .. (\d+)\]";
            const string memoryClockDeltaMinMaxPattern = @"c\[1\]\.freqDelta     = (\d+) kHz \[(-?\d+) .. (\d+)\]";
            const string coolSpeedMinMaxPattern = @"cooler\[0\]\.speed   = (\d+) % \[(-?\d+) .. (\d+)\]";
            const string powerMinPattern = @"policy\[0\]\.pwrLimitMin     = (\d+\.?\d*) %";
            const string powerMaxPattern = @"policy\[0\]\.pwrLimitMax     = (\d+\.?\d*) %";
            const string powerLimitCurrentPattern = @"policy\[0\]\.pwrLimitCurrent = (\d+\.?\d*) %";
            const string tempLimitMinPattern = @"policy\[0\]\.tempLimitMin     = (\d+\.?\d*)C";
            const string tempLimitMaxPattern = @"policy\[0\]\.tempLimitMax     = (\d+\.?\d*)C";
            const string tempLimitDefaultPattern = @"policy\[0\]\.tempLimitDefault = (\d+\.?\d*)C";
            const string tempLimitPattern = @"policy\[0\]\.tempLimitCurrent = (\d+\.?\d*)C";
            if (gpu.Index == NTMinerRoot.GpuAllId) {
                return;
            }
            int exitCode = -1;
            Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} ps20e", ref exitCode, out string output);
            int coreClockDeltaMin = 0;
            int coreClockDeltaMax = 0;
            int memoryClockDeltaMin = 0;
            int memoryClockDeltaMax = 0;
            int cool = 0;
            int coolMin = 0;
            int coolMax = 0;
            double powerMin = 0;
            double powerMax = 0;
            double power = 0;
            double tempLimitMin = 0;
            double tempLimitMax = 0;
            double tempLimitDefault = 0;
            double tempLimit = 0;
            if (exitCode == 0) {
                Match match = Regex.Match(output, coreClockDeltaMinMaxPattern, RegexOptions.Compiled);
                if (match.Success) {
                    int.TryParse(match.Groups[1].Value, out int coreClockDelta);
                    gpu.CoreClockDelta = coreClockDelta;
                    int.TryParse(match.Groups[2].Value, out coreClockDeltaMin);
                    int.TryParse(match.Groups[3].Value, out coreClockDeltaMax);
                }
                match = Regex.Match(output, memoryClockDeltaMinMaxPattern, RegexOptions.Compiled);
                if (match.Success) {
                    int.TryParse(match.Groups[1].Value, out int memoryClockDelta);
                    gpu.MemoryClockDelta = memoryClockDelta;
                    int.TryParse(match.Groups[2].Value, out memoryClockDeltaMin);
                    int.TryParse(match.Groups[3].Value, out memoryClockDeltaMax);
                }
                gpu.CoreClockDeltaMin = coreClockDeltaMin;
                gpu.CoreClockDeltaMax = coreClockDeltaMax;
                gpu.MemoryClockDeltaMin = memoryClockDeltaMin;
                gpu.MemoryClockDeltaMax = memoryClockDeltaMax;
            }
            Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} coolers", ref exitCode, out output);
            if (exitCode == 0) {
                Match match = Regex.Match(output, coolSpeedMinMaxPattern, RegexOptions.Compiled);
                if (match.Success) {
                    int.TryParse(match.Groups[1].Value, out cool);
                    int.TryParse(match.Groups[2].Value, out coolMin);
                    int.TryParse(match.Groups[3].Value, out coolMax);
                }
                gpu.Cool = cool;
                gpu.CoolMin = coolMin;
                gpu.CoolMax = coolMax;
            }
            Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} pwrinfo", ref exitCode, out output);
            if (exitCode == 0) {
                Match match = Regex.Match(output, powerMinPattern, RegexOptions.Compiled);
                if (match.Success) {
                    double.TryParse(match.Groups[1].Value, out powerMin);
                }
                match = Regex.Match(output, powerMaxPattern, RegexOptions.Compiled);
                if (match.Success) {
                    double.TryParse(match.Groups[1].Value, out powerMax);
                }
                match = Regex.Match(output, powerLimitCurrentPattern, RegexOptions.Compiled);
                if (match.Success) {
                    double.TryParse(match.Groups[1].Value, out power);
                }
                gpu.PowerMin = powerMin;
                gpu.PowerMax = powerMax;
                gpu.PowerCapacity = (int)power;
            }
            Windows.Cmd.RunClose(SpecialPath.NTMinerOverClockFileFullName, $"gpu:{gpu.Index} therminfo", ref exitCode, out output);
            if (exitCode == 0) {
                Match match = Regex.Match(output, tempLimitMinPattern, RegexOptions.Compiled);
                if (match.Success) {
                    double.TryParse(match.Groups[1].Value, out tempLimitMin);
                }
                match = Regex.Match(output, tempLimitMaxPattern, RegexOptions.Compiled);
                if (match.Success) {
                    double.TryParse(match.Groups[1].Value, out tempLimitMax);
                }
                match = Regex.Match(output, tempLimitDefaultPattern, RegexOptions.Compiled);
                if (match.Success) {
                    double.TryParse(match.Groups[1].Value, out tempLimitDefault);
                }
                match = Regex.Match(output, tempLimitPattern, RegexOptions.Compiled);
                if (match.Success) {
                    double.TryParse(match.Groups[1].Value, out tempLimit);
                }
                gpu.TempLimitMin = (int)Math.Ceiling(tempLimitMin);
                gpu.TempLimitMax = (int)Math.Floor(tempLimitMax);
                gpu.TempLimitDefault = (int)tempLimitDefault;
                gpu.TempLimit = (int)tempLimit;
            }
            VirtualRoot.Happened(new GpuStateChangedEvent(gpu));
        }
    }
}
