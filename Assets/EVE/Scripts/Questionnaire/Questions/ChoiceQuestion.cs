using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire.Enums;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire.Questions
{
    
    public class ChoiceQuestion : Question
    {
        [XmlAttribute] public Choice Choice;

        [XmlArray]
        [XmlArrayItem("Label")]
        public List<Label> ColumnLabels;

        [XmlArray]
        [XmlArrayItem("Label")]
        public List<Label> RowLabels;

        [XmlIgnore]
        public List<int> _answers;

        [XmlIgnore]
        public Dictionary<int, string> _answerStrings;

        public ChoiceQuestion() { }

        public ChoiceQuestion(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
        }

        public ChoiceQuestion(string name, string text, Choice choice, List<Label> rowLabels,
            List<Label> columnLabels)
        {
            Name = name;
            Text = text;
            Choice = choice;
            RowLabels = rowLabels;
            NRows = RowLabels!=null?RowLabels.Count:1;
            ColumnLabels = columnLabels;
            NColumns = ColumnLabels != null ? ColumnLabels.Count:1;
            _answers = new List<int>();
            _answerStrings = new Dictionary<int, string>();
        }

        public ChoiceQuestion(string name, string text, Choice choice, List<Label> rowLabels,
            List<Label> columnLabels, List<Jump> jumps )
        {
            Name = name;
            Text = text;
            Choice = choice;
            RowLabels = rowLabels;
            NRows = RowLabels != null ? RowLabels.Count : 1;
            ColumnLabels = columnLabels;
            NColumns = ColumnLabels != null ? ColumnLabels.Count : 1;
            Jumps = jumps;
            _answers = new List<int>();
            _answerStrings = new Dictionary<int, string>();
        }

        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            var imgCount = 0;
            var answerableCount = 0;
            var imgInds = new List<int>();
            var imgPaths = new List<string>();
            var answerableInds = new List<int>();
            var rlabels = new List<string>();
            var clabels = new List<string>();
            var output = new List<int>();
            if (RowLabels != null)
            {
                for (var index = 0; index < RowLabels.Count; index++)
                {
                    var rowLabel = RowLabels[index];
                    if (rowLabel.Answerable != null)
                    {
                        answerableCount++;
                        answerableInds.Add(index);
                    }
                    if (rowLabel.Image != null)
                    {
                        imgCount++;
                        imgInds.Add(index);
                        imgPaths.Add(rowLabel.Image);
                    }
                    if (rowLabel.Output != null)
                    {
                        output.Add(rowLabel.Output.Value);
                    }
                    rlabels.Add(rowLabel.Text);
                }
            }
            if (ColumnLabels != null)
            {
                foreach (var columnLabel in ColumnLabels)
                {
                    clabels.Add(columnLabel.Text);
                    if (columnLabel.Output != null)
                    {
                        output.Add(columnLabel.Output.Value);
                    }
                }
            }
            var vals = new int[5] {NRows, NColumns, (int)Choice, imgCount, answerableCount}
                .Concat(imgInds)
                .Concat(answerableInds);
            var labels = rlabels.Concat(clabels).Concat(imgPaths);
            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Choice,
                vals.ToArray(),
                labels.ToArray(),
                output.ToArray());
        }

        internal sealed override void FromDatabaseQuestion(QuestionData q)
        {
            if (q.QuestionType == (int)Enums.Question.Choice)
            {
                Name = q.QuestionName;
                Text = q.QuestionText;
                NRows = q.Vals[0];
                NColumns = q.Vals[1];
                Choice = (Choice)q.Vals[2];
                
                var rlabels = new List<string>();
                var clabels = new List<string>();
                var offset = 0;
                
                if (NRows > 1)
                {
                    for (var i = 0; i < NRows; i++)
                    {
                        rlabels.Add(q.Labels[i]);
                    }
                }
                else
                {
                    offset = -1;
                }
                if (NColumns > 1)
                {
                    for (var i = NRows + offset; i < (NRows + NColumns) && i < q.Labels.Length; i++)
                    {
                        clabels.Add(q.Labels[i]);
                    }
                }

                var output = q.Output!=null?q.Output.ToList():null;
                if (output == null)
                {
                    output = new List<int>();
                    var count = NColumns == 1?NRows:NColumns;
                    for (var i = 0; i < count; i++)
                    {
                        output.Add(i);
                    }
                    Debug.LogWarning("No output coding present. Default inserted.");
                }

                if (NColumns == 1)
                {
                    var imgInds = new List<int>();
                    var imgPaths = new List<string>();
                    var answerableInds = new List<int>();

                    var imgCount = q.Vals[3];
                    var answerableCount = q.Vals[4];
                    _answerStrings = new Dictionary<int, string>();


                    for (var i = 5; i < 5 + imgCount; i++)
                    {
                        imgInds.Add(q.Vals[i]);
                    }
                    for (var i = 5 + imgCount; i < q.Vals.Length; i++) 
                    {
                        answerableInds.Add(q.Vals[i]);
                    }
                    for (var i = NRows + NColumns -1; i < q.Labels.Length; i++)
                    {
                        imgPaths.Add(q.Labels[i]);
                    }

                    RowLabels = new List<Label>();
                    for (var i = 0; i < rlabels.Count; i++)
                    {
                        var hasImage = imgInds.Any(index => index == i);
                        var isAnswerable = answerableInds.Any(index => index == i);
                        Label label;
                        if (hasImage && isAnswerable)
                        {
                            label = new Label(rlabels[i], output[i], imgPaths[imgInds.IndexOf(i)], true);
                            _answerStrings[i] = "";
                        }
                        else if (isAnswerable)
                        {
                            label = new Label(rlabels[i], output[i], true);
                            _answerStrings[i] = "";
                        }
                        else if (hasImage)
                        {
                            label = new Label(rlabels[i], output[i], imgPaths[imgInds.IndexOf(i)]);
                        }
                        else
                        {
                            label = new Label(rlabels[i], output[i]);
                        }
                        RowLabels.Add(label);
                    }
                    ColumnLabels = new List<Label>();
                    foreach (var l in clabels)
                    {
                        ColumnLabels.Add(new Label(l));
                    }
                }
                else
                {
                    ColumnLabels = new List<Label>();
                    for (var i = 0; i < clabels.Count; i++)
                    {
                        ColumnLabels.Add(new Label(clabels[i],output[i]));
                    }
                    RowLabels = new List<Label>();
                    foreach (var l in rlabels)
                    {
                        RowLabels.Add(new Label(l));
                    }

                }
                RowLabels = RowLabels.Count > 0 ? RowLabels : null;
                ColumnLabels = ColumnLabels.Count > 0 ? ColumnLabels : null;
                _answers = new List<int>();
            }
            else
            {
                Debug.LogError("The question type is wrong: " + q.QuestionType);
            }
        }

        public override void Accept(IQuestionVisitor qv)
        {
            qv.Visit(this);
        }

        public override KeyValuePair<int, string>[] GetAnswer()
        {
            if (_answerStrings == null)
            {
                _answerStrings = new Dictionary<int, string>();
            }
            var kvpList = _answerStrings.Where(keyValuePair => keyValuePair.Value.Length > 0).ToList();
            kvpList.AddRange(_answers
                .Select(answer => NColumns > 0
                ? new KeyValuePair<int, string>(answer/NColumns, (answer%NColumns).ToString()) 
                : new KeyValuePair<int, string>(answer, "0")));
            return kvpList.ToArray();
        }

        public override bool IsAnswered()
        {
            if (_answerStrings != null)
            {
                if (_answerStrings.Where(keyValuePair => _answers.Contains(keyValuePair.Key)).Any(keyValuePair => keyValuePair.Value.Length == 0))
                {
                    return false;
                }
            }
            if (NColumns == 1)
            {
                return _answers.Count > 0;
            }
            else
            {
                var allRows = true;
                for (var i = 0; i < NRows && allRows; i++)
                {
                    allRows = allRows && _answers.Any(val => val >= i*NColumns && val < (i + 1)*NColumns);
                }
                return allRows;
            }
        }

        public override void RetainAnswer(int internalnumber)
        {
            if (Choice == this.Choice.Single )
            {
                if (NColumns == 1)
                {
                    _answers = new List<int>();
                }
                else
                {
                    var lowerBound = internalnumber - internalnumber%NColumns;
                    var upperBound = lowerBound + NColumns;
                    for (var i = lowerBound; i < upperBound; i++)
                    {
                        if (_answers.Contains(i))
                        {
                            _answers.Remove(i);
                        }
                    }
                }
            }
            if (_answers.Contains(internalnumber))
            {
                _answers.Remove(internalnumber);
            }
            else
            {
                _answers.Add(internalnumber);
            }
        }

        public override void RetainAnswer(int number, string internalAnswer)
        {
            _answerStrings[number] = internalAnswer;
        }

        public override string GetJumpDestination()
        {
            var answerB = new StringBuilder(new string('F', NRows * NColumns));
            _answers.ForEach(i => answerB[i] = 'T');
            var answer = answerB.ToString();
            if (Jumps != null)
            {
                return
                (from jump in Jumps
                    where jump.Activator.Equals("*") || jump.Activator.Equals(answer)
                    select jump.Destination).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
    }
}