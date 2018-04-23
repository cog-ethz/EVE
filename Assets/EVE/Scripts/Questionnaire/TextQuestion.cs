using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire2;

public class TextQuestion : Question
{
    private const int QuestionType = 1;

    private readonly string[] _temporaryStringAnswers;

    public TextQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log )
    {
        this.QuestionName = questionName;
        QuestionText = text;
        this.Vals = vals;
        this.Labels = labels;
        this.Log = log;
        this.QuestionSet = questionSetName;

        _temporaryStringAnswers = new string[vals[0] * vals[1]];
        for (var i = 0; i < vals[0] * vals[1]; i++) _temporaryStringAnswers[i] = "";
    }

    public override void SaveTemporaryAnswer(string internalAnswer)
    {
        _temporaryStringAnswers[0] = internalAnswer;
    }

    public override void SaveTemporaryAnswer(int pos, string answerString)
    {
        _temporaryStringAnswers[pos] = answerString;
    }

    public override void SaveAnswer()
    {
        if (Log == null)
        {
            Debug.LogError("No LoggingManager. Cannot write to Database.");
            return;
        }
        KeyValuePair<int, string>[] entryArray = new KeyValuePair<int, string>[_temporaryStringAnswers.Length];
        var i = 0;
        foreach (var answer in _temporaryStringAnswers)
        {
            entryArray[i] = new KeyValuePair<int, string>(i, answer);
            i++;
        }
        Log.InsertAnswer(QuestionName, this.QuestionSet, entryArray);
    }
    
    public override bool AnswerGiven()
    {
        return _temporaryStringAnswers.All(answer => answer.Length != 0);
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
