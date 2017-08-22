using System;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    /// </summary>
    public class CreationStackTrackerPolicy : ICreationStackTrackerPolicy
    {
        #region fields

        /// <summary>
        /// </summary>
        private readonly PeekableStack<Type> _typeStack = new PeekableStack<Type>();

        #endregion

        #region interface implementations

        /// <summary>
        /// </summary>
        public PeekableStack<Type> TypeStack => _typeStack;

        #endregion
    }
}