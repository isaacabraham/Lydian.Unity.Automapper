using System;
using Microsoft.Practices.Unity;
using System.Diagnostics.CodeAnalysis;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Represents a potential mapping between two types.
	/// </summary>
	internal sealed class TypeMapping
	{
		/// <summary>
		/// The type to map from.
		/// </summary>
		public Type From { get; private set; }
		/// <summary>
		/// The type to map to.
		/// </summary>
		public Type To { get; private set; }

		/// <summary>
		/// Creates a new Mapping.
		/// </summary>
		/// <param name="fromInterface">The interface to map from.</param>
		/// <param name="toConcrete">The concrete to map to.</param>
		public TypeMapping(Type fromInterface, Type toConcrete)
		{
			From = fromInterface;
			To = toConcrete;
		}
	}
}
