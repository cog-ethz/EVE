using Assets.EVE.Scripts.Questionnaire.Questions;

namespace Assets.EVE.Scripts.Questionnaire.Visitor
{
    public interface IQuestionVisitor
    {
        void Visit(Question q);

        void Visit(InfoScreen q);

        void Visit(ChoiceQuestion q);

        void Visit(TextQuestion q);

        void Visit(LadderQuestion q);

        void Visit(ScaleQuestion q);

        void Visit(VisualStimuli q);
    }
}
