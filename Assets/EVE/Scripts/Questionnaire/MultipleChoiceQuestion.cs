using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire2;

public class MultipleChoiceQuestion : Question
{
    private const int QuestionType = 2;

    private List<int> _answersClicked=new List<int>();
    

    public MultipleChoiceQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log, int[] output)
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
        _answersClicked.Add(answerNumber);
    }

    public override void SaveAnswer()
    {
        var countOfElement = new int[Labels.Length];
        foreach (var element in _answersClicked) {
            countOfElement[element]++;
        }
        var answer = 0;
        var answerFinal = new List<int>();
        foreach (var clickedButton in countOfElement) {
            if (clickedButton%2==1) {
                answerFinal.Add(answer);
            }
            answer++;
        }
        //check for unequal number of occurences of answernumber, save them
        var entryArray = new KeyValuePair<int, string>[answerFinal.Count];
        for (var i= 0; i < answerFinal.Count; i++) {            
            var entry = new KeyValuePair<int, string>(answerFinal[i], "");
            entryArray[i] = entry;
        }
        if (Log == null)
        {
            Debug.LogError("No LoggingManager. Cannot write to Database.");
            return;
        }
        Log.InsertAnswer(QuestionName, this.QuestionSet, entryArray);
        _answersClicked = new List<int>();
    }


    public override bool AnswerGiven()
    {
        var countOfElement = new int[Labels.Length];
        foreach (var element in _answersClicked)
        {
            countOfElement[element]++;
        }
        return countOfElement.Any(clickedButton => clickedButton%2 == 1);
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
        return Vals[0] * Vals[1];
    }
    public override QuestionData ToQuestionData()
    {
        return new QuestionData(QuestionName, QuestionText, QuestionSet, QuestionType, Vals, Labels, Output);
    }
    
}
