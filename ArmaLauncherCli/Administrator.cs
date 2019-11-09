using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace ArmaLauncherCli
{
    public class RunAsAdministrator
   {
       public bool IsRunAsAdministrator()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            var windowsPrincipal = new WindowsPrincipal(windowsIdentity);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
     
       public static void ValidateAdministratorModeAndRestart(bool displayMessage)
       {
    
           var runAsAdministrator = new RunAsAdministrator();
           if (runAsAdministrator.IsRunAsAdministrator()) return;
    
    
           if (displayMessage)
           {
               var fi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
               string productName = fi.FileDescription;
               MessageBox.Show(string.Format("{0} will launch itself with admin privileges", productName));
           }
    
           var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase)
                                 {
                                     UseShellExecute = true,
                                     Verb = "runas"
                                 };
           try
           {
               Process.Start(processInfo);
           }
           catch (Exception)
           {
               MessageBox.Show("Sorry, this application must be run as Administrator.");
           }
           Application.Current.Shutdown();
       }
   }

}
