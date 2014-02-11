using System;
using System.Diagnostics.CodeAnalysis;

namespace Lydian.Unity.Automapper
{
    /// <summary>
    /// Marks an interface as using a specific type of lifetime manager for the purposes of registration into Unity when auto-mapping.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes"), AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class CustomLifetimeManagerAttribute : Attribute
    {
        public Type LifetimeManagerType { get; private set; }

        public CustomLifetimeManagerAttribute(Type lifetimeManagerType)
        {
            LifetimeManagerType = lifetimeManagerType;
        }
    }
}
