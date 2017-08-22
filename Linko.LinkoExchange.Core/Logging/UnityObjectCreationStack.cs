using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    /// </summary>
    public class UnityObjectCreationStack
    {
        #region fields

        /// <summary>
        /// </summary>
        private readonly List<Type> _items;

        #endregion

        #region constructors and destructor

        /// <summary>
        /// </summary>
        /// <param name="stack">
        /// </param>
        public UnityObjectCreationStack(IEnumerable<Type> stack)
        {
            _items = new List<Type>(collection:stack);
        }

        #endregion

        #region public properties

        /// <summary>
        /// </summary>
        public IEnumerable<Type> Items => _items;

        #endregion

        #region public methods and operators

        /// <summary>
        /// </summary>
        /// <param name="peekBack">
        /// </param>
        /// <returns>
        /// </returns>
        public Type Peek(int peekBack = 0)
        {
            return _items[index:_items.Count - 1 - peekBack];
        }

        #endregion
    }
}