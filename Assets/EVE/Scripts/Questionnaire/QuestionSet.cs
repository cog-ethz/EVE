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
        [XmlArrayItem("TextQuestion", typeof(Questions.TextQuestion))]
        [XmlArrayItem("ScaleQuestion", typeof(ScaleQuestion))]
        [XmlArrayItem("LadderQuestion", typeof(Questions.LadderQuestion))]
        public List<Questions.Question> Questions;

        public QuestionSet() { }

        public QuestionSet(string name)
        {
            Name = name;
            Questions = new List<Questions.Question>();
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
                Questions.Question q = null;
                /*QuestionData qData;
                List<int> answerableInds = null;
                switch ((QuestionType) questionData.QuestionType)
                {
                    case QuestionType.NOVALIDTYPE:
                        break;
                    case QuestionType.TEXTANSWER:
                        qData = new QuestionData(questionData.QuestionName,
                            questionData.QuestionText,
                            questionData.QuestionSet,
                            (int)Enums.Question.Text,
                            questionData.Vals,
                            questionData.Labels,
                            questionData.Output);
                        q = new Questions.TextQuestion(qData);
                        break;
                    case QuestionType.MULTIPLECHOICEANSWER:
                        qData = new QuestionData(questionData.QuestionName,
                            questionData.QuestionText,
                            questionData.QuestionSet,
                            (int)Enums.Question.Choice,
                            questionData.Vals.Concat(new List<int> { (int)Enums.Choice.Multiple, 0, 0 }).ToArray(),
                            questionData.Labels,
                            questionData.Output);
                        q = new Questions.ChoiceQuestion(qData);
                        break;
                    case QuestionType.SINGLECHOICEANSWER:
                        qData = new QuestionData(questionData.QuestionName,
                            questionData.QuestionText,
                            questionData.QuestionSet,
                            (int)Enums.Question.Choice,
                            questionData.Vals.Concat(new List<int> {(int)Enums.Choice.Single,0,0}).ToArray(),
                            questionData.Labels,
                            questionData.Output);
                        q = new Questions.ChoiceQuestion(qData);
                        break;
                    case QuestionType.SLIDERANSWER:
                        break;
                    case QuestionType.MANIKINANSWER:
                        var scaleType = (int)Enums.Scale.Line;
                        if (questionData.QuestionName.Contains("pleasure"))
                        {
                            scaleType = (int)Enums.Scale.Pleasure;
                        }
                        if (questionData.QuestionName.Contains("arousal"))
                        {
                            scaleType = (int)Enums.Scale.Arousal;
                        }
                        if (questionData.QuestionName.Contains("dominance"))
                        {
                            scaleType = (int)Enums.Scale.Dominance;
                        }
                        qData = new QuestionData(questionData.QuestionName,
                            questionData.QuestionText,
                            questionData.QuestionSet,
                            (int)Enums.Question.Scale,
                            questionData.Vals.Concat(new List<int> { scaleType }).ToArray(),
                            questionData.Labels,
                            questionData.Output);
                        q = new Questions.ScaleQuestion(qData);
                        break;
                    case QuestionType.LADDERANSWER:
                        qData = new QuestionData(questionData.QuestionName,
                            questionData.QuestionText,
                            questionData.QuestionSet,
                            (int)Enums.Question.Ladder,
                            questionData.Vals,
                            questionData.Labels,
                            questionData.Output);
                        q = new Questions.LadderQuestion(qData);
                        break;
                    case QuestionType.SINGLECHOICETEXTANSWER:

                        answerableInds = new List<int>();
                        for (var i = 3; i < questionData.Vals.Length; i++)
                        {
                            answerableInds.Add(questionData.Vals[i]);
                        }
                        qData = new QuestionData(questionData.QuestionName,
                            questionData.QuestionText,
                            questionData.QuestionSet,
                            (int)Enums.Question.Choice,
                            new List<int> { questionData.Vals[0], questionData.Vals[1], (int)Enums.Choice.Single, 0, questionData.Vals[2] }
                            .Concat(answerableInds)
                            .ToArray(),
                            questionData.Labels,
                            questionData.Output);
                        q = new Questions.ChoiceQuestion(qData);
                        break;
                    case QuestionType.INFOSCREEN:
                        qData = new QuestionData(questionData.QuestionName,
                            questionData.QuestionText,
                            questionData.QuestionSet,
                            (int)Enums.Question.Info,
                            questionData.Vals,
                            questionData.Labels,
                            questionData.Output);
                        q = new Questions.InfoScreen(qData);
                        break;
                    case QuestionType.MULTIPLECHOICETEXTANSWER:
                        answerableInds = new List<int>();
                        for (var i = 3; i < questionData.Vals.Length; i++)
                        {
                            answerableInds.Add(questionData.Vals[i]);
                        }
                        qData = new QuestionData(questionData.QuestionName,
                            questionData.QuestionText,
                            questionData.QuestionSet,
                            (int)Enums.Question.Choice,
                            new List<int> { questionData.Vals[0], questionData.Vals[1], (int)Enums.Choice.Multiple, 0, questionData.Vals[2] }
                            .Concat(answerableInds)
                            .ToArray(),
                            questionData.Labels,
                            questionData.Output);
                        q = new Questions.ChoiceQuestion(qData);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }*/
                switch ((Enums.Question) questionData.QuestionType)
                {
                    case Enums.Question.Info:
                        q = new InfoScreen(questionData);
                        break;
                    case Enums.Question.Text:
                        q = new Questions.TextQuestion(questionData);
                        break;
                    case Enums.Question.Choice:
                        q = new ChoiceQuestion(questionData);
                        break;
                    case Enums.Question.Scale:
                        q = new ScaleQuestion(questionData);
                        break;
                    case Enums.Question.Ladder:
                        q = new Questions.LadderQuestion(questionData);
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
