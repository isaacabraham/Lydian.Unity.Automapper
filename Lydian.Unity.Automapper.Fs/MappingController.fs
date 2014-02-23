module internal Lydian.Unity.Automapper.MappingController

open System.Reflection

/// Registers types into the Unity container.
let registerTypes (behaviors, types) container createMappings registrar configurationBuilder = 
    let configurationDetails = configurationBuilder (types)
    let mappings = createMappings (behaviors, configurationDetails, types)
    registrar (container, mappings)

/// Registers types found in the supplied assemblies into the Unity container.
let RegisterAssemblies(behaviors, (assemblyNames : string seq)) = 
    let types = 
        assemblyNames
        |> Seq.map Assembly.Load
        |> Seq.collect (fun asm -> asm.GetTypes())
        |> Seq.cache
    registerTypes (behaviors, types)
