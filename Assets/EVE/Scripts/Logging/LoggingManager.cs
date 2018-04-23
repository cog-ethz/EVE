using UnityEngine;					// standart for Unity C#
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire2;
using Assets.EVE.Scripts.XML;
using Assets.EVE.Scripts.Questionnaire2.XMLHelper;

// for lists

public class LoggingManager
{
    //setup values for session

    private int currentSessionId = 0;
    private string currentQuestionnaireName;
    private string labChartFilePath = "";

    private DatabaseConnector dbConnector;

	public List<string> scenes = new List<string>();

    /// <summary>
    /// The logging manager takes care of all communications between the database and the framework.
    /// </summary>
    public LoggingManager()
    {
        
    }

    public void ConnectToServerAndCreateSchema(DatabaseSettings dbSettings)
    {
        ConnectToServer(dbSettings.Server, dbSettings.User, dbSettings.Password);
        CreateSchema();
        ConnectToServer(dbSettings);
    }

    public void ConnectToServer(DatabaseSettings settings)
    {
        try
        {
            dbConnector = new MySQLConnector();
            dbConnector.ConnectToServer(settings.Server, settings.Schema, settings.User, settings.Password);
            currentSessionId = dbConnector.GetNextSessionId();
        }
        catch (MySql.Data.MySqlClient.MySqlException mysqlEx)
        {
            Debug.LogError(mysqlEx.ToString());
            switch (mysqlEx.Number)
            {                  
                //http://dev.mysql.com/doc/refman/5.0/en/error-messages-server.html
                case 1042: // Unable to connect to any of the specified MySQL hosts (Check Server,Port)
                    currentSessionId = -2;
                    break;
                case 0: // Access denied (Check DB name,username,password)
                    if (mysqlEx.Message.Contains("Unknown database"))
                        currentSessionId = -4;
                    else
                        currentSessionId = -3;
                    break;
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
            currentSessionId = -1;
        }
    }

    public void ConnectToServer(string server, string user, string password)
    {
        try
        {
            dbConnector = new MySQLConnector();
            dbConnector.ConnectToServer(server, user, password);
            currentSessionId = dbConnector.GetNextSessionId();
        }
        
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
            currentSessionId = -1;
        }
    }


    // ------------------------------------------------
    //			Store and retrieve question answers
    //-------------------------------------------------

    /// <summary>
    /// Insert the answer to a question into the database
    /// </summary>
    /// <param name="questionName"> Name of the question the answer belongs to (name used in xml files</param>
    /// <param name="questionSetName"> Name of the question set the question belongs to</param>
    /// <param name="selectedIndeces"> Which answers where selected, and the values of them</param>
    public void InsertAnswer(string questionName, string questionSetName, KeyValuePair<int, string>[] selectedIndeces)
    {
        dbConnector.InsertAnswer(questionName, questionSetName, currentQuestionnaireName, currentSessionId, selectedIndeces);
    }
    
    public int[] readAnswerIndex(int questionId)
    {
        return dbConnector.readAnswerIndex(questionId, currentSessionId);        
    }

    public Dictionary<int,string> readAnswer(string questionName)
    {
        return dbConnector.readAnswer(questionName, currentSessionId);
    }

    private object[] readAllAnswerValues(string valueTypeName, int questionID, int sessionID, int valueCount)
    {
        throw (new NotImplementedException());
    }

    public string[] ReadAllAnswersToQuestion(int questionID, int sessionID, int questionnaireID)
    {
        throw (new NotImplementedException());
    }

    // -----------------------------------------
    //			Log experiment data
    //------------------------------------------

    /// <summary>
    /// Create an new experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    public void LogExperiment(string experimentName)
    {
        dbConnector.AddExperiment(experimentName);
    }

    /// <summary>
    /// Add scene to database
    /// </summary>
    /// <param name="sceneName"> Name of the scene</param>
    public void AddScene(string sceneName)
    {
        dbConnector.AddScene(sceneName);
    }

    /// <summary>
    /// Remove scene to database
    /// </summary>
    /// <param name="sceneName"> Name of the scene</param>
    public void RemoveScene(string sceneName)
    {
        dbConnector.RemoveScene(sceneName);
    }

	public List<string> GetExperimentScenes(string experimentName){
		int experimentId = dbConnector.getExperimentId(experimentName);
		scenes = dbConnector.GetExperimentScenes(experimentId);
		return scenes;
	}

    /// <summary>
    /// Set the order of the scenes of the experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    /// <param name="scenes">The scene names in order (can contain repetitions)</param>
    public void SetExperimentSceneOrder(string experimentName, string[] scenes)
    {
        dbConnector.SetExperimentSceneOrder(experimentName,scenes);
    }

     /// <summary>
    /// Remove the saved scene order of the experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    public void RemoveExperimentSceneOrder(string experimentName)
    {
        dbConnector.RemoveExperimentSceneOrder(experimentName);
    }
    
    /// <summary>
    /// Add a new parameter to an experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    /// <param name="parameterDescription"> Description of the parameter</param>
    public void CreateExperimentParameter(string experimentName, string parameterDescription)
    {
        dbConnector.CreateExperimentParameter(experimentName, parameterDescription);
    }

    // -----------------------------------------
    //			Log session data
    //------------------------------------------

    /// <summary>
    /// Log basic session information
    /// </summary>
    /// <param name="experimentName"> Name of the experiment this session belongs to</param>
    /// <param name="subjectId"> Id of subject of the experiment</param>
    public void LogSession(string experimentName, string subjectId)
    {
        dbConnector.AddSession(experimentName, subjectId);
    }

    // -----------------------------------------
    //			Log Session data
    //------------------------------------------

    /// Create a user answer for a given questionnaire
    /// </summary>
    /// <param name="sessionId"> Internal id of the session</param>
    /// <param name="questionnaireDescription"> Name of the questionnaire</param>
    public void CreateUserAnswer(int sessionId, string questionnaireDescription)
    {
        dbConnector.CreateUserAnswer(sessionId, questionnaireDescription);
    }

    /// <summary>
    /// Log a parameter used in a session
    /// </summary>
    /// <param name="parameterDescription"> Description of the parameter</param>
    /// <param name="value"> Value of the described parameter</param>
    public void LogSessionParameter(string parameterDescription, string value)
    {
        dbConnector.LogSessionParameter(currentSessionId, parameterDescription, value);
    }

    /// <summary>
    /// Log the labchart fileName used in the current session
    /// </summary>
    /// <param name="fileName"> fileName </param>
    public void LogLabChartFileName(string fileName)
    {
        dbConnector.AddLabchartFileName(currentSessionId, fileName);
    }

    public void RecordLabChartStartTime()
    {
        // NEW NAME AddLabchartStartTime
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

        dbConnector.AddLabchartStartTime(currentSessionId, timestamp);
    }

    private int getNextSessionID()
    {
        return dbConnector.GetNextSessionId();
    }

    public string getParameterValue(int sessionId, string parameterDescription)
    {
        return dbConnector.GetSessionParameter(sessionId, parameterDescription);
    }

    public string getParameterValue(string parameterDescription)
    {
        return dbConnector.GetSessionParameter(this.currentSessionId, parameterDescription);
    }

    // -----------------------------------------
    //			Log Questions
    //------------------------------------------

    public void InsertQuestionToDB(QuestionData question)
    {
        dbConnector.InsertQuestion(question);
    }

    public bool CreateQuestionSet(string name)
    {
        return dbConnector.CreateQuestionSet(name);
    }

    public void AddQuestionJumps(Assets.EVE.Scripts.Questionnaire2.Questions.Question q, string questionSet)
    {
        dbConnector.AddJumps(q,questionSet);
    }

    public void addQuestionJump(QuestionJumpImport jump, string questionSetName)
    {
        dbConnector.AddQuestionJumps(jump, questionSetName);
    }

    public List<Jump> GetJumps(QuestionData questionData)
    {
        var id = dbConnector.GetQuestionIdByName(questionData.QuestionName);
        var jumpIds = dbConnector.GetJumpIds(id);
        var jumps = (from jumpId in jumpIds
                     let dest = dbConnector.GetJumpDest(jumpId)
                     let destName = dest < 0 ? "*" : dbConnector.GetQuestionNameById(dest)
                     let activator = dbConnector.GetJumpCondition(jumpId)
                     select new Jump(destName, activator)).ToList();
        return jumps.Count>0?jumps:null;
    }

    public bool[,] getJumpConditions(List<int> jumpIds)
    {
        return dbConnector.GetJumpConditions(jumpIds);
    }

    public List<int> getQuestionsOfSet(string questionSetName)
    {
        return dbConnector.GetQuestionsOfSet(questionSetName);
    }

    public List<int> getJumpIds(int questionId)
    {
        return dbConnector.GetJumpIds(questionId);
    }

    public int getJumpDest(int jumpId)
    {
        return dbConnector.GetJumpDest(jumpId);
    }

    public void addQuestionnaire(string description)
    {
        dbConnector.AddQuestionnaire(description);
    }

    public void setupQuestionnaire(string description, string questionSetName)
    {
        dbConnector.SetupQuestionnaire(description, questionSetName);
    }

    public List<string> GetQuestionSets(string questionnaireName)
    {
        return dbConnector.GetQuestionSets(questionnaireName);
    }

    public List<string> GetQuestionnaireNames()
    {
        return dbConnector.GetAllQuestionnaireNames();
    }

    public List<string> GetQuestionSets(int questionnaireId)
    {
        return dbConnector.GetQuestionSets(questionnaireId);
    }

    /// <summary>
    /// Returns the structured data from all questions in a set.
    /// </summary>
    /// <param name="questionSet">Name of the set to be retrieved.</param>
    /// <returns>List of QuestionData objects to be parsed</returns>
    public List<QuestionData> GetQuestionSetContent(string questionSet)
    {
        var qIds = dbConnector.GetQuestionsOfSet(questionSet);
        var qDataList = new List<QuestionData>();
        foreach (var qId in qIds)
        {
            var questionVar = dbConnector.GetQuestionVars(qId);
            var questionName = questionVar[0].ToString();
            var questionText = questionVar[1].ToString();
            var type = int.Parse(questionVar[2].ToString());
            var vals = dbConnector.GetQuestionVals(qId);
            var labels = dbConnector.GetQuestionLabels(qId);
            var output = dbConnector.GetQuestionOutput(qId);
            qDataList.Add(new QuestionData(questionName,questionText, questionSet, type,vals,labels,output));
        }
        return qDataList;
    }

    public Question ReadQuestion(int internalQuestionId)
    {
        string questionSetName = "";
        int[] vals = null;
        string[] labels = null;
        int[] output = null;

        object[] questionVar = dbConnector.GetQuestionVars(internalQuestionId);

        string questionName = questionVar[0].ToString();
        string question_text = questionVar[1].ToString();
        int type = int.Parse(questionVar[2].ToString());

        vals = dbConnector.GetQuestionVals(internalQuestionId);
        labels = dbConnector.GetQuestionLabels(internalQuestionId);
        questionSetName = dbConnector.GetQuestionsSetName(internalQuestionId);
        output = dbConnector.GetQuestionOutput(internalQuestionId);

        Question question = null;
        switch (type)
        {
            case (int)QuestionType.TEXTANSWER:
                question = new TextQuestion(questionName, question_text, vals, labels, questionSetName, this);
                break;
            case (int)QuestionType.MULTIPLECHOICEANSWER:
                question = new MultipleChoiceQuestion(questionName, question_text, vals, labels, questionSetName, this, output);
                break;
            case (int)QuestionType.SINGLECHOICEANSWER:
                question = new SingleChoiceQuestion(questionName, question_text, vals, labels, questionSetName, this, output);
                break;
            case (int)QuestionType.SLIDERANSWER:
                question = new SliderQuestion(questionName, question_text, vals, labels, questionSetName, this);
                break;
            case (int)QuestionType.MANIKINANSWER:
                question = new ManikinQuestion(questionName, question_text, vals, labels, questionSetName, this);
                break;
            case (int)QuestionType.LADDERANSWER:
                question = new LadderQuestion(questionName, question_text, vals, labels, questionSetName, this);
                break;
            case (int)QuestionType.SINGLECHOICETEXTANSWER:
                question = new SingleChoiceTextQuestion(questionName, question_text, vals, labels, questionSetName, this, output);
                break;
            case (int)QuestionType.INFOSCREEN:
                question = new InfoScreenQuestion(questionName, question_text, vals, labels, questionSetName, this);
                break;
            case (int)QuestionType.MULTIPLECHOICETEXTANSWER:
                question = new MultipleChoiceTextQuestion(questionName, question_text, vals, labels, questionSetName, this, output);
                break;
        }
        return question;
    }

    // -----------------------------------------
    //			Log Scenes
    //------------------------------------------

    public void LogSceneStart(String name)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        InsertSystemEvent("Scene", "start", null, name, timestamp);
    }

    public void LogSceneEnd(String name)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        InsertSystemEvent("Scene", "end", null, name, timestamp);
    }

    public string[] getSceneTime(int sceneNumber, int sessionId)
    {
        string[] result = new string[2];

        List<string>[] sceneTimesList = dbConnector.GetSystemData("Scene", sessionId);        

        result[0] = sceneTimesList[2][2*sceneNumber];
        if (sceneTimesList[2].Count > 2 * sceneNumber + 1)
            result[1] = sceneTimesList[2][2*sceneNumber+1];
        return result;
    }

    public string getLabchartStarttime(int sessionId)
    {
        return dbConnector.GetLabChartStartTime(sessionId);
    }

    public int getNumberOfScenes(int sessionId)
    {
        int experimentId = dbConnector.getExperimentId(sessionId);
        int result = dbConnector.GetExperimentScenes(experimentId).Count;

        return result;

    }

    public void removeSession(int sessionId)
    {
        dbConnector.removeSession(sessionId);
    }

    public string[] getSceneNamesInOrder(string experimentName)
    {
        return dbConnector.GetExperimentScenes(dbConnector.getExperimentId(experimentName)).ToArray();
    }

    // -----------------------------------------
    //			Log Position, View and Input
    //------------------------------------------

    public void LogPositionAndView(float x, float y, float z, float ex, float ey, float ez)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        insert3DMeasurement("Player", "position", null, x.ToString(), y.ToString(), z.ToString(),timestamp);
        insert3DMeasurement("Player", "euler_angles", null, ex.ToString(), ey.ToString(), ez.ToString(), timestamp);

    }

    public string[] getListOfEnvironments(int sessionId)
    {
        List<string> result = new List<string>();

        Dictionary<DateTime, Dictionary<string, string>> data = dbConnector.GetSystemDataByTime("Scene", "start", sessionId);
        List<DateTime> dataKeys = new List<DateTime>(data.Keys);
        List<DateTime> dataSorted = SortDatesAscending(dataKeys);

        for (int i = 0; i < dataSorted.Count; i++)
        {
            result.Add(data[dataSorted[i]]["start"]);
        }
        return result.ToArray();
    }

    public List<float>[] getXYZ(int sessionId, int sceneNumber)
    {
        List<float>[] xyz = new List<float>[6];

        xyz[0] = new List<float>();
        xyz[1] = new List<float>();
        xyz[2] = new List<float>();
        xyz[3] = new List<float>();
        xyz[4] = new List<float>();
        xyz[5] = new List<float>();

        string[] sceneTime = getSceneTime(sceneNumber, sessionId);

        Dictionary<DateTime, string[]> dataPosition = dbConnector.Get3DMeasuredDataByTime("Player", "position", sessionId);
        Dictionary<DateTime, string[]> dataView = dbConnector.Get3DMeasuredDataByTime("Player", "euler_angles", sessionId);
        List<DateTime> dataKeys = new List<DateTime>(dataPosition.Keys);
        List<DateTime> dataSorted = SortDatesAscending(dataKeys);

        DateTime sceneStart = Convert.ToDateTime(sceneTime[0]);
        DateTime sceneEnd = Convert.ToDateTime(sceneTime[1]);
        for (int j = 0; j < dataSorted.Count; j++)
        {
            if (dataSorted[j] > sceneStart)
            {
                if (dataSorted[j] < sceneEnd)
                {
                    xyz[0].Add(float.Parse(dataPosition[dataSorted[j]][0]));
                    xyz[1].Add(float.Parse(dataPosition[dataSorted[j]][1]));
                    xyz[2].Add(float.Parse(dataPosition[dataSorted[j]][2]));
                    xyz[3].Add(float.Parse(dataView[dataSorted[j]][0]));
                    xyz[4].Add(float.Parse(dataView[dataSorted[j]][1]));
                    xyz[5].Add(float.Parse(dataView[dataSorted[j]][2]));
                }
                else
                    break;
            }
        }
        return xyz;
    }

    public List<float>[] getXYZ(int sessionId)
    {   
        string[] environments = getListOfEnvironments(sessionId);
        List<float>[] result = new List<float>[6];
        result[0] = new List<float>();
        result[1] = new List<float>();
        result[2] = new List<float>();
        result[3] = new List<float>();
        result[4] = new List<float>();
        result[5] = new List<float>();

        int envIdx = 0;
        for (int i = 0; i < environments.Length; i++)
        {
            List<float>[] tmp = getXYZ(sessionId, i);
            result[0].AddRange(tmp[0]);
            result[1].AddRange(tmp[1]);
            result[2].AddRange(tmp[2]);
            result[3].AddRange(tmp[3]);
            result[4].AddRange(tmp[4]);
            result[5].AddRange(tmp[5]);
            envIdx++;
        }
        return result;
    }

    public List<string> getXYZtimestamp(int sessionId, int sceneNumber)
    {
        List<string> xyzT = new List<string>();
        
        string[] sceneTime = getSceneTime(sceneNumber, sessionId);

        Dictionary<DateTime, string[]> dataPosition = dbConnector.Get3DMeasuredDataByTime("Player", "position", sessionId);
        List<DateTime> dataKeys = new List<DateTime>(dataPosition.Keys);
        List<DateTime> dataSorted = SortDatesAscending(dataKeys);

        DateTime sceneStart = Convert.ToDateTime(sceneTime[0]);
        DateTime sceneEnd = Convert.ToDateTime(sceneTime[1]);
        for (int j = 0; j < dataSorted.Count; j++)
        {
            if (dataSorted[j] > sceneStart)
            {
                if (dataSorted[j] < sceneEnd)
                {
                    xyzT.Add(dataSorted[j].ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
                else
                    break;
            }
        }
        return xyzT;
    }

    public void LogInput(string input)
    {
        InsertLiveSystemEvent("Player", "input", null, input);
    }

    public List<string>[] getAllInput(int sessionId, int sceneNumber)
    {
        List<string>[] result = new List<string>[2];

        List<string>[] input = dbConnector.GetSystemData("Player", "input", sessionId);

        if (input[0] != null) {             

            for (int i = 0; i < input[0].Count; i++)
            {
                DateTime time1DT = Convert.ToDateTime(input[0][i]);
                result[0].Add(time1DT.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                result[1].Add(input[1][i]);
            }         
        }
        return result;
    }

    public string getAbortTime(int sessionId, int sceneId)
    {

        List<string> timestamps = getXYZtimestamp(sessionId, sceneId);

        if (timestamps.Count > 0)
            return timestamps[timestamps.Count - 1];
        else
            return string.Empty;
    }

    public float timeDifference(string time1, string time2)
    {
        // time difference in microseconds

        DateTime time1DT = Convert.ToDateTime(time1);
        DateTime time2DT = Convert.ToDateTime(time2);

        TimeSpan difference = time2DT - time1DT;

        return (float)difference.TotalMilliseconds * 1000;
    }

    public TimeSpan timeDifferenceTimespan(string time1, string time2)
    {
        // time difference in microseconds

        DateTime time1DT = Convert.ToDateTime(time1);
        DateTime time2DT = Convert.ToDateTime(time2);

        TimeSpan difference = time2DT - time1DT;

        return difference;
    }

    // -----------------------------------------
    //			Get data for export
    //------------------------------------------


    public string[][] getAllSessionsData(string experimentName)
    {
        return dbConnector.GetAllSessionsData(experimentName);
    }

    public string[] getSessionData(int sessionId)
    {
        return dbConnector.GetSessionData(sessionId);
    }

    public int[] getQuestionnaireIds(int[] sessions)
    {
        return dbConnector.GetAnsweredQuestionnaireIds(sessions);
    }

    public List<int>[] getAnswerIDs(int[] questionnaires, int[] sessions)
    {
        return dbConnector.GetAnswerIds(questionnaires, sessions);
    }


    // -----------------------------------------
    //			Measured data
    //------------------------------------------


    public void insertMeasurement(String deviceName, String outputDescription, String unit, String value, String time)
    {
        dbConnector.AddDataOrigin(deviceName);
        dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        dbConnector.AddSensorData(deviceName, outputDescription, value, time, currentSessionId);
    }

    public void insertLiveMeasurement(String deviceName, String[] outputDescriptions, String[] units, String[] values)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        dbConnector.AddDataOrigin(deviceName);
        for (int i =0; i< outputDescriptions.Length; i++)
        {
            dbConnector.AddDataOutput(deviceName, outputDescriptions[i]);
            dbConnector.AddDataUnit(deviceName, outputDescriptions[i], units[i]);
            dbConnector.AddSensorData(deviceName, outputDescriptions[i], values[i], timestamp, currentSessionId);
        }              
    }

    public void insertLiveMeasurement(String deviceName, String outputDescription, String unit, String value)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        dbConnector.AddDataOrigin(deviceName);
        dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        dbConnector.AddSensorData(deviceName, outputDescription, value, timestamp, currentSessionId);
    }

    public void insert3DMeasurement(String deviceName, String outputDescription, String unit, String valueX, String valueY, String valueZ, String time)
    {
        dbConnector.AddDataOrigin(deviceName);
        dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        dbConnector.AddSensorData(deviceName, outputDescription, valueX, valueY, valueZ, time, currentSessionId);
    }

    public void insertLive3DMeasurement(String deviceName, String outputDescription, String unit, String valueX, String valueY, String valueZ)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        dbConnector.AddDataOrigin(deviceName);
        dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        dbConnector.AddSensorData(deviceName, outputDescription, valueX, valueY, valueZ, timestamp, currentSessionId);
    }

    public void InsertSystemEvent(String deviceName, String outputDescription, String unit, String value, String time)
    {
        dbConnector.AddDataOrigin(deviceName);
        dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        dbConnector.AddSystemData(deviceName, outputDescription, value, time, currentSessionId);
    }

    public void InsertLiveSystemEvent(String deviceName, String outputDescription, String unit, String value)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        dbConnector.AddDataOrigin(deviceName);
        dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        dbConnector.AddSystemData(deviceName, outputDescription, value, timestamp, currentSessionId);
    }

    // -----------------------------------------
    //			Blood pressure measurements
    //------------------------------------------
    public void insertBPMeasurement(string NBPs, string NBPd, string NBPm, string Sp02, string HR_pulse, string NBPs_unit, string NBPd_unit, string NBPm_unit, string Sp02_unit, string HR_pulse_unit, string time)
    {
        string device_name = "Blood pressure machine";
        if (NBPs != null)
            insertMeasurement(device_name, "NBPs", NBPs_unit, NBPs, time);
        if (NBPd != null)
            insertMeasurement(device_name, "NBPd", NBPd_unit, NBPd, time);
        if (NBPm != null)
            insertMeasurement(device_name, "NBPm", NBPm_unit, NBPm, time);
        if (Sp02 != null)
            insertMeasurement(device_name, "Sp02", Sp02_unit, Sp02, time);
        if (HR_pulse != null)
            insertMeasurement(device_name, "HR_pulse", HR_pulse_unit, HR_pulse, time);
    }

    public Dictionary<DateTime, Dictionary<string, string>> getMeasurementsByTime(string originName, int sessionId)
    {
        return dbConnector.GetMeasuredDataByTime(originName, sessionId);
    }

    public List<string>[] getSessionMeasurmentsAsString(string originName, int session_id)
    {
		return dbConnector.GetMeasurmentsDataAsString(originName, session_id);
    }

    public bool checkSchemaExists(string schemaName)
    {
        return dbConnector.CheckSchemaExists(schemaName);
    }

    public bool checkQuestionnaireExists(string questionnaireName)
    {
        return dbConnector.CheckQuestionnaireExists(questionnaireName);
    }

    public void RemoveExperimentParameter(string parameterName, string experimentName)
    {
        dbConnector.RemoveExperimentParameter(parameterName, experimentName);
    }

    public List<string> GetExperimentParameters(string experimentName)
    {
        return dbConnector.GetExperimentParameters(experimentName);
    }

    public void AddSensor(string deviceName)
    {
        dbConnector.AddDataOrigin(deviceName);
    }

    public void RemoveSensor(string deviceName)
    {
        dbConnector.RemoveDataOrigin(deviceName);
    }

    public List<string> getSensors()
    {
        List<string> origins = dbConnector.GetDataOrigins();
        origins.Remove("Player");
        origins.Remove("Scene");
        origins.Remove("Collectible");
        origins.Remove("EventMarker");
        origins.Remove("CharacterPlacement");
        return origins;
    }

    public void CreateSchema()
    {
        dbConnector.CreateSchema();
    }


    // -----------------------------------------
    //			Update Parameters
    //------------------------------------------
    public void updateParameters()
    {
        //same as in setup in constructor
        currentSessionId = getNextSessionID();
    }

    // -----------------------------------------
    //			get logging variables
    //------------------------------------------

    public int GetCurrentSessionID()
    {
        return currentSessionId;
    }

    public void SetLabChartFileName(string path)
    {
        labChartFilePath = path;
        LogLabChartFileName(path);
    }

    public string GetLabChartFileName()
    {
        return labChartFilePath;
    }

    public void setQuestionnaireName(string name)
    {
        this.currentQuestionnaireName = name;
    }

    // -----------------------------------------
    //			Helper functions
    //------------------------------------------

    static List<DateTime> SortDatesAscending(List<DateTime> list)
    {
        list.Sort((a, b) => a.CompareTo(b));
        return list;
    }
}
