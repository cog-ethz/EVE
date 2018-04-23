using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using Assets.EVE.Scripts.Questionnaire2;

public class SingleChoiceQuestion : Question
{
    private const int QuestionType = 3;

    private readonly int[] _temporaryIntAnswers;

    private readonly bool _hasImages;
    private string[] _imageSource;

    private Dictionary<int, Sprite> _imageDict;

    public SingleChoiceQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log, int[] output)
    {
        QuestionName = questionName;
        QuestionText = text;
        Vals = vals;
        if (vals.Length > 2)
        {
            int numImages = vals[2];
            
            Labels = new string[labels.Length - numImages];
            Labels = labels.Take(labels.Length - numImages).ToArray();
            _imageSource = labels.Skip(labels.Length - numImages).ToArray();
            
            _imageDict = new Dictionary<int, Sprite>();
            for (int i = 3; i < Vals.Length; i++)
            {
                var image = Resources.Load<Sprite>(_imageSource[i - 3]);
                _imageDict.Add(Vals[i], image);
            }
            _hasImages = true;
        }
        else
        {
            Labels = labels;
            _hasImages = false;
        }
        
        Log = log;
        QuestionSet = questionSetName;
        Output = output;
        _temporaryIntAnswers = new int[vals[0]];

        for (var i = 0; i < _temporaryIntAnswers.Length; i++) {
            _temporaryIntAnswers[i] = -1;
        }          

    }

    /// <summary>
    /// This class is only used by the factory to package in images resource paths.
    /// </summary>
    /// <param name="questionName"></param>
    /// <param name="text"></param>
    /// <param name="vals"></param>
    /// <param name="labels"></param>
    /// <param name="questionSetName"></param>
    /// <param name="log"></param>
    /// <param name="output"></param>
    /// <param name="images"></param>
    public SingleChoiceQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log, int[] output, string[] images)
    {
        QuestionName = questionName;
        QuestionText = text;
        Vals = vals;
        Labels = labels;
        Log = log;
        QuestionSet = questionSetName;
        Output = output;
        _imageSource = images; 
        _temporaryIntAnswers = new int[vals[0]];
        _hasImages = images.Any(s => !string.IsNullOrEmpty(s));

        for (var i = 0; i < _temporaryIntAnswers.Length; i++)
        {
            _temporaryIntAnswers[i] = -1;
        }
        
        if (_hasImages)
        {
            var _imgSrc = new Dictionary<int, string>();
            for (var i = 0; i < _imageSource.Length; i++)
            {
                if (!string.IsNullOrEmpty(_imageSource[i]))
                {
                    _imgSrc.Add(i, _imageSource[i]);
                }
            }
            Vals = vals.Concat(new[] { _imgSrc.Count }).Concat(_imgSrc.Keys).ToArray();
            Labels = labels.Concat(_imgSrc.Values).ToArray();
        }
    }

    public Dictionary<int, Sprite> GetImages()
    {
        return _hasImages ? _imageDict : null;
    }

    public override void SaveTemporaryAnswer(int answerNumber) {
        //overwrite number for row
        var nCols = GetColumns();
        if (nCols == 1)
        {
            _temporaryIntAnswers[0] = answerNumber;
        }
        else
        {
            var answerRow = (int)Math.Floor((double)(answerNumber / nCols));
            _temporaryIntAnswers[answerRow] = answerNumber - answerRow * nCols;
        }
    }

    public override void SaveAnswer()
    {
        if (Log == null)
        {
            Debug.LogError("No LoggingManager. Cannot write to Database.");
            return;
        }
        var entryArray = new List<KeyValuePair<int, string>>();
        for (var i = 0; i < _temporaryIntAnswers.Length; i++)
        {
            if (_temporaryIntAnswers[i] >= 0)
            {
                entryArray.Add(new KeyValuePair<int, string>(i, _temporaryIntAnswers[i].ToString()));
            }
        }
           
        Log.InsertAnswer(QuestionName, this.QuestionSet, entryArray.ToArray());
    }

    public override bool AnswerGiven()
    {
        var nCols = GetColumns();
        if (nCols == 1)
        {
            return _temporaryIntAnswers.Any(t => t >= 0);
        }
        else
        {
            return _temporaryIntAnswers.All(t => t >= 0);
        }
    }

    public override string[] AnswerToString(object[][] answer)
    {
        throw new NotImplementedException();
    }

    public override string[][] ToCSVTable()
    {
        throw new NotImplementedException();
    }


    //---------------------------
    // 		Set & Get
    //---------------------------

    public override string GetQuestionText()
    {
        return QuestionText;
    }

    public override string GetQuestionName()
    {
        return QuestionName;
    }

    public override int GetRows()
    {
        return Vals[0];
    }

    public override int GetColumns()
    {
        return Vals[1];
    }

    public override int[] GetVals()
    {
        return Vals;
    }

    public override string[] GetLabels()
    {
        return Labels;
    }

    public override string GetQuestionSet()
    {
        return QuestionSet;
    }


    public override int GetQuestionType()
    {
        return QuestionType;
    }

    public override int[] GetOutput()
    {
        return Output;
    }

    public override int GetNumberOutputColumns()
    {
        return Vals[0];
    }
    public override QuestionData ToQuestionData()
    {
        return new QuestionData(QuestionName, QuestionText, QuestionSet, QuestionType, Vals, Labels, Output);
    }
}
