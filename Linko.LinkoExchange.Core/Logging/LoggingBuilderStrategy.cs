using System;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class LoggingBuilderStrategy : BuilderStrategy
    {
        #region constructors and destructor

        public LoggingBuilderStrategy(UnityBuildStage unityBuildStage)
        {
            Stage = unityBuildStage;
        }

        #endregion

        #region public properties

        public UnityBuildStage Stage { get; }

        #endregion

        public override void PreBuildUp(IBuilderContext context)
        {
            Console.WriteLine(format:"UnityBuildStage: {0}, {1}", arg0:Stage, arg1:"PreBuildUp");
            base.PreBuildUp(context:context);
        }

        public override void PostBuildUp(IBuilderContext context)
        {
            Console.WriteLine(format:"UnityBuildStage: {0}, {1}", arg0:Stage, arg1:"PostBuildUp");
            base.PostBuildUp(context:context);
        }

        public override void PreTearDown(IBuilderContext context)
        {
            Console.WriteLine(format:"UnityBuildStage: {0}, {1}", arg0:Stage, arg1:"PreTearDown");
            base.PreTearDown(context:context);
        }

        public override void PostTearDown(IBuilderContext context)
        {
            Console.WriteLine(format:"UnityBuildStage: {0}, {1}", arg0:Stage, arg1:"PostTearDown");
            base.PostTearDown(context:context);
        }
    }
}