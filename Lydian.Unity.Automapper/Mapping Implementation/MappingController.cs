using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Handles auto-mapping of interfaces and concrete types into Unity.
	/// </summary>
	internal sealed class MappingController
	{	
		private readonly IUnityContainer container;

		/// <summary>
		/// Creates a new instance of the Registrar.
		/// </summary>
		/// <param name="container">The container to register mappings into.</param>
		public MappingController(IUnityContainer container)
		{
			Contract.Requires(container != null);
			this.container = container;
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
			var mappings = TypeMappingFactory.CreateMappings(types, behaviors, configurationDetails);
			return TypeMappingHandler.PerformRegistrations(container, mappings, behaviors, configurationDetails);
		}

		private static AutomapperConfig GetConfigurationDetails(Type[] types)
		{
			Contract.Requires(types != null, "types is null.");

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
			Contract.Invariant(container != null);
		}
	}
}