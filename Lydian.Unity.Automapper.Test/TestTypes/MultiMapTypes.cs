using System;

namespace Lydian.Unity.Automapper.Test.Types
{
	[Multimap]
	public interface IMultiInterface { }
	public class ConcreteOneMulti : IMultiInterface { }
	public class ConcreteTwoMulti : IMultiInterface { }
	public class GenericMulti<T> : IMultiInterface { }
	public class ClosedGenericMulti : GenericMulti<Boolean> { }

	[MapAs("Test")]
	public class NamedMultiConcrete : IMultiInterface { }
	[MapAs("Test")]
	public class NamedMultiConcreteTwo : IMultiInterface{ }


	[Multimap]
	public interface IMultiGeneric<T> { }
	public class MultiGenericConcrete<T> : IMultiGeneric<T> { }
	public class MultiGenericConcreteTwo<T> : IMultiGeneric<T> { }
	public class MultiGenericClosedConcrete : IMultiGeneric<String> { }
}
