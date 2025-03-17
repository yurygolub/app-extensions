using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AppExtensionsLib;

public static class UnhandledExceptionLogger
{
    public static void SetupExceptionLogging(ILogger logger = null)
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
                logger?.LogError(ex, $"Exception in {nameof(LogUnhandledException)}", null);
            }
            finally
            {
                logger?.LogError(exception, "Unhandled exception in {name} v{version}. Source: {source}", name, version, source);
            }
        }
    }
}
