using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AppExtensionsLib;

public static class UnhandledExceptionLogger
{
    public static void SetupExceptionLogging(Action<Exception, string, object[]> errorHandler = null)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            LogUnhandledException(e.ExceptionObject as Exception, nameof(AppDomain.CurrentDomain.UnhandledException));
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            LogUnhandledException(e.Exception, nameof(TaskScheduler.UnobservedTaskException));
            e.SetObserved();
        };

        void LogUnhandledException(Exception exception, string source)
        {
            object name = null, version = null;
            try
            {
                AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
                name = assemblyName.Name;
                version = assemblyName.Version;
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(ex, $"Exception in {nameof(LogUnhandledException)}", null);
            }
            finally
            {
                errorHandler?.Invoke(
                    exception,
                    "Unhandled exception in {name} v{version}. Source: {source}",
                    new object[] { name, version, source });
            }
        }
    }
}
