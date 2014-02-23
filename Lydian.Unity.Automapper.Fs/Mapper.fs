namespace Lydian.Unity.Automapper

open System
open Microsoft.Practices.Unity
open Lydian.Unity.Automapper.MappingController

/// <summary>
/// Contains extension method entry points to the Unity Auto-mapper.
/// </summary>
type Mapper() =
    static member private NULL_MAPPING_OPTIONS = { Behaviors = MappingBehaviors.None }
      
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="options">Any custom options to use as guidance when mapping.</param>
    /// <param name="types">The array of interfaces and concretes to map up and register.</param>
    static member AutomapTypes(container, options, types) = container |> registerTypes types options.Behaviors
    
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="types">The array of interfaces and concretes to map up and register.</param>
    static member AutomapTypes(container, types) = Mapper.AutomapTypes(container, Mapper.NULL_MAPPING_OPTIONS, types)

    /// <summary>
    /// Automatically maps and registers interfaces and concrete types found in the supplied assemblies into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="options">Any custom options to use as guidance when mapping.</param>
    /// <param name="assemblyNames">The list of full assembly names to register types from.</param>
    static member AutomapAssemblies(container, options, assemblyNames) = container |> registerAssemblies assemblyNames options.Behaviors
        
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types found in the supplied assemblies into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="assemblyNames">The list of full assembly names to register types from.</param>
    static member AutomapAssemblies(container, assemblyNames) = Mapper.AutomapAssemblies(container, Mapper.NULL_MAPPING_OPTIONS, assemblyNames)