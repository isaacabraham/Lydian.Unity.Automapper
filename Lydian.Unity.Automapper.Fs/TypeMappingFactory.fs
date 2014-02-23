module internal Lydian.Unity.Automapper.TypeMappingFactory

open Microsoft.Practices.Unity
open System
open System.Collections.Generic
open Lydian.Unity.Automapper

let private getGenericTypeSafely ((destination : Type), genericParameter) = 
    if not destination.IsGenericTypeDefinition then 
        raise <| ArgumentException("source must be a generic type definition", "source")
    if destination.GetGenericArguments().Length <> 1 then 
        raise <| ArgumentException("incorrect number of arguments", "destination")
    destination.MakeGenericType [| genericParameter |]

let private createAcrMappings (mappings, (configurationDetails : AutomapperConfigData)) = 
    mappings
    |> Seq.countBy fst
    |> Seq.filter (fun (key, count) -> count > 1 || key |> configurationDetails.IsMultimap)
    |> Seq.map 
           (fun (key, count) -> 
           getGenericTypeSafely (typedefof<IEnumerable<_>>, key), 
           getGenericTypeSafely (typedefof<UnityCollectionFacade<_>>, key))
    |> Seq.toList

let private getGenericallyOpenInterfaces (concrete : Type) = 
    query { 
        for concreteInterface in concrete.GetInterfaces() do
            let comparisonInterface = 
                if concreteInterface.IsGenericType then concreteInterface.GetGenericTypeDefinition()
                else concreteInterface
            
            let destinationInterface = 
                if concrete.IsGenericType then comparisonInterface
                else concreteInterface
            
            select (comparisonInterface, destinationInterface)
    }

/// Creates auto-generated mappings from which to perform registrations on.
let createMappings ((behaviors : MappingBehaviors), (configurationDetails : AutomapperConfigData), (types : Type seq)) = 
    let results = 
        query { 
            for availableInterface in types
                                      |> Seq.filter (fun t -> t.IsInterface)
                                      |> Seq.filter configurationDetails.IsMappable do
                for concrete in types
                                |> Seq.filter (fun t -> not t.IsInterface)
                                |> Seq.filter configurationDetails.IsMappable do
                    for concreteInterface in (getGenericallyOpenInterfaces (concrete) 
                                              |> Seq.filter (fun (comp, _) -> comp = availableInterface)) do
                        select (concrete, concreteInterface)
        }
    
    let results = 
        query { 
            for result in results do
                groupBy (result
                         |> snd
                         |> snd) into mappingsForAnInterface
                for mapping in mappingsForAnInterface do
                    select ((mapping
                             |> snd
                             |> snd), (fst mapping))
        }
    
    let acrMappings = 
        match behaviors.HasFlag MappingBehaviors.CollectionRegistration with
        | true -> createAcrMappings (results, configurationDetails)
        | _ -> []
    
    (results |> Seq.toList) @ acrMappings
