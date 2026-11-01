using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace WinFormUI.Core
{
    public static class UserSecurityHelper
    {
        public static bool IsRunningAsAdministrator()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool RestartAsAdministrator()
        {
            if (IsRunningAsAdministrator())
            {
                return true;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = Application.ExecutablePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
                Application.Exit();
                return true;
            }
            catch (Win32Exception)
            {
                // User cancelled or denied UAC prompt
                return false;
            }
        }
    }
}
