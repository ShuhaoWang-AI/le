using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace Linko.LinkoExchange.Core.Logging
{
    public class LoggingBuilderExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            var builder = new LoggingBuilderStrategy(unityBuildStage:UnityBuildStage.Creation);
            Context.Strategies.Add(strategy:builder, stage:builder.Stage);

            builder = new LoggingBuilderStrategy(unityBuildStage:UnityBuildStage.Initialization);
            Context.Strategies.Add(strategy:builder, stage:builder.Stage);

            builder = new LoggingBuilderStrategy(unityBuildStage:UnityBuildStage.Lifetime);
            Context.Strategies.Add(strategy:builder, stage:builder.Stage);

            builder = new LoggingBuilderStrategy(unityBuildStage:UnityBuildStage.PostInitialization);
            Context.Strategies.Add(strategy:builder, stage:builder.Stage);

            builder = new LoggingBuilderStrategy(unityBuildStage:UnityBuildStage.PreCreation);
            Context.Strategies.Add(strategy:builder, stage:builder.Stage);

            builder = new LoggingBuilderStrategy(unityBuildStage:UnityBuildStage.Setup);
            Context.Strategies.Add(strategy:builder, stage:builder.Stage);

            builder = new LoggingBuilderStrategy(unityBuildStage:UnityBuildStage.TypeMapping);
            Context.Strategies.Add(strategy:builder, stage:builder.Stage);
        }
    }
}