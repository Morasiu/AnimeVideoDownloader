using System;
using OpenQA.Selenium;

namespace DownloaderLibrary.Providers {
	public class ProviderFactory {
		private readonly WebDriver _driver;

		public ProviderFactory(WebDriver driver) {
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