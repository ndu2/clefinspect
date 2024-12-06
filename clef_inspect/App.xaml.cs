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

        static Guid pipename = new Guid("1cd72b42-7bf4-4fca-9a1b-67d3bb849886");
        static Mutex mutex = new Mutex(true, new Guid("c44c5066-fbaa-479c-b015-7a34429bed02").ToString());
        static bool mutexOwned = false;


        private void Clef_Startup(object sender, StartupEventArgs e)
        {
            if (mutex.WaitOne(0))
            {
                mutexOwned = true;

                MainView mainWindow = new MainView();
                mainWindow.Show();

                Task.Run(() =>
                {
                    while (true)
                    {
                        using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipename.ToString(), PipeDirection.In))
                        {
                            try
                            {
                                // Wait for a client to connect
                                pipeServer.WaitForConnection();
                                StreamReader r = new StreamReader(pipeServer);
                                string? dat = r.ReadLine();
                                string[] files = dat?.Split('\u0002') ?? new string[0];
                                Application.Current?.Dispatcher?.Invoke(() => { mainWindow.OpenFiles(files); });
                                pipeServer.Close();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                });
            }
            else
            {
                try
                {
                    using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipename.ToString(), PipeDirection.Out, PipeOptions.CurrentUserOnly, System.Security.Principal.TokenImpersonationLevel.None))
                    {
                        pipeClient.Connect();
                        string filesSTX = string.Join('\u0002', e.Args);
                        StreamWriter sw = new StreamWriter(pipeClient);
                        sw.AutoFlush = true;
                        sw.WriteLine(filesSTX);
                        pipeClient.Close();
                    }
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
