using clef_inspect.View;
using System.IO;
using System.IO.Pipes;
using System.Windows;

namespace clef_inspect
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        static readonly Guid pipename = new("1cd72b42-7bf4-4fca-9a1b-67d3bb849886");
        static readonly Mutex mutex = new(true, new Guid("c44c5066-fbaa-479c-b015-7a34429bed02").ToString());
        static bool mutexOwned = false;


        private void Clef_Startup(object sender, StartupEventArgs e)
        {
            if (mutex.WaitOne(0))
            {
                mutexOwned = true;

                MainView mainWindow = new();
                mainWindow.Show();

                Task.Run(() =>
                {
                    while (true)
                    {
                        using NamedPipeServerStream pipeServer = new(pipename.ToString(), PipeDirection.In);
                        try
                        {
                            // Wait for a client to connect
                            pipeServer.WaitForConnection();
                            StreamReader r = new(pipeServer);
                            string? dat = r.ReadLine();
                            string[] files = dat?.Split('\u0002') ?? Array.Empty<string>();
                            Application.Current?.Dispatcher?.Invoke(() => { mainWindow.OpenFiles(files); });
                            pipeServer.Close();
                        }
                        catch (Exception)
                        {
                        }
                    }
                });
            }
            else
            {
                try
                {
                    using NamedPipeClientStream pipeClient = new(".", pipename.ToString(), PipeDirection.Out, PipeOptions.CurrentUserOnly, System.Security.Principal.TokenImpersonationLevel.None);
                    pipeClient.Connect();
                    string filesSTX = string.Join('\u0002', e.Args);
                    StreamWriter sw = new(pipeClient)
                    {
                        AutoFlush = true
                    };
                    sw.WriteLine(filesSTX);
                    pipeClient.Close();
                }
                catch (Exception)
                {
                }
                this.Shutdown();
            }
        }

        private void Clef_Exit(object sender, ExitEventArgs e)
        {
            if (mutexOwned)
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
