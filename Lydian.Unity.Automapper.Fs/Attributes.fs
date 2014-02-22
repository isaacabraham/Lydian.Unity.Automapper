namespace Lydian.Unity.Automapper

open Microsoft.Practices.Unity
open System

/// Marks an interface as using a specific type of lifetime manager for the purposes of registration into Unity when auto-mapping.
type CustomLifetimeManagerAttribute(lifetimeManagerType : Type) = 
    inherit Attribute()
    /// The type of lifetime manager specified.
    member x.LifetimeManagerType with get () = lifetimeManagerType

/// Marks an interface or concrete class to explicitly be ignored by the auto-mapper.
[<AttributeUsage(AttributeTargets.Interface ||| AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
[<Sealed>]
type DoNotMapAttribute() = 
    inherit Attribute()

/// Specifies the name of the mapping that this concrete should be registered into Unity as. For multimaps, if this attribute is not specified, the full name of the type is used.
[<AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)>]
[<Sealed>]
type MapAsAttribute(mappingName : string) = 
    inherit Attribute()
    /// The name of the mapping.
    member x.MappingName with get () = mappingName

/// Marks an interface as a multi-map i.e. many concrete types can be mapped to this interface. Each registration into Unity will be named based on the full name of the concrete type.
[<AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)>]
[<Sealed>]
type MultimapAttribute() = 
    inherit Attribute()

/// Specifies that this interface should be registered to take part in policy injection. This is the same as manually applying a Unity type registration with both the PolicyInjection InjectionBehaviour and the Interface Interceptor.
[<AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)>]
[<Sealed>]
type PolicyInjectionAttribute() = 
    inherit Attribute()

/// Marks an interface as being a singleton for the purposes of registration into Unity when auto-mapping.
[<AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)>]
[<Sealed>]
type SingletonAttribute() = 
    inherit CustomLifetimeManagerAttribute(typeof<ContainerControlledLifetimeManager>)