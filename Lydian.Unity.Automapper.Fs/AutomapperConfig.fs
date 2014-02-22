namespace Lydian.Unity.Automapper

open Microsoft.Practices.Unity
open System

type internal AutomapperConfigData = 
    { DoNotMapTypes : Type list
      ExplicitNamedMappings : (Type * string) list
      MultimapTypes : Type list
      PolicyInjectionTypes : Type list
      CustomLifetimeManagerTypes : (Type * Type) list }
    member this.IsMappable(theType : Type) = this.DoNotMapTypes |> List.forall ((<>) theType)
    
    member this.IsMultimap(theType : Type) = 
        let theType = 
            if theType.IsGenericType then theType.GetGenericTypeDefinition()
            else theType
        this.MultimapTypes |> List.exists ((=) theType)
    
    member this.IsMarkedWithCustomLifetimeManager theType = 
        match this.CustomLifetimeManagerTypes |> Seq.tryFind ((fst) >> ((=) theType)) with
        | Some(_, lifetimeManager) -> Some(Activator.CreateInstance(lifetimeManager) :?> LifetimeManager)
        | None -> None
    
    member this.IsNamedMapping theType = this.ExplicitNamedMappings |> Seq.exists ((fst) >> ((=) theType))
    member this.IsMarkedForPolicyInjection theType = this.PolicyInjectionTypes |> Seq.exists ((=) theType)
    member this.GetNamedMapping mapping = 
        match this.ExplicitNamedMappings |> Seq.tryFind ((fst) >> (=) (snd mapping)) with
        | Some mapping -> snd mapping
        | None -> (snd mapping).FullName

/// Represents a set of configuration instructions that guide the Automapper regarding mapping of specific types such as whether to register as a singleton, use policy injection or multimapping etc.
type AutomapperConfig private (data : AutomapperConfigData) = 
    /// Creates a new AutomapperConfig that can be composed using chained fluent-API style methods.
    static member Create() = 
        AutomapperConfig { DoNotMapTypes = []
                           ExplicitNamedMappings = []
                           MultimapTypes = []
                           PolicyInjectionTypes = []
                           CustomLifetimeManagerTypes = [] }
    
    /// <summary>
    /// Indicates that the provided types should not participate in automapping.
    /// </summary>
    /// <param name="types">The set of types to ignore.</param>
    /// <returns></returns>
    member x.AndDoNotMapFor([<ParamArray>] types) = 
        AutomapperConfig { data with DoNotMapTypes = data.DoNotMapTypes @ (types |> List.ofArray) }
    
    /// <summary>
    /// Indicates that the provided type should be mapped using a specific name.
    /// </summary>
    /// <param name="type">The concrete type to map.</param>
    /// <param name="mappingName">The name to use for the mapping.</param>
    /// <returns></returns>
    member x.AndUseNamedMappingFor(theType, mappingName) = 
        AutomapperConfig { data with ExplicitNamedMappings = (theType, mappingName) :: data.ExplicitNamedMappings }
    
    /// <summary>
    /// Indicates that the specified interfaces should use multimapping if many concretes are found for them.
    /// </summary>
    /// <param name="types">The set of interfaces to register as potential multimaps.</param>
    /// <returns></returns>
    member x.AndUseMultimappingFor([<ParamArray>] types) = 
        AutomapperConfig { data with MultimapTypes = data.MultimapTypes @ (types |> List.ofArray) }
    
    /// <summary>
    /// Indicates that the specified types should partake in policy injection.
    /// </summary>
    /// <param name="types">The set of types to register with policy injection.</param>
    /// <returns></returns>
    member x.AndUsePolicyInjectionFor([<ParamArray>] types) = 
        AutomapperConfig { data with PolicyInjectionTypes = data.PolicyInjectionTypes @ (types |> List.ofArray) }
    
    /// <summary>
    /// Indicates that the specified types should be registered using the specified lifetime manager.
    /// </summary>
    /// <param name="types">The set of types to register.</param>
    /// <returns></returns>
    member x.AndMapWithLifetimeManager<'TLifetimePolicy when 'TLifetimePolicy :> LifetimeManager>([<ParamArray>] types : Type []) = 
        match types |> Seq.tryFind (fun t -> data.IsMarkedWithCustomLifetimeManager(t).IsSome) with
        | Some duplicateLifetimeManager -> 
            raise 
                (InvalidOperationException
                     (sprintf "The type %s has multiple lifetime managers specified." duplicateLifetimeManager.Name))
        | None -> 
            let lifetimeManagerType = typeof<'TLifetimePolicy>
            AutomapperConfig { data with CustomLifetimeManagerTypes = 
                                             data.CustomLifetimeManagerTypes @ (types
                                                                                |> Seq.map 
                                                                                       (fun theType -> 
                                                                                       theType, lifetimeManagerType)
                                                                                |> Seq.toList) }
    
    /// <summary>
    /// Indicates that the specified types should be registered as singletons, that is using the ContainerControlledLifetimeManager.
    /// </summary>
    /// <param name="types">The set of types to register as singletons.</param>
    /// <returns></returns>
    member x.AndMapAsSingleton([<ParamArray>] types) = 
        x.AndMapWithLifetimeManager<ContainerControlledLifetimeManager>(types)

/// Represents a provider of AutomapperConfig. Implement this interface if you wish to register type-specific mapping guidance instead of (or addition to) using attribute-based configuration on types directly.
type IAutomapperConfigProvider =
    /// Returns an instance of the AutomapperConfig to use for configuration.
    abstract member CreateConfiguration : unit -> AutomapperConfig