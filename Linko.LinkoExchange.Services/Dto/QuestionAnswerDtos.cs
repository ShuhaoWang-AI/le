namespace Linko.LinkoExchange.Services.Dto
{
    public enum QuestionTypeName
    {
        KBQ,
        SQ
    }

    public class QuestionDto
    {
        #region public properties

        public int? QuestionId { get; set; }
        public QuestionTypeName QuestionType { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }

        #endregion
    }

    public class AnswerDto
    {
        #region public properties

        public int? UserQuestionAnswerId { get; set; }
        public string Content { get; set; }
        public int QuestionId { get; set; }
        public QuestionTypeName  QuestionTypeName {get;set; }
        #endregion
    }

    public class QuestionAnswerPairDto
    {
        #region public properties

        public QuestionDto Question { get; set; }
        public AnswerDto Answer { get; set; }

        #endregion
    }
}