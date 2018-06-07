using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire.Questions;

namespace Assets.EVE.Scripts.Questionnaire
{
    [Serializable]
    public class QuestionSet
    {
        [XmlAttribute]
        public string Name;

        [XmlArray]
        [XmlArrayItem("ChoiceQuestion", typeof(ChoiceQuestion))]
        [XmlArrayItem("InfoScreen", typeof(InfoScreen))]
        [XmlArrayItem("TextQuestion", typeof(TextQuestion))]
        [XmlArrayItem("ScaleQuestion", typeof(ScaleQuestion))]
        [XmlArrayItem("LadderQuestion", typeof(LadderQuestion))]
        [XmlArrayItem("VisualStimuli", typeof(VisualStimuli))]
        public List<Question> Questions;

        public QuestionSet() { }

        public QuestionSet(string name)
        {
            Name = name;
            Questions = new List<Question>();
        }

        public void WriteToDatabase(LoggingManager log)
        {
            log.CreateQuestionSet(Name);
            Questions.ForEach(q =>
            {
                log.InsertQuestionToDB(q.AsDatabaseQuestion(Name));
            });
            Questions.ForEach(q =>
            {
                log.AddQuestionJumps(q, Name);
            });
        }

        public void LoadFromDatabase(LoggingManager log)
        {
            var qDataList = log.GetQuestionSetContent(Name);
            foreach (var questionData in qDataList)
            {
                Question q;
                switch ((Enums.Question) questionData.QuestionType)
                {
                    case Enums.Question.Info:
                        q = new InfoScreen(questionData);
                        break;
                    case Enums.Question.Text:
                        q = new TextQuestion(questionData);
                        break;
                    case Enums.Question.Choice:
                        q = new ChoiceQuestion(questionData);
                        break;
                    case Enums.Question.Scale:
                        q = new ScaleQuestion(questionData);
                        break;
                    case Enums.Question.Ladder:
                        q = new LadderQuestion(questionData);
                        break;
                    case Enums.Question.Stimuli:
                        q = new VisualStimuli(questionData);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                q.Jumps = log.GetJumps(questionData);
                Questions.Add(q);
            }

        }
    }
}
