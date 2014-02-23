using Lydian.Unity.Automapper.Core.Handling;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
			var extensionSetter = new PolicyExtensionSetter(target);
			foreach (var typeMapping in typeMappings)
			{
				mappingValidator.ValidateTypeMapping(typeMapping);

				var injectionMembers = injectionFactory.CreateInjectionMembers(typeMapping);
				var lifetimeManager = lifetimeFactory.CreateLifetimeManager(typeMapping);
				var registrationName = nameFactory.GetRegistrationName(typeMapping);

				target.RegisterType(typeMapping.From, typeMapping.To, registrationName, lifetimeManager, injectionMembers);
				extensionSetter.CheckIfInterceptionExtensionIsRequired(injectionMembers);
			}

			return changeTracker.GetNewRegistrations();
		}

		class PolicyExtensionSetter
		{
			private Boolean alreadyAdded = false;
			private readonly IUnityContainer container;

			public PolicyExtensionSetter(IUnityContainer container)
			{
				this.container = container;
			}

			public void CheckIfInterceptionExtensionIsRequired(IEnumerable<InjectionMember> members)
			{
				if (alreadyAdded || !members.Any())
					return;

				container.AddNewExtension<Interception>();
				alreadyAdded = true;
			}
		}
	}
}