using System;
using System.ComponentModel;

namespace DownloaderLibrary.Data.EpisodeSources {
	public enum Quality {
		Unknown = 0,
		/// <summary>
		/// 240p
		/// </summary>
		[Description("240p")]
		P240, 
		/// <summary>
		/// 480P
		/// </summary>
		[Description("360p")]
		P360, 
		/// <summary>
		/// 480P
		/// </summary>
		[Description("480p")]
		P480, 
		/// <summary>
		/// 720P
		/// </summary>
		[Description("720p")]
		P720, 
		/// <summary>
		/// 1080P
		/// </summary>
		[Description("1080p")]
		P1080, 
		/// <summary>
		/// 4K
		/// </summary>
		[Description("4k")]
		P4K
	}

	public static class QualityParser {
		public static Quality FromString(string s) {
			var formatted = s.ToLower();
			if (formatted.Contains("240")) return Quality.P240;
			if (formatted.Contains("360")) return Quality.P360;
			if (formatted.Contains("480")) return Quality.P480;
			if (formatted.Contains("720")) return Quality.P720;
			if (formatted.Contains("1080")) return Quality.P1080;
			if (formatted.Contains("4k")) return Quality.P4K;
			throw new ArgumentOutOfRangeException(nameof(s));
		}
	}
}