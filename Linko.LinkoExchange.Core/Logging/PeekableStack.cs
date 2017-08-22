using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class PeekableStack<T>
    {
        #region fields

        /// <summary>
        /// </summary>
        private readonly List<T> _list;

        #endregion

        #region constructors and destructor

        /// <summary>
        /// </summary>
        public PeekableStack()
        {
            _list = new List<T>();
        }

        /// <summary>
        /// </summary>
        /// <param name="initialItems">
        /// </param>
        public PeekableStack(IEnumerable<T> initialItems)
        {
            _list = new List<T>(collection:initialItems);
        }

        #endregion

        #region public properties

        /// <summary>
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// </summary>
        public IEnumerable<T> Items => _list.ToArray();

        #endregion

        #region public methods and operators

        /// <summary>
        /// </summary>
        /// <param name="depth">
        /// </param>
        /// <returns>
        /// </returns>
        public T Peek(int depth)
        {
            var index = _list.Count - 1 - depth;
            return _list[index:index];
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public T Pop()
        {
            var index = _list.Count - 1;
            var ret = _list[index:index];
            _list.RemoveAt(index:index);
            return ret;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj">
        /// </param>
        public void Push(T obj)
        {
            _list.Add(item:obj);
        }

        #endregion
    }
}