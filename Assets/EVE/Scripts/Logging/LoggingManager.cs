using UnityEngine;					// standart for Unity C#
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.XML;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.XML.XMLHelper;
using EVE.Scripts.Utils;
using EVE.Scripts.XML;

// for lists

public class LoggingManager
{
    //setup values for session

    private string _currentQuestionnaireName;
    private string _labChartFilePath = "";
    private DatabaseConnector _dbConnector;

    private List<SceneEntry> _scenes;

    /// <summary>
    /// The logging manager takes care of all communications between the database and the framework.
    /// </summary>
    public LoggingManager()
    {
        CurrentSessionId = 0;
        _scenes = new List<SceneEntry>();
    }
    
    // ------------------------------------------------
    //			Connect to EVE database
    //-------------------------------------------------


    /// <summary>
    /// Connects to a server without the EVE scheme and creates a new scheme.
    /// </summary>
    /// <remarks>
    /// BE CAREFUL: Drops all data in the schema if it is already present!
    /// </remarks>
    /// <param name="dbSettings"></param>
    public void ConnectToServerAndCreateSchema(DatabaseSettings dbSettings)
    {
        ConnectToServerWithoutSchema(dbSettings);
        if (CheckSchemaExists(dbSettings.Schema)){ _dbConnector.DropSchema();}
        _dbConnector.CreateSchema();
        ConnectToServer(dbSettings);
    }


    /// <summary>
    /// Connects to a database with an EVE schema.
    /// </summary>
    /// <param name="settings">Settings for the database</param>
    /// <returns>Whether the connection works.</returns>
    public bool ConnectToServer(DatabaseSettings settings)
    {
        _dbConnector = new MySqlConnector();
        var errorId = _dbConnector.ConnectToServer(settings.Server, settings.Schema, settings.User, settings.Password);
        CurrentSessionId = errorId >= 0 ? _dbConnector.GetNextSessionId() : errorId;
        return errorId>=0;
    }

    /// <summary>
    /// Connects to a database without an EVE schema.
    /// </summary>
    /// <param name="settings">Settings for the database.</param>
    private void ConnectToServerWithoutSchema(DatabaseSettings settings)
    {
        _dbConnector = new MySqlConnector();
        var errorId = _dbConnector.ConnectToServer(settings.Server, settings.User, settings.Password);
        CurrentSessionId = errorId >= 0 ? _dbConnector.GetNextSessionId() : errorId;
    }

    // ------------------------------------------------
    //			Store and retrieve question answers
    //-------------------------------------------------
    
    /// <summary>
    /// Create a user answer for a given questionnaire.
    /// </summary>
    /// <remarks>
    /// This step connects a specific session with a questionnaire.
    /// This relation is used later to insert answers to specific
    /// questions in a question set.
    /// </remarks>
    /// <param name="sessionId"> Internal id of the session</param>
    /// <param name="questionnaireDescription"> Name of the questionnaire</param>
    public void CreateUserAnswer(int sessionId, string questionnaireDescription)
    {
        _dbConnector.CreateUserAnswer(sessionId, questionnaireDescription);
    }

    /// <summary>
    /// Insert the answer to a question into the database
    /// </summary>
    /// <param name="questionName"> Name of the question the answer belongs to (name used in xml files</param>
    /// <param name="questionSetName"> Name of the question set the question belongs to</param>
    /// <param name="selectedIndices"> Which answers where selected, and the values of them</param>
    public void InsertAnswer(string questionName, string questionSetName, Dictionary<int, string> selectedIndices)
    {
        _dbConnector.InsertAnswer(questionName, questionSetName, _currentQuestionnaireName, CurrentSessionId, selectedIndices);
    }
    
    /// <summary>
    /// Returns the answer the current participant gave to a question.
    /// </summary>
    /// <param name="questionName">Question to be called.</param>
    /// <returns>Answer the current participant gave.</returns>
    public Dictionary<int,string> ReadAnswer(string questionName)
    {
        return _dbConnector.readAnswer(questionName, CurrentSessionId);
    }

    // -----------------------------------------
    //			Experiment meta data
    //------------------------------------------

    /// <summary>
    /// Create an new experiment.
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    public void LogExperiment(string experimentName)
    {
        _dbConnector.AddExperiment(experimentName);
    }

    /// <summary>
    /// Add a scene to the database.
    /// </summary>
    /// <param name="sceneName"> Name of the scene.</param>
    public void AddScene(SceneEntry sceneName)
    {
        _dbConnector.AddScene(sceneName);
    }

    /// <summary>
    /// Remove a scene from thedatabase.
    /// </summary>
    /// <param name="sceneName"> Name of the scene.</param>
    public void RemoveScene(SceneEntry sceneName)
    {
        _dbConnector.RemoveScene(sceneName);
    }

	public List<SceneEntry> GetExperimentScenes(string experimentName){
		var experimentId = _dbConnector.getExperimentId(experimentName);
		_scenes = _dbConnector.GetExperimentScenes(experimentId);
		return _scenes;
	}

    /// <summary>
    /// Set the order of the scenes of the experiment.
    /// </summary>
    /// <param name="experimentName">Experiment to be edited.</param>
    /// <param name="scenes">The scene names in order (can contain repetitions)</param>
    public void SetExperimentSceneOrder(string experimentName, IEnumerable<SceneEntry> scenes)
    {
        _dbConnector.SetExperimentSceneOrder(experimentName,scenes.ToArray());
    }

     /// <summary>
    /// Remove the saved scene order of the experiment.
    /// </summary>
    /// <param name="experimentName">Experiment to be edited.</param>
    public void RemoveExperimentSceneOrder(string experimentName)
    {
        _dbConnector.RemoveExperimentSceneOrder(experimentName);
    }
    
    /// <summary>
    /// Add a new parameter to an experiment.
    /// </summary>
    /// <param name="experimentName">Experiment to be edited.</param>
    /// <param name="parameterDescription">Description of the parameter</param>
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


    /// <summary>
    /// Log a parameter used in a session
    /// </summary>
    /// <param name="parameterDescription"> Description of the parameter</param>
    /// <param name="value"> Value of the described parameter</param>
    public void LogSessionParameter(string parameterDescription, string value)
    {
        _dbConnector.LogSessionParameter(CurrentSessionId, parameterDescription, value);
    }

    /// <summary>
    /// Log the labchart fileName used in the current session
    /// </summary>
    /// <param name="fileName"> fileName </param>
    public void LogLabChartFileName(string fileName)
    {
        _dbConnector.AddLabchartFileName(CurrentSessionId, fileName);
    }

    /// <summary>
    /// The time labchart starts recording.
    /// </summary>
    public void RecordLabChartStartTime()
    {
        _dbConnector.AddLabchartStartTime(CurrentSessionId, TimeUtils.GetDbTimeStamp());
    }

    /// <summary>
    /// Gets parameter value for a particular session.
    /// </summary>
    /// <param name="sessionId">Session to be looked at</param>
    /// <param name="parameterDescription">parameter to be returned</param>
    /// <returns></returns>
    public string GetParameterValue(int sessionId, string parameterDescription)
    {
        return _dbConnector.GetSessionParameter(sessionId, parameterDescription);
    }

    /// <summary>
    /// Gets parameter value for the current session.
    /// </summary>
    /// <param name="sessionId">Session to be looked at</param>
    /// <param name="parameterDescription">parameter to be returned</param>
    /// <returns></returns>
    public string GetParameterValue(string parameterDescription)
    {
        return _dbConnector.GetSessionParameter(CurrentSessionId, parameterDescription);
    }

    // -----------------------------------------
    //	   Insert Questions Meta Data
    //------------------------------------------

    /// <summary>
    /// Inserts a new question into the database.
    /// </summary>
    /// <param name="question">Question data.</param>
    public void InsertQuestionToDb(QuestionData question)
    {
        _dbConnector.InsertQuestion(question);
    }

    /// <summary>
    /// Creates a question set in the database.
    /// </summary>
    /// <param name="name">Name of the new question set.</param>
    /// <returns></returns>
    public bool CreateQuestionSet(string name)
    {
        return _dbConnector.CreateQuestionSet(name);
    }

    /// <summary>
    /// Adds a jump to a question in a question set.
    /// </summary>
    /// <param name="q">Question to be edited.</param>
    /// <param name="questionSet">Question set to be used.</param>
    public void AddQuestionJumps(Question q, string questionSet)
    {
        _dbConnector.AddJumps(q,questionSet);
    }

    /// <summary>
    /// Return the jumps of a question.
    /// </summary>
    /// <param name="questionData">Question data to be used.</param>
    /// <returns>List of jumps for a question.</returns>
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

    /// <summary>
    /// Obtain question set id from database.
    /// </summary>
    /// <param name="questionSetName">Name of the question set.</param>
    /// <returns>Id of the question set.</returns>
    public int GetQuestionSetId(string questionSetName)
    {
        return _dbConnector.GetQuestionSetId(questionSetName);
    }

    /// <summary>
    /// Adds a questionnaire to the database.
    /// </summary>
    /// <param name="description">Name of new questionnaire.</param>
    public void AddQuestionnaire(string description)
    {
        _dbConnector.AddQuestionnaire(description);
    }

    /// <summary>
    /// Adds a question set to a questionnaire.
    /// </summary>
    /// <param name="questionnaireName">Questionnaire to be used.</param>
    /// <param name="questionSetName">Question set to be used.</param>
    public void SetupQuestionnaire(string questionnaireName, string questionSetName)
    {
        _dbConnector.SetupQuestionnaire(questionnaireName, questionSetName);
    }

    /// <summary>
    /// All question sets associated to a questionnaire.
    /// </summary>
    /// <param name="questionnaireName">Questionnaire to be used.</param>
    /// <returns>List of all associated question sets.</returns>
    public List<string> GetQuestionSets(string questionnaireName)
    {
        return _dbConnector.GetQuestionSets(questionnaireName);
    }

    /// <summary>
    /// A list of all questionnaires in the database.
    /// </summary>
    /// <returns>List of questionnaires.</returns>
    public List<string> GetQuestionnaireNames()
    {
        return _dbConnector.GetAllQuestionnaireNames();
    }

    /// <summary>
    /// Returns the structured data from all questions in a set.
    /// </summary>
    /// <param name="questionSet">Name of the set to be retrieved.</param>
    /// <returns>List of QuestionData objects to be parsed</returns>
    public IEnumerable<QuestionData> GetQuestionSetContent(string questionSet)
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

    /// <summary>
    /// Inserts into database start of scene.
    /// </summary>
    /// <param name="name">Scene to be started.</param>
    public void LogSceneStart(string name)
    {
        InsertLiveSystemEvent("Scene", "start", null, name);
    }

    /// <summary>
    /// Inserts nto database end of scene.
    /// </summary>
    /// <param name="name">Scene to be ended.</param>
    public void LogSceneEnd(string name)
    {
        InsertLiveSystemEvent("Scene", "end", null, name);
    }

    /// <summary>
    /// Gets the start and end time of a scene.
    /// </summary>
    /// <param name="sceneId">Scene to be retrieved.</param>
    /// <param name="sessionId">Session to be retrieved.</param>
    /// <returns>Start and end time.</returns>
    public string[] GetSceneTime(int sceneId, int sessionId)
    {
        var result = new string[2];

        var sceneTimesList = _dbConnector.GetSystemData("Scene", sessionId);

        if (sceneTimesList[2].Count <= 2 * sceneId) return null;
        result[0] = sceneTimesList[2][2 * sceneId];
        if (sceneTimesList[2].Count > 2 * sceneId + 1)
            result[1] = sceneTimesList[2][2 * sceneId + 1];
        return result;
    }
    
    /// <summary>
    /// Gets the start and end time of a scene.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public string[] GetSceneTime(string sceneName, int sessionId)
    {
        var result = new string[2];

        var sceneTimesList = _dbConnector.GetSystemData("Scene", sessionId);

        for (var index = 0; index < sceneTimesList[0].Count; index++)
        {
            if (sceneTimesList[1][index] != sceneName) {continue;}
            if (sceneTimesList[0][index] == "start") {result[0] = sceneTimesList[2][index];}
            else if (sceneTimesList[0][index] == "end")
            {
                result[1] = sceneTimesList[2][index];
                break;
            }
        }
        return result;
    }

    /// <summary>
    /// Returns the time LabChart started running.
    /// </summary>
    /// <param name="sessionId">Session to be checked.</param>
    /// <returns></returns>
    public string GetLabChartStartTime(int sessionId)
    {
        return _dbConnector.GetLabChartStartTime(sessionId);
    }

    /// <summary>
    /// Removes session.
    /// </summary>
    /// <param name="sessionId">Session to be removed.</param>
    public void RemoveSession(int sessionId)
    {
        _dbConnector.RemoveSession(sessionId);
    }

    /// <summary>
    /// All the scenes in the experiment.
    /// </summary>
    /// <param name="experimentName">Experiment to be taken.</param>
    /// <returns>List of scene names.</returns>
    public SceneEntry[] GetSceneNames(string experimentName)
    {
        return _dbConnector.GetExperimentScenes(_dbConnector.getExperimentId(experimentName)).ToArray();
    }

    // -----------------------------------------
    //			Log Position, View and Input
    //------------------------------------------

    /// <summary>
    /// Logs both the orientation and position.
    /// </summary>
    /// <param name="x">Position X.</param>
    /// <param name="y">Position Y.</param>
    /// <param name="z">Position Z.</param>
    /// <param name="ex">Orientation X.</param>
    /// <param name="ey">Orientation Y.</param>
    /// <param name="ez">Orientation Z.</param>
    public void LogPositionAndView(float x, float y, float z, float ex, float ey, float ez)
    {
        var timestamp = TimeUtils.GetDbTimeStamp();
        Insert3DMeasurement("Player", "position", null, x.ToString(), y.ToString(), z.ToString(),timestamp);
        Insert3DMeasurement("Player", "euler_angles", null, ex.ToString(), ey.ToString(), ez.ToString(), timestamp);
    }

    /// <summary>
    /// List of scenes visited by the participant.
    /// </summary>
    /// <param name="sessionId">Session to be retrieved.</param>
    /// <returns>List of scenes.</returns>
    public string[] GetListOfEnvironments(int sessionId)
    {
        var data = _dbConnector.GetSystemDataByTime("Scene", "start", sessionId);
        var dataSorted = TimeUtils.SortDatesAscending(new List<DateTime>(data.Keys));

        return dataSorted.Select(t => data[t]["start"]).ToArray();
    }

    /// <summary>
    /// Participants' path and orientation data from a specific scene.
    /// </summary>
    /// <param name="sessionId">Session to be used.</param>
    /// <param name="sceneId">Scene Id to be used.</param>
    /// <returns>Returns path [0:2], orientation [3:5] and time [6] in seconds since scene start.</returns>
    public List<float>[] GetPath(int sessionId, int sceneId)
    {
        var xyz = new List<float>[7];

        xyz[0] = new List<float>();
        xyz[1] = new List<float>();
        xyz[2] = new List<float>();
        xyz[3] = new List<float>();
        xyz[4] = new List<float>();
        xyz[5] = new List<float>();
        xyz[6] = new List<float>();

        var sceneTime = GetSceneTime(sceneId, sessionId);

        if (sceneTime == null) return xyz;

        var dataPosition = _dbConnector.Get3DMeasuredDataByTime("Player", "position", sessionId);
        var dataView = _dbConnector.Get3DMeasuredDataByTime("Player", "euler_angles", sessionId);
        var dataKeys = new List<DateTime>(dataPosition.Keys);
        var dataSorted = TimeUtils.SortDatesAscending(dataKeys);

        if (dataSorted.Count <= 0) return xyz;
        
        var sceneStart = Convert.ToDateTime(sceneTime[0]);
        var sceneEnd = dataSorted[dataSorted.Count - 1];
        if (sceneTime[1] != null)
        {
            sceneEnd = Convert.ToDateTime(sceneTime[1]);
        }

        for (var j = 0; j < dataSorted.Count; j++)
        {
            if (dataSorted[j] <= sceneStart) continue;
            
            if (dataSorted[j] < sceneEnd)
            {
                xyz[0].Add(float.Parse(dataPosition[dataSorted[j]][0]));
                xyz[1].Add(float.Parse(dataPosition[dataSorted[j]][1]));
                xyz[2].Add(float.Parse(dataPosition[dataSorted[j]][2]));
                xyz[3].Add(float.Parse(dataView[dataSorted[j]][0]));
                xyz[4].Add(float.Parse(dataView[dataSorted[j]][1]));
                xyz[5].Add(float.Parse(dataView[dataSorted[j]][2]));
                xyz[6].Add((float)(dataSorted[j]-sceneStart).TotalSeconds);
            }
            else
                break;
        }

        return xyz;
    }

    /// <summary>
    /// Logs participant inputs for replay.
    /// </summary>
    /// <param name="input">Input key to be logged.</param>
    public void LogInput(string input)
    {
        InsertLiveSystemEvent("Player", "input", null, input);
    }

    /// <summary>
    /// Returns all input users made.
    /// </summary>
    /// <param name="sessionId">Session to be retrieved.</param>
    /// <param name="sceneId">Scene to be used.</param>
    /// <returns></returns>
    public List<string>[] GetAllInput(int sessionId, int sceneId)
    {
        var result = new List<string>[2];
        result[0] = new List<string>();
        result[1] = new List<string>();

        var input = _dbConnector.GetSystemData("Player", "input", sessionId);

        if (input[0] == null) {return result;}
        for (var i = 0; i < input[0].Count; i++)
        {
            var time1Dt = Convert.ToDateTime(input[1][i]);
            result[0].Add(time1Dt.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            result[1].Add(input[1][i]);
        }
        return result;
    }

    // -----------------------------------------
    //			Session data 
    //------------------------------------------


    /// <summary>
    /// Returns all session data in experiment.
    /// </summary>
    /// <param name="experimentName">Experiment to be called.</param>
    /// <returns>Session data.</returns>
    public string[][] GetAllSessionsData(string experimentName)
    {
        return _dbConnector.GetAllSessionsData(experimentName);
    }

    /// <summary>
    /// Gets session data of a single session.
    /// </summary>
    /// <param name="sessionId">Sessin to be called.</param>
    /// <returns>Session data.</returns>
    public string[] GetSessionData(int sessionId)
    {
        return _dbConnector.GetSessionData(sessionId);
    }

    // -----------------------------------------
    //			Insert measured data
    //------------------------------------------


    /// <summary>
    /// Inserts a measure for a particular sensor of a device into the database.
    /// </summary>
    /// <param name="deviceName">Device producing the data</param>
    /// <param name="outputDescription">Parameter to me inserted</param>
    /// <param name="unit">Unit used in device</param>
    /// <param name="value">Value of parameter.</param>
    /// <param name="time">Time of measurement</param>
    public void InsertMeasurement(string deviceName, string outputDescription, string unit, string value, string time)
    {
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSensorData(deviceName, outputDescription, value, time, CurrentSessionId);
    }
    
    /// <summary>
    /// Inserts measures for multiple sensors of a device into the database.
    /// </summary>
    /// <remarks>
    /// This inserts multiple outputs for one device at once.
    /// </remarks>
    /// <param name="deviceName">Device producing the data</param>
    /// <param name="outputDescriptions">Parameter to me inserted</param>
    /// <param name="units">Unit used in device</param>
    /// <param name="values">Value of parameter.</param>
    public void InsertLiveMeasurements(string deviceName, string[] outputDescriptions, string[] units, string[] values)
    {
        _dbConnector.AddDataOrigin(deviceName);
        for (var i =0; i< outputDescriptions.Length; i++)
        {
            _dbConnector.AddDataOutput(deviceName, outputDescriptions[i]);
            _dbConnector.AddDataUnit(deviceName, outputDescriptions[i], units[i]);
            _dbConnector.AddSensorData(deviceName, outputDescriptions[i], values[i], TimeUtils.GetDbTimeStamp(), CurrentSessionId);
        }              
    }
        
    /// <summary>
    /// Inserts a measure for a particular sensor of a device into the database.
    /// </summary>
    /// <param name="deviceName">Device producing the data</param>
    /// <param name="outputDescription">Parameter to me inserted</param>
    /// <param name="unit">Unit used in device</param>
    /// <param name="value">Value of parameter.</param>
    public void InsertLiveMeasurement(string deviceName, string outputDescription, string unit, string value)
    {
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSensorData(deviceName, outputDescription, value, TimeUtils.GetDbTimeStamp(), CurrentSessionId);
    }

    /// <summary>
    /// Inserts a 3D measure for a particular sensor of a device into the database.
    /// </summary>
    /// <param name="deviceName">Device producing the data</param>
    /// <param name="outputDescription">Parameter to me inserted</param>
    /// <param name="unit">Unit used in device</param>
    /// <param name="valueX">Value of X parameter.</param>
    /// <param name="valueY">Value of Y parameter.</param>
    /// <param name="valueZ">Value of Z parameter.</param>
    /// <param name="time">Time of insertion.</param>
    public void Insert3DMeasurement(string deviceName, string outputDescription, string unit, string valueX, string valueY, string valueZ, string time)
    {
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSensorData(deviceName, outputDescription, valueX, valueY, valueZ, time, CurrentSessionId);
    }

    /// <summary>
    /// Inserts a 3D measure for a particular sensor of a device into the database.
    /// </summary>
    /// <param name="deviceName">Device producing the data</param>
    /// <param name="outputDescription">Parameter to me inserted</param>
    /// <param name="unit">Unit used in device</param>
    /// <param name="valueX">Value of X parameter.</param>
    /// <param name="valueY">Value of Y parameter.</param>
    /// <param name="valueZ">Value of Z parameter.</param>
    public void InsertLive3DMeasurement(string deviceName, string outputDescription, string unit, string valueX, string valueY, string valueZ)
    {
        var timestamp = TimeUtils.GetDbTimeStamp();
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSensorData(deviceName, outputDescription, valueX, valueY, valueZ, timestamp, CurrentSessionId);
    }

    /// <summary>
    /// Inserts a measure for a particular system sensor of a device into the database.
    /// </summary>
    /// <param name="deviceName">Device producing the data</param>
    /// <param name="outputDescription">Parameter to me inserted</param>
    /// <param name="unit">Unit used in device</param>
    /// <param name="value">Value of parameter.</param>
    /// <param name="time">Time of insertion.</param>
    public void InsertSystemEvent(string deviceName, string outputDescription, string unit, string value, string time)
    {
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSystemData(deviceName, outputDescription, value, time, CurrentSessionId);
    }

    /// <summary>
    /// Inserts a measure for a particular system sensor of a device into the database.
    /// </summary>
    /// <param name="deviceName">Device producing the data</param>
    /// <param name="outputDescription">Parameter to me inserted</param>
    /// <param name="unit">Unit used in device</param>
    /// <param name="value">Value of parameter.</param>
    public void InsertLiveSystemEvent(string deviceName, string outputDescription, string unit, string value)
    {
        _dbConnector.AddDataOrigin(deviceName);
        _dbConnector.AddDataOutput(deviceName, outputDescription);
        if (unit != null) _dbConnector.AddDataUnit(deviceName, outputDescription, unit);
        _dbConnector.AddSystemData(deviceName, outputDescription, value, TimeUtils.GetDbTimeStamp(), CurrentSessionId);
    }

    // -----------------------------------------
    //			Blood pressure measurements
    //------------------------------------------
    
    /// <summary>
    /// Inserts a blood pressure measure into the database.
    /// </summary>
    /// <param name="nbPs">nbPs of participant.</param>
    /// <param name="nbPd">nbPd of participant.</param>
    /// <param name="nbPm">nbPm of participant.</param>
    /// <param name="sp02">sp02 of participant.</param>
    /// <param name="hrPulse">Pulse of participant.</param>
    /// <param name="nbPsUnit">Unit of nbPs</param>
    /// <param name="nbPdUnit">Unit of nbPd.</param>
    /// <param name="nbPmUnit">Unit of nbPm.</param>
    /// <param name="sp02Unit">Unit of sp02.</param>
    /// <param name="hrPulseUnit">Unit of pulse.</param>
    /// <param name="time">Time of insertion.</param>
    public void InsertBpMeasurement(string nbPs, string nbPd, string nbPm, string sp02, string hrPulse, string nbPsUnit, string nbPdUnit, string nbPmUnit, string sp02Unit, string hrPulseUnit, string time)
    {
        const string deviceName = "Blood pressure machine";
        if (nbPs != null)
            InsertMeasurement(deviceName, "NBPs", nbPsUnit, nbPs, time);
        if (nbPd != null)
            InsertMeasurement(deviceName, "NBPd", nbPdUnit, nbPd, time);
        if (nbPm != null)
            InsertMeasurement(deviceName, "NBPm", nbPmUnit, nbPm, time);
        if (sp02 != null)
            InsertMeasurement(deviceName, "Sp02", sp02Unit, sp02, time);
        if (hrPulse != null)
            InsertMeasurement(deviceName, "HR_pulse", hrPulseUnit, hrPulse, time);
    }

    /// <summary>
    /// Returns all measures from a Device.
    /// </summary>
    /// <param name="originName">Device to be called.</param>
    /// <param name="sessionId">Session to be retrieved.</param>
    /// <returns>All measures on device.</returns>
    public List<string>[] GetSessionMeasurementsAsString(string originName, int sessionId)
    {
		return _dbConnector.GetMeasurementsDataAsString(originName, sessionId);
    }
    
    /// <summary>
    /// Queries the database whether an EVE schema exists.
    /// </summary>
    /// <param name="schemaName">Name of the schema.</param>
    /// <returns>Whether the schema exists.</returns>
    private bool CheckSchemaExists(string schemaName)
    {
        return _dbConnector.CheckSchemaExists(schemaName);
    }

    /// <summary>
    /// Queries whether a questionnaire exists.
    /// </summary>
    /// <param name="questionnaireName">Questionnaire to be checked.</param>
    /// <returns>Whether questionnaire exists.</returns>
    public bool CheckQuestionnaireExists(string questionnaireName)
    {
        return _dbConnector.CheckQuestionnaireExists(questionnaireName);
    }

    /// <summary>
    /// Removes an experiment parameter.
    /// </summary>
    /// <param name="parameterName">Parameter to be removed.</param>
    /// <param name="experimentName">Experiment to be called.</param>
    public void RemoveExperimentParameter(string parameterName, string experimentName)
    {
        _dbConnector.RemoveExperimentParameter(parameterName, experimentName);
    }

    /// <summary>
    /// Get all parameters in an experiment.
    /// </summary>
    /// <param name="experimentName">Experiment to be called.</param>
    /// <returns>All defined parameters.</returns>
    public List<string> GetExperimentParameters(string experimentName)
    {
        return _dbConnector.GetExperimentParameters(experimentName);
    }

    /// <summary>
    /// Adds a sensor to the database.
    /// </summary>
    /// <param name="deviceName">Sensors to be added.</param>
    public void AddSensor(string deviceName)
    {
        _dbConnector.AddDataOrigin(deviceName);
    }

    /// <summary>
    /// Removes a sensor from the database.
    /// </summary>
    /// <param name="deviceName">Sensor to be removed.</param>
    public void RemoveSensor(string deviceName)
    {
        _dbConnector.RemoveDataOrigin(deviceName);
    }

    /// <summary>
    /// Gets the sensors that are user set.
    /// </summary>
    /// <returns>List of sensors.</returns>
    public List<string> GetSensors()
    {
        var origins = _dbConnector.GetDataOrigins();
        origins.Remove("Player");
        origins.Remove("Scene");
        origins.Remove("Collectible");
        origins.Remove("EventMarker");
        origins.Remove("CharacterPlacement");
        return origins;
    }
    
    /// <summary>
    /// Restarts the experiment.
    /// </summary>
    public void ResetExperiment()
    {
        CurrentSessionId = _dbConnector.GetNextSessionId();
    }

    // -----------------------------------------
    //			get logging variables
    //------------------------------------------

    /// <summary>
    /// The current session id is managed by EVE.
    ///
    /// It either consists of the id provided by the
    /// database or an error id indicating the problem
    /// currently encountered with the database.
    /// </summary>
    /// <remarks>
    /// Error ids:
    /// 
    /// -4 = unknown schema
    /// -3 = other database error
    /// -2 = no connection
    /// -1 = not database related error
    /// </remarks>
    public int CurrentSessionId { get; private set; }

    /// <summary>
    /// Sets the path for LabChart files.
    /// </summary>
    /// <param name="path">Location for files.</param>
    public void SetLabChartFilePath(string path)
    {
        _labChartFilePath = path;
        LogLabChartFileName(path);
    }

    /// <summary>
    /// Returns the LabChart files.
    /// </summary>
    /// <returns>Path to LabChart files.</returns>
    public string GetLabChartFilePath()
    {
        return _labChartFilePath;
    }

    /// <summary>
    /// Sets which questionnaire is active.
    /// </summary>
    /// <param name="name">Questionnaire to be set active.</param>
    public void SetQuestionnaireName(string name)
    {
        _currentQuestionnaireName = name;
    }
}
