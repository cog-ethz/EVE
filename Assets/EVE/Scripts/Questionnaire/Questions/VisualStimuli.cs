using System.Collections.Generic;
using System.Xml.Serialization;

using System.Linq;
using Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using Assets.EVE.Scripts.Questionnaire.XMLHelper.VisualStimuli;

namespace Assets.EVE.Scripts.Questionnaire.Questions
{
    /// <summary>
    /// Visual Stimuli represent a question type that allows researchers to compare two visual stimuli at a time and ask for the respondents preference.
    /// 
    /// Additional option allow the researcher to specify how many different stimuli pairs to show and how to randomise their combination.
    /// 
    /// Both images and videos are supported.
    /// </summary>
    public class VisualStimuli : Question
    {
        [XmlArray]
        [XmlArrayItem("Stimulus")]
        public List<string> Stimuli;

        [XmlElement]
        public Configuration Configuration;
        
        [XmlElement]
        public string ExternalRandomisation;
        
        [XmlElement]
        public Times Times;

        [XmlIgnore] private readonly List<KeyValuePair<int, string>> _answers;

        [XmlIgnore] private bool answersCompleted;

        /// <summary>
        /// Creates an empty VisualStimuli.
        /// </summary>
        public VisualStimuli()
        {
            _answers = new List<KeyValuePair<int, string>>();
        }

        public VisualStimuli(QuestionData questionData)
        {
            _answers = new List<KeyValuePair<int, string>>();
            FromDatabaseQuestion(questionData);
        }

        public VisualStimuli(string name, 
            string text, 
            Separator separator, 
            Choice choice, 
            Randomisation randomisation, 
            Type type, 
            string externalRandomisation, 
            bool separatorFirst, 
            float fixationTime, 
            float decisionTime, 
            float expositionTime, 
            List<string> stimuli)
        {
            Name = name;
            Text = text;
            Configuration = new Configuration
            {
                Separator = separator,
                Choice = choice,
                Randomisation = randomisation,
                Type = type,
                SeparatorFirst = separatorFirst
            };
            ExternalRandomisation = externalRandomisation;
            Times = new Times
            {
                Fixation = fixationTime,
                Exposition = expositionTime,
                Decision = decisionTime
            };
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
            Configuration = new Configuration
            {
                Separator = (Separator) q.Vals[2],
                Choice = (Choice) q.Vals[3],
                Randomisation = (Randomisation) q.Vals[4],
                Type = (Type) q.Vals[5],
                SeparatorFirst = q.Vals[6] > 0
        };
            Times = new Times
            {
                Fixation = q.Vals[7]/1000.0f,
                Exposition = q.Vals[8]/1000.0f,
                Decision = q.Vals[9]/1000.0f
            };
            ExternalRandomisation = q.Labels[0];
            Stimuli = q.Labels.Skip(1).ToList();
        }

        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            var vals = new int[10]
            {
                NRows,
                NColumns,
                (int) Configuration.Separator,
                (int) Configuration.Choice,
                (int) Configuration.Randomisation,
                (int) Configuration.Type,
                Configuration.SeparatorFirst?1:0,
                (int) (1000*Times.Fixation),
                (int) (1000*Times.Exposition),
                (int) (1000*Times.Decision),
            };
            var labels = new string[1]
            {
                ExternalRandomisation
            }.Concat(Stimuli).ToArray();

            var output = new int[3] {-1, 1, 2};

            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Stimuli,
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

        /// <summary>
        /// Visual Stimuli Representation sets externally, whether the question has been answered.
        /// </summary>
        /// <param name="state">state of whether the question is answered.</param>
        public void SetIsAnswered(bool state)
        {
            answersCompleted = state;
        }
        
        /// <summary>
        /// Stores the user Answer
        /// </summary>
        /// <param name="number"></param>
        /// <param name="internalAnswer"></param>
        public override void RetainAnswer(int number, string internalAnswer)
        {
            _answers.Add(new KeyValuePair<int, string>(number,internalAnswer));
        }

        /// <summary>
        /// Jumps are not available in Visual Stimuli
        /// </summary>
        /// <returns>returns null</returns>
        public override string GetJumpDestination()
        {
            return null;
        }


        /// <summary>
        /// Returns Randomisation order from experiment parameters.
        /// </summary>
        /// <param name="experimentParameters">Dictionary of experiment Parameters returned from the database</param>
        /// <returns>A list describing the order of items.</returns>
        public List<int> RandomisationOrder(Dictionary<string, string> experimentParameters)
        {
            return experimentParameters[ExternalRandomisation].Split(',').Select(int.Parse).ToList();
        }

    }
}
