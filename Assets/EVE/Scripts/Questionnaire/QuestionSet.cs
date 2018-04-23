using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class QuestionSet
{

    //question params
    private string name;
    private List<Question> questionList;
    private List<QuestionJump> jumpList;

    //---------------------------
    // 		Constructor
    //---------------------------
    public QuestionSet(string name)
    {
        this.name = name;
        questionList = new List<Question>();
        jumpList = new List<QuestionJump>();
    }

    //---------------------------
    // 		Set & Get
    //---------------------------


    public string getName()
    {
        return name;
    }

    public void addQuestion(Question q, QuestionJump j)
    {
        questionList.Add(q);
        jumpList.Add(j);
    }

    public void addQuestion(Question q)
    {
        questionList.Add(q);
        QuestionJump j = new QuestionJump();
        jumpList.Add(j);
    }

    public List<Question> getQuestions()
    {
        return questionList;
    }

    public Question getQuestion(int i)
    {
        return questionList[i];
    }

    public int getPageAmount()
    {
        return questionList.Count();
    }

    public int getJumpSize(int i)
    {
        return jumpList[i].getJumpForGivenAnswer();
    }



}
