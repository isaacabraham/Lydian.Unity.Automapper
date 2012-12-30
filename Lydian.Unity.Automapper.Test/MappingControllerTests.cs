using ConfigProviderAssembly;
using Lydian.Unity.Automapper.Core;
using Lydian.Unity.Automapper.Core.Handling;
using Lydian.Unity.Automapper.Test.TestAssembly;
using Lydian.Unity.Automapper.Test.TestAssemblyTwo;
using Lydian.Unity.Automapper.Test.Types;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lydian.Unity.Automapper.Test
{
	[TestClass]
	public class MappingControllerTests
	{
		private UnityContainer target;
		private MappingController controller;
		private TypeMappingFactory factory;

		[TestInitialize]
		public void TestInit()
		{
			target = new UnityContainer();
			factory = new TypeMappingFactory();
			
			var internalContainer = new UnityContainer();
			internalContainer.RegisterType<ITypeMappingHandler, TypeMappingHandler>();
			internalContainer.RegisterType<IRegistrationNameFactory, RegistrationNameFactory>();
			internalContainer.RegisterType<ITypeMappingValidator, TypeMappingValidator>();
			internalContainer.RegisterType<IConfigLifetimeManagerFactory, ConfigLifetimeManagerFactory>();
			internalContainer.RegisterType<IInjectionMemberFactory, InjectionMemberFactory>();

			controller = new MappingController(target, factory, internalContainer);

			TestableUnityConfigProvider.Reset();
		}

		#region Basic mapping tests
		[TestMethod]
		public void RegisterTypes_SingleInterfaceSingleConcrete_RegistersIt()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation));
		}

		[TestMethod]
		public void RegisterTypes_InterfaceHasNoConcrete_DoesNotRegisterAnything()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface));

			// Assert
			AssertNumberOfExpectedCustomMappings(0);
		}

		[TestMethod]
		public void RegisterTypes_MultipleTypesAndInterfaces_RegistersThemAll()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None,
			typeof(IInterface), typeof(InterfaceImplementation),
			typeof(IOther), typeof(OtherImplementation)
			);

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation));
			CheckRegistrationExists(typeof(IOther), typeof(OtherImplementation));
		}

		[TestMethod]
		public void RegisterTypes_SameConcreteRegisteredToMultipleInterfaces_RegistersBoth()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(IOther), typeof(CompoundImplementation));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(CompoundImplementation));
			CheckRegistrationExists(typeof(IOther), typeof(CompoundImplementation));
		}
		#endregion

		[TestMethod]
		public void RegisterTypes_GenericInterfaceWithOpenImplementation_RegistersIt()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>));

			// Assert
			CheckRegistrationExists(typeof(IGenericInterface<object, object>), typeof(OpenGenericConcrete<object, object>));
		}

		[TestMethod]
		public void RegisterTypes_GenericInterfaceWithOpenAndClosedImplementations_RegistersThemAll()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>), typeof(ClosedGenericConcrete));

			// Assert
			CheckRegistrationExists(typeof(IGenericInterface<object, object>), typeof(OpenGenericConcrete<object, object>));
			CheckRegistrationExists(typeof(IGenericInterface<string, bool>), typeof(ClosedGenericConcrete));
		}

		[TestMethod]
		public void RegisterTypes_ConcreteImplementsSomeExternalInterface_IgnoresIt()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IGenericInterface<,>), typeof(EnumerableConcrete));

			// Assert
			CheckRegistrationExists(typeof(IGenericInterface<string, bool>), typeof(EnumerableConcrete));
			AssertNumberOfExpectedCustomMappings(1);
		}
		
		[TestMethod]
		public void RegisterTypes_NoSingletonAttribute_RegistersAsTranscient()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			var first = target.Resolve<IInterface>();
			var second = target.Resolve<IInterface>();
			Assert.AreNotSame(first, second);
		}

		[TestMethod]
		public void RegisterTypes_InterfaceConfiguredAsSingleton_RegistersAsSingleton()
		{
			TestableUnityConfigProvider.AddSingletons(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			var first = target.Resolve<IInterface>();
			var second = target.Resolve<IInterface>();
			Assert.AreSame(first, second);
		}

		#region Duplicate mapping tests
		[TestMethod, ExpectedException(typeof(DuplicateMappingException))]
		public void RegisterTypes_MultipleConcretesForSameInterfaceInSingleCall_ThrowsException()
		{
			// Act
			try
			{
				controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo));
			}
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual(typeof(IInterface), ex.MappingInterface);
				Assert.AreEqual(typeof(InterfaceImplementation), ex.MappedConcrete);
				Assert.AreEqual(typeof(InterfaceImplementationTwo), ex.DuplicateMappingConcrete);
				Assert.AreEqual("Attempted to map at least two concrete types (Lydian.Unity.Automapper.Test.Types.InterfaceImplementation and Lydian.Unity.Automapper.Test.Types.InterfaceImplementationTwo) to the same interface (Lydian.Unity.Automapper.Test.Types.IInterface).", ex.Message);
				throw;
			}
		}

		[TestMethod, ExpectedException(typeof(DuplicateMappingException))]
		public void RegisterTypes_MultipleConcretesForSameInterfaceInMultipleCalls_ThrowsException()
		{
			// Act
			try
			{
				controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation));
				controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementationTwo));
			}
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual(typeof(IInterface), ex.MappingInterface);
				Assert.AreEqual(typeof(InterfaceImplementation), ex.MappedConcrete);
				Assert.AreEqual(typeof(InterfaceImplementationTwo), ex.DuplicateMappingConcrete);
				Assert.AreEqual("Attempted to map at least two concrete types (Lydian.Unity.Automapper.Test.Types.InterfaceImplementation and Lydian.Unity.Automapper.Test.Types.InterfaceImplementationTwo) to the same interface (Lydian.Unity.Automapper.Test.Types.IInterface).", ex.Message);
				throw;
			}
		}

		#endregion

		#region Multimap Behavior tests
		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndInterfaceNotMarkedAsMultimapWithOneConcrete_DoesNotCreateAsAMultimap()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), false);
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndInterfaceExplictlyMarkedAsMultimapWithOneConcrete_StillCreatesAsAMultimap()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), true);
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorWithMultipleConcretesAndInterfaceIsMultimap_RegistersAllAsMultimap()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), true);
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementationTwo), true);
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndInterfaceIsNotExplicitMultimap_RegistersAsMultimap()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), true);
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementationTwo), true);
		}

		[TestMethod]
		public void RegisterTypes_NoMultimapBehaviorAndConcreteHasExplicitNamedMapping_StillUsesIt()
		{
			TestableUnityConfigProvider.AddNamedMapping(typeof(InterfaceImplementation), "Test");

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), true, "Test");
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndConcreteHasExplicitNamedMapping_StillUsesIt()
		{
			TestableUnityConfigProvider.AddNamedMapping(typeof(InterfaceImplementation), "Test");

			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), true, "Test");
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndSecondTypeHasDoNotMap_OnlyMapsTheFirstOne()
		{
			TestableUnityConfigProvider.AddDoNotMaps(typeof(InterfaceImplementationTwo));

			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), false);
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorWithOtherBehaviors_UsesMultimapBehavior()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault | MappingBehaviors.CollectionRegistration, typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), true);
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementationTwo), true);
		}

		#endregion

		[TestMethod]
		public void RegisterTypes_NothingProvided_DoesNothing()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None);

			// Assert
			AssertNumberOfExpectedCustomMappings(0);
		}

		#region Do Not Map tests
		[TestMethod]
		public void RegisterTypes_InterfaceHasDoNotMapAttribute_DoesNotMap()
		{
			TestableUnityConfigProvider.AddDoNotMaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			AssertNumberOfExpectedCustomMappings(0);
		}

		[TestMethod]
		public void RegisterTypes_ConcreteHasDoNotMapAttribute_DoesNotMap()
		{
			TestableUnityConfigProvider.AddDoNotMaps(typeof(InterfaceImplementation));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			AssertNumberOfExpectedCustomMappings(0);
		}

		#endregion

		#region Multimap tests
		[TestMethod]
		public void RegisterTypes_ConcreteMultiMapRegistration_IsNamedWithType()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), true);
		}

		[TestMethod]
		public void RegisterTypes_MultipleRegistrationsOfMultimapInterface_GetsThemAll()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo), typeof(TestableUnityConfigProvider));

			// Assert
			var allConcretes = target.ResolveAll<IInterface>();
			Assert.IsTrue(allConcretes.Any(c => c is InterfaceImplementation));
			Assert.IsTrue(allConcretes.Any(c => c is InterfaceImplementationTwo));
		}

		[TestMethod]
		public void RegisterTypes_ConcreteGenericMultiRegistration_IsNamedWithType()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(OpenGenericImplementation<Int32>), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(OpenGenericImplementation<Int32>), true);
		}

		[TestMethod]
		public void RegisterTypes_DifferentGenericArgumentsSameTypeForMultimap_BothAreMapped()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(OpenGenericImplementation<Int32>), typeof(OpenGenericImplementation<String>), typeof(TestableUnityConfigProvider));

			// Assert
			target.ResolveAll<IInterface>().ToArray();
			CheckRegistrationExists(typeof(IInterface), typeof(OpenGenericImplementation<Int32>), true);
			CheckRegistrationExists(typeof(IInterface), typeof(OpenGenericImplementation<String>), true);
		}

		[TestMethod]
		public void RegisterTypes_ClosedGenericTypeRegisteredToMultimap_MappedWithCorrectName()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(ClosedGenericImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(ClosedGenericImplementation), true);
		}

		[TestMethod]
		public void RegisterTypes_ClosedGenericAndOpenMappedToMultimap_BothMapped()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(ClosedGenericImplementation), typeof(OpenGenericImplementation<Boolean>), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(ClosedGenericImplementation), true);
			CheckRegistrationExists(typeof(IInterface), typeof(OpenGenericImplementation<Boolean>), true);
		}

		[TestMethod]
		public void RegisterTypes_OpenGenericInterfaceMultimap_RegistersAllConcretes()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IGenericInterface<,>));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>), typeof(OpenGenericConcreteTwo<,>), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IGenericInterface<String, String>), typeof(OpenGenericConcrete<String, String>), true);
			CheckRegistrationExists(typeof(IGenericInterface<String, String>), typeof(OpenGenericConcreteTwo<String, String>), true);
		}

		[TestMethod]
		public void RegisterTypes_OpenAndClosedTypesRegisteredToOpenGenericMultimap_RegistersAllConcretes()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IGenericInterface<,>));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>), typeof(OpenGenericConcreteTwo<,>), typeof(ClosedGenericConcrete), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IGenericInterface<String, String>), typeof(OpenGenericConcrete<String, String>), true);
			CheckRegistrationExists(typeof(IGenericInterface<String, String>), typeof(OpenGenericConcreteTwo<String, String>), true);
			CheckRegistrationExists(typeof(IGenericInterface<String, Boolean>), typeof(ClosedGenericConcrete), true);
		}

		[TestMethod]
		public void RegisterTypes_ClosedTypesRegisteredToOpenGenericMultimap_OnlyRegistersForTheSpecificGenericArgument()
		{
			TestableUnityConfigProvider.AddMultimaps(typeof(IGenericInterface<,>));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IGenericInterface<,>), typeof(ClosedGenericConcrete), typeof(TestableUnityConfigProvider));

			// Assert
			var resolutions = target.ResolveAll<IGenericInterface<String, String>>();
			Assert.IsFalse(resolutions.Any());
		}

		#endregion

		#region Named Mappings tests
		[TestMethod]
		public void RegisterTypes_NamedMappingSpecified_RegistersWithThatName()
		{
			TestableUnityConfigProvider.AddNamedMapping(typeof(InterfaceImplementation), "Test");

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), mappingName: "Test");
		}

		[TestMethod]
		[ExpectedException(typeof(DuplicateMappingException))]
		public void RegisterTypes_SameNamedMappingSpecifiedOnTwoConcretes_ThrowsException()
		{
			TestableUnityConfigProvider.AddNamedMapping(typeof(InterfaceImplementation), "Test");
			TestableUnityConfigProvider.AddNamedMapping(typeof(InterfaceImplementationTwo), "Test");
			TestableUnityConfigProvider.AddMultimaps(typeof(IInterface));

			// Act
			try
			{
				controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo), typeof(TestableUnityConfigProvider));
			}

			// Assert
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual(typeof(IInterface), ex.MappingInterface);
				Assert.AreEqual(typeof(InterfaceImplementation), ex.MappedConcrete);
				Assert.AreEqual(typeof(InterfaceImplementationTwo), ex.DuplicateMappingConcrete);
				Assert.AreEqual("Attempted to map at least two concrete types (Lydian.Unity.Automapper.Test.Types.InterfaceImplementation and Lydian.Unity.Automapper.Test.Types.InterfaceImplementationTwo) with the same name ('Test').", ex.Message);
				throw;
			}
		}

		[TestMethod]
		public void RegisterTypes_NamedMappingSpecifiedOnDifferentMappings_CompletesMapping()
		{
			TestableUnityConfigProvider.AddNamedMapping(typeof(InterfaceImplementation), "Test");
			TestableUnityConfigProvider.AddNamedMapping(typeof(OtherImplementation), "Test");
	
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(IOther), typeof(OtherImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			CheckRegistrationExists(typeof(IInterface), typeof(InterfaceImplementation), mappingName: "Test");
			CheckRegistrationExists(typeof(IOther), typeof(OtherImplementation), mappingName: "Test");
		}
		#endregion

		#region Automatic Collection Registration tests
		[TestMethod]
		public void RegisterTypes_AutomaticCollectionRegistration_CreatesAMultimapCollection()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.CollectionRegistration | MappingBehaviors.MultimapByDefault, typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo));

			// Asert
			var collection = target.Resolve<IEnumerable<IInterface>>();
			Assert.AreEqual(2, collection.Count());
			Assert.IsTrue(collection.Any(c => c is InterfaceImplementation));
			Assert.IsTrue(collection.Any(c => c is InterfaceImplementationTwo));
		}

		[TestMethod]
		public void RegisterTypes_AutomaticCollectionRegistration_DoesNotAffectExistingMappings()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.CollectionRegistration | MappingBehaviors.MultimapByDefault,
									 typeof(IOther), typeof(OtherImplementation),
									 typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo));
			// Assert
			CheckRegistrationExists(typeof(IOther), typeof(OtherImplementation));
		}

		[TestMethod]
		public void RegisterTypes_AutomaticCollectionRegistrationNotSpecified_DoesNotCreateAMultimapCollection()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault,
									 typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo));

			// Asert
			Assert.IsFalse(target.Registrations.Any(r => r.RegisteredType == typeof(IEnumerable<IInterface>)));
		}

		#endregion

		#region Register Assemblies tests
		[TestMethod]
		public void RegisterAssemblies_MultipleAssemblies_MergesThemAll()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "Lydian.Unity.Automapper.Test.TestAssembly", "Lydian.Unity.Automapper.Test.TestAssemblyTwo");

			// Assert
			CheckRegistrationExists(typeof(IDependencyInversionPrinciple), typeof(DependencyInversionPrincipleExample));
		}

		[TestMethod]
		public void RegisterAssemblies_AssemblyNameProvided_RegistersIt()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "Lydian.Unity.Automapper.Test.TestAssembly");

			// Assert
			CheckRegistrationExists(typeof(MyInterface), typeof(MyClass));
		}

		[TestMethod, ExpectedException(typeof(FileNotFoundException))]
		public void RegisterAssemblies_AssemblyNameDoesNotExist_ThrowsException()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "RandomName!");
		}

		[TestMethod]
		public void RegisterAssemblies_NothingProvided_DoesNothing()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None);

			// Assert
			AssertNumberOfExpectedCustomMappings(0);
		}

		#endregion
		
		#region Policy Injection tests
		[TestMethod]
		public void RegisterTypes_PolicyInjectionRequested_RegistersWithInjection()
		{
			TestableUnityConfigProvider.AddPolicyInjection(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			var resolvedType = target.Resolve<IInterface>();
			Assert.AreNotEqual(typeof(InterfaceImplementation), resolvedType.GetType());
		}

		[TestMethod]
		public void RegisterTypes_PolicyInjectionNotRequested_DoesNotActivateInterceptionExtension()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			Assert.IsTrue(target.Registrations.All(r => !r.RegisteredType.Equals(typeof(InjectionPolicy))));
		}

		[TestMethod]
		public void RegisterTypes_PolicyInjectionRequested_ActivatesInterceptionExtension()
		{
			TestableUnityConfigProvider.AddPolicyInjection(typeof(IInterface));

			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation), typeof(TestableUnityConfigProvider));

			// Assert
			Assert.IsTrue(target.Registrations.Any(r => r.RegisteredType.Equals(typeof(InjectionPolicy))));
		}

		[TestMethod]
		public void RegisterTypes_PolicyInjectionNotRequested_DoesNotRegisterWithInjection()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			var resolvedType = target.Resolve<IInterface>();
			Assert.AreEqual(typeof(InterfaceImplementation), resolvedType.GetType());
		}

		#endregion

		#region Configuration Provider tests
		[TestMethod]
		public void RegisterAssemblies_ConfigurationFound_IgnoresMarkedInterface()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "ConfigProviderAssembly");

			// Assert
			var registrationExists = target.Registrations.Any(r => r.RegisteredType == typeof(InterfaceToIgnore));
			Assert.IsFalse(registrationExists);
		}

		[TestMethod]
		public void RegisterAssemblies_ConfigurationFound_MapsSingletonCorrectly()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "ConfigProviderAssembly");

			// Assert
			var registration = target.Registrations.First(r => r.RegisteredType == typeof(SingletonInterface));
			Assert.IsInstanceOfType(registration.LifetimeManager, typeof(ContainerControlledLifetimeManager));
		}

		[TestMethod]
		public void RegisterAssemblies_ConfigurationFound_PerformsMultimappingCorrectly()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "ConfigProviderAssembly");

			// Assert
			var concretes = target.ResolveAll<MultimappingInterface>();
			Assert.AreEqual(2, concretes.Count());
		}

		[TestMethod]
		public void RegisterAssemblies_ConfigurationFound_PerformsNamedMultimappingCorrectly()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "ConfigProviderAssembly");

			// Assert
			var concrete = target.Resolve<MultimappingInterface>("Bananas");
			Assert.IsInstanceOfType(concrete, typeof(NamedType));
		}
		#endregion

		private void AssertNumberOfExpectedCustomMappings(Int32 expectedMappings)
		{
			var actualCustomMappings = target.Registrations.Count() - 1;
			Assert.AreEqual(expectedMappings, actualCustomMappings, String.Format("Should be {0} custom mappings in the container.", expectedMappings));
		}

		private void CheckRegistrationExists(Type fromType, Type toType, Boolean isNamedMapping = false, String mappingName = null)
		{
			try
			{
				mappingName = mappingName ?? (!isNamedMapping ? null : fromType.IsGenericType && toType.IsGenericType ? toType.GetGenericTypeDefinition().FullName : toType.FullName);
				var resolvedToType = target.Resolve(fromType, mappingName).GetType();
				Assert.AreEqual(toType, resolvedToType, String.Format("Mapping is not correct! Expected mapping from {0} to {1}, but mapping goes to {2}", fromType.FullName, toType.FullName, resolvedToType.FullName));
			}
			catch (Exception ex)
			{
				var innermostException = ex;
				while (innermostException.InnerException != null)
					innermostException = innermostException.InnerException;

				Assert.Fail(String.Format("An exception occurred whilst resolving {0} (using mapping name '{1}'): {2}", fromType.FullName, mappingName, innermostException.Message));
			}
		}
	}
}
