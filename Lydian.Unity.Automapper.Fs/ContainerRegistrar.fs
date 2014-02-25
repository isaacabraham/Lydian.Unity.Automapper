/// Validates and enters mappings into the Unity container.
module internal Lydian.Unity.Automapper.Core.ContainerRegistrar

open Lydian.Unity.Automapper
open Lydian.Unity.Automapper.Core.MappingValidator
open Microsoft.Practices.Unity
open Microsoft.Practices.Unity.InterceptionExtension
open System
open System.Reflection

let private getInjectionMembers (configuration : AutomapperConfigData) (mapFrom, mapTo) = 
    let hasHandlerAttribute (theType : Type) = 
        let memberInfoHasHandlerAttribute (mi : MemberInfo) = mi.GetCustomAttributes(typeof<HandlerAttribute>, false)
                                                              |> Seq.length > 0
        theType
        |> memberInfoHasHandlerAttribute || theType.GetMethods() |> Seq.exists memberInfoHasHandlerAttribute
    
    let requiresPolicyInjection = 
        mapFrom
        |> configuration.IsMarkedForPolicyInjection || mapFrom |> hasHandlerAttribute || mapTo |> hasHandlerAttribute
    if requiresPolicyInjection then 
        [| Interceptor<InterfaceInterceptor>() :> InjectionMember
           InterceptionBehavior<PolicyInjectionBehavior>() :> InjectionMember |]
    else Array.empty<InjectionMember>

let private getLifetimeManager (configuration : AutomapperConfigData) mappingType = 
    match mappingType |> configuration.IsMarkedWithCustomLifetimeManager with
    | Some lifetime -> lifetime
    | None -> TransientLifetimeManager() :> LifetimeManager

let private getRegistrationName mappings = 
    let multimapTypes = 
        mappings
        |> Seq.groupBy fst
        |> Seq.filter (fun grp -> (snd grp)
                                  |> Seq.length > 1)
        |> Seq.map fst
        |> Seq.toArray
    fun (configuration : AutomapperConfigData) (behaviors : MappingBehaviors) (mapFrom, mapTo) -> 
        let namedMappingRequested = mapFrom
                                    |> configuration.IsMultimap || mapTo |> configuration.IsNamedMapping
        let namedMappingRequired = 
            behaviors.HasFlag(MappingBehaviors.MultimapByDefault) && multimapTypes |> Seq.exists ((=) mapFrom)
        if (namedMappingRequested || namedMappingRequired) then Some((mapFrom, mapTo) |> configuration.GetNamedMapping)
        else None

let private setInterceptionIfRequired mappingDetails (container : IUnityContainer) = 
    if mappingDetails 
       |> Seq.exists (fun (_, _, _, injectionMembers : InjectionMember [], _) -> injectionMembers.Length > 0) then 
        container.AddNewExtension<Interception>() |> ignore

/// Registers the supplied set of mappings into the container using the provided configuration and behaviors.
let registerMappings (container : IUnityContainer, mappings, configuration, behaviors) = 
    let getRegistrationName = getRegistrationName mappings
    
    let mappingDetails = 
        mappings
        |> Seq.map (fun mapping -> 
               let mapFrom, mapTo = mapping
               let lifetime = mapFrom |> getLifetimeManager configuration
               let injectionMembers = mapping |> getInjectionMembers configuration
               let registrationName = mapping |> getRegistrationName configuration behaviors
               mapFrom, mapTo, lifetime, injectionMembers, registrationName)
        |> Seq.cache
    
    let validateMapping = validateMapping configuration container behaviors
    for mappingDetail in mappingDetails do
        let mapFrom, mapTo, lifetime, injectionMembers, registrationName = mappingDetail
        validateMapping (mapFrom, mapTo)
        match registrationName with
        | Some registrationName -> container.RegisterType(mapFrom, mapTo, registrationName, lifetime, injectionMembers)
        | None -> container.RegisterType(mapFrom, mapTo, lifetime, injectionMembers)
        |> ignore
    container |> setInterceptionIfRequired (mappingDetails)
