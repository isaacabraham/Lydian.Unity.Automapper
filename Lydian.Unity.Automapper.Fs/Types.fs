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
