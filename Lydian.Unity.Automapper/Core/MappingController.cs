using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Lydian.Unity.Automapper.Core
{
	/// <summary>
	/// Handles auto-mapping of interfaces and concrete types into Unity.
	/// </summary>
	internal sealed class MappingController
	{
		private readonly IUnityContainer internalContainer;
		private readonly IUnityContainer target;
		private readonly ITypeMappingFactory mappingFactory;

		/// <summary>
		/// Creates a new instance of the Registrar.
		/// </summary>
		/// <param name="target">The container to register mappings into.</param>
		/// <param name="mappingFactory">The factory to create mappings.</param>
		/// <param name="internalContainer">The container used internally by the mapper itself. This is NOT the target for registration!</param>
		public MappingController(IUnityContainer target, ITypeMappingFactory mappingFactory, IUnityContainer internalContainer)
		{
			Contract.Requires(target != null);
			Contract.Requires(mappingFactory != null);
			Contract.Requires(internalContainer != null);

			this.internalContainer = internalContainer;
			this.mappingFactory = mappingFactory;
			this.target = target;
		}


		/// <summary>
		/// Registers types into the Unity container.
		/// </summary>
		/// <param name="types">The array of interfaces and concretes to map up and register.</param>
		/// <param name="behaviors">The behaviors to use to guide auto-mapping.</param>
		public IEnumerable<ContainerRegistration> RegisterTypes(MappingBehaviors behaviors, params Type[] types)
		{
			Contract.Requires(types != null, "types");
		
			var configurationDetails = GetConfigurationDetails(types);
			var mappings = mappingFactory.CreateMappings(types, behaviors, configurationDetails);

			var configParameter = new DependencyOverride<AutomapperConfig>(configurationDetails);
			var mappingParameter = new DependencyOverride<IEnumerable<TypeMapping>>(mappings);
			var behaviorParameter = new DependencyOverride<MappingBehaviors>(behaviors);
			var containerParameter = new DependencyOverride<IUnityContainer>(target);
			
			var mappingHandler = internalContainer.Resolve<ITypeMappingHandler>(configParameter, mappingParameter, behaviorParameter, containerParameter);
			return mappingHandler.PerformRegistrations(target, mappings);
		}

		private static AutomapperConfig GetConfigurationDetails(IEnumerable<Type> types)
		{
			return types
					.Where(type => typeof(IAutomapperConfigProvider).IsAssignableFrom(type))
					.Select(providerType => (IAutomapperConfigProvider)Activator.CreateInstance(providerType))
					.Cast<IAutomapperConfigProvider>()
					.Select(provider => provider.CreateConfiguration())
					.Aggregate(AutomapperConfig.Create(types), (accumulator, config) => accumulator.MergeWith(config));
		}

		/// <summary>
		/// Registers types found in the supplied assemblies into the Unity container.
		/// </summary>
		/// <param name="assemblyNames">The list of assembly names to register types from.</param>
		/// <param name="behaviors">The behaviors to use to guide auto-mapping.</param>
		public void RegisterAssemblies(MappingBehaviors behaviors, params String[] assemblyNames)
		{
			Contract.Requires(assemblyNames != null);
			var types = assemblyNames.SelectMany(assemblyName => Assembly.Load(assemblyName).GetTypes())
									 .ToArray();
			RegisterTypes(behaviors, types);
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Code Contracts")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code Contracts")]
		[ContractInvariantMethod]
		private void ObjectInvariant()
		{
			Contract.Invariant(target != null);
		}
	}
}