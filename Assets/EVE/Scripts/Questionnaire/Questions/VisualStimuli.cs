using System.Collections.Generic;
using System.Xml.Serialization;

using System.Linq;
using Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli;
using Assets.EVE.Scripts.Questionnaire.Visitor;

namespace Assets.EVE.Scripts.Questionnaire.Questions
{
    public class VisualStimuli : Question
    {
        [XmlArray]
        [XmlArrayItem("Stimulus")]
        public List<string> Stimuli;

        [XmlAttribute]
        public Separator Separator;

        [XmlAttribute]
        public Enums.VisualStimuli.Choice Choice;

        [XmlAttribute]
        public Randomisation Randomisation;

        [XmlAttribute]
        public Enums.VisualStimuli.Type Type;

        [XmlAttribute]
        public string ExternalRandomisation;
        
        [XmlAttribute]
        public bool SeparatorFirst;

        [XmlAttribute]
        public float FixationTime, DecisionTime, ExpositionTime;

        [XmlIgnore] private List<KeyValuePair<int, string>> _answers;

        [XmlIgnore] private bool answersCompleted;

        public VisualStimuli(QuestionData questionData)
        {
            _answers = new List<KeyValuePair<int, string>>();
            FromDatabaseQuestion(questionData);
        }

        public VisualStimuli(string name, string text, Separator separator, Enums.VisualStimuli.Choice choice, Randomisation randomisation, Enums.VisualStimuli.Type type, string externalRandomisation, bool separatorFirst, float fixationTime, float decisionTime, float expositionTime, List<string> stimuli)
        {
            Name = name;
            Text = text;
            Separator = separator;
            Choice = choice;
            Randomisation = randomisation;
            Type = type;
            ExternalRandomisation = externalRandomisation;
            SeparatorFirst = separatorFirst;
            FixationTime = fixationTime;
            DecisionTime = decisionTime;
            ExpositionTime = expositionTime;
            Stimuli = stimuli;

            _answers = new List<KeyValuePair<int, string>>();
        }

        public override void Accept(IQuestionVisitor qv)
        {
            qv.Visit(this);
        }

        internal sealed override void FromDatabaseQuestion(QuestionData q)
        {
            Name = q.QuestionName;
            Text = q.QuestionText;
            NRows = q.Vals[0];
            NColumns = q.Vals[1];
            Separator = (Separator)q.Vals[2];
            Choice = (Enums.VisualStimuli.Choice)q.Vals[3];
            Randomisation = (Randomisation)q.Vals[4];
            Type = (Enums.VisualStimuli.Type)q.Vals[5];
            FixationTime = q.Vals[6]/1000.0f;
            DecisionTime = q.Vals[7]/1000.0f;
            ExpositionTime = q.Vals[8]/100.0f;
            SeparatorFirst = q.Vals[9] > 0;

            ExternalRandomisation = q.Labels[0];
            Stimuli = q.Labels.Skip(1).ToList();
        }

        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            var vals = new int[10]
            {
                NRows, NColumns, (int) Separator, (int) Choice, (int) Randomisation, (int) Type, (int) (1000*FixationTime),
                (int) (1000*DecisionTime), (int) (1000*ExpositionTime), SeparatorFirst?1:0
            };
            var labels = new string[1]
            {
                ExternalRandomisation
            }.Concat(Stimuli).ToArray();

            var output = new int[3] {-1, 1, 2};

            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Choice,
                vals.ToArray(),
                labels.ToArray(),
                output.ToArray());
        }

        public override KeyValuePair<int, string>[] GetAnswer()
        {
            return _answers.ToArray();
        }

        public override bool IsAnswered()
        {
            return answersCompleted;
        }

        public void SetIsAnswered(bool state)
        {
            answersCompleted = state;
        }
        

        public override void RetainAnswer(int number, string internalAnswer)
        {
            _answers.Add(new KeyValuePair<int, string>(number,internalAnswer));
        }


        public List<int> RandomisationOrder(Dictionary<string, string> experimentParameters)
        {

            if (Randomisation == Randomisation.ExperimentParameter)
            {
                return experimentParameters[ExternalRandomisation].Split(',').Select(int.Parse).ToList();
            }
            else //StimuliRandomisation.None
            {
                return Enumerable.Range(1, Stimuli.Count).ToList(); 
            }
        }

    }
}
