using System;
using Microsoft.Practices.ObjectBuilder2;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    /// </summary>
    public interface ICreationStackTrackerPolicy : IBuilderPolicy
    {
        #region public properties

        /// <summary>
        /// </summary>
        PeekableStack<Type> TypeStack { get; }

        #endregion
    }
}