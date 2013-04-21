using Lydian.Unity.Automapper.Core;
using Lydian.Unity.Automapper.Core.Handling;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Lydian.Unity.Automapper.Test.Core.Handling
{
	[TestClass]
	public class ConfigLifetimeManagerFactoryTests
	{
		[TestMethod]
		public void CreateLifetimeManager_MappingIsNotASingleton_ReturnsTranscientLifetimeManager()
		{
			var factory = new ConfigLifetimeManagerFactory(AutomapperConfig.Create());

			// Act
			var lifetimeManager = factory.CreateLifetimeManager(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));

			// Assert
			Assert.IsInstanceOfType(lifetimeManager, typeof(TransientLifetimeManager));
		}

		[TestMethod]
		public void CreateLifetimeManager_InterfaceIsASingleton_ReturnsContainerControlledLifetimeManager()
		{
			var factory = new ConfigLifetimeManagerFactory(AutomapperConfig.Create().AndMapAsSingleton(typeof(IEnumerable)));

			// Act
			var lifetimeManager = factory.CreateLifetimeManager(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));

			// Assert
			Assert.IsInstanceOfType(lifetimeManager, typeof(ContainerControlledLifetimeManager));
		}

		[TestMethod]
		public void CreateLifetimeManager_ImplementationIsASingleton_ReturnsTransientLifetimeManager()
		{
			var factory = new ConfigLifetimeManagerFactory(AutomapperConfig.Create().AndMapAsSingleton(typeof(ArrayList)));

			// Act
			var lifetimeManager = factory.CreateLifetimeManager(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));

			// Assert
			Assert.IsInstanceOfType(lifetimeManager, typeof(TransientLifetimeManager));
		}

        [TestMethod]
        public void CreateLifetimeManager_InterfaceHasCustomLifetimeManager_ReturnsIt()
        {
            var factory = new ConfigLifetimeManagerFactory(AutomapperConfig.Create().AndMapWithLifetimeManager<HierarchicalLifetimeManager>(typeof(IEnumerable)));

            // Act
            var lifetimeManager = factory.CreateLifetimeManager(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));

            // Assert
            Assert.IsInstanceOfType(lifetimeManager, typeof(HierarchicalLifetimeManager));
        }
	}
}
