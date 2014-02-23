module Lydian.Unity.Automapper.MappingValidator

open Microsoft.Practices.Unity

let private checkForExistingNamedMapping (container : IUnityContainer, mapping, configuration : AutomapperConfigData) = 
    let mapFrom, mapTo = mapping
    let mappingName = configuration.GetNamedMapping(mapping)
    let existingRegistration = 
        container.Registrations |> Seq.tryFind (fun r -> r.Name = mappingName && r.RegisteredType = mapFrom)
    match existingRegistration with
    | Some existingRegistration -> raise <| DuplicateMappingException(mapFrom, mapTo, existingRegistration.MappedToType, mappingName)
    | None -> ()

let private checkForExistingTypeMapping ((container : IUnityContainer), (mapFrom, mapTo)) = 
    let existingRegistration = container.Registrations |> Seq.tryFind (fun r -> r.RegisteredType = mapFrom)
    match existingRegistration with
    | Some existingRegistration -> raise <| DuplicateMappingException(mapFrom, mapTo, existingRegistration.MappedToType)
    | None -> ()

let validateMapping (configurationData : AutomapperConfigData) container (behaviors : MappingBehaviors) mapping = 
    let mapFrom, mapTo = mapping
    let usingMultimapping = 
        behaviors.HasFlag(MappingBehaviors.MultimapByDefault) || mapFrom |> configurationData.IsMultimap
    if not usingMultimapping then checkForExistingTypeMapping (container, mapping) |> ignore
    checkForExistingNamedMapping (container, mapping, configurationData)
