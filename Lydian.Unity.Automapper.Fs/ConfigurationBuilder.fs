module internal Lydian.Unity.Automapper.ConfigurationBuilder

open System

let private hasAttribute<'a> (t:Type) = t.GetCustomAttributes(typeof<'a>, false) |> Seq.length > 0;
let private getTypesWith<'a> = Seq.filter hasAttribute<'a> >> Seq.toArray
let private getMapAsName (t:Type) =
    let mapAsAttribute = t.GetCustomAttributes(typeof<MapAsAttribute>, false) |> Seq.map(fun att -> att :?> MapAsAttribute) |> Seq.tryFind(fun t -> true)
    match mapAsAttribute with
    | Some mapAsAttribute -> Some (mapAsAttribute.MappingName)
    | None -> None

let private buildConfigurationFromAttributes types =
    let config = (((AutomapperConfig.Create(), (types |> getTypesWith<DoNotMapAttribute>))
                 ||> Seq.fold(fun config t -> config.AndDoNotMapFor(t)), (types |> getTypesWith<MultimapAttribute>))
                 ||> Seq.fold(fun config t -> config.AndUseMultimappingFor(t)), (types |> getTypesWith<PolicyInjectionAttribute>))
                 ||> Seq.fold(fun config t -> config.AndUsePolicyInjectionFor(t))

    let namedMappings = types
                        |> Seq.map(fun t -> t, getMapAsName(t))
                        |> Seq.choose(fun (t,mapName) -> match mapName with
                                                        | Some mapName -> Some (t, mapName)
                                                        | None -> None)
    (config,namedMappings)
    ||> Seq.fold(fun config namedMapping -> config.AndUseNamedMappingFor(namedMapping))

    config

//    let registerLifetimeManagers((source:Type seq), config) =
//        let mapMethodCallsite = typeof<AutomapperConfig>.GetMethod("AndMapWithLifetimeManager").GetGenericMethodDefinition()
//        for (lifetimeManager,items) in source |> Seq.collect(fun t -> t.GetCustomAttributes(typeof(CustomLifetimeManagerAttribute), false) |> Seq.map(fun att -> t, att :?> CustomLifetimeManagerAttribute))
//                                              |> Seq.groupBy snd
//                                              do            
//            if not (typeof(LifetimeManager).IsAssignableFrom(lifetimeManager.LifetimeManagerType)) then
//                raise InvalidOperationException(sprintf "The type %s has been marked with the type %s as a Lifetime Manager; lifetime managers must derive from LifetimeManager.", lifetimeManagerGrouping.FirstOrDefault().Source.Name, lifetimeManager.LifetimeManagerType.FullName));
//
//            try
//                mapMethodCallsite.MakeGenericMethod(lifetimeManagerGrouping.Key.LifetimeManagerType)
//                                    .Invoke(configuration, new[] { lifetimeManagerGrouping.Select(x => x.Source).ToArray() });
//            catch (Exception ex)
//                throw ex.InnerException;    