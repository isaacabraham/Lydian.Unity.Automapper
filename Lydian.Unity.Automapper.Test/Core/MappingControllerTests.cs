using Lydian.Unity.Automapper.Core;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

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
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac != null), types));
		}

		#region Provider-based tests
		[TestMethod]
		public void RegisterTypes_SingletonConfiguration_MergedIntoOutput()
		{
			TestableUnityConfigProvider.AddSingletons(typeof(String));
			var types = new[] { typeof(TestableUnityConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac.IsMarkedWithCustomLifetimeManager(typeof(String)).Item1), types));
		}

		[TestMethod]
		public void RegisterTypes_MultimapConfiguration_MergedIntoOutput()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(String));
			var types = new[] { typeof(TestableUnityConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac.IsMultimap(typeof(String))), types));
		}

		[TestMethod]
		public void RegisterTypes_NamedMappingConfiguration_MergedIntoOutput()
		{
			TestableUnityConfigProvider.AddNamedMapping(typeof(String), "TEST");
			var types = new[] { typeof(TestableUnityConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac.IsNamedMapping(typeof(String))), types));
		}

		[TestMethod]
		public void RegisterTypes_DoNotMapConfiguration_MergedIntoOutput()
		{
			TestableUnityConfigProvider.AddDoNotMaps(typeof(String));
			var types = new[] { typeof(TestableUnityConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => !ac.IsMappable(typeof(String))), types));
		}

		[TestMethod]
		public void RegisterTypes_PolicyInjectionConfiguration_MergedIntoOutput()
		{
			TestableUnityConfigProvider.AddPolicyInjection(typeof(String));
			var types = new[] { typeof(TestableUnityConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac.IsMarkedForPolicyInjection(typeof(String))), types));
		}
		#endregion

		#region Attribute-derived tests
		[TestMethod]
		public void RegisterTypes_SuppliedTypesHasSingletonAttribute_MergedIntoOutput()
		{
			var types = new[] { typeof(ISingleton), typeof(SecondaryConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => (!ac.IsMappable(typeof(Int32)) && ac.IsMarkedWithCustomLifetimeManager(typeof(ISingleton)).Item1)), types));
		}

		[TestMethod]
		public void RegisterTypes_SuppliedTypesHasDoNotMapAttribute_MergedIntoOutput()
		{
			var types = new[] { typeof(IDoNotMap) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => !ac.IsMappable(typeof(IDoNotMap))), types));
		}

		[TestMethod]
		public void RegisterTypes_SuppliedTypesHasMultimapAttribute_MergedIntoOutput()
		{
			var types = new[] { typeof(IMultiMap) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac.IsMultimap(typeof(IMultiMap))), types));
		}

		[TestMethod]
		public void RegisterTypes_SuppliedTypesHasPolicyInjectionAttribute_MergedIntoOutput()
		{
			var types = new[] { typeof(IPolicyInjection) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac.IsMarkedForPolicyInjection(typeof(IPolicyInjection))), types));
		}

		[TestMethod]
		public void RegisterTypes_SuppliedTypesHasNamedMappingAttribute_MergedIntoOutput()
		{
			var types = new[] { typeof(INamedMapping) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => ac.IsNamedMapping(typeof(INamedMapping))), types));
		}
#endregion

		[TestMethod]
		public void RegisterTypes_ManyConfigurationsFound_ConfigurationsMergedIntoOutput()
		{
			TestableUnityConfigProvider.AddSingletons(typeof(String));
			var types = new[] { typeof(TestableUnityConfigProvider), typeof(SecondaryConfigProvider) };

			// Act
			controller.RegisterTypes(MappingBehaviors.None, types);

			// Assert
			mappingFactory.Verify(mf => mf.CreateMappings(MappingBehaviors.None, It.Is<AutomapperConfig>(ac => (!ac.IsMappable(typeof(Int32)) && ac.IsMarkedWithCustomLifetimeManager(typeof(String)).Item1)), types));
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
			mappingFactory.Setup(mf => mf.CreateMappings(It.IsAny<MappingBehaviors>(), It.IsAny<AutomapperConfig>(), It.IsAny<Type[]>()))
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

		[Singleton]		  public interface ISingleton { }
		[DoNotMap]		  public interface IDoNotMap { }
		[Multimap]		  public interface IMultiMap { }
		[PolicyInjection] public interface IPolicyInjection { }
		[MapAs("Foo")]	  public class INamedMapping { }

		public class SecondaryConfigProvider : IAutomapperConfigProvider
		{
			public AutomapperConfig CreateConfiguration()
			{
				return AutomapperConfig.Create().AndDoNotMapFor(typeof(Int32));
			}
		}
	}
}
