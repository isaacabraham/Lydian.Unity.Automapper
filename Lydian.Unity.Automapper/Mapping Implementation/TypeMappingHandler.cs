using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Carries out registrations on the container.
	/// </summary>
	internal static class TypeMappingHandler
	{
		/// <summary>
		/// Performs registrations using a supplied set of Mappings and guiding behaviors on a container.
		/// </summary>
		/// <param name="container">The cotainer to use to perform registrations.</param>
		/// <param name="typeMappings">The mappings to use.</param>
		/// <param name="mappingBehaviors">The behaviours to help guide the registration process.</param>
		/// <param name="configurationDetails">Details of the mappings, such as which types to ignore or to use policy injection with.</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="Even catching exceptions to dispose of the lifetime manager does not remove this CA warning.")]
		public static IEnumerable<ContainerRegistration> PerformRegistrations(IUnityContainer container, IEnumerable<TypeMapping> typeMappings, MappingBehaviors mappingBehaviors, AutomapperConfig configurationDetails)
		{
			Contract.Requires(container != null, "container is null.");
			Contract.Requires(typeMappings != null, "mappings is null.");

			if (configurationDetails.PolicyInjectionRequired())
				container.AddNewExtension<Interception>();

			var changeTracker = new UnityRegistrationTracker(container);

			var multimapTypes = GetTypesWithOneThanOneMapping(typeMappings);

			foreach (var typeMapping in typeMappings)
			{
				ValidateTypeMapping(container, mappingBehaviors, typeMapping, configurationDetails);
				var injectionMembers = configurationDetails.IsPolicyInjection(typeMapping.From) ? new InjectionMember[] { new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<PolicyInjectionBehavior>() } : new InjectionMember[0];
				var lifetimeManager = configurationDetails.IsSingleton(typeMapping.From) ? (LifetimeManager)new ContainerControlledLifetimeManager() : new TransientLifetimeManager();
				var registrationName = GetRegistrationName(mappingBehaviors, configurationDetails, multimapTypes, typeMapping);

				container.RegisterType(typeMapping.From, typeMapping.To, registrationName, lifetimeManager, injectionMembers);
			}

			return changeTracker.GetNewRegistrations();
		}

		private static Type[] GetTypesWithOneThanOneMapping(IEnumerable<TypeMapping> typeMappings)
		{
			return typeMappings
						.GroupBy(t => t.From)
						.Where(t => t.Count() > 1)
						.Select(group => group.Key)
						.ToArray();
		}

		private static String GetRegistrationName(MappingBehaviors mappingBehaviors, AutomapperConfig configurationDetails, Type[] multimapTypes, TypeMapping typeMapping)
		{
			var namedMappingRequested = configurationDetails.IsMultimap(typeMapping.From) || configurationDetails.IsNamedMapping(typeMapping.To);
			var namedMappingRequired = mappingBehaviors.HasFlag(MappingBehaviors.MultimapByDefault) && multimapTypes.Contains(typeMapping.From);
			return namedMappingRequested || namedMappingRequired ? configurationDetails.GetNamedMapping(typeMapping) : null;
		}

		private static void ValidateTypeMapping(IUnityContainer container, MappingBehaviors mappingBehaviors, TypeMapping mapping, AutomapperConfig configurationDetails)
		{
			var usingMultimapping = configurationDetails.IsMultimap(mapping.From) || mappingBehaviors.HasFlag(MappingBehaviors.MultimapByDefault);
			if (!usingMultimapping)
				CheckForExistingTypeMapping(container, mapping);
			CheckForExistingNamedMapping(container, mapping, configurationDetails);
		}

		private static void CheckForExistingTypeMapping(IUnityContainer container, TypeMapping mapping)
		{
			Contract.Assume(container.Registrations != null);
			var existingRegistration = container.Registrations
												.FirstOrDefault(r => r.RegisteredType.Equals(mapping.From));
			if (existingRegistration != null)
				throw new DuplicateMappingException(mapping.From, existingRegistration.MappedToType, mapping.To);
		}

		private static void CheckForExistingNamedMapping(IUnityContainer container, TypeMapping mapping, AutomapperConfig configurationDetails)
		{
			Contract.Assume(container.Registrations != null);

			var mappingName = configurationDetails.GetNamedMapping(mapping);

			var existingRegistration = container.Registrations
												.Where(r => r.Name != null)
												.Where(r => r.RegisteredType.Equals(mapping.From))
												.Where(r => r.Name.Equals(mappingName))
												.FirstOrDefault();
			if (existingRegistration != null)
				throw new DuplicateMappingException(mapping.From, existingRegistration.MappedToType, mapping.To, mappingName);
		}
	}
}
