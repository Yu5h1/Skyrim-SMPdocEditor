using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SMPEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       public static string[] Args;
        protected override void OnStartup(StartupEventArgs e)
        {
            Args = e.Args;
            base.OnStartup(e);
        }
    }
}
