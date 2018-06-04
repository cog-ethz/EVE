namespace Assets.EVE.Scripts.Questionnaire.Visitor
{
    public interface IQuestionVisitor
    {
        void Visit(Questions.Question q);

        void Visit(Questions.InfoScreen q);

        void Visit(Questions.ChoiceQuestion q);

        void Visit(Questions.TextQuestion q);

        void Visit(Questions.LadderQuestion q);

        void Visit(Questions.ScaleQuestion q);
    }
}
