using DownloaderLibrary.Providers;
using OpenQA.Selenium;

namespace DownloaderLibrary.Data.EpisodeSources {
	public class EpisodeSource {
		public ProviderType ProviderType { get; set; }
		public string SourceUrl { get; set; }
		public Quality Quality { get; set; }
		public Language Language { get; set; }
		public IWebElement Button { get; set; }
	}
}