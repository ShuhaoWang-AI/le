using Microsoft.Practices.ObjectBuilder2;
using NLog;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    /// </summary>
    public class NLogStrategy : BuilderStrategy
    {
        #region public methods and operators

        /// <summary>
        /// </summary>
        /// <param name="context">
        /// </param>
        public override void PreBuildUp(IBuilderContext context)
        {
            var policy = context.Policies.Get<ICreationStackTrackerPolicy>(buildKey:null, localOnly:true);

            // The stack seems to contain duplicate of each type.
            // Thus, the 3rd element is the actual type that the logger should be named after.
            if (policy.TypeStack.Count >= 3 && policy.TypeStack.Peek(depth:0) == typeof(ILogger))
            {
                context.Existing = LogManager.GetLogger(name:policy.TypeStack.Peek(depth:2).FullName);
            }

            base.PreBuildUp(context:context);
        }

        #endregion
    }
}