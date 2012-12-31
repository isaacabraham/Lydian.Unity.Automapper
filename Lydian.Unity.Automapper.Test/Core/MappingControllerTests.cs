using Lydian.Unity.Automapper.Core;
using Lydian.Unity.Automapper.Core.Handling;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lydian.Unity.Automapper.Test.Core
{
	[TestClass]
	public class MappingControllerTests
	{
		private Mock<ITypeMappingHandler> mappingHandler;
		private Mock<IUnityContainer> target;
		private Mock<IUnityContainer> internalContainer;
		private Mock<ITypeMappingFactory> mappingFactory;
		private MappingController controller;
		
		[TestInitialize]
		public void Setup()
		{
			target = new Mock<IUnityContainer>();
			mappingFactory = new Mock<ITypeMappingFactory>();
			internalContainer = new Mock<IUnityContainer>();
			mappingHandler = new Mock<ITypeMappingHandler>();

			internalContainer.Setup(c => c.Resolve(typeof(ITypeMappingHandler), null, It.IsAny<ResolverOverride[]>())).Returns(mappingHandler.Object);
			controller = new MappingController(target.Object, mappingFactory.Object, internalContainer.Object);
			TestableUnityConfigProvider.Reset();
		}

		[TestMethod]
		public void RegisterTypes_NoExplicitConfiguration_CallsMappingFactory()
		{
			var types = new Type[0];

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(types, MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac != null)));
		}

		[TestMethod]
		public void RegisterTypes_OneConfigurationFound_ConfigurationMergedIntoOutput()
		{
			TestableUnityConfigProvider.AddSingletons(typeof(String));
			var types = new [] { typeof (TestableUnityConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(types, MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac.IsSingleton(typeof(String)))));
		}

		[TestMethod]
		public void RegisterTypes_ManyConfigurationsFound_ConfigurationsMergedIntoOutput()
		{
			TestableUnityConfigProvider.AddSingletons(typeof(String));
			var types = new[] { typeof(TestableUnityConfigProvider), typeof(SecondaryConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(types, MappingBehaviors.None, It.Is<AutomapperConfig>(ac => (!ac.IsMappable(typeof(Int32)) && ac.IsSingleton(typeof(String))))));
		}

		[TestMethod]
		public void RegisterTypes_SuppliedTypesHasAttribute_MergedIntoOutput()
		{
			var types = new[] { typeof(ISampleMapping), typeof(SecondaryConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(types, MappingBehaviors.None, It.Is<AutomapperConfig>(ac => (!ac.IsMappable(typeof(Int32)) && ac.IsSingleton(typeof(ISampleMapping))))));
		}

		[TestMethod]
		public void RegisterTypes_GotMappings_ResolvesMappingHandler()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None);

			// Assert
			internalContainer.Verify(i => i.Resolve(typeof(ITypeMappingHandler), null, It.Is<ResolverOverride[]>(r => r[0] is DependencyOverride<AutomapperConfig>
																												   && r[1] is DependencyOverride<IEnumerable<TypeMapping>>
																												   && r[2] is DependencyOverride<MappingBehaviors>
																												   && r[3] is DependencyOverride<IUnityContainer>)));
		}

		[TestMethod]
		public void RegisterTypes_ResolvedHandler_PerformsRegistrations()
		{
			var mappings = new TypeMapping[0];
			mappingFactory.Setup(mf => mf.CreateMappings(It.IsAny<IEnumerable<Type>>(), It.IsAny<MappingBehaviors>(), It.IsAny<AutomapperConfig>()))
						  .Returns(mappings);

			// Act
			controller.RegisterTypes(MappingBehaviors.None);
		
			// Assert
			mappingHandler.Verify(mh => mh.PerformRegistrations(target.Object, mappings));
		}

		[TestMethod]
		public void RegisterTypes_PerformedRegistrations_ReturnsResults()
		{
			var registrations = new ContainerRegistration[0];
			mappingHandler.Setup(mh => mh.PerformRegistrations(It.IsAny<IUnityContainer>(), It.IsAny<IEnumerable<TypeMapping>>()))
						  .Returns(registrations);

			// Act
			var result = controller.RegisterTypes(MappingBehaviors.None);
			
			// Assert
			Assert.AreSame(registrations, result);
		}

		[Singleton]
		public interface ISampleMapping { }

		public class SecondaryConfigProvider : IAutomapperConfigProvider
		{
			public AutomapperConfig CreateConfiguration()
			{
				return AutomapperConfig.Create().AndDoNotMapFor(typeof(Int32));
			}
		}
	}
}
