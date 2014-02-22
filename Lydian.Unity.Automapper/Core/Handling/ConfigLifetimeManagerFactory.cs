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
            var customLifetimeManager = configurationDetails.IsMarkedWithCustomLifetimeManager(typeMapping.From);
            return customLifetimeManager.Item1 ? customLifetimeManager.Item2 : new TransientLifetimeManager();
        }
    }
}
