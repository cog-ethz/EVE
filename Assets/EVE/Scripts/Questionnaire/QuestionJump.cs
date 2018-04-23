using UnityEngine;
using System.Collections;
using System.Linq;

public class QuestionJump {

	private int questionID;
	private int jumpsN;
	private LoggingManager log;

	private int[] 		jump;
	private bool[,] 	conditions;
	private bool noSkipping;

	//---------------------------
	// 		Constructor
	//---------------------------

	//default jump of +1
	public QuestionJump(){
		this.questionID = -1;
		//this.noSkipping = true;
	}

	public QuestionJump(int questionID, int[] jump, bool[,] conditions, LoggingManager log){
        // Used when loading questions out of database (ids are defined)
		this.questionID = questionID;
		this.jumpsN 	= jump.Length;
		this.jump 		= jump;
		this.conditions = conditions;
		this.log 		= log;
		this.noSkipping = false;
	}



	public int getJumpForGivenAnswer(){

		//Debug Variable to see all questions 
		if (noSkipping)
			return 1;

        int[] selectedIndeces;

		if( questionID >= 0 ) 	
			//if questionID >= 0 means that all other values also got set
			selectedIndeces = log.readAnswerIndex(questionID);
		else 
			//if not, we have the default jump
			return 1;	

		//if no condition (so in all cases, not depending on answer), return the stored jump
		if (conditions == null) return jump [0];

		//if answer could not be read, use the normal jump
		if (selectedIndeces.Length == 0) return 1;

        bool[] answer = new bool[conditions.GetLength(1)];

        for (int i = 0; i < answer.Length; i++)
        {
            for (int j = 0; j < selectedIndeces.Length; j++)
            {
                if (i == selectedIndeces[j])
                {
                    answer[i] = true;
                } else
                {
                    answer[i] = false;
                }        
            }
            
        }

        for (int i = 0; i < jumpsN; i++)
        {
            int matched = 0;
            for (int j = 0; j < answer.Length; j++)
            {
                if (!answer[j].Equals(conditions[i, j])) break;
                matched++;
            }
            if (matched == answer.Length) return jump[i];
        }

        return 1;

	}

  /*  public int getNextQuestionID()
    {
        //read answer from database
        bool[] answer;

        if (questionID >= 0 )
            //if questionID >= 0 means that all other values also got set
            answer = log.ReadAnswer(questionID);
        else
            //if not, we have the default jump
            return 1;

        //if no condition (so in all cases, not depending on answer), return the stored jump
        if (conditions == null) return jump[0];

        //if answer could not be read, use the normal jump
        if (answer == null) return 1;

        //loop through conditions and compare
        for (int i = 0; i < jumpsN; i++)
        {
            int matched = 0;
            for (int j = 0; j < answer.Length; j++)
            {
                if (!answer[j].Equals(conditions[i, j])) break;
                matched++;
            }
            if (matched == answer.Length) return jump[i];
        }

        //default jump
        return 1;
    }*/

    public bool[,] getConditions()
    {
        return conditions;
    }
}
