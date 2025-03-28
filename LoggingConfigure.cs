using Serilog;

internal static class LoggingConfigure {
    public static void ConfigureLogging() {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            // .WriteTo.File("logs/log.zaza",
            //     rollingInterval: RollingInterval.Day,
            //     rollOnFileSizeLimit: true)
            .CreateLogger();
    }
}

