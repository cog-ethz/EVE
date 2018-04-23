using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class QuestionnaireBuilder {
	
	private LoggingManager log;


	public QuestionnaireBuilder(LoggingManager log){
		this.log = log;
	}

    public Dictionary< string, QuestionSet > LoadQuestionnaire(string name) {
        Dictionary< string, QuestionSet> questionnaire = new Dictionary<string,QuestionSet>();
        // Load question sets names from DB
        List<string> qsNames = log.GetQuestionSets(name);
        // Load all questionSets
        foreach (string qs in qsNames) {
            questionnaire.Add(qs, getQuestionSet(qs));
        }
        return questionnaire;
    }

    private QuestionSet getQuestionSet(string name)
    {
        QuestionSet qSet = new QuestionSet(name);

        // Load list of question ids from DB
        List<int> questionIDs = log.getQuestionsOfSet(name);
        // Loop through all questions in list
        foreach (int id in questionIDs)
        {
            // Check if a question has jumps
            List<int> jumps = log.getJumpIds(id);
            if (jumps.Count > 0)
            {
                //      get indeces of destination in questionIDs list -> jumps are differences of indeces to origins index
                int[] jumpLength = new int[jumps.Count];
                for (int i = 0; i < jumps.Count; i++)
                {
                    int jumpL = log.getJumpDest(jumps[i]);
                    if (jumpL > 0)
                    {
                        jumpLength[i] = log.getJumpDest(jumps[i]) - id;
                    }
                    else
                    {
                        jumpLength[i] = 0;
                    }
                }
                //      conditions can be loaded from database (using the jump_id) (check QuestionJumpImport)
                 bool[,] conditions = log.getJumpConditions(jumps);
                
                QuestionJump jumpObj = new QuestionJump(id, jumpLength, conditions,log);
                // Add question to question set (with jump)
                qSet.addQuestion(log.ReadQuestion(id), jumpObj);
            }
            else
            {
                // Add question to question set (without jump)
                qSet.addQuestion(log.ReadQuestion(id));
            }
        }

        return qSet;
    }

	public void saveAllQuestionsToDatabase(string path){
		
        string[] fileNames = Directory.GetFiles(@""+path+"QuestionSets/", "*.xml");

		QuestionFactory qFactory = new QuestionFactory(fileNames,log);

		foreach ( KeyValuePair< string, Dictionary<string, Question> > questionSet in qFactory.questionSets) {
            // Create question set in DB
            string qsName = questionSet.Key;
            bool qsCreated = log.CreateQuestionSet(qsName);
			foreach(Question question in questionSet.Value.Values){
                // Add question to DB
                log.InsertQuestionToDB(question.ToQuestionData());
			}
            // Only add jumps if the question set is new in the database
            if (qsCreated)
            {
                // Add jumps to database
                foreach (QuestionJumpImport jump in qFactory.jumpSets[qsName].Values)
                {
                    log.addQuestionJump(jump, qsName);
                }
            }
        }
        string[] questionnaireFiles = Directory.GetFiles(@""+path+"" +
                                                         "Questionnaires" +
                                                         "/", "*.xml"); ;

        // Load the questionnaire
        QuestionnaireFactory qf = new QuestionnaireFactory(questionnaireFiles, log);
        
        qf.storeAllQuestionnairesInDB();


	}

}
