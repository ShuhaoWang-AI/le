namespace Linko.LinkoExchange.Core.Enum
{
    public enum AccountLockEvent
    {
        ManualAction,
        ExceededKBQMaxAnswerAttemptsDuringPasswordReset,
        ExceededKBQMaxAnswerAttemptsDuringProfileAccess,
        ExceededPasswordMaxAttemptsDuringSignatureCeremony,
        ExceededKBQMaxAnswerAttemptsDuringSignatureCeremony,

        ExceededPasswordMaxAttemptsDuringRepudiationCeremony,
        ExceededKBQMaxAnswerAttemptsDuringRepudiationCeremony
    }
}