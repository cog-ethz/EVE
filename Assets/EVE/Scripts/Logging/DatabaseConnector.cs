using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.Questionnaire.Questions;

public abstract class DatabaseConnector
{

    // ------------------------------------------------
    //			Store and retrieve question answers
    //-------------------------------------------------

    /// <summary>
    /// Opens a connection to a specific database on a server.
    /// </summary>
    /// <param name="server">server address</param>
    /// <param name="database">database name</param>
    /// <param name="user">user name</param>
    /// <param name="password">user password</param>
    public abstract void ConnectToServer(string server, string database, string user, string password);

    /// <summary>
    /// Opens a connection to a server with no database selected.
    /// </summary>
    /// <param name="server">server address</param>
    /// <param name="user">user name</param>
    /// <param name="password">user password</param>
    public abstract void ConnectToServer(string server, string user, string password);

    /// <summary>
    /// Insert the answer to a question into the database
    /// </summary>
    /// <param name="questionName"> Name of the question the answer belongs to (name used in xml files</param>
    /// <param name="questionSetName"> Name of the question set the question belongs to</param>
    /// <param name="questionnaireName"> Name of the questionnaire the question belongs to</param>
    /// <param name="sessionId"> Id of the sessions the answer belongs to</param>
    /// <param name="selectedIndeces"> Contains the the selected indeces (keys) and (if applicable) the values at the selecte indeces</param>
    public abstract void InsertAnswer(string questionName, string questionSetName, string questionnaireName, int sessionId, KeyValuePair<int, string>[] selectedIndeces);

    /// <summary>
    /// Get the selection index of the last given answer of a question
    /// </summary>
    /// <param name="questionId"> Internal (database) id of the questions</param>
    /// <param name="sessionId"> Id of the session the returned answer should belong to</param>
    /// <returns> int array with all indeces</returns>
    public abstract int[] readAnswerIndex(int questionId, int sessionId);

    /// <summary>
    /// Get the answer text of a question
    /// </summary>
    /// <param name="questionName"> name of the questions</param>
    /// <param name="sessionId"> Id of the session the returned answer should belong to</param>
    /// <returns>Returns the answers given to a question</returns>
    public abstract Dictionary<int,string> readAnswer(string questionName, int sessionId);

    // -----------------------------------------
    //			Experiment parameters
    //------------------------------------------

    /// <summary>
    /// Add a new parameter to an experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    /// <param name="parameterDescription"> Description of the parameter</param>
    public abstract void CreateExperimentParameter(string experimentName, string parameterDescription);

    /// <summary>
    /// Remove a parameter from the database
    /// </summary>
    /// <param name="parameterName"> Name of the parameter</param>
    /// <param name="experimentName"> Name of the experiment the parameter belongs to</param>
    public abstract void RemoveExperimentParameter(string parameterName, string experimentName);

    /// <summary>
    /// Get the parameters of an experiment from the database
    /// </summary>
    /// <param name="experimentName"> Name of the experiment the parameters belongs to</param>
    public abstract List<string> GetExperimentParameters(string experimentName);

    /// <summary>
    /// Log a parameter used in a session
    /// </summary>
    /// <param name="sessionId"> Id of the session</param>
    /// <param name="parameterDescription"> Description of the parameter</param>
    /// <param name="parameterValue"> Value of the described parameter</param>
    public abstract void LogSessionParameter(int sessionId, string parameterDescription, string parameterValue);
    
    /// <summary>
    /// Get the value of a parameter of a experiment session
    /// </summary>
    /// <param name="sessionId"> Internal id (databse) of the session</param>
    /// <param name="parameterName"> Name of the parameter</param>
    /// <returns> The value of the parameter as a string</returns>
    public abstract string GetSessionParameter(int sessionId, string parameterName);


    // -----------------------------------------
    //			Log experiment data
    //------------------------------------------

    /// <summary>
    /// Create an new experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    public abstract void AddExperiment(string experimentName);

    /// <summary>
    /// Remove a session from the database
    /// </summary>
    /// <param name="sessionId"> internal id of the session</param>
    public abstract void removeSession(int sessionId);

    /// <summary>
    /// Add scene to database
    /// </summary>
    /// <param name="sceneName"> Name of the scene</param>
    public abstract void AddScene(string sceneName);

    /// <summary>
    /// Remove scene to database
    /// </summary>
    /// <param name="sceneName"> Name of the scene</param>
    public abstract void RemoveScene(string sceneName);

    /// <summary>
    /// Set the order of the scenes of the experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    /// <param name="scenes">The scene names in order (can contain repetitions)</param>
    public abstract void SetExperimentSceneOrder(string experimentName, string[] scenes);

     /// <summary>
    /// Remove the saved scene order of the experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    public abstract void RemoveExperimentSceneOrder(string experimentName);



    // -----------------------------------------
    //			Log session data
    //------------------------------------------

    /// <summary>
    /// Log basic session information
    /// </summary>
    /// <param name="experimentName"> Name of the experiment this session belongs to</param>
    /// <param name="subjectId"> Id of subject of the experiment</param>
    public abstract void AddSession(string experimentName, string subjectId);


    /// <summary>
    /// Log the labchart file name used in the current session
    /// </summary>
    /// <param name="sessionId"> Id of the session</param>
    /// <param name="fileName"> file path</param>
    public abstract void AddLabchartFileName(int sessionId, string fileName);

    /// <summary>
    /// Record the current time on the local computer as the timestamp (start time of recording) of the labchart file
    /// </summary>
    /// <param name="sessionId"> Id of the session</param>
    /// <param name="timestamp"> timestamp of the start time</param>
    public abstract void AddLabchartStartTime(int sessionId, string timestamp);

    /// <summary>
    /// Get the time the labchart recording was started
    /// </summary>
    /// <param name="experimentId"> Id the of the experiment the data belongs to</param>
    /// <returns> Labchart start time (timestamp)</returns>
    public abstract string GetLabChartStartTime(int sessionId);

    /// <summary>
    /// Get the next internal session id (from the database)
    /// </summary>
    /// <returns> Next internal session id as integer</returns>
    public abstract int GetNextSessionId();



    /// <summary>
    /// Create a user answer for a given questionnaire
    /// </summary>
    /// <param name="sessionId"> Internal id of the session</param>
    /// <param name="questionnaireDescription"> Name of the questionnaire</param>
    public abstract void CreateUserAnswer(int sessionId, string questionnaireDescription);

    /// <summary>
    /// Get the scenes of an experiment
    /// </summary>
    /// <param name="experimentId"> Id the of the experiment the scenes belongs to</param>
    public abstract List<string> GetExperimentScenes(int experimentId);

    // -----------------------------------------
    //			Log Questions
    //------------------------------------------

    /// <summary>
    /// Add a question to a question set (in the database)
    /// </summary>
    /// <param name="question"> Question to add to the database</param>
    /// <param name="questionSetName"> Name of the question set</param>
    public abstract void InsertQuestion(QuestionData question);

    /// <summary>
    /// Create a new question set
    /// </summary>
    /// <param name="name"> Name of the new question set</param>
    /// <returns> Return true if creation was sucessfull </returns>
    public abstract bool CreateQuestionSet(String name);
    
    /// <summary>
    /// Get the conditions for the jumps given their ids
    /// </summary>
    /// <param name="jumpIds"> List of jump ids</param>
    /// <returns> Returns boolean matrix containing the patterns relating to the jumps </returns>
    public abstract bool[,] GetJumpConditions(List<int> jumpIds);

    /// <summary>
    /// Get the internal ids of the questions of a specific questoin set
    /// </summary>
    /// <param name="questionSetName"> Name of the question set</param>
    /// <returns> Returns a list of the question ids </returns>
    public abstract List<int> GetQuestionsOfSet(string questionSetName);

    /// <summary>
    /// Get the internal ids of jumps of a specific question
    /// </summary>
    /// <param name="questionId"> Internal id (database) of a question</param>
    /// <returns> Returns a list of the jump ids </returns>
    public abstract List<int> GetJumpIds(int questionId);

    /// <summary>
    /// Get the internal question id of the destination of a jump
    /// </summary>
    /// <param name="jumpId"> Internal id of a jump</param>
    /// <returns> Returns the question id </returns>
    public abstract int GetJumpDest(int jumpId);

    /// <summary>
    /// Add a new questionnaire
    /// </summary>
    /// <param name="name"> Name of the questoinnaire</param>
    public abstract void AddQuestionnaire(string name);

    /// <summary>
    /// Add a question set to a questionnaire
    /// </summary>
    /// <param name="questionnaireName"> Name of the questionnaire</param>
    /// <param name="questionSetName"> Name of the question set</param>
    public abstract void SetupQuestionnaire(string questionnaireName, string questionSetName);

    /// <summary>
    /// Get the question sets that are part of a questionnaire
    /// </summary>
    /// <param name="questionnaireName"> Name of the questionnaire</param>
    /// <returns> Returns the names of the question sets </returns>
    public abstract List<string> GetQuestionSets(string questionnaireName);

    /// <summary>
    /// Get the question sets that are part of a questionnaire
    /// </summary>
    /// <param name="questionnaireId"> Internal id of the questionnaire (database)</param>
    /// <returns> Returns the names of the question sets </returns>
    public abstract List<string> GetQuestionSets(int questionnaireId);

    /// <summary>
    /// Get the basic vars of a question
    /// </summary>
    /// <param name="internalQuestionID"> Internal id of the question (database)</param>
    /// <returns> Returns an array: Name (string), Question (string), Type (int), Allignment (string) </returns>
    public abstract object[] GetQuestionVars(int internalQuestionId);

    /// <summary>
    /// Get the interal values of a question that are used for the 
    /// </summary>
    /// <param name="internalQuestionID"> Internal id of the question (database)</param>
    /// <returns> First value is the number of columns, second number is the number of rows, an the rest of the array is used to determine if an answer option has a writable field </returns>
    public abstract int[] GetQuestionVals(int internalQuestionId);

    /// <summary>
    /// Returns all answer labels
    /// </summary>
    /// <param name="internalQuestionID"> Internal id of the question (database)</param>
    /// <returns> Labels of all the answer rows (and if applicable, columns) </returns>
    public abstract string[] GetQuestionLabels(int internalQuestionId);

    /// <summary>
    /// Get the name of the question set a question belongs to
    /// </summary>
    /// <param name="internalQuestionID"> Internal id of the question (database)</param>
    /// <returns> question set name </returns>
    public abstract string GetQuestionsSetName(int internalQuestionId);

    /// <summary>
    /// Get the the ansewr encoding of a question
    /// </summary>
    /// <param name="internalQuestionID"> Internal id of the question (database)</param>
    /// /// <returns> Encoding of each possible answer </returns>
    public abstract int[] GetQuestionOutput(int internalQuestionId);

    // -----------------------------------------
    //			Get data for export
    //------------------------------------------

    /// <summary>
    /// Get basic informations about the sessions of an experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    /// <returns> Returns the basic information of a session (session id, labchart file name, labchart file timestamp)</returns>
    public abstract string[][] GetAllSessionsData(string experimentName);

    /// <summary>
    /// Get basic informations about the session
    /// </summary>
    /// <param name="sessionId">Internal id of the sessions</param>
    /// <returns> Returns the basic information of a session (session id, labchart file name, labchart file timestamp)</returns>
    public abstract string[] GetSessionData(int sessionId);

    /// <summary>
    /// Get the internal id of the experiment a session belongs to
    /// </summary>
    /// <param name="sessionId"> Internal id of a session (database)</param>
    /// <returns> Returns the internal id (database) of the experiment</returns>
    public abstract int getExperimentId(int sessionId);

    /// <summary>
    /// Get the internal id of the experiment a session belongs to
    /// </summary>
    /// <param name="experimentName"> Name of the experiment (database)</param>
    /// <returns> Returns the internal id (database) of the experiment</returns>
    public abstract int getExperimentId(string experimentName);

    /// <summary>
    /// Get the ids of the questionnaires that have been answered in a specific session
    /// </summary>
    /// <param name="sessionIds"> Internal ids of sessions (database)</param>
    /// <returns> Returns the internal ids (database) of the questionnaires</returns>
    public abstract int[] GetAnsweredQuestionnaireIds(int[] sessionsIds);

    /// <summary>
    /// Get the ids of the questions that have been answered
    /// </summary>
    /// <param name="questionnaireIds"> Internal ids of questionnaires (database)</param>
    /// <param name="sessionIds"> Internal ids of sessions (database)</param>
    /// <returns> Returns the internal ids (database) of the questions</returns>
    public abstract List<int>[] GetAnswerIds(int[] questionnaireIds, int[] sessionsIds);


    // -----------------------------------------
    //			Sensor information
    //------------------------------------------

    /// <summary>
    /// Get all data origins that are regsitered in the database
    /// </summary>
    public abstract List<string> GetDataOrigins();

    /// <summary>
    /// Add a new data origin (if it doesn't already exist)
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    public abstract void AddDataOrigin(String originName);

    /// <summary>
    /// Remove a new data origin (if it exists)
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    public abstract void RemoveDataOrigin(String originName);

    /// <summary>
    /// Add a particular data output of a data origin (if it doesn't already exist)
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="outputDescription"> Description of the output</param>
    public abstract void AddDataOutput(String originName, String outputDescription);

    /// <summary>
    /// Add a unit for a data output (if it doesn't already exist)
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="outputDescription"> Description of the data output</param>
    /// <param name="unitName"> Name of the data unit</param>
    public abstract void AddDataUnit(String originName, String outputDescription, String unitName);


    // -----------------------------------------
    //			Measured data
    //------------------------------------------

    /// <summary>
    /// Add measured sensor data (1 dimensional)
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="outputDescription"> Description of the data output</param>
    /// <param name="unitName"> Name of the data unit</param>
    /// <param name="value"> Value of the measured data</param>
    /// <param name="time"> Timestamp of the measured data</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    public abstract void AddSensorData(String originName, String outputDescription, String value, String time, int sessionId);

    /// <summary>
    /// Add measured sensor data (3 dimensional)
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="outputDescription"> Description of the data output</param>
    /// <param name="unitName"> Name of the data unit</param>
    /// <param name="valueX"> X value of the measured data</param>
    /// <param name="valueY"> Y value of the measured data</param>
    /// <param name="valueZ"> Z value of the measured data</param>
    /// <param name="time"> Timestamp of the measured data</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    public abstract void AddSensorData(String originName, String outputDescription, String valueX, String valueY, String valueZ, String time, int sessionId);

    /// <summary>
    /// Add system data
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="outputDescription"> Description of the data output</param>
    /// <param name="unitName"> Name of the data unit</param>
    /// <param name="value"> Value of the data</param>
    /// <param name="time"> Timestamp of the data</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    public abstract void AddSystemData(String originName, String outputDescription, String value, String time, int sessionId);

    /// <summary>
    /// Get the measured data of a specific origin 
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    /// <returns> Data measured (Format: Timestamp -> (output description -> value))</returns>
    public abstract Dictionary<DateTime, Dictionary<string, string>> GetMeasuredDataByTime(string originName, int sessionId);

    /// <summary>
    /// Get the measured data of a specific origin 
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    /// <returns> Data measured (Format: output description -> ( value -> Timestamp))</returns>
    public abstract Dictionary<string, Dictionary<string, List<DateTime>>> GetMeasuredDataByName(string originName, int sessionId);

    /// <summary>
    /// Get the measured data of a specific origin 
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    /// <returns> Data measured (Format: Timestamp -> (output description -> value))</returns>
    public abstract Dictionary<DateTime, string[]> Get3DMeasuredDataByTime(string originName, string description, int sessionId);

    /// <summary>
    /// Get the measured data of a specific origin in a list of strings
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    /// <returns> Data measured (Format: Timestamp -> (output description -> value))</returns>
    public abstract List<string>[] GetMeasurmentsDataAsString(string originName, int sessionId);

    /// <summary>
    /// Get the system data of a specific origin 
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    /// <returns> Data measured (Format: Timestamp -> (output description -> value))</returns>
    public abstract Dictionary<DateTime, Dictionary<string, string>> GetSystemDataByTime(string originName, string description, int sessionId);

    /// <summary>
    /// Get the recorded system data of a session
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    /// <returns> Result[0] contains the time description; Result[1] contains the values; Result[2] contains the times</returns>
    public abstract List<string>[] GetSystemData(string originName, int sessionId);

    /// <summary>
    /// Get the recorded system data of a session
    /// </summary>
    /// <param name="originName"> Name of the data origin</param>
    /// <param name="description">  description of the data</param>
    /// <param name="sessionId">  Id the of the session the data belongs to</param>
    /// <returns> Result[0] contains the values; Result[1] contains the times</returns>
    public abstract List<string>[] GetSystemData(string originName, string description, int sessionId);



    // -----------------------------------------
    //			Database test functions
    //------------------------------------------

    /// <summary>
    /// Check if a database scheme exists
    /// </summary>
    /// <param name="schemaName"> Name of the database schema</param>
    /// <returns> Returns true if schema exits</returns>
    public abstract bool CheckSchemaExists(string schemaName);

    /// <summary>
    /// Check if a database scheme exists
    /// </summary>
    /// <param name="questionnaireName"> Name of the database schema</param>
    /// <returns> Returns true if schema exits</returns>
    public abstract bool CheckQuestionnaireExists(string questionnaireName);

    public abstract void CreateSchema();

    public abstract void AddJumps(Question question1, string questionSetName);
    public abstract int GetQuestionIdByName(string name);
    public abstract string GetQuestionNameById(int id);
    public abstract string GetJumpCondition(int jumpId);
    public abstract List<string> GetAllQuestionnaireNames();
}
