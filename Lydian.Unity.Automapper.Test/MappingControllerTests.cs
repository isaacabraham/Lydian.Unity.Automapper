using ConfigProviderAssembly;
using Lydian.Unity.Automapper.Test.TestAssembly;
using Lydian.Unity.Automapper.Test.TestAssemblyTwo;
using Lydian.Unity.Automapper.Test.Types;
using Microsoft.Practices.Unity;
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
		private UnityContainer container;
		private MappingController controller;

		[TestInitialize]
		public void TestInit()
		{
			container = new UnityContainer();
			controller = new MappingController(container);
		}

		[TestMethod]
		public void RegisterTypes_SingleInterfaceSingleConcrete_RegistersIt()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne), typeof(ConcreteOne));

			// Assert
			CheckRegistrationExists(typeof(IInterfaceOne), typeof(ConcreteOne));
		}

		[TestMethod]
		public void RegisterTypes_InterfaceHasNoConcrete_DoesNotRegisterAnything()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne));

			// Assert
			AssertNumberOfExpectedCustomMappings(0);
		}

		[TestMethod]
		public void RegisterTypes_MultipleTypesAndInterfaces_RegistersThemAll()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None,
			typeof(IInterfaceOne), typeof(ConcreteOne),
			typeof(IInterfaceTwo), typeof(ConcreteTwo)
			);

			// Assert
			CheckRegistrationExists(typeof(IInterfaceOne), typeof(ConcreteOne));
			CheckRegistrationExists(typeof(IInterfaceTwo), typeof(ConcreteTwo));
		}

		[TestMethod]
		public void RegisterTypes_SameConcreteRegisteredToMultipleInterfaces_RegistersBoth()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne), typeof(IInterfaceTwo),
			typeof(CompoundConcrete)
			);

			// Assert
			CheckRegistrationExists(typeof(IInterfaceOne), typeof(CompoundConcrete));
			CheckRegistrationExists(typeof(IInterfaceTwo), typeof(CompoundConcrete));
		}

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
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne), typeof(ConcreteOne));

			// Assert
			var first = container.Resolve<IInterfaceOne>();
			var second = container.Resolve<IInterfaceOne>();
			Assert.AreNotSame(first, second);
		}

		[TestMethod]
		public void RegisterTypes_HasSingletonAttributeOnInterface_RegistersAsSingleton()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(ISingleton), typeof(SingletonConcrete));

			// Assert
			var first = container.Resolve<ISingleton>();
			var second = container.Resolve<ISingleton>();
			Assert.AreSame(first, second);
		}

		#region Duplicate mapping tests
		[TestMethod, ExpectedException(typeof(DuplicateMappingException))]
		public void RegisterTypes_MultipleConcretesForSameInterfaceInSingleCall_ThrowsException()
		{
			// Act
			try
			{
				controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne), typeof(ConcreteOne), typeof(DuplicateConcrete));
			}
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual(typeof(IInterfaceOne), ex.MappingInterface);
				Assert.AreEqual(typeof(ConcreteOne), ex.MappedConcrete);
				Assert.AreEqual(typeof(DuplicateConcrete), ex.DuplicateMappingConcrete);
				Assert.AreEqual("Attempted to map at least two concrete types (Lydian.Unity.Automapper.Test.Types.ConcreteOne and Lydian.Unity.Automapper.Test.Types.DuplicateConcrete) to the same interface (Lydian.Unity.Automapper.Test.Types.IInterfaceOne).", ex.Message);
				throw;
			}
		}

		[TestMethod, ExpectedException(typeof(DuplicateMappingException))]
		public void RegisterTypes_MultipleConcretesForSameInterfaceInMultipleCalls_ThrowsException()
		{
			// Act
			try
			{
				controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne), typeof(ConcreteOne));
				controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne), typeof(DuplicateConcrete));
			}
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual(typeof(IInterfaceOne), ex.MappingInterface);
				Assert.AreEqual(typeof(ConcreteOne), ex.MappedConcrete);
				Assert.AreEqual(typeof(DuplicateConcrete), ex.DuplicateMappingConcrete);
				Assert.AreEqual("Attempted to map at least two concrete types (Lydian.Unity.Automapper.Test.Types.ConcreteOne and Lydian.Unity.Automapper.Test.Types.DuplicateConcrete) to the same interface (Lydian.Unity.Automapper.Test.Types.IInterfaceOne).", ex.Message);
				throw;
			}
		}

		#endregion

		#region Multimap tests
		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndInterfaceNotMarkedAsMultimapWithOneConcrete_DoesNotCreateAsAMultimap()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IInterfaceOne), typeof(ConcreteOne));

			// Assert
			CheckRegistrationExists(typeof(IInterfaceOne), typeof(ConcreteOne), false);
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndInterfaceExplictlyMarkedAsMultimapWithOneConcrete_StillCreatesAsAMultimap()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IMultiInterface), typeof(ConcreteOneMulti));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(ConcreteOneMulti), true);
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndMultipleConcretesForInterface_UsesNamedMapping()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IMultiInterface), typeof(ConcreteOneMulti), typeof(ConcreteTwoMulti));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(ConcreteOneMulti), true);
			CheckRegistrationExists(typeof(IMultiInterface), typeof(ConcreteTwoMulti), true);
		}

		[TestMethod]
		public void RegisterTypes_NoMultimapBehaviorAndConcreteHasExplicitNamedMapping_StillUsesIt()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(NamedMultiConcrete));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(NamedMultiConcrete), true, "Test");
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndConcreteHasExplicitNamedMapping_StillUsesIt()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IMultiInterface), typeof(NamedMultiConcrete));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(NamedMultiConcrete), true, "Test");
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorAndSecondTypeHasDoNotMap_OnlyMapsTheFirstOne()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault, typeof(IInterfaceOne), typeof(ConcreteOne), typeof(IgnoredExplicitConcrete));

			// Assert
			CheckRegistrationExists(typeof(IInterfaceOne), typeof(ConcreteOne), false);
		}

		[TestMethod]
		public void RegisterTypes_MultimapBehaviorWithOtherBehaviors_UsesMultimapBehavior()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault | MappingBehaviors.CollectionRegistration, typeof(IMultiInterface), typeof(ConcreteOneMulti), typeof(ConcreteTwoMulti));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(ConcreteOneMulti), true);
			CheckRegistrationExists(typeof(IMultiInterface), typeof(ConcreteTwoMulti), true);
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
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IIgnoredInterface), typeof(IgnoredConcrete));

			// Assert
			AssertNumberOfExpectedCustomMappings(0);
		}

		[TestMethod]
		public void RegisterTypes_ConcreteHasDoNotMapAttribute_DoesNotMap()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne), typeof(IgnoredExplicitConcrete));

			// Assert
			AssertNumberOfExpectedCustomMappings(0);
		}

		#endregion

		#region Multimap tests
		[TestMethod]
		public void RegisterTypes_ConcreteMultiMapRegistration_IsNamedWithType()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(ConcreteOneMulti));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(ConcreteOneMulti), true);
		}

		[TestMethod]
		public void RegisterTypes_MultipleRegistrationsOfMultimapInterface_GetsThemAll()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(ConcreteOneMulti), typeof(ConcreteTwoMulti));

			// Assert
			var allConcretes = container.ResolveAll<IMultiInterface>();
			Assert.IsTrue(allConcretes.Any(c => c is ConcreteOneMulti));
			Assert.IsTrue(allConcretes.Any(c => c is ConcreteTwoMulti));
		}

		[TestMethod]
		public void RegisterTypes_ConcreteGenericMultiRegistration_IsNamedWithType()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(GenericMulti<Int32>));

			// Assert
			container.ResolveAll<IMultiInterface>().ToArray();
			CheckRegistrationExists(typeof(IMultiInterface), typeof(GenericMulti<Int32>), true);
		}

		[TestMethod]
		public void RegisterTypes_DifferentGenericArgumentsSameTypeForMultimap_BothAreMapped()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(GenericMulti<Int32>), typeof(GenericMulti<String>));

			// Assert
			container.ResolveAll<IMultiInterface>().ToArray();
			CheckRegistrationExists(typeof(IMultiInterface), typeof(GenericMulti<Int32>), true);
			CheckRegistrationExists(typeof(IMultiInterface), typeof(GenericMulti<String>), true);
		}

		[TestMethod]
		public void RegisterTypes_ClosedGenericTypeRegisteredToMultimap_MappedWithCorrectName()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(ClosedGenericMulti));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(ClosedGenericMulti), true);
		}

		[TestMethod]
		public void RegisterTypes_ClosedGenericAndOpenMappedToMultimap_BothMapped()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(ClosedGenericMulti), typeof(GenericMulti<Boolean>));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(ClosedGenericMulti), true);
			CheckRegistrationExists(typeof(IMultiInterface), typeof(GenericMulti<Boolean>), true);
		}

		[TestMethod]
		public void RegisterTypes_OpenGenericInterfaceMultimap_RegistersAllConcretes()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiGeneric<>), typeof(MultiGenericConcrete<>), typeof(MultiGenericConcreteTwo<>));

			// Assert
			CheckRegistrationExists(typeof(IMultiGeneric<String>), typeof(MultiGenericConcrete<String>), true);
			CheckRegistrationExists(typeof(IMultiGeneric<String>), typeof(MultiGenericConcreteTwo<String>), true);
		}

		[TestMethod]
		public void RegisterTypes_OpenAndClosedTypesRegisteredToOpenGenericMultimap_RegistersAllConcretes()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiGeneric<>), typeof(MultiGenericConcrete<>), typeof(MultiGenericConcreteTwo<>), typeof(MultiGenericClosedConcrete));

			// Assert
			CheckRegistrationExists(typeof(IMultiGeneric<String>), typeof(MultiGenericConcrete<String>), true);
			CheckRegistrationExists(typeof(IMultiGeneric<String>), typeof(MultiGenericConcreteTwo<String>), true);
			CheckRegistrationExists(typeof(IMultiGeneric<String>), typeof(MultiGenericClosedConcrete), true);
		}

		[TestMethod]
		public void RegisterTypes_ClosedTypesRegisteredToOpenGenericMultimap_OnlyRegistersForTheSpecificGenericArgument()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiGeneric<>), typeof(MultiGenericClosedConcrete));

			// Assert
			var resolutions = container.ResolveAll<IMultiGeneric<Boolean>>();
			Assert.IsFalse(resolutions.Any());
		}

		#endregion

		#region Named Mappings tests
		[TestMethod]
		public void RegisterTypes_NamedMappingSpecified_RegistersWithThatName()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IInterfaceOne), typeof(NamedConcrete));

			// Assert
			CheckRegistrationExists(typeof(IInterfaceOne), typeof(NamedConcrete), mappingName: "Test");
		}

		[TestMethod]
		[ExpectedException(typeof(DuplicateMappingException))]
		public void RegisterTypes_NamedMappingSpecifiedOnTwoConcretes_ThrowsException()
		{
			// Act
			try
			{
				controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(NamedMultiConcrete), typeof(NamedMultiConcreteTwo));
			}

			// Assert
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual(typeof(IMultiInterface), ex.MappingInterface);
				Assert.AreEqual(typeof(NamedMultiConcrete), ex.MappedConcrete);
				Assert.AreEqual(typeof(NamedMultiConcreteTwo), ex.DuplicateMappingConcrete);
				Assert.AreEqual("Attempted to map at least two concrete types (Lydian.Unity.Automapper.Test.Types.NamedMultiConcrete and Lydian.Unity.Automapper.Test.Types.NamedMultiConcreteTwo) with the same name ('Test').", ex.Message);
				throw;
			}
		}

		[TestMethod]
		public void RegisterTypes_NamedMappingSpecifiedOnDifferentMappings_CompletesMapping()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IMultiInterface), typeof(NamedMultiConcrete), typeof(IInterfaceOne), typeof(NamedConcrete));

			// Assert
			CheckRegistrationExists(typeof(IMultiInterface), typeof(NamedMultiConcrete), mappingName: "Test");
			CheckRegistrationExists(typeof(IInterfaceOne), typeof(NamedConcrete), mappingName: "Test");
		}

		#endregion

		#region Automatic Collection Registration tests
		[TestMethod]
		public void RegisterTypes_AutomaticCollectionRegistration_CreatesAMultimapCollection()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.CollectionRegistration | MappingBehaviors.MultimapByDefault, typeof(IMultiInterface), typeof(ConcreteOneMulti), typeof(ConcreteTwoMulti));

			// Asert
			var collection = container.Resolve<IEnumerable<IMultiInterface>>();
			Assert.AreEqual(2, collection.Count());
			Assert.IsTrue(collection.Any(c => c is ConcreteOneMulti));
			Assert.IsTrue(collection.Any(c => c is ConcreteTwoMulti));
		}

		[TestMethod]
		public void RegisterTypes_AutomaticCollectionRegistration_DoesNotAffectExistingMappings()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.CollectionRegistration | MappingBehaviors.MultimapByDefault,
			typeof(IInterfaceOne), typeof(ConcreteOne),
			typeof(IMultiInterface), typeof(ConcreteOneMulti), typeof(ConcreteTwoMulti));
			// Assert
			CheckRegistrationExists(typeof(IInterfaceOne), typeof(ConcreteOne));
		}

		[TestMethod]
		public void RegisterTypes_AutomaticCollectionRegistrationNotSpecified_DoesNotCreateAMultimapCollection()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.MultimapByDefault,
			typeof(IMultiInterface), typeof(ConcreteOneMulti), typeof(ConcreteTwoMulti));

			// Asert
			Assert.IsFalse(container.Registrations.Any(r => r.RegisteredType == typeof(IEnumerable<IInterfaceOne>)));
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
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IPolicyInterface), typeof(PolicyConcrete));

			// Assert
			var resolvedType = container.Resolve<IPolicyInterface>();
			Assert.AreNotEqual(typeof(PolicyConcrete), resolvedType.GetType());
		}

		[TestMethod]
		public void RegisterTypes_PolicyInjectionNotRequested_DoesNotActivateInterceptionExtension()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(INonPolicyInterface), typeof(PolicyConcrete));

			// Assert
			Assert.IsTrue(container.Registrations.All(r => !r.RegisteredType.Name.Equals("InjectionPolicy")));
		}

		[TestMethod]
		public void RegisterTypes_PolicyInjectionRequested_ActivatesInterceptionExtension()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(IPolicyInterface), typeof(PolicyConcrete));

			// Assert
			Assert.IsTrue(container.Registrations.Any(r => r.RegisteredType.Name.Equals("InjectionPolicy")));
		}

		[TestMethod]
		public void RegisterTypes_PolicyInjectionNotRequested_DoesNotRegisterWithInjection()
		{
			// Act
			controller.RegisterTypes(MappingBehaviors.None, typeof(INonPolicyInterface), typeof(PolicyConcrete));

			// Assert
			var resolvedType = container.Resolve<INonPolicyInterface>();
			Assert.AreEqual(typeof(PolicyConcrete), resolvedType.GetType());
		}

		#endregion

		#region Configuration Provider tests
		[TestMethod]
		public void RegisterAssemblies_ConfigurationFound_IgnoresMarkedInterface()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "ConfigProviderAssembly");

			// Assert
			var registrationExists = container.Registrations.Any(r => r.RegisteredType == typeof(InterfaceToIgnore));
			Assert.IsFalse(registrationExists);
		}

		[TestMethod]
		public void RegisterAssemblies_ConfigurationFound_MapsSingletonCorrectly()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "ConfigProviderAssembly");

			// Assert
			var registration = container.Registrations.First(r => r.RegisteredType == typeof(SingletonInterface));
			Assert.IsInstanceOfType(registration.LifetimeManager, typeof(ContainerControlledLifetimeManager));
		}

		[TestMethod]
		public void RegisterAssemblies_ConfigurationFound_PerformsMultimappingCorrectly()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "ConfigProviderAssembly");

			// Assert
			var concretes = container.ResolveAll<MultimappingInterface>();
			Assert.AreEqual(2, concretes.Count());
		}

		[TestMethod]
		public void RegisterAssemblies_ConfigurationFound_PerformsNamedMultimappingCorrectly()
		{
			// Act
			controller.RegisterAssemblies(MappingBehaviors.None, "ConfigProviderAssembly");

			// Assert
			var concrete = container.Resolve<MultimappingInterface>("Bananas");
			Assert.IsInstanceOfType(concrete, typeof(NamedType));
		}		
		#endregion

		private void AssertNumberOfExpectedCustomMappings(Int32 expectedMappings)
		{
			var actualCustomMappings = container.Registrations.Count() - 1;
			Assert.AreEqual(expectedMappings, actualCustomMappings, String.Format("Should be {0} custom mappings in the container.", expectedMappings));
		}

		private void CheckRegistrationExists(Type fromType, Type toType, Boolean isNamedMapping = false, String mappingName = null)
		{
			try
			{
				mappingName = mappingName ?? (!isNamedMapping ? null : fromType.IsGenericType && toType.IsGenericType ? toType.GetGenericTypeDefinition().FullName : toType.FullName);
				var resolvedToType = container.Resolve(fromType, mappingName).GetType();
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
