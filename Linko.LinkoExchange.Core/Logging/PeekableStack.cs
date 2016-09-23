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


        #region constructor and destructor

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
            _list = new List<T>(initialItems);
        }

        #endregion


        #region public properties

        /// <summary>
        /// </summary>
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        /// <summary>
        /// </summary>
        public IEnumerable<T> Items
        {
            get
            {
                return _list.ToArray();
            }
        }

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
            int index = _list.Count - 1 - depth;
            return _list[index];
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public T Pop()
        {
            int index = _list.Count - 1;
            T ret = _list[index];
            _list.RemoveAt(index);
            return ret;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj">
        /// </param>
        public void Push(T obj)
        {
            _list.Add(obj);
        }

        #endregion
    }
}
