using Serilog;

namespace DownloaderLibrary.Logging {
	public static class LoggerBuilder {
		static LoggerBuilder() {
			Log.Logger = new LoggerConfiguration()
			             .WriteTo.File("logs.txt", rollingInterval: RollingInterval.Infinite, shared: true)
			             .CreateLogger();
		}
	}
}