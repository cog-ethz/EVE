using UnityEngine;
using System.Collections;
using System;
using Assets.EVE.Scripts.Questionnaire2;

public class SliderQuestion : Question
{
    private const int QuestionType = 4;

    private readonly float[] _sliderAnswers;
    

    public SliderQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log)
    {
        this.QuestionName = questionName;
        QuestionText = text;
        this.Vals = vals;
        this.Labels = labels;
        this.Log = log;
        this.QuestionSet = questionSetName;

        _sliderAnswers = new float[vals[0]];
        for (var i = 0; i < vals[0]; i++) _sliderAnswers[i] = vals[i + 1];
    }
    
    public override void SaveAnswer()
    {
        if (Log == null)
        {
            Debug.LogError("No LoggingManager. Cannot write to Database.");
            return;
        }

        throw new NotImplementedException();

    }
    public override bool AnswerGiven()
    {
        // There is no way to distinguish if the question is skipped or if the participant wants to use the default value of the slider
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
        throw new NotImplementedException();
    }

    public override QuestionData ToQuestionData()
    {
        return new QuestionData(QuestionName, QuestionText, QuestionSet, QuestionType, Vals, Labels, Output);
    }
}
