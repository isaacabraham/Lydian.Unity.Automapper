using Lydian.Unity.Automapper.Core.Handling;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lydian.Unity.Automapper.Core
{
	internal class TypeMappingHandler : ITypeMappingHandler
	{
		private readonly IInjectionMemberFactory injectionFactory;
		private readonly IConfigLifetimeManagerFactory lifetimeFactory;
		private readonly ITypeMappingValidator mappingValidator;
		private readonly IRegistrationNameFactory nameFactory;

		public TypeMappingHandler(IRegistrationNameFactory nameFactory, IConfigLifetimeManagerFactory lifetimeFactory, IInjectionMemberFactory injectionFactory, ITypeMappingValidator mappingValidator)
		{
			this.mappingValidator = mappingValidator;
			this.injectionFactory = injectionFactory;
			this.lifetimeFactory = lifetimeFactory;
			this.nameFactory = nameFactory;
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="Even catching exceptions to dispose of the lifetime manager does not remove this CA warning.")]
		public IEnumerable<ContainerRegistration> PerformRegistrations(IUnityContainer target, IEnumerable<TypeMapping> typeMappings)
		{
			var changeTracker = new UnityRegistrationTracker(target);

			foreach (var typeMapping in typeMappings)
			{
				mappingValidator.ValidateTypeMapping(typeMapping);

				var injectionMembers = injectionFactory.CreateInjectionMembers(typeMapping);
				var lifetimeManager = lifetimeFactory.CreateLifetimeManager(typeMapping);
				var registrationName = nameFactory.GetRegistrationName(typeMapping);

				target.RegisterType(typeMapping.From, typeMapping.To, registrationName, lifetimeManager, injectionMembers);
			}

			return changeTracker.GetNewRegistrations();
		}
	}
}
