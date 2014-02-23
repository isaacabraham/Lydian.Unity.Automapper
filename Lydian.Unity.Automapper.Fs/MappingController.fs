module internal Lydian.Unity.Automapper.MappingController

open ConfigurationBuilder
open ContainerRegistrar
open System.Reflection
open TypeMappingFactory

/// Registers types into the Unity container.
let registerTypes types behaviors container = 
    let configurationDetails = buildConfiguration (types)
    let mappings = createMappings (behaviors, configurationDetails, types)
    registerTypes (container, mappings, configurationDetails, behaviors)

/// Registers types found in the supplied assemblies into the Unity container.
let registerAssemblies(assemblyNames : string seq) = 
    let types = 
        assemblyNames
        |> Seq.map Assembly.Load
        |> Seq.collect (fun asm -> asm.GetTypes())
        |> Seq.cache
    registerTypes types