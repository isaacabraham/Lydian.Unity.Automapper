namespace Lydian.Unity.Automapper

open Lydian.Unity.Automapper.Core.ConfigurationBuilder
open Lydian.Unity.Automapper.Core.ContainerRegistrar
open Lydian.Unity.Automapper.Core.TypeMappingFactory
open Microsoft.Practices.Unity
open System
open System.Reflection
open System.Runtime.CompilerServices

[<Extension>]
/// <summary>
/// Contains extension method entry points to the Unity Automapper.
/// </summary>
type Mapper() = 
    
    static member private registerTypes types (behaviors : MappingBehaviors) container = 
        let configuration = buildConfiguration (types)
        let mappings = createMappings (behaviors, configuration, types)
        registerMappings (container, mappings, configuration, behaviors)
    
    static member private registerAssemblies (assemblyNames : string seq) = 
        let types = 
            assemblyNames
            |> Seq.map Assembly.Load
            |> Seq.collect (fun asm -> asm.GetTypes())
            |> Seq.cache
        Mapper.registerTypes types
    
    static member private NULL_MAPPING_OPTIONS = { Behaviors = MappingBehaviors.None }
    
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="options">Any custom options to use as guidance when mapping.</param>
    /// <param name="types">The array of interfaces and concretes to map up and register.</param>
    [<Extension>]
    static member AutomapTypes((container : IUnityContainer), options, [<ParamArray>] types : Type []) = 
        container |> Mapper.registerTypes types options.Behaviors
    
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="types">The array of interfaces and concretes to map up and register.</param>
    [<Extension>]
    static member AutomapTypes((container : IUnityContainer), [<ParamArray>] types : Type []) = 
        Mapper.AutomapTypes(container, Mapper.NULL_MAPPING_OPTIONS, types)
    
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types found in the supplied assemblies into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="options">Any custom options to use as guidance when mapping.</param>
    /// <param name="assemblyNames">The list of full assembly names to register types from.</param>
    [<Extension>]
    static member AutomapAssemblies((container : IUnityContainer), options, [<ParamArray>] assemblyNames : string []) = 
        container |> Mapper.registerAssemblies assemblyNames options.Behaviors
    
    /// <summary>
    /// Automatically maps and registers interfaces and concrete types found in the supplied assemblies into the Unity container.
    /// </summary>
    /// <param name="container">The container to be configured.</param>
    /// <param name="assemblyNames">The list of full assembly names to register types from.</param>
    [<Extension>]
    static member AutomapAssemblies((container : IUnityContainer), assemblyNames) = 
        Mapper.AutomapAssemblies(container, Mapper.NULL_MAPPING_OPTIONS, assemblyNames)
