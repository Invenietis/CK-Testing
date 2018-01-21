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
        /// </summary>
        void OnTestHelperGraphResolved();
    }
}
