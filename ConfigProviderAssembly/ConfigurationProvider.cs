using Lydian.Unity.Automapper;

namespace ConfigProviderAssembly
{
	public class ConfigurationProvider : IAutomapperConfigProvider
    {
		public AutomapperConfig CreateConfiguration()
		{
			return AutomapperConfig.Create()
									.AndDoNotMapFor(typeof(InterfaceToIgnore))
									.AndMapAsSingleton(typeof(SingletonInterface))
									.AndUseMultimappingFor(typeof(MultimappingInterface))
									.AndUseNamedMappingFor(typeof(NamedType), "Bananas");
		}
	}

	public interface InterfaceToIgnore { }
	public class TypeForIgnoredInterface : InterfaceToIgnore { }

	public interface SingletonInterface { }
	public class TypeForSingletonInterface : SingletonInterface { }

	public interface MultimappingInterface { }
	public class TypeWithImplicitMultimapping : MultimappingInterface { }
	public class NamedType : MultimappingInterface { }
}
