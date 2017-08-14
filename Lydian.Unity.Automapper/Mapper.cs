using Lydian.Unity.Automapper.Core;
using Lydian.Unity.Automapper.Core.Handling;
using Microsoft.Practices.Unity;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Lydian.Unity.Automapper
{
    /// <summary>
    /// Contains extension method entry points to the Unity Auto-mapper.
    /// </summary>
    public static class Mapper
	{
		private readonly static MappingOptions NULL_MAPPING_OPTIONS = new MappingOptions();

		/// <summary>
		/// Automatically maps and registers interfaces and concrete types into the Unity container.
		/// </summary>
		/// <param name="container">The container to be configured.</param>
		/// <param name="types">The array of interfaces and concretes to map up and register.</param>
		public static void AutomapTypes(this IUnityContainer container, params Type[] types)
		{
			Contract.Requires(container != null, "container");
			Contract.Requires(types != null, "types");

			container.AutomapTypes(NULL_MAPPING_OPTIONS, types);
		}

		/// <summary>
		/// Automatically maps and registers interfaces and concrete types into the Unity container.
		/// </summary>
		/// <param name="container">The container to be configured.</param>
		/// <param name="options">Any custom options to use as guidance when mapping.</param>
		/// <param name="types">The array of interfaces and concretes to map up and register.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Code Contracts", MessageId = "1")]
		public static void AutomapTypes(this IUnityContainer container, MappingOptions options, params Type[] types)
		{
			Contract.Requires(container != null, "container");
			Contract.Requires(options != null, "options");
			Contract.Requires(types != null, "types");

			var controller = CreateController(container);
			controller.RegisterTypes(options.Behaviors, types);
		}

		/// <summary>
		/// Automatically maps and registers interfaces and concrete types found in the supplied assemblies into the Unity container.
		/// </summary>
		/// <param name="container">The container to be configured.</param>
		/// <param name="assemblyNames">The list of full assembly names to register types from.</param>
		public static void AutomapAssemblies(this IUnityContainer container, params String[] assemblyNames)
		{
			Contract.Requires(container != null, "container");
			Contract.Requires(assemblyNames != null, "assemblyNames");

			container.AutomapAssemblies(NULL_MAPPING_OPTIONS, assemblyNames);
		}

		/// <summary>
		/// Automatically maps and registers interfaces and concrete types found in the supplied assemblies into the Unity container.
		/// </summary>
		/// <param name="container">The container to be configured.</param>
		/// <param name="options">Any custom options to use as guidance when mapping.</param>
		/// <param name="assemblyNames">The list of full assembly names to register types from.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Code Contracts", MessageId = "1")]
		public static void AutomapAssemblies(this IUnityContainer container, MappingOptions options, params String[] assemblyNames)
		{
			Contract.Requires(container != null, "container");
			Contract.Requires(options != null, "options");
			Contract.Requires(assemblyNames != null, "assemblyNames");

			var controller = CreateController(container);
			controller.RegisterAssemblies(options.Behaviors, assemblyNames);
		}

		private static MappingController CreateController(IUnityContainer target)
		{
			var internalContainer = new UnityContainer();

			internalContainer.RegisterType<ITypeMappingHandler, TypeMappingHandler>();
			internalContainer.RegisterType<IRegistrationNameFactory, RegistrationNameFactory>();
			internalContainer.RegisterType<ITypeMappingValidator, TypeMappingValidator>();
			internalContainer.RegisterType<IConfigLifetimeManagerFactory, ConfigLifetimeManagerFactory>();
			internalContainer.RegisterType<IInjectionMemberFactory, InjectionMemberFactory>();

			return new MappingController(target, new TypeMappingFactory(), internalContainer);
		}
	}
}