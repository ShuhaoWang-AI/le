using System;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class LoggingBuilderStrategy : BuilderStrategy
    {
        private readonly UnityBuildStage _unityBuildStage;

        public LoggingBuilderStrategy(UnityBuildStage unityBuildStage)
        {
            this._unityBuildStage = unityBuildStage;
        }

        public UnityBuildStage Stage
        {
            get
            {
                return _unityBuildStage;
            }
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            Console.WriteLine(format: "UnityBuildStage: {0}, {1}", arg0: _unityBuildStage, arg1: "PreBuildUp");
            base.PreBuildUp(context);
        }

        public override void PostBuildUp(IBuilderContext context)
        {
            Console.WriteLine(format: "UnityBuildStage: {0}, {1}", arg0: _unityBuildStage, arg1: "PostBuildUp");
            base.PostBuildUp(context);
        }

        public override void PreTearDown(IBuilderContext context)
        {
            Console.WriteLine(format: "UnityBuildStage: {0}, {1}", arg0: _unityBuildStage, arg1: "PreTearDown");
            base.PreTearDown(context);
        }

        public override void PostTearDown(IBuilderContext context)
        {
            Console.WriteLine(format: "UnityBuildStage: {0}, {1}", arg0: _unityBuildStage, arg1: "PostTearDown");
            base.PostTearDown(context);
        }
    }
}
