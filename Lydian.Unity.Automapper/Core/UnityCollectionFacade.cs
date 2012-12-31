using Microsoft.Practices.Unity;
using System.Collections;
using System.Collections.Generic;

namespace Lydian.Unity.Automapper.Core
{
	/// <summary>
	/// A facade on top of a Unity call to ResolveAll for a particular type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class UnityCollectionFacade<T> : IEnumerable<T>
	{
		private readonly IEnumerable<T> resolvedTypes;

		/// <summary>
		/// Initializes a new instance of the CollectionFacade class.
		/// </summary>
		public UnityCollectionFacade(IUnityContainer target)
		{
			resolvedTypes = target.ResolveAll<T>();
		}

		public IEnumerator<T> GetEnumerator()
		{
			foreach (var resolvedType in resolvedTypes)
				yield return resolvedType;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
