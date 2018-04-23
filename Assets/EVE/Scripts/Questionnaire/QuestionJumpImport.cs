using UnityEngine;
using System.Collections;
using System.Linq;

public class QuestionJumpImport
{

    private object[,] conditions;		//condition for jump to destination i

    private string questionName;
    private string[] destNames;

    //---------------------------
    // 		Constructor
    //---------------------------


    public QuestionJumpImport(string questionName, string[] destNames, object[,] conditions)
    {
        // Used during XML import (ids and therefore jumplength are not defined)
        this.questionName = questionName;
        this.destNames = destNames;
        this.conditions = conditions;
    }

    public string getQuestionName()
    {
        return questionName;
    }

    public string[] getDestNames()
    {
        return destNames;
    }

    public object[,] getConditions()
    {
        return conditions;
    }
}
