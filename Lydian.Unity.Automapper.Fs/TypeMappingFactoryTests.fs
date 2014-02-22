module Lydian.Unity.Automapper.Fs.TypeMappingFactoryTests      

open FsUnit.Xunit
open FsUnit.CustomMatchers
open Lydian.Unity.Automapper.Fs.TypeMappingFactory
open System
open System.Collections.Generic
open Swensen.Unquote.Assertions
open Xunit

type IInterface = interface end
type IOther = interface end
type IGenericInterface<'T1, 'T2> = interface end

type InterfaceImplementation() = interface IInterface
type InterfaceImplementationTwo() = interface IInterface
type OtherImplementation() = interface IOther
type CompoundImplementation() = interface IInterface interface IOther
type OpenGenericImplementation<'T>() = interface IInterface
type ClosedGenericImplementation() = inherit OpenGenericImplementation<bool>()
type OpenGenericConcrete<'T1, 'T2>() = interface IGenericInterface<'T1, 'T2>
type ClosedGenericConcrete() = interface IGenericInterface<string, bool>
type OpenGenericConcreteTwo<'T1, 'T2>() = interface IGenericInterface<'T1, 'T2>

let private assertMapping fromType toType shouldExist mappings =
    let mapping = mappings |> Seq.filter(fun mapping -> fst mapping = fromType)
                           |> Seq.filter(fun mapping -> snd mapping = toType)
                           |> Seq.exists(fun t -> true)

    test <@ mapping = shouldExist @>

let private assertMappingExists<'TFrom, 'TTo> = assertMapping typeof<'TFrom> typeof<'TTo> true
let private assertMappingDoesNotExist<'TFrom, 'TTo> = assertMapping typeof<'TFrom> typeof<'TTo> false

let defaultConfig = { DoNotMapTypes = []; ExplicitNamedMappings = []; MultimapTypes = []; PolicyInjectionTypes = []; CustomLifetimeManagerTypes = [] }

[<Fact>]
let ``Single concrete to an interface``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typeof<IInterface>; typeof<InterfaceImplementation> ]);

    // Assert
    mappings |> assertMappingExists<IInterface, InterfaceImplementation>
    Seq.length mappings |> should equal 1

[<Fact>]
let ``Many concretes to a single interface``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typeof<IInterface>; typeof<InterfaceImplementation>; typeof<InterfaceImplementationTwo> ])

    // Assert
    mappings |> assertMappingExists<IInterface, InterfaceImplementation>
    mappings |> assertMappingExists<IInterface, InterfaceImplementationTwo>
    Seq.length mappings |> should equal 2

[<Fact>]
let ``No concrete returns no mappings``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [typeof<IInterface>])

    // Assert
    Seq.length mappings |> should equal 0

[<Fact>]
let ``No matching concrete returns no mappings``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typeof<IInterface>; typeof<String> ])

    // Assert
    Seq.length mappings |> should equal 0

[<Fact>]
let ``One interface has a match and another does not``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typeof<IInterface>; typeof<InterfaceImplementation>; typeof<IOther> ])

    // Assert
    mappings |> assertMappingExists<IInterface, InterfaceImplementation>
    Seq.length mappings |> should equal 1

[<Fact>]
let ``Two interfaces each with a concrete is matched``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typeof<IInterface>; typeof<InterfaceImplementation>; typeof<IOther>; typeof<OtherImplementation> ])

    // Assert
    mappings |> assertMappingExists<IInterface, InterfaceImplementation>
    mappings |> assertMappingExists<IOther, OtherImplementation>
    Seq.length mappings |> should equal 2

[<Fact>]
let ``One concrete implementing two interfaces``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typeof<IInterface>; typeof<CompoundImplementation>; typeof<IOther>] )

    // Assert
    mappings |> assertMappingExists<IInterface, CompoundImplementation>
    mappings |> assertMappingExists<IOther, CompoundImplementation>
    Seq.length mappings |> should equal 2

[<Fact>]
let ``A concrete marked with DoNotMap is not mapped``() =
    let config = { defaultConfig with DoNotMapTypes = [ typeof<InterfaceImplementation> ] }

    // Act
    let mappings = createMappings(MappingBehaviors.None, config, [ typeof<IInterface>; typeof<InterfaceImplementation> ])

    // Assert
    mappings |> assertMappingDoesNotExist<IInterface, InterfaceImplementation>

[<Fact>]
let CreateMappings_InterfaceIsDoNotMap_DoesNotMap() =
    let config = { defaultConfig with DoNotMapTypes = [ typeof<IInterface> ] }

    // Act
    let mappings = createMappings(MappingBehaviors.None, config, [ typeof<IInterface>; typeof<InterfaceImplementation> ])

    // Assert
    mappings |> assertMappingDoesNotExist<IInterface, InterfaceImplementation>

[<Fact>]
let ``Open generic concrete can implement a non-generic interface``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typeof<IInterface>; typeof<OpenGenericImplementation<Boolean>> ])

    // Assert
    mappings |> assertMappingExists<IInterface, OpenGenericImplementation<Boolean>>

[<Fact>]
let ``Closed generic concrete can implement a non-generic interface``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typeof<IInterface>; typeof<ClosedGenericImplementation> ])

    // Assert
    mappings |> assertMappingExists<IInterface, ClosedGenericImplementation>

[<Fact>]
let ``Generic interface is mapped to an open generic concrete``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typedefof<IGenericInterface<_,_>>; typedefof<OpenGenericConcrete<_,_>> ])

    // Assert
    mappings |> assertMapping (typedefof<IGenericInterface<_,_>>) (typedefof<OpenGenericConcrete<_,_>>) true

[<Fact>]
let ``Generic interface is mapped to a closed generic concrete``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [typedefof<IGenericInterface<_,_>>; typeof<ClosedGenericConcrete> ])

    // Assert
    mappings |> assertMappingExists<IGenericInterface<String, Boolean>, ClosedGenericConcrete>

[<Fact>]
let ``Generic interface is mapped to open and closed concretes simultaneously``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typedefof<IGenericInterface<_,_>>; typedefof<OpenGenericConcrete<_,_>>; typeof<ClosedGenericConcrete> ])

    // Assert
    mappings |> assertMapping typedefof<IGenericInterface<_,_>> typedefof<OpenGenericConcrete<_,_>> true
    mappings |> assertMappingExists<IGenericInterface<string, bool>, ClosedGenericConcrete>

[<Fact>]
let ``Generic interface has multiple open concrete implementations map them all``() =
    // Act
    let mappings = createMappings(MappingBehaviors.None, defaultConfig, [ typedefof<IGenericInterface<_,_>>; typedefof<OpenGenericConcrete<_,_>>; typedefof<OpenGenericConcreteTwo<_,_>> ])

    // Assert
    mappings |> assertMapping typedefof<IGenericInterface<_,_>> typedefof<OpenGenericConcrete<_,_>> true
    mappings |> assertMapping typedefof<IGenericInterface<_,_>> typedefof<OpenGenericConcreteTwo<_,_>> true
    Seq.length mappings |> should equal 2

[<Fact>]
let ``ACR with multiple mappings creates a collection``() =
    // Act
    let mappings = createMappings(MappingBehaviors.CollectionRegistration, defaultConfig, [ typeof<IInterface>; typeof<InterfaceImplementation>; typeof<InterfaceImplementationTwo> ])

    // Assert
    mappings |> assertMappingExists<IEnumerable<IInterface>, UnityCollectionFacade<IInterface>>

[<Fact>]
let ``ACR is not enabled for a single concrete if it does not have multimap specified in config``() =
    // Act
    let mappings = createMappings(MappingBehaviors.CollectionRegistration, defaultConfig, [ typeof<IInterface>; typeof<InterfaceImplementation> ])

    // Assert
    mappings |> assertMappingDoesNotExist<IEnumerable<IInterface>, UnityCollectionFacade<IInterface>>

[<Fact>]
let ``ACR is enabled for a single concrete when it has multimap specified in config``() =
    let config = { defaultConfig with MultimapTypes = [ typeof<IInterface> ] }

    // Act
    let mappings = createMappings(MappingBehaviors.CollectionRegistration, config, [ typeof<IInterface>; typeof<InterfaceImplementation> ])

    // Assert
    mappings |> assertMappingExists<IEnumerable<IInterface>, UnityCollectionFacade<IInterface>>

[<Fact>]
let ``ACR and many single mappings across different types does not create a collection``() =
    // Act
    let mappings = createMappings(MappingBehaviors.CollectionRegistration, defaultConfig, [ typeof<IInterface>; typeof<InterfaceImplementation>; typeof<IOther>; typeof<OtherImplementation> ])

    // Assert
    mappings |> assertMappingDoesNotExist<IEnumerable<IInterface>, UnityCollectionFacade<IInterface>>
    mappings |> assertMappingDoesNotExist<IEnumerable<IOther>, UnityCollectionFacade<IOther>>