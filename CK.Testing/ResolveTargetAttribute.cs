using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing
{

    /// <summary>
    /// This attribute forwards the resolution of a type to anotner one.
    /// This can typically be used by a core interface or an implementation of a core interface to
    /// forward their resolution to their associated mixin.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class|AttributeTargets.Interface, Inherited = false, AllowMultiple = false )]
    public class ResolveTargetAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="ResolveTargetAttribute"/> with an interface
        /// type that must be resolved instead of the type to which this attribute applies.
        /// The <paramref name="interfaceType"/> is typically the mixin interface type.
        /// </summary>
        /// <param name="interfaceType">The target interface type.</param>
        public ResolveTargetAttribute( Type interfaceType )
        {
            Target = interfaceType;
        }

        /// <summary>
        /// Gets the target type.
        /// </summary>
        public Type Target { get; }
    }
}
