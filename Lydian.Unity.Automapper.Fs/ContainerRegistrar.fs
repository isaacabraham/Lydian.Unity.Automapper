module internal Lydian.Unity.Automapper.ContainerRegistrar

open System
open System.Reflection
open Lydian.Unity.Automapper
open Microsoft.Practices.Unity
open Microsoft.Practices.Unity.InterceptionExtension

let private getInjectionMembers (configuration:AutomapperConfigData) (mapFrom, mapTo) =
    let hasHandlerAttribute (theType:Type) =
        let memberInfoHasHandlerAttribute (mi:MemberInfo) = mi.GetCustomAttributes(typeof<HandlerAttribute>, false) |> Seq.length > 0
        theType |> memberInfoHasHandlerAttribute || theType.GetMethods() |> Seq.exists memberInfoHasHandlerAttribute
    let requiresPolicyInjection = mapFrom |> configuration.IsMarkedForPolicyInjection
                                || mapFrom |> hasHandlerAttribute
                                || mapTo |> hasHandlerAttribute

    if requiresPolicyInjection then
        [| Interceptor<InterfaceInterceptor>() :> InjectionMember;
            InterceptionBehavior<PolicyInjectionBehavior>() :> InjectionMember |]
    else
        Array.empty<InjectionMember>

let private getLifetimeManager (configuration:AutomapperConfigData) mappingType =
    match mappingType |> configuration.IsMarkedWithCustomLifetimeManager with
    | Some lifetime -> lifetime
    | None -> TransientLifetimeManager() :> LifetimeManager

let private getRegistrationName (configuration:AutomapperConfigData) (behaviors:MappingBehaviors) multimapTypes (mapFrom, mapTo) =
    let namedMappingRequested = mapFrom |> configuration.IsMultimap || mapTo |> configuration.IsNamedMapping
    let namedMappingRequired = behaviors.HasFlag(MappingBehaviors.MultimapByDefault) && multimapTypes |> Seq.exists ((=) mapFrom)
    if (namedMappingRequested || namedMappingRequired)
    then Some ((mapFrom,mapTo) |> configuration.GetNamedMapping)
    else None

let registerTypes ((container:IUnityContainer),(mappings:(Type*Type) seq),(configuration:AutomapperConfigData),behaviors) =
    let multimapTypes = mappings
                        |> Seq.groupBy fst
                        |> Seq.filter(fun grp -> (snd grp) |> Seq.length > 1)
                        |> Seq.map fst
                        |> Seq.toArray

    for (mapFrom,mapTo) in mappings do
        let lifetime = mapFrom |> getLifetimeManager configuration
        let injectionMembers = (mapFrom,mapTo) |> getInjectionMembers configuration
        let registrationName = (mapFrom,mapTo) |> getRegistrationName configuration behaviors multimapTypes
        match registrationName with
        | Some registrationName -> container.RegisterType(mapFrom, mapTo, registrationName, lifetime, injectionMembers)
        | None -> container.RegisterType(mapFrom, mapTo, lifetime, injectionMembers)
        |> ignore