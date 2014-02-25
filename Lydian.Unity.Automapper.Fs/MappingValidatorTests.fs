module ``Mapping Validator Tests``

open Lydian.Unity.Automapper
open Lydian.Unity.Automapper.Core.MappingValidator
open Microsoft.Practices.Unity
open Swensen.Unquote.Assertions
open System
open Xunit

let internal defaultConfig = 
    { DoNotMapTypes = []
      ExplicitNamedMappings = []
      MultimapTypes = []
      PolicyInjectionTypes = []
      CustomLifetimeManagerTypes = [] }

let internal defaultBehaviors = MappingBehaviors.None

[<Fact>]
let ``Single mapping that already exists in the container throws an exception``() = 
    let container = new UnityContainer()
    container.RegisterType(typeof<obj>, typeof<string>) |> ignore
    raises<DuplicateMappingException>
        (<@ validateMapping (defaultConfig, container, defaultBehaviors) (typeof<obj>, typeof<int>) @>)

[<Fact>]
let ``Single mapping that does not already exist in the container does not throw an exception``() = 
    let container = new UnityContainer()
    validateMapping (defaultConfig, container, defaultBehaviors) (typeof<obj>, typeof<int>)

[<Fact>]
let ``Single mapping that already exists in the container does not throw an exception when multimapping is the default``() = 
    let container = new UnityContainer()
    container.RegisterType(typeof<obj>, typeof<string>) |> ignore
    validateMapping (defaultConfig, container, MappingBehaviors.MultimapByDefault) (typeof<obj>, typeof<int>)

[<Fact>]
let ``Single mapping that already exists in the container does not throw an exception when the type is a multimap``() = 
    let container = new UnityContainer()
    container.RegisterType(typeof<obj>, typeof<string>) |> ignore
    validateMapping ({ defaultConfig with MultimapTypes = [ typeof<obj> ] }, container, defaultBehaviors) 
        (typeof<obj>, typeof<int>)

[<Fact>]
let ``Existing named mapping with the same name but for a different type does not throw an exception``() = 
    let container = new UnityContainer()
    container.RegisterType(typeof<obj>, typeof<DateTimeOffset>, "TESTNAME") |> ignore
    validateMapping 
        ({ defaultConfig with ExplicitNamedMappings = [ typeof<DateTimeOffset>, "TESTNAME" ] }, container, 
         defaultBehaviors) (typeof<DateTime>, typeof<DateTimeOffset>)

[<Fact>]
let ``Existing named mapping with the same name for the same type throws an exception``() = 
    let container = new UnityContainer()
    container.RegisterType(typeof<obj>, typeof<string>, "TESTNAME") |> ignore
    let config = { defaultConfig with ExplicitNamedMappings = [ typeof<DateTimeOffset>, "TESTNAME" ] }
    raises<DuplicateMappingException>
        (<@ validateMapping (config, container, MappingBehaviors.MultimapByDefault) 
                (typeof<obj>, typeof<DateTimeOffset>) @>)