using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Practices.Unity;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Keeps track of registrations that have taken place on a Unity Container.
	/// </summary>
	internal sealed class UnityRegistrationTracker
	{
		private readonly IEnumerable<ContainerRegistration> initialRegistrations;
		private readonly IUnityContainer container;

		/// <summary>
		/// Creates a new instance of the UnityRegistrationTracker and begins tracking changes immediately.
		/// </summary>
		/// <param name="container">The container to track changes of.</param>
		public UnityRegistrationTracker(IUnityContainer container)
		{
			Contract.Requires(container != null, "container is null.");
			Contract.Ensures(initialRegistrations != null);

			this.container = container;
			Contract.Assume(container.Registrations != null);
			initialRegistrations = container.Registrations.ToArray();
		}
		
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Code Contracts")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code Contracts")]
		[ContractInvariantMethod]
		private void ObjectInvariant()
		{
			Contract.Invariant(container != null);
			Contract.Invariant(initialRegistrations != null);
			Contract.Invariant(container.Registrations != null);
		}
		
		/// <summary>
		/// Gets the list of newly-added registrations since the registration tracker was created.
		/// </summary>
		/// <returns>The collection of new registrations.</returns>
		public IEnumerable<ContainerRegistration> GetNewRegistrations()
		{
			return container.Registrations.Except(initialRegistrations).ToArray();
		}
	}
}
