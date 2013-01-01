using Microsoft.Practices.Unity;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Lydian.Unity.Automapper.Core.Handling
{
	internal class TypeMappingValidator : ITypeMappingValidator
	{
		private readonly IUnityContainer target;
		private readonly AutomapperConfig configurationDetails;
		private readonly MappingBehaviors mappingBehaviors;

		/// <summary>
		/// Initializes a new instance of the TypeMappingValidator class.
		/// </summary>
		/// <param name="configurationDetails"></param>
		/// <param name="target"></param>
		/// <param name="behaviors"></param>
		public TypeMappingValidator(AutomapperConfig configurationDetails, IUnityContainer target, MappingBehaviors behaviors)
		{
			this.target = target;
			this.configurationDetails = configurationDetails;
			this.mappingBehaviors = behaviors;
		}

		public void ValidateTypeMapping(TypeMapping mapping)
		{
			var usingMultimapping = mappingBehaviors.HasFlag(MappingBehaviors.MultimapByDefault) || configurationDetails.IsMultimap(mapping.From);
			if (!usingMultimapping)
				CheckForExistingTypeMapping(target, mapping);
			CheckForExistingNamedMapping(target, mapping, configurationDetails);
		}

		private static void CheckForExistingTypeMapping(IUnityContainer target, TypeMapping mapping)
		{
			var existingRegistration = target.Registrations
										     .FirstOrDefault(r => r.RegisteredType.Equals(mapping.From));
			if (existingRegistration != null)
				throw new DuplicateMappingException(mapping.From, existingRegistration.MappedToType, mapping.To);
		}

		private static void CheckForExistingNamedMapping(IUnityContainer target, TypeMapping mapping, AutomapperConfig configurationDetails)
		{
			var mappingName = configurationDetails.GetNamedMapping(mapping);
			var existingRegistration = target.Registrations
												.Where(r => String.Equals(r.Name, mappingName))
												.Where(r => r.RegisteredType.Equals(mapping.From))
												.FirstOrDefault();
			if (existingRegistration != null)
				throw new DuplicateMappingException(mapping.From, existingRegistration.MappedToType, mapping.To, mappingName);
		}

	}
}
