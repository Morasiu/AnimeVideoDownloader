using System.Reflection;
using AutoMapper;

namespace DownloaderLibrary.Services.Mapping {
	public static class Mapper {
		public static IMapper Instance;
		
		static Mapper() {
			var configurationProvider = new MapperConfiguration(a => 
				a.AddMaps(Assembly.GetAssembly(typeof(Mapper))));
			Instance = new AutoMapper.Mapper(configurationProvider);
		}
	}
}