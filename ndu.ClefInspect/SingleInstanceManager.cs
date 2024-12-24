using System.IO;
using System.IO.Pipes;
using System.Windows;
using System.Windows.Threading;

namespace ndu.ClefInspect
{
    public class SingleInstanceManager : IDisposable
    {
        public interface IMainView
        {
            void Show();
            void OpenFiles(string[] files);
        }

        private readonly Guid pipename = new("1cd72b42-7bf4-4fca-9a1b-67d3bb849886");
        private readonly Mutex mutex = new(false, new Guid("c44c5066-fbaa-479c-b015-7a34429bee03").ToString());
        private bool mutexOwned = false;
        private bool disposedValue;
        private readonly DispatcherTimer dispatcherTimer = new();

        public SingleInstanceManager(Application app, StartupEventArgs e, Func<IMainView> mainViewFactoryMethod)
        {
            mutexOwned = mutex.WaitOne(0);
            if (mutexOwned || e.Args.Length == 0)
            {
                IMainView mainWindow = mainViewFactoryMethod();
                mainWindow.Show();
                // if not mutex owner, start a timer to recheck
                dispatcherTimer.Tick += (sender, e) =>
                {
                    if (!mutexOwned)
                    {
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                        mutexOwned = mutex.WaitOne(0);
                    }
                    if (mutexOwned)
                    {
                        dispatcherTimer.Stop();
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
                                    Application.Current?.Dispatcher?.Invoke(() =>
                                    {
                                        if (app.MainWindow.WindowState == WindowState.Minimized)
                                        {
                                            app.MainWindow.WindowState = WindowState.Normal;
                                        }
                                        app.MainWindow.Activate();
                                        mainWindow.OpenFiles(files);
                                    });
                                    pipeServer.Close();
                                }
                                catch (Exception)
                                {
                                }
                            }
                        });
                    }
                };
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0);
                dispatcherTimer.Start();
            }
            else
            {
                try
                {
                    using NamedPipeClientStream pipeClient = new(".", pipename.ToString(), PipeDirection.Out, PipeOptions.CurrentUserOnly, System.Security.Principal.TokenImpersonationLevel.None);
                    pipeClient.Connect(1000);
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
                    MessageBox.Show("Could not communicate with running clef to open file.", "Clef Inspect");
                }
                app.Shutdown();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                if (mutexOwned)
                {
                    mutex.ReleaseMutex();
                    mutexOwned = false;
                }
                disposedValue = true;
            }
        }

        ~SingleInstanceManager()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
