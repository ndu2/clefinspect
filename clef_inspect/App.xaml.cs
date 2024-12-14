using clef_inspect.View;
using System.Windows;

namespace clef_inspect
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SingleInstanceManager? _instanceManager;
        private void Clef_Startup(object sender, StartupEventArgs e)
        {
            _instanceManager = new SingleInstanceManager(this, e, () => { return new MainView(); });
        }

        private void Clef_Exit(object sender, ExitEventArgs e)
        {
            _instanceManager?.Dispose();
            _instanceManager = null;
        }
    }
}
