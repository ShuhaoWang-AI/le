namespace Linko.LinkoExchange.Services.TermCondition
{
    public interface ITermConditionService
    {
        string GetTermCondtionContent(int termConditionId);
        int GetLatestTermConditionId();
    }
}
