namespace Lydian.Unity.Automapper

open System

/// Specifies mapping behaviours to guide the automapping process.
[<Flags>]
type MappingBehaviors =
    /// No custom behaviours are specified.
    | None = 0
    /// If two types are mapped to the same interface, even if you do not specify the Multimapping on the source interface, multimap behaviour will be used.
    | MultimapByDefault = 1
    /// If an interface is multimapped, an extra registration will be made of the generic IEnumerable of T, allowing you to easily retrieve all registrations for the interface.
    | CollectionRegistration = 2

/// Contains any options that can be used to help guide the auto-mapping process.
type MappingOptions = {
    /// Any custom behaviors to use when mapping.
    Behaviors : MappingBehaviors
}

/// The exception raised when a concrete type has attempted to be mapped to an interface that is already registered into Unity.
type DuplicateMappingException private (mapFromType:Type, mapToType:Type, existingToType:Type, message:string, inner) =
    inherit Exception(message, inner)

    /// The concrete that failed to map in as either the interface or the mapping name it was mapping to has already been registered into Unity.
    member x.DuplicateMappingConcrete = mapToType
    /// The concrete that has already been mapped into Unity against the interface.
    member x.MappedConcrete = existingToType
    /// The interface that the concrete was attempting to map against and has already been registered into Unity.
    member x.MappingInterface = mapFromType

    internal new (mapFromType:Type, mapToType:Type, existingToType:Type, mappingName) =
        DuplicateMappingException(mapFromType, mapToType, existingToType, sprintf "Attempted to map at least two concrete types (%s and %s) with the same name ('%s')." existingToType.FullName mapToType.FullName mappingName, null)

    internal new (mapFromType:Type, mapToType:Type, existingToType:Type) =
        DuplicateMappingException(mapFromType, mapToType, existingToType, sprintf "Attempted to map at least two concrete types (%s and %s) to the same interface (%s)." existingToType.FullName mapToType.FullName mapFromType.FullName, null)
   
open Microsoft.Practices.Unity
open System.Collections.Generic

/// A facade on top of a Unity call to ResolveAll for a particular type.
type internal UnityCollectionFacade<'a>(targetContainer : IUnityContainer) = 
    
    let collection = 
        seq { 
            for resolvedType in targetContainer.ResolveAll<'a>() do
                yield resolvedType
        }
    
    interface IEnumerable<'a> with
        member this.GetEnumerator() = collection.GetEnumerator()
    
    interface System.Collections.IEnumerable with
        member this.GetEnumerator() = collection.GetEnumerator() :> _