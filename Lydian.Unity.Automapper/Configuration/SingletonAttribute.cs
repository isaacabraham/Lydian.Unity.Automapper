using Microsoft.Practices.Unity;
using System;

namespace Lydian.Unity.Automapper
{
    /// <summary>
    /// Marks an interface as being a singleton for the purposes of registration into Unity when auto-mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class SingletonAttribute : CustomLifetimeManagerAttribute
    {
        public SingletonAttribute() : base(typeof(ContainerControlledLifetimeManager)){ }
    }
}
