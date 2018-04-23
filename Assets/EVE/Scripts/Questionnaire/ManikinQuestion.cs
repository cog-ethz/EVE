using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.EVE.Scripts.Questionnaire2;

public class ManikinQuestion : Question
{
    private const int QuestionType = 5;

    private int _temporaryIntAnswer = -1;

    public ManikinQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log)
    {
        this.QuestionName = questionName;
        QuestionText = text;
        this.Vals = vals;
        this.Labels = labels;
        this.Log = log;
        this.QuestionSet = questionSetName;
        
    }
  
    public override void SaveTemporaryAnswer(int answerNumber)
    {
        _temporaryIntAnswer = answerNumber;
    }

    public override void SaveAnswer()
    {
        if (Log == null)
        {
            Debug.LogError("No LoggingManager. Cannot write to Database.");
            return;
        }
        KeyValuePair<int, string> entry = new KeyValuePair<int, string>(_temporaryIntAnswer, "");
        KeyValuePair<int, string>[] entryArray = new KeyValuePair<int, string>[1];
        entryArray[0] = entry;
        Log.InsertAnswer(QuestionName, this.QuestionSet, entryArray);
    }
    
    public override bool AnswerGiven()
    {
        return _temporaryIntAnswer > -1;
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
