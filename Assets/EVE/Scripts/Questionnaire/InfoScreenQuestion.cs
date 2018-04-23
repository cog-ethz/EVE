using UnityEngine;
using System.Collections;
using System;
using Assets.EVE.Scripts.Questionnaire2;

public class InfoScreenQuestion : Question
{
    private const int QuestionType = 8;


    public InfoScreenQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log)
    {
        this.QuestionName = questionName;
        QuestionText = text;
        this.Vals = vals;
        this.Labels = labels;
        this.Log = log;
        this.QuestionSet = questionSetName;



    }

    public override void SaveAnswer()
    {
        if (Log == null)
        {
            Debug.LogError("No LoggingManager. Cannot write to Database.");
        }
    }
    public override bool AnswerGiven()
    {
        return true;
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


    public override string GetQuestionSet()
    {
        return QuestionSet;
    }


    public override int GetQuestionType()
    {
        return QuestionType;
    }

    public override int GetRows()
    {
        if (Vals != null)
            return Vals[0];
        return -1;
    }

    public override int GetColumns()
    {
        if (Vals != null)
            return Vals[1];
        return -1;
    }

    public override int[] GetVals()
    {
        return Vals;
    }

    public override string[] GetLabels()
    {
        return Labels;
    }

    public override int[] GetOutput()
    {
        return Output;
    }

    public override int GetNumberOutputColumns()
    {
        return 1;
    }

    public override QuestionData ToQuestionData()
    {
        return new QuestionData(QuestionName,QuestionText,QuestionSet,QuestionType,Vals,Labels,Output);
    }
}
