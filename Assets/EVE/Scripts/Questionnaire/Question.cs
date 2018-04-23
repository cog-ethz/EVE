using Assets.EVE.Scripts.Questionnaire2;
using UnityEngine;

public abstract class Question
{

    protected string QuestionName;
    protected LoggingManager Log;

    protected string QuestionText;

    protected int[] Vals;
    protected string[] Labels;
    protected int[] Output;
    protected string QuestionSet;

    /*
	abstract public void display();
    */
    public abstract void SaveAnswer();
    public abstract bool AnswerGiven();
    public virtual void SaveTemporaryAnswer(int internalnumber) { }
    public virtual void SaveTemporaryAnswer(string internalAnswer) { }
    public virtual void SaveTemporaryAnswer(int number, string internalAnswer) { }
    public abstract string GetQuestionName();
    public abstract string GetQuestionText();
    public abstract string[] AnswerToString(object[][] answer);

    public abstract int GetRows();
    public abstract int GetColumns();
    public abstract int[] GetVals();
    public abstract string[] GetLabels();
    public abstract string GetQuestionSet();
    public abstract int GetQuestionType();
    public abstract int[] GetOutput();
    public abstract string[][] ToCSVTable();
    public abstract int GetNumberOutputColumns();

    public abstract QuestionData ToQuestionData();

}
