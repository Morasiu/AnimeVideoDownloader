using AutoMapper;
using DownloaderLibrary.Episodes;

namespace DownloaderLibrary.Services.Mapping.Profiles {
	public class EpisodeMapProfile : Profile {
		public EpisodeMapProfile() {
			CreateMap<Episode, Episode>();
		}
	}
}