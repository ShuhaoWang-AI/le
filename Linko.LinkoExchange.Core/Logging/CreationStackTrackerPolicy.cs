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


        #region public properties

        /// <summary>
        /// </summary>
        public PeekableStack<Type> TypeStack
        {
            get { return _typeStack; }
        }

        #endregion
    }
}
