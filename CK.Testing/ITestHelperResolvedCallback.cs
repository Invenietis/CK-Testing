using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// Optional interface for core interfaces that enables mixin parts to
    /// be called once the graph to which they belong has been resolved.
    /// </summary>
    public interface ITestHelperResolvedCallback
    {
        /// <summary>
        /// Called once the whole graph has been resolved, in the order of resolution (dependent parts
        /// are called before the parts that depend on them).
        /// <para>
        /// The <paramref name="resolvedObject"/> is not necessarily a mixin. If an implementation (a class A : IACore)
        /// needs to have an access to the Mixin facade (the IA interface), it can throw here with a message
        /// that states that A or IACore MUST not be resolved, only IA should be resolved.
        /// TODO: introduce a marker interface or attribute to specify that an interface or a class CAN NOT be resolved or
        /// (may be better) must be resolved only through a given interface.
        /// </para>
        /// </summary>
        /// <param name="resolvedObject">The concrete object that has been resolved.</param>
        void OnTestHelperGraphResolved( object resolvedObject );
    }
}
