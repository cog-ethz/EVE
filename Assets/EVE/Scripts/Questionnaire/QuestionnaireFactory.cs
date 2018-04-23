﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;

public class QuestionnaireFactory
{
	
	//private int questionID;
	//private int labelCount;
	//private string currentText;
	//private string currentKey;
	private string currentValue;
	private List<string> questionnaireNames;
    public Dictionary<string, QuestionSet> questionSets;
	//public Dictionary<string, Dictionary<int, QuestionJump>> jumpSets;
	
//	private Dictionary<string, SimpleQuestion> currentQuestionSet;
//	private Dictionary<int, QuestionJump> currentJumpSet;
	
	//private GUISkin skin;
	private LoggingManager log;

    List<string> currentQuestionSetList;
    String currentQuestionnaireName;

    Dictionary<string, List<string>> questionnaireSets;
	
	Dictionary<string, string> questionSetKeys;
	//Dictionary<string, Dictionary<string, string>> questionJumpKeys;
	//Dictionary<string, Dictionary<string, string>> questionLabelKeys;
	//Dictionary<string, Dictionary<string, string>> questionValueKeys;
	//Dictionary<string, Dictionary<string, string>> questionTextKeys;
//	Dictionary<string, List<string> > questionKeys;
	//Dictionary<string, string> textKeys;
	//Dictionary<string, string> textValues;
	//Dictionary<string, string> valueKeys;
	//Dictionary<string, string> labelKeys;
	//Dictionary<string, string> jumpKeys;
	//Dictionary<string, string> jumpValues;
	//ArrayList jumpTable;
	
	XmlTextReader textReader;
	
	bool inQuestionSet;
	bool inQuestion;
	bool inText;
	bool inValue;
	bool inLabel;
	bool inJump;
	
	public QuestionnaireFactory (string[] files, LoggingManager log){
		
		this.log = log;
		//this.skin = skin;
		
		//this.questionID = 0;
		questionnaireNames = new List<string>();
        currentQuestionnaireName = "";
		//jumpSets = new Dictionary<string, Dictionary<int, QuestionJump>> ();
        questionSets = new Dictionary<string, QuestionSet>();
        questionnaireSets = new Dictionary<string,List<string>>();

		foreach (string file in files) {
			textReader = new XmlTextReader (file);
			
			//currentQuestionSet = new Dictionary<string, SimpleQuestion > ();
			textReader.Read ();
			
			// If the node has value
			
			string text = "";
			text += "XmlTextReader Properties Test\n";
			
			bool validQuestionnaire = true;
			
			this.inQuestionSet = false;
			this.inQuestion = false;
			this.inText = false;
			this.inValue = false;
			this.inLabel = false;
			this.inJump = false;
			
			//this.currentText = "";
			//this.currentKey = "";
			this.currentValue = "";
			
			questionSetKeys = new Dictionary<string, string> ();
            currentQuestionSetList = new List<string>();
			
			//questionLabelKeys = new Dictionary<string, Dictionary<string, string>> ();
			//questionJumpKeys = new Dictionary<string, Dictionary<string, string>> ();
			//questionValueKeys = new Dictionary<string, Dictionary<string, string>> ();
			//questionTextKeys = new Dictionary<string, Dictionary<string, string>> ();
			//questionKeys = new Dictionary<string, List<string> > ();
			//textKeys = new Dictionary<string, string> ();
			//textValues = new Dictionary<string, string> ();
			//valueKeys = new Dictionary<string, string> ();
			//labelKeys = new Dictionary<string, string> ();
			//jumpKeys = new Dictionary<string, string> ();
			//jumpValues = new Dictionary<string, string> ();
			
			//jumpTable = new ArrayList();
			
			//labelCount = 0;
			
			while (textReader.Read() && validQuestionnaire) {
				
				XmlNodeType nType = textReader.NodeType;
				
				// skip empty node types
				if (nType == XmlNodeType.Whitespace) {				
					//Debug.Log("Skip WhiteSpace");
					continue;
				} 
				//backtrack state in end-nodes and store complete questions
				else if (nType == XmlNodeType.EndElement) {
					BacktrackState();
				} 
				// extract meta data from structural elements
				else if (nType == XmlNodeType.Element) {
					ClassifyNode();
					ExtractAttributes();
				}
				// extract content of text element
				else  if (nType == XmlNodeType.Text) {
					Debug.LogWarning ("Text in question sets is not supported.");
				} else {
					Debug.LogWarning ("Unhandled XML Node: " + nType.ToString () + " " + textReader.Name.ToString ());
				}
			}
			questionnaireNames.Add(questionSetKeys["name"]);
			questionSetKeys.Clear ();
		}       
	}
	
	//Cleanup state after reading a complete node. Create question when question element is closed.
	private void BacktrackState(){
		//Close question set
		if (textReader.Name.ToString () == "questionnaire") {
			//.Log ("ElementEnd");
            questionnaireSets.Add(currentQuestionnaireName, currentQuestionSetList);
            currentQuestionSetList = new List<string>() ;
			inQuestionSet = false;
		} 
		//Close question
		else if (textReader.Name.ToString () == "question_set") {
			//.Log ("\tQElementEnd");
            //questionKeys.Clear();
			//labelCount = 0;
			inQuestion = false;
		}
	}
	
	private void ClassifyNode(){
		//Only process valid questionnaire
		if (!inQuestionSet && textReader.Name.ToString () != "questionnaire") {
			Debug.LogError ("The xml does not contain a questionnaire, but: " + textReader.Name.ToString ());
		}
		//Open questionnaire
		else if (textReader.Name.ToString () == "questionnaire") {
			//Debug.Log ("Element: " + textReader.Name.ToString ());
			inQuestionSet = true;
		}
		//Only process if question set is valid
		else if (!inQuestion && textReader.Name.ToString () != "question_set") {
			Debug.LogError ("The xml does not contain a question, but: " + textReader.Name.ToString ());
		}
		//Open question set
		else if (textReader.Name.ToString () == "question_set") {
			//Debug.Log ("\tQElement: " + textReader.Name.ToString ());
			inQuestion = true;
		}
	}
	
	private void ExtractAttributes(){
		if (textReader.AttributeCount > 0) {
			//read out questionnaire attributes
			if (!inQuestion && !inText && !inValue && !inLabel && !inJump) {
				for (int attInd = 0; attInd < textReader.AttributeCount; attInd++) {
					textReader.MoveToAttribute (attInd);
					currentValue = textReader.Value;
					//if (textReader.Name == "name")
						//currentKey = currentValue;
					//Debug.Log ("\t\tAttribute: " + textReader.Name + " - " + currentValue);
					questionSetKeys.Add (textReader.Name, currentValue);
                    currentQuestionnaireName = currentValue;
				}
			}
			//read out question set attributes
			else 	if (inQuestion && !inText && !inValue && !inLabel && !inJump) {
				for (int attInd = 0; attInd < textReader.AttributeCount; attInd++) {
					textReader.MoveToAttribute (attInd);
					currentValue = textReader.Value;
					//if (textReader.Name == "name")
						//currentKey = currentValue;
					//Debug.Log ("\t\t\tQAttribute: " + textReader.Name + " - " + currentValue);
                    currentQuestionSetList.Add(currentValue);
				}
			}
			textReader.MoveToElement (); 
		} else {
			Debug.LogError("Undefined question set object: "  + textReader.Name.ToString ());
		}
	}

   public void storeAllQuestionnairesInDB()
    {
        foreach (string QName in questionnaireNames)
        {
            storeQuestionnaireInDB(QName);
        }
    }

    private void storeQuestionnaireInDB(string questionnaireDescription)
    {
        // Create a questionnaire entry in DB
        log.addQuestionnaire(questionnaireDescription);
        // Then for each question set of the questionnaire add an entry into DB (q_q_sets table)
        foreach (string qsName in questionnaireSets[questionnaireDescription])
        {
            log.setupQuestionnaire(questionnaireDescription, qsName);
        }
    }

	private void RequestQuestionSet(){
		//TODO create questionFactory OR database access and load questions, store locally

	}

}
