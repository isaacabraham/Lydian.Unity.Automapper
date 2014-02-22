namespace Lydian.Unity.Automapper.Fs

open System

type AutomapperConfig = { DoNotMapTypes : Type seq
                          ExplicitNamedMappings : (Type * string) seq
                          MultimapTypes : Type seq
                          PolicyInjectionTypes : Type seq
                          CustomLifetimeManagerTypes : (Type * Type) seq }
                          member internal this.IsMappable testType = not <| (this.DoNotMapTypes |> Seq.exists ((=) testType))
                          member internal this.IsMultimap (testType:Type) =
                            let testType = if testType.IsGenericType then testType.GetGenericTypeDefinition() else testType
                            this.MultimapTypes |> Seq.exists ((=) testType)
