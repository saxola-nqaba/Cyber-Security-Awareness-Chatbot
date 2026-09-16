// WPF Application class and startup model:
//   Microsoft Docs — Application Management Overview:
//   https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/application-management-overview


using System.Windows;

namespace st10057997_PROG_POE_GUI
{
    
    public partial class App : Application
    {
        // No additional startup logic required.
        // WPF reads StartupUri="MainWindow.xaml" from App.xaml and handles launching automatically.
    }
}