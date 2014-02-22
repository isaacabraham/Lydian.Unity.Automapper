namespace Lydian.Unity.Automapper.Fs

open System

/// Specifies mapping behaviours to guide the automapping process.
[<Flags>]
type MappingBehaviors =
    /// No custom behaviours are specified.
    | None = 0
    /// If two types are mapped to the same interface, even if you do not specify the Multimapping on the source interface, multimap behaviour will be used.
    | MultimapByDefault = 1
    /// If an interface is multimapped, an extra registration will be made of the generic IEnumerable of T, allowing you to easily retrieve all registrations for the interface.
    | CollectionRegistration = 2

/// Contains any options that can be used to help guide the auto-mapping process.
type MappingOptions = {
    /// Any custom behaviors to use when mapping.
    Behaviors : MappingBehaviors
}

/// <summary>
/// Contains extension method entry points to the Unity Auto-mapper.
/// </summary>
module Mapper =
    open Microsoft.Practices.Unity

    let private NULL_MAPPING_OPTIONS = { Behaviors = MappingBehaviors.None }
      
    let CreateController(target) =
        let internalContainer = new UnityContainer()
        
//        internalContainer.RegisterType<ITypeMappingHandler, TypeMappingHandler>();
//        internalContainer.RegisterType<IRegistrationNameFactory, RegistrationNameFactory>();
//        internalContainer.RegisterType<ITypeMappingValidator, TypeMappingValidator>();
//        internalContainer.RegisterType<IConfigLifetimeManagerFactory, ConfigLifetimeManagerFactory>();
//        internalContainer.RegisterType<IInjectionMemberFactory, InjectionMemberFactory>();
        ()
//        new MappingController(target, new TypeMappingFactory(), internalContainer);
            
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="options">Any custom options to use as guidance when mapping.</param>
    /// <param name="types">The array of interfaces and concretes to map up and register.</param>
    let AutomapTypes(container, options, types) =
        let controller = CreateController(container)
        ()
//        controller.RegisterTypes(options.Behaviors, types)
    
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="types">The array of interfaces and concretes to map up and register.</param>
//    let AutomapTypes(container, types) =
//        AutomapTypes(container, NULL_MAPPING_OPTIONS, types)

    /// <summary>
    /// Automatically maps and registers interfaces and concrete types found in the supplied assemblies into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="options">Any custom options to use as guidance when mapping.</param>
    /// <param name="assemblyNames">The list of full assembly names to register types from.</param>
    let AutomapAssemblies(container, options, assemblyNames) =
        let controller = CreateController(container)
        ()
//   controller.RegisterAssemblies(options.Behaviors, assemblyNames)
        
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types found in the supplied assemblies into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="assemblyNames">The list of full assembly names to register types from.</param>
//    let AutomapAssemblies(container, assemblyNames) =
//        AutomapAssemblies(container, NULL_MAPPING_OPTIONS, assemblyNames);   