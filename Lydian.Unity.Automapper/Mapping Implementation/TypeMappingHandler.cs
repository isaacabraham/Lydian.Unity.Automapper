using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Lydian.Unity.Automapper
{
	internal class TypeMappingHandler : ITypeMappingHandler
	{
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="Even catching exceptions to dispose of the lifetime manager does not remove this CA warning.")]
		public IEnumerable<ContainerRegistration> PerformRegistrations(IUnityContainer target, IEnumerable<TypeMapping> typeMappings, MappingBehaviors mappingBehaviors, AutomapperConfig configurationDetails)
		{
			var changeTracker = new UnityRegistrationTracker(target);
			var registrationNameFactory = new RegistrationNameFactory(configurationDetails, typeMappings, mappingBehaviors);
			var lifetimeManagerFactory = new LifetimeManagerFactory(configurationDetails);
			var injectionMemberFactory = new InjectionMemberFactory(configurationDetails, target);
			var mappingValidator = new TypeMappingValidator(configurationDetails, target, mappingBehaviors);

			foreach (var typeMapping in typeMappings)
			{
				mappingValidator.ValidateTypeMapping(typeMapping);
				
				var injectionMembers = injectionMemberFactory.CreateInjectionMembers(typeMapping);
				var lifetimeManager = lifetimeManagerFactory.CreateLifetimeManager(typeMapping);
				var registrationName = registrationNameFactory.GetRegistrationName(typeMapping);

				target.RegisterType(typeMapping.From, typeMapping.To, registrationName, lifetimeManager, injectionMembers);
			}

			return changeTracker.GetNewRegistrations();
		}

		class TypeMappingValidator
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
				Contract.Assume(target.Registrations != null);
				var existingRegistration = target.Registrations
													.FirstOrDefault(r => r.RegisteredType.Equals(mapping.From));
				if (existingRegistration != null)
					throw new DuplicateMappingException(mapping.From, existingRegistration.MappedToType, mapping.To);
			}

			private static void CheckForExistingNamedMapping(IUnityContainer target, TypeMapping mapping, AutomapperConfig configurationDetails)
			{
				Contract.Assume(target.Registrations != null);

				var mappingName = configurationDetails.GetNamedMapping(mapping);
				var existingRegistration = target.Registrations
													.Where(r => r.Name != null)
													.Where(r => r.RegisteredType.Equals(mapping.From))
													.Where(r => r.Name.Equals(mappingName))
													.FirstOrDefault();
				if (existingRegistration != null)
					throw new DuplicateMappingException(mapping.From, existingRegistration.MappedToType, mapping.To, mappingName);
			}

		}
		class InjectionMemberFactory
		{
			private readonly AutomapperConfig configurationDetails;
			
			public InjectionMemberFactory(AutomapperConfig configurationDetails, IUnityContainer target)
			{
				this.configurationDetails = configurationDetails;
				
				if (configurationDetails.PolicyInjectionRequired())
					target.AddNewExtension<Interception>();
			}

			public InjectionMember[] CreateInjectionMembers(TypeMapping typeMapping)
			{
				return configurationDetails.IsPolicyInjection(typeMapping.From) ? new InjectionMember[] { new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<PolicyInjectionBehavior>() }
																				: new InjectionMember[0];
			}
		}
		class LifetimeManagerFactory
		{
			private readonly AutomapperConfig configurationDetails;
			
			public LifetimeManagerFactory(AutomapperConfig configurationDetails)
			{
				this.configurationDetails = configurationDetails;
			}

			public LifetimeManager CreateLifetimeManager(TypeMapping typeMapping)
			{
				return configurationDetails.IsSingleton(typeMapping.From) ? (LifetimeManager)new ContainerControlledLifetimeManager()
																	      : new TransientLifetimeManager();
			}
		}
		class RegistrationNameFactory
		{
			private readonly AutomapperConfig configurationDetails;
			private readonly MappingBehaviors mappingBehaviors;
			private readonly IEnumerable<Type> multimapTypes;
			
			public RegistrationNameFactory(AutomapperConfig configurationDetails, IEnumerable<TypeMapping> typeMappings, MappingBehaviors mappingBehaviors)
			{
				this.mappingBehaviors = mappingBehaviors;
				this.configurationDetails = configurationDetails;
				multimapTypes = typeMappings
									.GroupBy(t => t.From)
									.Where(t => t.Count() > 1)
									.Select(group => group.Key)
									.ToArray();
			}

			public String GetRegistrationName(TypeMapping typeMapping)
			{
				var namedMappingRequested = configurationDetails.IsMultimap(typeMapping.From) || configurationDetails.IsNamedMapping(typeMapping.To);
				var namedMappingRequired = mappingBehaviors.HasFlag(MappingBehaviors.MultimapByDefault) && multimapTypes.Contains(typeMapping.From);
				return namedMappingRequested || namedMappingRequired ? configurationDetails.GetNamedMapping(typeMapping) : null;
			}
		}
	}
}
