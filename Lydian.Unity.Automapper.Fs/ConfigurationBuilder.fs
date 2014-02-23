module internal Lydian.Unity.Automapper.ConfigurationBuilder

open Microsoft.Practices.Unity
open System

let private hasAttribute<'a> (t : Type) = t.GetCustomAttributes(typeof<'a>, false)
                                          |> Seq.length > 0
let private getTypesWith<'a> = Seq.filter hasAttribute<'a> >> Seq.toList

let private getMapAsName (t : Type) = 
    let mapAsAttribute = 
        t.GetCustomAttributes(typeof<MapAsAttribute>, false)
        |> Seq.map (fun att -> att :?> MapAsAttribute)
        |> Seq.tryFind (fun t -> true)
    match mapAsAttribute with
    | Some mapAsAttribute -> Some(t, mapAsAttribute.MappingName)
    | None -> None

let private buildConfigurationFromAttributes types = 
    { DoNotMapTypes = types |> getTypesWith<DoNotMapAttribute>
      MultimapTypes = types |> getTypesWith<MultimapAttribute>
      PolicyInjectionTypes = types |> getTypesWith<PolicyInjectionAttribute>
      ExplicitNamedMappings = types
                              |> Seq.choose getMapAsName
                              |> Seq.toList
      CustomLifetimeManagerTypes = types
                                   |> Seq.collect 
                                           (fun t -> t.GetCustomAttributes(typeof<CustomLifetimeManagerAttribute>, false) 
                                                     |> Seq.map (fun att -> t, (att :?> CustomLifetimeManagerAttribute).LifetimeManagerType))
                                   |> Seq.toList }

let private buildConfigurationFromProviders types = 
    types
    |> Seq.filter typeof<IAutomapperConfigProvider>.IsAssignableFrom
    |> Seq.map Activator.CreateInstance
    |> Seq.cast<IAutomapperConfigProvider>
    |> Seq.map (fun p -> p.CreateConfiguration().Data)
    |> Seq.toList

let buildConfiguration types = 
    buildConfigurationFromAttributes types :: buildConfigurationFromProviders types
    |> Seq.reduce (fun output current -> 
           { CustomLifetimeManagerTypes = output.CustomLifetimeManagerTypes @ current.CustomLifetimeManagerTypes
             DoNotMapTypes = output.DoNotMapTypes @ current.DoNotMapTypes
             ExplicitNamedMappings = output.ExplicitNamedMappings @ current.ExplicitNamedMappings
             MultimapTypes = output.MultimapTypes @ current.MultimapTypes
             PolicyInjectionTypes = output.PolicyInjectionTypes @ current.PolicyInjectionTypes })