namespace AmongClass.Models
{
    public class AIResponse
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
    }

}
