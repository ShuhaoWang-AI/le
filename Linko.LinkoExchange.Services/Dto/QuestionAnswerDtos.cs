namespace Linko.LinkoExchange.Services.Dto
{
    public enum QuestionType
    {
        KnowledgeBased = 1,
        Security = 2 
    }

    public class QuestionDto
    {
        public int? QuestionId { get; set; }
        public QuestionType QuestionType { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
    }

    public class AnswerDto
    {
        public int? UserQuestionAnswerId { get; set; }
        public string Content { get; set; }
        public int QuestionId { get; set; }

    }

    public class QuestionAnswerPairDto
    {
        public QuestionDto Question { get; set; }
        public AnswerDto Answer { get; set; }
    }

}
