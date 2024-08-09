using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WeatherAPI.Tests.Unit
{
    public static class SqlServerInstance
    {
        public static string GetName()
        {
            var sqlServerName = Environment.MachineName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                {
                    var instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                    if (instanceKey != null)
                    {
                        var valueNames = instanceKey.GetValueNames();

                        if (valueNames.Contains("MSSQL2019"))
                        {
                            sqlServerName = $"{Environment.MachineName}\\MSSQL2019";
                        }
                        else if (valueNames.Contains("MSSQLSERVER01"))
                        {
                            sqlServerName = $"{Environment.MachineName}\\MSSQLSERVER01";
                        }
                        else if (valueNames.Contains("MSSQL2017"))
                        {
                            sqlServerName = $"{Environment.MachineName}\\MSSQL2017";
                        }
                    }
                }
            }
            else
            {
                //TODO: Read from configuration
                sqlServerName = "localhost";
            }

            return sqlServerName;
        }
    }
}
