using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.EVE.Scripts.Questionnaire2;

public class SingleChoiceTextQuestion : Question
{
    private const int QuestionType = 7;

    private int _temporaryIntAnswer = -1;
    private string _temporaryStringAnswer = "";

    public SingleChoiceTextQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log, int[] output)
    {
        this.QuestionName = questionName;
        QuestionText = text;
        this.Vals = vals;
        this.Labels = labels;
        this.Log = log;
        this.QuestionSet = questionSetName;
        this.Output = output;
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
        var entry = _temporaryStringAnswer != null ? new KeyValuePair<int, string>(_temporaryIntAnswer, _temporaryStringAnswer) : new KeyValuePair<int, string>(_temporaryIntAnswer, "");
        var entryArray = new KeyValuePair<int, string>[1];
        entryArray[0] = entry;
        Log.InsertAnswer(QuestionName, this.QuestionSet, entryArray);


    }

    public override void SaveTemporaryAnswer(string answer) {
        _temporaryStringAnswer = answer;
    }


  
    public override bool AnswerGiven()
    {
        return _temporaryIntAnswer > -1 && (Vals[3 + _temporaryIntAnswer] == 0 || _temporaryStringAnswer.Length != 0);
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
        // One column for selection of each row, and one column for potential text
        return 2 * Vals[1];
    }

    public override QuestionData ToQuestionData()
    {
        return new QuestionData(QuestionName, QuestionText, QuestionSet, QuestionType, Vals, Labels, Output);
    }
}
