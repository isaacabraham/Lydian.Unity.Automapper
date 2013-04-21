using Lydian.Unity.Automapper.Core;
using Lydian.Unity.Automapper.Test.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lydian.Unity.Automapper.Test.Core
{
	[TestClass]
	public class TypeMappingFactoryTests
	{
		private TypeMappingFactory factory;
		
		[TestInitialize]
		public void Setup()
		{
			factory = new TypeMappingFactory();
		}

		[TestMethod]
		public void CreateMappings_SimpleMatch_ReturnsTypeMapping()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			AssertMapping<IInterface, InterfaceImplementation>(mappings);
			Assert.AreEqual(1, mappings.Count());
		}

		[TestMethod]
		public void CreateMappings_MultipleSimpleMatches_ReturnsAllTypeMappings()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo));

			// Assert
			AssertMapping<IInterface, InterfaceImplementation>(mappings);
			AssertMapping<IInterface, InterfaceImplementationTwo>(mappings);
			Assert.AreEqual(2, mappings.Count());
		}

		[TestMethod]
		public void CreateMappings_NoConcrete_ReturnsNoMappings()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface));

			// Assert
			Assert.IsFalse(mappings.Any());
		}

		[TestMethod]
		public void CreateMappings_ConcreteDoesNotMatch_ReturnsNoMappings()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(String));

			// Assert
			Assert.IsFalse(mappings.Any());
		}

		[TestMethod]
		public void CreateMappings_MultipleInterfacesOneHasAMatch_ReturnsThatMapping()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(InterfaceImplementation), typeof(IOther));

			// Assert
			AssertMapping<IInterface, InterfaceImplementation>(mappings);
			Assert.AreEqual(1, mappings.Count());
		}

		[TestMethod]
		public void CreateMappings_MultipleInterfacesBothHaveADifferentMatch_ReturnsBothMappings()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(InterfaceImplementation), typeof(IOther), typeof(OtherImplementation));

			// Assert
			AssertMapping<IInterface, InterfaceImplementation>(mappings);
			AssertMapping<IOther, OtherImplementation>(mappings);
			Assert.AreEqual(2, mappings.Count());
		}

		[TestMethod]
		public void CreatMappings_MultipleInterfacesWithSameConcreteImplentation_ReturnsBothMappings()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(CompoundImplementation), typeof(IOther));

			// Assert
			AssertMapping<IInterface, CompoundImplementation>(mappings);
			AssertMapping<IOther, CompoundImplementation>(mappings);
			Assert.AreEqual(2, mappings.Count());
		}

		[TestMethod]
		public void CreateMappings_ConcreteIsDoNotMap_DoesNotMap()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create().AndDoNotMapFor(typeof(InterfaceImplementation)),
												  typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			AssertMapping<IInterface, InterfaceImplementation>(mappings, Expectation.ShouldNotExist);			
		}
		
		[TestMethod]
		public void CreateMappings_InterfaceIsDoNotMap_DoesNotMap()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create().AndDoNotMapFor(typeof(IInterface)),
												  typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			AssertMapping<IInterface, InterfaceImplementation>(mappings, Expectation.ShouldNotExist);			
		}

		[TestMethod]
		public void CreateMappings_OpenGenericImplementationForNonGenericInterface_MapsIt()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(OpenGenericImplementation<Boolean>));

			// Assert
			AssertMapping<IInterface, OpenGenericImplementation<Boolean>>(mappings);
		}

		[TestMethod]
		public void CreateMappings_ClosedGenericImplementationForNonGenericInterface_MapsIt()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(ClosedGenericImplementation));

			// Assert
			AssertMapping<IInterface, ClosedGenericImplementation>(mappings);
		}

		#region Generic mapping tests
		[TestMethod]
		public void CreateMappings_OpenGenericInterfaceWithOpenImplementation_MapsIt()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>));

			// Assert
			AssertMapping(mappings, typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>));
		}

		[TestMethod]
		public void CreateMappings_GenericInterfaceWithClosedImplementation_MapsIt()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IGenericInterface<,>), typeof(ClosedGenericConcrete));

			// Assert
			AssertMapping(mappings, typeof(IGenericInterface<String, Boolean>), typeof(ClosedGenericConcrete));
		}

		[TestMethod]
		public void CreateMappings_GenericInterfaceMultipleImplementationsForDifferentTypes_MapsThem()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>), typeof(ClosedGenericConcrete));

			// Assert
			AssertMapping(mappings, typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>));
			AssertMapping(mappings, typeof(IGenericInterface<String, Boolean>), typeof(ClosedGenericConcrete));
		}

		[TestMethod]
		public void CreateMappings_GenericInterfaceWithMultipleOpenImplementations_MapsThem()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.None, AutomapperConfig.Create(),
												  typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>), typeof(OpenGenericConcreteTwo<,>));

			// Assert
			AssertMapping(mappings, typeof(IGenericInterface<,>), typeof(OpenGenericConcrete<,>));
			AssertMapping(mappings, typeof(IGenericInterface<,>), typeof(OpenGenericConcreteTwo<,>));
			Assert.AreEqual(2, mappings.Count());
		}
		#endregion

		#region Collection Registration tests
		[TestMethod]
		public void CreateMappings_CollectionRegistrationAndMultipleMappings_CreatesCollectionMapping()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.CollectionRegistration, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(InterfaceImplementation), typeof(InterfaceImplementationTwo));

			// Assert
			AssertMapping<IEnumerable<IInterface>, UnityCollectionFacade<IInterface>>(mappings);
		}

		[TestMethod]
		public void CreateMappings_CollectionRegistrationAndSingleMapping_DoesNotCreateCollectionMapping()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.CollectionRegistration, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(InterfaceImplementation));

			// Assert
			AssertMapping<IEnumerable<IInterface>, UnityCollectionFacade<IInterface>>(mappings, Expectation.ShouldNotExist);
		}

		[TestMethod]
		public void CreateMappings_CollectionRegistrationAndMultipleMappingsAcrossManyTimes_DoesNotCreateCollectionMapping()
		{
			// Act
			var mappings = factory.CreateMappings(MappingBehaviors.CollectionRegistration, AutomapperConfig.Create(),
												  typeof(IInterface), typeof(InterfaceImplementation), typeof(IOther), typeof(OtherImplementation));

			// Assert
			AssertMapping<IEnumerable<IInterface>, UnityCollectionFacade<IInterface>>(mappings, Expectation.ShouldNotExist);
			AssertMapping<IEnumerable<IOther>, UnityCollectionFacade<IOther>>(mappings, Expectation.ShouldNotExist);
		}
		#endregion

		internal enum Expectation
		{
			ShouldExist = 1,
			ShouldNotExist = 2
		}

		private static void AssertMapping(IEnumerable<TypeMapping> mappings, Type fromType, Type toType, Expectation expectation = Expectation.ShouldExist)
		{
			var match = mappings.Where(m => m.From == fromType)
								.Where(m => m.To == toType)
								.Any();

			if (expectation == Expectation.ShouldNotExist)
				match = !match;

			Assert.IsTrue(match, String.Format("Could not locate mapping from {0} to {1}", fromType.FullName, toType.FullName));
		}

		private static void AssertMapping<TFrom, TTo>(IEnumerable<TypeMapping> mappings, Expectation expectation = Expectation.ShouldExist)
		{
			AssertMapping(mappings, typeof(TFrom), typeof(TTo), expectation);
		}
	}
}
