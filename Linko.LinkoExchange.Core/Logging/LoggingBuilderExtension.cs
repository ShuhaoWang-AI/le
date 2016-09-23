using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace Linko.LinkoExchange.Core.Logging
{
    public class LoggingBuilderExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            LoggingBuilderStrategy builder = new LoggingBuilderStrategy(UnityBuildStage.Creation);
            Context.Strategies.Add(builder, builder.Stage);

            builder = new LoggingBuilderStrategy(UnityBuildStage.Initialization);
            Context.Strategies.Add(builder, builder.Stage);

            builder = new LoggingBuilderStrategy(UnityBuildStage.Lifetime);
            Context.Strategies.Add(builder, builder.Stage);

            builder = new LoggingBuilderStrategy(UnityBuildStage.PostInitialization);
            Context.Strategies.Add(builder, builder.Stage);

            builder = new LoggingBuilderStrategy(UnityBuildStage.PreCreation);
            Context.Strategies.Add(builder, builder.Stage);

            builder = new LoggingBuilderStrategy(UnityBuildStage.Setup);
            Context.Strategies.Add(builder, builder.Stage);

            builder = new LoggingBuilderStrategy(UnityBuildStage.TypeMapping);
            Context.Strategies.Add(builder, builder.Stage);
        }
    }
}
