using System;
using OpenQA.Selenium.Remote;

namespace DownloaderLibrary.Providers {
	public class ProviderFactory {
		private readonly RemoteWebDriver _driver;

		public ProviderFactory(RemoteWebDriver driver) {
			_driver = driver;
		}

		public BaseProvider GetProvider(ProviderType type) {
			switch (type) {
				case ProviderType.Cda:
					return new CdaProvider(_driver);
				case ProviderType.GDrive:
					return new GDriveProvider(_driver);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}
}