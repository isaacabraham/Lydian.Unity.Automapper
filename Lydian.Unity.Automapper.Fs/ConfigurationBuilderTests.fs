module ``Configuration Builder Tests``

open Lydian.Unity.Automapper
open Lydian.Unity.Automapper.Core.ConfigurationBuilder
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
let ``A single configuration provider is identified``() = 
    let provider = 
        { new IAutomapperConfigProvider with
              member this.CreateConfiguration() = 
                  AutomapperConfig.Create()
                                  .AndDoNotMapFor(typeof<string>)
                                  .AndMapAsSingleton(typeof<int>)
                                  .AndMapWithLifetimeManager<HierarchicalLifetimeManager>(typeof<DateTime>)
                                  .AndUseMultimappingFor(typeof<Exception>)
                                  .AndUseNamedMappingFor(typeof<Uri>, "NAME")
                                  .AndUsePolicyInjectionFor(typeof<IAsyncResult>) }
    
    let config = buildConfiguration [ provider.GetType() ]
    test <@ config.CustomLifetimeManagerTypes = [ typeof<int>, typeof<ContainerControlledLifetimeManager>
                                                  typeof<DateTime>, typeof<HierarchicalLifetimeManager> ] @>
    test <@ config.DoNotMapTypes = [ typeof<string> ] @>
    test <@ config.MultimapTypes = [ typeof<Exception> ] @>
    test <@ config.ExplicitNamedMappings = [ typeof<Uri>, "NAME" ] @>
    test <@ config.PolicyInjectionTypes = [ typeof<IAsyncResult> ] @>

[<Fact>]
let ``Two configuration providers are merged together``() =
    let providerA = 
        { new IAutomapperConfigProvider with
              member this.CreateConfiguration() = 
                  AutomapperConfig.Create()
                                  .AndDoNotMapFor(typeof<int>)
                                  .AndMapAsSingleton(typeof<int>)
                                  .AndMapWithLifetimeManager<HierarchicalLifetimeManager>(typeof<DateTime>)
                                  .AndUseMultimappingFor(typeof<int>)
                                  .AndUseNamedMappingFor(typeof<int>, "NAME")
                                  .AndUsePolicyInjectionFor(typeof<int>) }
    let providerB = 
        { new IAutomapperConfigProvider with
              member this.CreateConfiguration() = 
                  AutomapperConfig.Create()
                                  .AndDoNotMapFor(typeof<string>)
                                  .AndMapAsSingleton(typeof<string>)
                                  .AndMapWithLifetimeManager<HierarchicalLifetimeManager>(typeof<obj>)
                                  .AndUseMultimappingFor(typeof<string>)
                                  .AndUseNamedMappingFor(typeof<string>, "NAME")
                                  .AndUsePolicyInjectionFor(typeof<string>) }

    let config = buildConfiguration [ providerA.GetType(); providerB.GetType() ]
    test <@ config.CustomLifetimeManagerTypes = [ typeof<int>, typeof<ContainerControlledLifetimeManager>
                                                  typeof<DateTime>, typeof<HierarchicalLifetimeManager>
                                                  typeof<string>, typeof<ContainerControlledLifetimeManager>
                                                  typeof<obj>, typeof<HierarchicalLifetimeManager> ] @>
                                                  
    test <@ config.DoNotMapTypes = [ typeof<int>; typeof<string> ] @>
    test <@ config.MultimapTypes = [ typeof<int>; typeof<string> ] @>
    test <@ config.ExplicitNamedMappings = [ typeof<int>, "NAME"; typeof<string>, "NAME" ] @>
    test <@ config.PolicyInjectionTypes = [ typeof<int>; typeof<string> ] @>

[<DoNotMap>]
[<Singleton>]
[<CustomLifetimeManager(typeof<HierarchicalLifetimeManager>)>]
[<MapAs("TEST")>]
[<Multimap>]
[<PolicyInjection>]
type TestType() = class end

[<Fact>]
let ``A single type with attributes is identified``() =
    let config = buildConfiguration [ typeof<TestType> ]
    test <@ config.CustomLifetimeManagerTypes = [ typeof<TestType>, typeof<ContainerControlledLifetimeManager>
                                                  typeof<TestType>, typeof<HierarchicalLifetimeManager> ] @>                                                  
    test <@ config.DoNotMapTypes = [ typeof<TestType> ] @>
    test <@ config.MultimapTypes = [ typeof<TestType> ] @>
    test <@ config.ExplicitNamedMappings = [ typeof<TestType>, "TEST" ] @>
    test <@ config.PolicyInjectionTypes = [ typeof<TestType> ] @>

[<Fact>]
let ``A type with attributes is merged with provider config``() =
    let provider = { new IAutomapperConfigProvider with member this.CreateConfiguration() = AutomapperConfig.Create().AndDoNotMapFor(typeof<string>) }
    let config = buildConfiguration [ typeof<TestType>; provider.GetType() ]

    test <@ config.DoNotMapTypes = [ typeof<TestType>; typeof<string> ] @>