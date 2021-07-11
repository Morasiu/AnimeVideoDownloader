using AutoMapper;
using DownloaderLibrary.Data.Episodes;

namespace DownloaderLibrary.Services.Mapping.Profiles {
	public class EpisodeMapProfile : Profile {
		public EpisodeMapProfile() {
			CreateMap<Episode, Episode>();
		}
	}
}