module internal Lydian.Unity.Automapper.ConfigurationBuilder

open System
open Microsoft.Practices.Unity

let private hasAttribute<'a> (t:Type) = t.GetCustomAttributes(typeof<'a>, false) |> Seq.length > 0;
let private getTypesWith<'a> = Seq.filter hasAttribute<'a> >> Seq.toArray
let private getMapAsName (t:Type) =
    let mapAsAttribute = t.GetCustomAttributes(typeof<MapAsAttribute>, false) |> Seq.map(fun att -> att :?> MapAsAttribute) |> Seq.tryFind(fun t -> true)
    match mapAsAttribute with
    | Some mapAsAttribute -> Some (mapAsAttribute.MappingName)
    | None -> None

let private buildConfigurationFromAttributes types =
    let config = AutomapperConfig.Create()
                                 .AndDoNotMapFor(types |> getTypesWith<DoNotMapAttribute>)
                                 .AndUseMultimappingFor(types |> getTypesWith<MultimapAttribute>)
                                 .AndUsePolicyInjectionFor(types |> getTypesWith<PolicyInjectionAttribute>)

    let namedMappings = types
                        |> Seq.map(fun t -> t, getMapAsName(t))
                        |> Seq.choose(fun (t,mapName) -> match mapName with
                                                        | Some mapName -> Some (t, mapName)
                                                        | None -> None)
    let config = (config,namedMappings)
                 ||> Seq.fold(fun config namedMapping -> config.AndUseNamedMappingFor(namedMapping))



    config

let registerLifetimeManagers((source:Type seq), config) =
    let mapMethodCallsite = typeof<AutomapperConfig>.GetMethod("AndMapWithLifetimeManager").GetGenericMethodDefinition()
    for (lifetimeManager,items) in source  
                                   |> Seq.collect(fun t -> t.GetCustomAttributes(typeof<CustomLifetimeManagerAttribute>, false) |> Seq.map(fun att -> t, att :?> CustomLifetimeManagerAttribute))
                                   |> Seq.groupBy snd
                                   do                    
        
        if not (typeof<LifetimeManager>.IsAssignableFrom(lifetimeManager.LifetimeManagerType)) then
            raise <| InvalidOperationException(sprintf "The type %s has been marked with the type %s as a Lifetime Manager; lifetime managers must derive from LifetimeManager." (items |> Seq.head |> fst).FullName lifetimeManager.LifetimeManagerType.FullName)
        try
            mapMethodCallsite.MakeGenericMethod(lifetimeManager.LifetimeManagerType)
                             .Invoke(config, [| items |> Seq.map snd |> Seq.toArray |])
                             |> ignore
        with
        | ex -> raise ex.InnerException