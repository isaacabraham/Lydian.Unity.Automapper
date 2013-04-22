using System;

namespace Lydian.Unity.Automapper
{
    /// <summary>
    /// Marks an interface as using a specific type of lifetime manager for the purposes of registration into Unity when auto-mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class CustomLifetimeManagerAttribute : Attribute
    {
        public Type LifetimeManagerType { get; private set; }

        public CustomLifetimeManagerAttribute(Type lifetimeManagerType)
        {
            LifetimeManagerType = lifetimeManagerType;
        }
    }
}
