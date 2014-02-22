module Lydian.Unity.Automapper.Fs.TypeMappingFactory
    open System
    open System.Collections.Generic
    open Microsoft.Practices.Unity

    /// A facade on top of a Unity call to ResolveAll for a particular type.
    type internal UnityCollectionFacade<'a>(targetContainer:IUnityContainer) =
        let collection = seq { for resolvedType in targetContainer.ResolveAll<'a>() do yield resolvedType }
        interface IEnumerable<'a> with
            member this.GetEnumerator() = collection.GetEnumerator()
        interface System.Collections.IEnumerable with
            member this.GetEnumerator() = collection.GetEnumerator() :> _

    let private getGenericTypeSafely((destination:Type), genericParameter) =
        if not destination.IsGenericTypeDefinition then raise <| ArgumentException("source must be a generic type definition", "source")
        if destination.GetGenericArguments().Length <> 1 then raise <| ArgumentException("incorrect number of arguments", "destination")
        destination.MakeGenericType [|genericParameter|]

    let private createAcrMappings(mappings, (configurationDetails:AutomapperConfig)) =
        mappings
        |> Seq.countBy fst
        |> Seq.filter(fun (key,count) -> count > 1 || key |> configurationDetails.IsMultimap)
        |> Seq.map(fun (key,count) -> getGenericTypeSafely(typedefof<IEnumerable<_>>, key), getGenericTypeSafely(typedefof<UnityCollectionFacade<_>>, key))
        |> Seq.toList
    
    let getGenericallyOpenInterfaces(concrete:Type) =
        query { for concreteInterface in concrete.GetInterfaces() do
                let comparisonInterface = if concreteInterface.IsGenericType then concreteInterface.GetGenericTypeDefinition() else concreteInterface
                let destinationInterface = if concrete.IsGenericType then comparisonInterface else concreteInterface
                select (comparisonInterface, destinationInterface) }

    let createMappings((behaviors:MappingBehaviors), (configurationDetails:AutomapperConfig), (types:Type seq)) =          
        let results = query { for availableInterface in types
                                                        |> Seq.filter(fun t -> t.IsInterface)
                                                        |> Seq.filter configurationDetails.IsMappable do
                              for concrete in types |> Seq.filter(fun t -> not t.IsInterface)
                                                    |> Seq.filter configurationDetails.IsMappable do
                              for concreteInterface in (getGenericallyOpenInterfaces(concrete)
                                                        |> Seq.filter(fun (comp,_) -> comp = availableInterface)) do
                              select (concrete, concreteInterface) }
        
        let results = query { for result in results do
                              groupBy (result |> snd |> snd) into mappingsForAnInterface
                              for mapping in mappingsForAnInterface do
                              select ((mapping |> snd |> snd), (fst mapping)) }

        let acrMappings = match behaviors.HasFlag MappingBehaviors.CollectionRegistration with
                          | true -> createAcrMappings(results, configurationDetails)
                          | _ -> []

        (results |> Seq.toList) @ acrMappings