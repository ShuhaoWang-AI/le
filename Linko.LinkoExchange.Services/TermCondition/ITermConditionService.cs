namespace Linko.LinkoExchange.Services.TermCondition
{
    public interface ITermConditionService
    {
        /// <summary>
        ///     Finds the most recent object in the tTermCondition table and returns it's content.
        /// </summary>
        /// <returns> tTermCondition.Content </returns>
        string GetTermConditionContent();

        /// <summary>
        ///     Finds the most recent object in the tTermCondition table and returns it's Id.
        /// </summary>
        /// <returns> tTermCondition.TermConditionId </returns>
        int GetLatestTermConditionId();
    }
}