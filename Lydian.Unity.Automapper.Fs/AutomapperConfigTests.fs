module ``Automapper Config Tests``

open FsUnit.CustomMatchers
open FsUnit.Xunit
open Lydian.Unity.Automapper
open Lydian.Unity.Automapper.TypeMappingFactory
open Microsoft.Practices.Unity
open Swensen.Unquote.Assertions
open System
open System.Collections.Generic
open Xunit

let internal defaultConfig = { DoNotMapTypes = []
                               ExplicitNamedMappings = []
                               MultimapTypes = []
                               PolicyInjectionTypes = [] 
                               CustomLifetimeManagerTypes = [] }

[<Fact>]
let ``Mappable types are correctly identified``() =
    let config = { defaultConfig with DoNotMapTypes = [ typeof<string> ] }

    test <@ config.IsMappable typeof<int> @>
    test <@ not <| config.IsMappable typeof<string> @>

[<Fact>]
let ``Multimapped types are correctly identified``() =
    let config = { defaultConfig with MultimapTypes = [ typeof<string>; typedefof<IEnumerable<_>> ] }

    test <@ config.IsMultimap typeof<string> @>
    test <@ config.IsMultimap typeof<IEnumerable<int>> @>
    test <@ not <| config.IsMultimap typeof<int> @>

[<Fact>]
let ``Lifetime managed types are correctly identified``() =
    let config = { defaultConfig with CustomLifetimeManagerTypes = [ (typeof<string>, typeof<ContainerControlledLifetimeManager>) ] }
    
    let customManager = config.IsMarkedWithCustomLifetimeManager typeof<string>
    test <@ customManager.IsSome @>
    test <@ customManager.Value.GetType() = typeof<ContainerControlledLifetimeManager> @>
    test <@ (config.IsMarkedWithCustomLifetimeManager typeof<int>).IsNone @>

[<Fact>]
let ``Named mappings are correctly identified``() =
    let config = { defaultConfig with ExplicitNamedMappings = [ typeof<string>, "TEST" ] }

    test <@ config.IsNamedMapping typeof<string> @>
    test <@ not <| config.IsNamedMapping typeof<int> @>
    test <@ config.GetNamedMapping (typeof<int>,typeof<string>) = "TEST" @>
    test <@ config.GetNamedMapping (typeof<string>,typeof<int>) = "System.Int32" @>

[<Fact>]
let ``Types marked for policy injection are correctly identified``() =
    let config = { defaultConfig with PolicyInjectionTypes = [ typeof<string> ] }

    test <@ config.IsMarkedForPolicyInjection typeof<string> @>
    test <@ not <| config.IsMarkedForPolicyInjection typeof<int> @>

[<Fact>]
let ``AndDoNotMapFor correctly merges blacklist``() =
    let config = (AutomapperConfig { defaultConfig with DoNotMapTypes = [ typeof<string> ] })
                    .AndDoNotMapFor typeof<int>

    test <@ Seq.length config.Data.DoNotMapTypes = 2 @>

[<Fact>]
let ``AndUseNamedMappingsFor correctly merges list``() =
    let config = (AutomapperConfig { defaultConfig with ExplicitNamedMappings = [ typeof<string>, "TEST" ] })
                    .AndUseNamedMappingFor (typeof<int>, "OTHER")

    test <@ Seq.length config.Data.ExplicitNamedMappings = 2 @>

[<Fact>]
let ``AndUseMultimappingFor correctly merges list``() =
    let config = (AutomapperConfig { defaultConfig with MultimapTypes = [ typeof<string> ] })
                    .AndUseMultimappingFor typeof<int>

    test <@ Seq.length config.Data.MultimapTypes = 2 @>

[<Fact>]
let ``AndUsePolicyInjectionFor correctly merges list``() =
    let config = (AutomapperConfig { defaultConfig with PolicyInjectionTypes = [ typeof<string> ] })
                    .AndUsePolicyInjectionFor (typeof<int>)

    test <@ Seq.length config.Data.PolicyInjectionTypes = 2 @>
   
[<Fact>]
let ``MapWithLifetimeManager correctly merges list``() =
    let config = (AutomapperConfig { defaultConfig with CustomLifetimeManagerTypes = [ typeof<string> , typeof<ContainerControlledLifetimeManager> ] })
                    .AndMapWithLifetimeManager<ContainerControlledLifetimeManager>(typeof<int>)

    test <@ Seq.length config.Data.CustomLifetimeManagerTypes = 2 @>

[<Fact>]
let ``MapWithLifetimeManager prevents adding multiple lifetime managers for the same type``() =
    let config = AutomapperConfig { defaultConfig with CustomLifetimeManagerTypes = [ typeof<string> , typeof<ContainerControlledLifetimeManager> ] }
    
    raises<InvalidOperationException> <@ config.AndMapWithLifetimeManager<HierarchicalLifetimeManager>(typeof<string>) @>

[<Fact>]
let ``MapAsSingleton uses Container Controlled Lifetime Manager``() =
    let config = AutomapperConfig.Create()
                                 .AndMapAsSingleton(typeof<int>)

    test <@ config.Data.CustomLifetimeManagerTypes.Head |> snd = typeof<ContainerControlledLifetimeManager> @>
