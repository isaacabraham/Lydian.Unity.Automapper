using Microsoft.Practices.Unity;

namespace Lydian.Unity.Automapper.Core.Handling
{
	internal class ConfigLifetimeManagerFactory : IConfigLifetimeManagerFactory
	{
		private readonly AutomapperConfig configurationDetails;

		public ConfigLifetimeManagerFactory(AutomapperConfig configurationDetails)
		{
			this.configurationDetails = configurationDetails;
		}

		public LifetimeManager CreateLifetimeManager(TypeMapping typeMapping)
		{
			return configurationDetails.IsSingleton(typeMapping.From) ? (LifetimeManager)new ContainerControlledLifetimeManager()
																	  : new TransientLifetimeManager();
		}
	}
}
