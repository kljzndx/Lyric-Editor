using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace LyricsEditor.Model
{
    public static class SystemInfo
    {
        public static ulong Version { get { return (Convert.ToUInt64(AnalyticsInfo.VersionInfo.DeviceFamilyVersion)) >> 16 & 0xFFFF; } }
        public static string DeviceType { get => AnalyticsInfo.VersionInfo.DeviceFamily; }
        public static string DeviceName
        {
            get
            {
                var deviceInfo = new EasClientDeviceInformation();
                return $"{deviceInfo.SystemManufacturer} {deviceInfo.SystemProductName}";
            }
        }

        public static string PrintInfo()
        {
            return $"设备类型： {DeviceType} ，设备名： {DeviceName} ，系统版本： {Version}";
        }
    }
}
