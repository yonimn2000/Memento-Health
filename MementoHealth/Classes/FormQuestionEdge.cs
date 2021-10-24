using MementoHealth.Entities;

namespace MementoHealth.Classes
{
    public class FormQuestionEdge
    {
        public FormQuestion Question { get; set; }
        public FormQuestionCondition Condition { get; set; }
    }
}