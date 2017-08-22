using System;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    /// </summary>
    public class CreationStackTrackerStrategy : BuilderStrategy
    {
        #region methods

        /// <summary>
        /// </summary>
        /// <param name="typeStack">
        /// </param>
        /// <returns>
        /// </returns>
        private object ReportStack(PeekableStack<Type> typeStack)
        {
            return string.Join(separator:", ", values:typeStack.Items.Select(s => s.Name));
        }

        #endregion

        #region public methods and operators

        /// <summary>
        /// </summary>
        /// <param name="context">
        /// </param>
        public override void PostBuildUp(IBuilderContext context)
        {
            var policy = context.Policies.Get<ICreationStackTrackerPolicy>(buildKey:null, localOnly:true);

            if (policy.TypeStack.Count > 0)
            {
                policy.TypeStack.Pop();
            }

            base.PostBuildUp(context:context);
        }

        /// <summary>
        /// </summary>
        /// <param name="context">
        /// </param>
        public override void PreBuildUp(IBuilderContext context)
        {
            var policy = context.Policies.Get<ICreationStackTrackerPolicy>(buildKey:null, localOnly:true);
            if (policy == null)
            {
                context.Policies.Set(policyInterface:typeof(ICreationStackTrackerPolicy), policy:new CreationStackTrackerPolicy(), buildKey:null);
                policy = context.Policies.Get<ICreationStackTrackerPolicy>(buildKey:null, localOnly:true);
            }

            policy.TypeStack.Push(obj:context.BuildKey.Type);

            base.PreBuildUp(context:context);
        }

        #endregion
    }
}