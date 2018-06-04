using UnityEngine;					// standart for Unity C#
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.XML;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;
using Assets.EVE.Scripts.Questionnaire.Questions;

// for lists

public class LoggingManager
{
    //setup values for session

    private int _currentSessionId = 0;
    private string _currentQuestionnaireName;
    private string _labChartFilePath = "";

    private DatabaseConnector _dbConnector;

	public List<string> Scenes = new List<string>();

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
            _dbConnector = new MySQLConnector();
            _dbConnector.ConnectToServer(settings.Server, settings.Schema, settings.User, settings.Password);
            _currentSessionId = _dbConnector.GetNextSessionId();
        }
        catch (MySql.Data.MySqlClient.MySqlException mysqlEx)
        {
            Debug.LogError(mysqlEx.ToString());
            switch (mysqlEx.Number)
            {                  
                //http://dev.mysql.com/doc/refman/5.0/en/error-messages-server.html
                case 1042: // Unable to connect to any of the specified MySQL hosts (Check Server,Port)
                    _currentSessionId = -2;
                    break;
                case 0: // Access denied (Check DB name,username,password)
                    if (mysqlEx.Message.Contains("Unknown database"))
                        _currentSessionId = -4;
                    else
                        _currentSessionId = -3;
                    break;
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
            _currentSessionId = -1;
        }
    }

    public void ConnectToServer(string server, string user, string password)
    {
        try
        {
            _dbConnector = new MySQLConnector();
            _dbConnector.ConnectToServer(server, user, password);
            _currentSessionId = _dbConnector.GetNextSessionId();
        }
        
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
            _currentSessionId = -1;
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
        _dbConnector.InsertAnswer(questionName, questionSetName, _currentQuestionnaireName, _currentSessionId, selectedIndeces);
    }
    
    public int[] readAnswerIndex(int questionId)
    {
        return _dbConnector.readAnswerIndex(questionId, _currentSessionId);        
    }

    public Dictionary<int,string> readAnswer(string questionName)
    {
        return _dbConnector.readAnswer(questionName, _currentSessionId);
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
        _dbConnector.AddExperiment(experimentName);
    }

    /// <summary>
    /// Add scene to database
    /// </summary>
    /// <param name="sceneName"> Name of the scene</param>
    public void AddScene(string sceneName)
    {
        _dbConnector.AddScene(sceneName);
    }

    /// <summary>
    /// Remove scene to database
    /// </summary>
    /// <param name="sceneName"> Name of the scene</param>
    public void RemoveScene(string sceneName)
    {
        _dbConnector.RemoveScene(sceneName);
    }

	public List<string> GetExperimentScenes(string experimentName){
		int experimentId = _dbConnector.getExperimentId(experimentName);
		Scenes = _dbConnector.GetExperimentScenes(experimentId);
		return Scenes;
	}

    /// <summary>
    /// Set the order of the scenes of the experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    /// <param name="scenes">The scene names in order (can contain repetitions)</param>
    public void SetExperimentSceneOrder(string experimentName, string[] scenes)
    {
        _dbConnector.SetExperimentSceneOrder(experimentName,scenes);
    }

     /// <summary>
    /// Remove the saved scene order of the experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    public void RemoveExperimentSceneOrder(string experimentName)
    {
        _dbConnector.RemoveExperimentSceneOrder(experimentName);
    }
    
    /// <summary>
    /// Add a new parameter to an experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    /// <param name="parameterDescription"> Description of the parameter</param>
    public void CreateExperimentParameter(string experimentName, string parameterDescription)
    {
        _dbConnector.CreateExperimentParameter(experimentName, parameterDescription);
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
        _dbConnector.AddSession(experimentName, subjectId);
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
        _dbConnector.CreateUserAnswer(sessionId, questionnaireDescription);
    }

    /// <summary>
    /// Log a parameter used in a session
    /// </summary>
    /// <param name="parameterDescription"> Description of the parameter</param>
    /// <param name="value"> Value of the described parameter</param>
    public void LogSessionParameter(string parameterDescription, string value)
    {
        _dbConnector.LogSessionParameter(_currentSessionId, parameterDescription, value);
    }

    /// <summary>
    /// Log the labchart fileName used in the current session
    /// </summary>
    /// <param name="fileName"> fileName </param>
    public void LogLabChartFileName(string fileName)
    {
        _dbConnector.AddLabchartFileName(_currentSessionId, fileName);
    }

    public void RecordLabChartStartTime()
    {
        // NEW NAME AddLabchartStartTime
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

        _dbConnector.AddLabchartStartTime(_currentSessionId, timestamp);
    }

    private int getNextSessionID()
    {
        return _dbConnector.GetNextSessionId();
    }

    public string getParameterValue(int sessionId, string parameterDescription)
    {
        return _dbConnector.GetSessionParameter(sessionId, parameterDescription);
    }

    public string getParameterValue(string parameterDescription)
    {
        return _dbConnector.GetSessionParameter(this._currentSessionId, parameterDescription);
    }

    // -----------------------------------------
    //			Log Questions
    //------------------------------------------

    public void InsertQuestionToDB(QuestionData question)
    {
        _dbConnector.InsertQuestion(question);
    }

    public bool CreateQuestionSet(string name)
    {
        return _dbConnector.CreateQuestionSet(name);
    }

    public void AddQuestionJumps(Question q, string questionSet)
    {
        _dbConnector.AddJumps(q,questionSet);
    }


    public List<Jump> GetJumps(QuestionData questionData)
    {
        var id = _dbConnector.GetQuestionIdByName(questionData.QuestionName);
        var jumpIds = _dbConnector.GetJumpIds(id);
        var jumps = (from jumpId in jumpIds
                     let dest = _dbConnector.GetJumpDest(jumpId)
                     let destName = dest < 0 ? "*" : _dbConnector.GetQuestionNameById(dest)
                     let activator = _dbConnector.GetJumpCondition(jumpId)
                     select new Jump(destName, activator)).ToList();
        return jumps.Count>0?jumps:null;
    }

    public bool[,] getJumpConditions(List<int> jumpIds)
    {
        return _dbConnector.GetJumpConditions(jumpIds);
    }

    public List<int> getQuestionsOfSet(string questionSetName)
    {
        return _dbConnector.GetQuestionsOfSet(questionSetName);
    }

    public List<int> getJumpIds(int questionId)
    {
        return _dbConnector.GetJumpIds(questionId);
    }

    public int getJumpDest(int jumpId)
    {
        return _dbConnector.GetJumpDest(jumpId);
    }

    public void addQuestionnaire(string description)
    {
        _dbConnector.AddQuestionnaire(description);
    }

    public void setupQuestionnaire(string description, string questionSetName)
    {
        _dbConnector.SetupQuestionnaire(description, questionSetName);
    }

    public List<string> GetQuestionSets(string questionnaireName)
    {
        return _dbConnector.GetQuestionSets(questionnaireName);
    }

    public List<string> GetQuestionnaireNames()
    {
        return _dbConnector.GetAllQuestionnaireNames();
    }

    public List<string> GetQuestionSets(int questionnaireId)
    {
        return _dbConnector.GetQuestionSets(questionnaireId);
    }

    /// <summary>
    /// Returns the structured data from all questions in a set.
    /// </summary>
    /// <param name="questionSet">Name of the set to be retrieved.</param>
    /// <returns>List of QuestionData objects to be parsed</returns>
    public List<QuestionData> GetQuestionSetContent(string questionSet)
    {
        var qIds = _dbConnector.GetQuestionsOfSet(questionSet);
        var qDataList = new List<QuestionData>();
        foreach (var qId in qIds)
        {
            var questionVar = _dbConnector.GetQuestionVars(qId);
            var questionName = questionVar[0].ToString();
            var questionText = questionVar[1].ToString();
            var type = int.Parse(questionVar[2].ToString());
            var vals = _dbConnector.GetQuestionVals(qId);
            var labels = _dbConnector.GetQuestionLabels(qId);
            var output = _dbConnector.GetQuestionOutput(qId);
            qDataList.Add(new QuestionData(questionName,questionText, questionSet, type,vals,labels,output));
        }
        return qDataList;
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

        List<string>[] sceneTimesList = _dbConnector.GetSystemData("Scene", sessionId);        

        result[0] = sceneTimesList[2][2*sceneNumber];
        if (sceneTimesList[2].Count > 2 * sceneNumber + 1)
            result[1] = sceneTimesList[2][2*sceneNumber+1];
        return result;
    }

    public string getLabchartStarttime(int sessionId)
    {
        return _dbConnector.GetLabChartStartTime(sessionId);
    }

    public int getNumberOfScenes(int sessionId)
    {
        int experimentId = _dbConnector.getExperimentId(sessionId);
        int result = _dbConnector.GetExperimentScenes(experimentId).Count;

        return result;

    }

    public void removeSession(int sessionId)
    {
        _dbConnector.removeSession(sessionId);
    }

    public string[] getSceneNamesInOrder(string experimentName)
    {
        return _dbConnector.GetExperimentScenes(_dbConnector.getExperimentId(experimentName)).ToArray();
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

        Dictionary<DateTime, Dictionary<string, string>> data = _dbConnector.GetSystemDataByTime("Scene", "start", sessionId);
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

        Dictionary<DateTime, string[]> dataPosition = _dbConnector.Get3DMeasuredDataByTime("Player", "position", sessionId);
        Dictionary<DateTime, string[]> dataView = _dbConnector.Get3DMeasuredDataByTime("Player", "euler_angles", sessionId);
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

        Dictionary<DateTime, string[]> dataPosition = _dbConnector.Get3DMeasuredDataByTime("Player", "position", sessionId);
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

        List<string>[] input = _dbConnector.GetSystemData("Player", "input", sessionId);

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
        return _dbConnector.GetAllSessionsData(experimentName);
    }

    public string[] getSessionData(int sessionId)
    {
        return _dbConnector.GetSessionData(sessionId);
    }

    public int[] getQuestionnaireIds(int[] sessions)
    {
        return _dbConnector.GetAnsweredQuestionnaireIds(sessions);
    }

    public List<int>[] getAnswerIDs(int[] questionnaires, int[] sessions)
    {
        return _dbConnector.GetAnswerIds(questionnaires, sessions);
    }


    // -----------------------------------------
    //			Measured data
    //------------------------------------------


    public void insertMeasurement(String deviceName, String outputDescription, String unit, String value, String time)
    {
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSensorData(deviceName, outputDescription, value, time, _currentSessionId);
    }

    public void insertLiveMeasurement(String deviceName, String[] outputDescriptions, String[] units, String[] values)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        _dbConnector.AddDataOrigin(deviceName);
        for (int i =0; i< outputDescriptions.Length; i++)
        {
            _dbConnector.AddDataOutput(deviceName, outputDescriptions[i]);
            _dbConnector.AddDataUnit(deviceName, outputDescriptions[i], units[i]);
            _dbConnector.AddSensorData(deviceName, outputDescriptions[i], values[i], timestamp, _currentSessionId);
        }              
    }

    public void insertLiveMeasurement(String deviceName, String outputDescription, String unit, String value)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSensorData(deviceName, outputDescription, value, timestamp, _currentSessionId);
    }

    public void insert3DMeasurement(String deviceName, String outputDescription, String unit, String valueX, String valueY, String valueZ, String time)
    {
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSensorData(deviceName, outputDescription, valueX, valueY, valueZ, time, _currentSessionId);
    }

    public void insertLive3DMeasurement(String deviceName, String outputDescription, String unit, String valueX, String valueY, String valueZ)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSensorData(deviceName, outputDescription, valueX, valueY, valueZ, timestamp, _currentSessionId);
    }

    public void InsertSystemEvent(String deviceName, String outputDescription, String unit, String value, String time)
    {
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSystemData(deviceName, outputDescription, value, time, _currentSessionId);
    }

    public void InsertLiveSystemEvent(String deviceName, String outputDescription, String unit, String value)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSystemData(deviceName, outputDescription, value, timestamp, _currentSessionId);
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
        return _dbConnector.GetMeasuredDataByTime(originName, sessionId);
    }

    public List<string>[] getSessionMeasurmentsAsString(string originName, int session_id)
    {
		return _dbConnector.GetMeasurmentsDataAsString(originName, session_id);
    }

    public bool checkSchemaExists(string schemaName)
    {
        return _dbConnector.CheckSchemaExists(schemaName);
    }

    public bool checkQuestionnaireExists(string questionnaireName)
    {
        return _dbConnector.CheckQuestionnaireExists(questionnaireName);
    }

    public void RemoveExperimentParameter(string parameterName, string experimentName)
    {
        _dbConnector.RemoveExperimentParameter(parameterName, experimentName);
    }

    public List<string> GetExperimentParameters(string experimentName)
    {
        return _dbConnector.GetExperimentParameters(experimentName);
    }

    public void AddSensor(string deviceName)
    {
        _dbConnector.AddDataOrigin(deviceName);
    }

    public void RemoveSensor(string deviceName)
    {
        _dbConnector.RemoveDataOrigin(deviceName);
    }

    public List<string> getSensors()
    {
        List<string> origins = _dbConnector.GetDataOrigins();
        origins.Remove("Player");
        origins.Remove("Scene");
        origins.Remove("Collectible");
        origins.Remove("EventMarker");
        origins.Remove("CharacterPlacement");
        return origins;
    }

    public void CreateSchema()
    {
        _dbConnector.CreateSchema();
    }


    // -----------------------------------------
    //			Update Parameters
    //------------------------------------------
    public void updateParameters()
    {
        //same as in setup in constructor
        _currentSessionId = getNextSessionID();
    }

    // -----------------------------------------
    //			get logging variables
    //------------------------------------------

    public int GetCurrentSessionID()
    {
        return _currentSessionId;
    }

    public void SetLabChartFileName(string path)
    {
        _labChartFilePath = path;
        LogLabChartFileName(path);
    }

    public string GetLabChartFileName()
    {
        return _labChartFilePath;
    }

    public void setQuestionnaireName(string name)
    {
        this._currentQuestionnaireName = name;
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
