using Lydian.Unity.Automapper.Core;
using Lydian.Unity.Automapper.Core.Handling;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lydian.Unity.Automapper.Test.Core.Handling
{

	[TestClass]
	public class TypeMappingValidatorTests
	{
		private IUnityContainer target;

		[TestInitialize]
		public void Setup()
		{
			target = new UnityContainer();
		}

		[TestMethod]
		public void ValidateTypeMapping_UniqueMapping_DoesNotThrowException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create(), target, MappingBehaviors.None);

			// Act
			validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
		}

		[TestMethod]
		public void ValidateTypeMapping_ConcreteMappedToOtherInterface_DoesNotThrowException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create(), target, MappingBehaviors.None);
			target.RegisterType<Object, ArrayList>();

			// Act
			validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
		}

		[TestMethod]
		[ExpectedException(typeof(DuplicateMappingException))]
		public void ValidateTypeMapping_InterfaceAlreadyMapped_ThrowsDuplicateMappingException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create(), target, MappingBehaviors.None);
			target.RegisterType<IEnumerable, SortedList>();

			// Act
			try
			{
				validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
			}
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual("Attempted to map at least two concrete types (System.Collections.SortedList and System.Collections.ArrayList) to the same interface (System.Collections.IEnumerable).", ex.Message);
				Assert.AreEqual(typeof(IEnumerable), ex.MappingInterface);
				Assert.AreEqual(typeof(ArrayList), ex.DuplicateMappingConcrete);
				Assert.AreEqual(typeof(SortedList), ex.MappedConcrete);
				throw;
			}
		}

		[TestMethod]
		public void ValidateTypeMapping_InterfaceAlreadyMappedWithMultimappingBehavior_DoesNotThrowException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create(), target, MappingBehaviors.MultimapByDefault);
			target.RegisterType<IEnumerable, SortedList>();

			// Act
			validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
		}

		[TestMethod]
		public void ValidateTypeMapping_InterfaceAlreadyMappedAndIsAMultimap_DoesNotThrowException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create().AndUseMultimappingFor(typeof(IEnumerable)), target, MappingBehaviors.None);
			target.RegisterType<IEnumerable, SortedList>();

			// Act
			validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
		}

		[TestMethod]
		[ExpectedException(typeof(DuplicateMappingException))]
		public void ValidateTypeMapping_MultimappingExistsWithSameImplicitNameForSameType_ThrowsDuplicateMappingException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create(), target, MappingBehaviors.MultimapByDefault);
			target.RegisterType<IEnumerable, SortedList>("System.Collections.ArrayList");

			// Act
			try
			{
				validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
			}
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual("Attempted to map at least two concrete types (System.Collections.SortedList and System.Collections.ArrayList) with the same name ('System.Collections.ArrayList').", ex.Message);
				Assert.AreEqual(typeof(IEnumerable), ex.MappingInterface);
				Assert.AreEqual(typeof(ArrayList), ex.DuplicateMappingConcrete);
				Assert.AreEqual(typeof(SortedList), ex.MappedConcrete);
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(DuplicateMappingException))]
		public void ValidateTypeMapping_MultimappingExistsWithSameExplicitNameForSameType_ThrowsDuplicateMappingException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create().AndUseNamedMappingFor(typeof(ArrayList), "TEST"), target, MappingBehaviors.MultimapByDefault);
			target.RegisterType<IEnumerable, SortedList>("TEST");

			// Act
			try
			{
				validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
			}
			catch (DuplicateMappingException ex)
			{
				Assert.AreEqual("Attempted to map at least two concrete types (System.Collections.SortedList and System.Collections.ArrayList) with the same name ('TEST').", ex.Message);
				Assert.AreEqual(typeof(IEnumerable), ex.MappingInterface);
				Assert.AreEqual(typeof(ArrayList), ex.DuplicateMappingConcrete);
				Assert.AreEqual(typeof(SortedList), ex.MappedConcrete);
				throw;
			}
		}

		[TestMethod]
		public void ValidateTypeMapping_NamedMappingExistsForAnotherType_DoesNotThrowException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create().AndUseNamedMappingFor(typeof(ArrayList), "TEST"), target, MappingBehaviors.MultimapByDefault);
			target.RegisterType<IList, List<String>>("TEST");

			// Act
			validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
		}

		[TestMethod]
		public void ValidateTypeMapping_MappingExistsForTypeWithAnotherName_DoesNotThrowException()
		{
			var validator = new TypeMappingValidator(AutomapperConfig.Create().AndUseNamedMappingFor(typeof(ArrayList), "TEST"), target, MappingBehaviors.MultimapByDefault);
			target.RegisterType<IEnumerable, SortedList>("ANOTHER NAME");

			// Act
			validator.ValidateTypeMapping(new TypeMapping(typeof(IEnumerable), typeof(ArrayList)));
		}
	}
}
