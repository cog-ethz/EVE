using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire2;

/// <summary>
///  Multiple Choice Question that allows one answer with a text field.
/// </summary>
public class MultipleChoiceTextQuestion : Question
{
    private const int QuestionType = 9;

    private List<int> _answerNumbers = new List<int>();
    private string _temporaryStringAnswer = "";

    public MultipleChoiceTextQuestion(string questionName, string text, int[] vals, string[] labels, string questionSetName, LoggingManager log, int[] output)
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
        _answerNumbers.Add(answerNumber);
    }

    public override void SaveTemporaryAnswer(string answer)
    {
        _temporaryStringAnswer = answer;
    }

    public override void SaveAnswer()
    {
        int[] countOfElement = new int[Labels.Length];
        foreach (var element in _answerNumbers)
        {
            countOfElement[element]++;
        }
        int answer = 0;
        var answerFinal = new List<int>();
        foreach (var clickedButton in countOfElement)
        {
            if (clickedButton % 2 == 1)
            {
                answerFinal.Add(answer);
            }
            answer++;
        }
        //check for unequal number of occurences of answernumber, save them
        KeyValuePair<int, string>[] entryArray = new KeyValuePair<int, string>[answerFinal.Count];
        for (var i = 0; i < answerFinal.Count; i++)
        {
            if (Vals[answerFinal[i]+2] != 0)
            {
                entryArray[i] = new KeyValuePair<int, string>(answerFinal[i], _temporaryStringAnswer);
            }
            else
            {
                entryArray[i] = new KeyValuePair<int, string>(answerFinal[i], "");
            }
        }
        if (Log == null)
        {
            Debug.LogError("No LoggingManager. Cannot write to Database.");
            return;
        }
        Log.InsertAnswer(QuestionName, this.QuestionSet, entryArray);
        _answerNumbers = new List<int>();
    }


    public override bool AnswerGiven()
    {
        var countOfElement = new int[Labels.Length];
        foreach (var element in _answerNumbers)
        {
            countOfElement[element]++;
        }
        return (from clickedButton in countOfElement where clickedButton%2 == 1 select Vals[3 + clickedButton] == 0 || _temporaryStringAnswer.Length != 0).FirstOrDefault();
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
