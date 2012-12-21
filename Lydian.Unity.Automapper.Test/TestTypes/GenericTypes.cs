using System;
using System.Collections;
using System.Collections.Generic;

namespace Lydian.Unity.Automapper.Test.Types
{
	public interface IGenericInterface<T1, T2> { }
	public class OpenGenericConcrete<T1, T2> : IGenericInterface<T1, T2> { }
	public class ClosedGenericConcrete : IGenericInterface<String, Boolean> { }
	public class EnumerableConcrete : IGenericInterface<String, Boolean>, IEnumerable<String>
	{
		public IEnumerator<String> GetEnumerator() { throw new NotImplementedException(); }
		IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
	}
}

