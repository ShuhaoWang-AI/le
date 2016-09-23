using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace Linko.LinkoExchange.Core.Logging
{
    /// <summary>
    /// </summary>
    public class NLogExtension : UnityContainerExtension
    {
        #region methods

        /// <summary>
        /// </summary>
        protected override void Initialize()
        {
            Context.Strategies.AddNew<CreationStackTrackerStrategy>(UnityBuildStage.TypeMapping);
            Context.Strategies.AddNew<NLogStrategy>(UnityBuildStage.TypeMapping);
        }

        #endregion
    }
}
