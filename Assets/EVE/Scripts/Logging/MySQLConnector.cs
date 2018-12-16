using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.XML.XMLHelper;

public class MySqlConnector : DatabaseConnector
{

    // MySQL instance specific items
    private MySqlConnection _con = null; // connection object
    private MySqlCommand _cmd = null; // command object
    private MySqlDataReader _rdr = null;


    public override void ConnectToServer(string server, string database, string user, string password)
    {
        var constr = "Server=" + server + ";Database=" + database + ";User ID=" + user + ";Password=" + password + ";Pooling=true";
        _con = new MySqlConnection(constr);
        _con.Open();
        Debug.Log("Connection State: " + _con.State);
    }

    public override void ConnectToServer(string server, string user, string password)
    {
        var constr = "Server=" + server + ";User ID=" + user + ";Password=" + password + ";Pooling=true";
        _con = new MySqlConnection(constr);
        _con.Open();
        Debug.Log("Connection State: " + _con.State);
    }

    public override void InsertAnswer(string questionName, string questionSetName, string questionnaireName, int sessionId, Dictionary<int,string> selectedIndices)
    {
        if (selectedIndices == null) return;
        
        var query = string.Empty;
        var answerId = -1;

        try
        {
            // FIRST: insert answer into the store_answers
            query = "INSERT INTO store_answers (question_id, user_answer_id) " +
                "VALUES ((SELECT tmp.id FROM (SELECT qs.id,qs.name,qset.name AS qsetName FROM questions AS qs " +
                "INNER JOIN question_question_sets AS qqs ON qs.id = qqs.question_id " +
                "INNER JOIN question_sets AS qset ON  qqs.question_set_id = qset.id) AS tmp " +
                "WHERE  tmp.qsetName = ?setName AND tmp.name = ?question_name), (SELECT id FROM user_answers WHERE session_id = ?session_id && questionnaire_id = " +
                "(SELECT id FROM questionnaires WHERE name = ?questionnaire_name) ORDER BY id DESC LIMIT 1))";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?question_name", MySqlDbType.VarChar); oParam0.Value = questionName;
                    var oParam1 = _cmd.Parameters.Add("?session_id", MySqlDbType.Int32); oParam1.Value = sessionId;
                    var oParam2 = _cmd.Parameters.Add("?setName", MySqlDbType.VarChar); oParam2.Value = questionSetName;
                    var oParam3 = _cmd.Parameters.Add("?questionnaire_name", MySqlDbType.VarChar); oParam3.Value = questionnaireName;

                    _cmd.ExecuteNonQuery();
                }
            }

            answerId = (int)_cmd.LastInsertedId;

            // ============ string values ==============
            
            var keys = selectedIndices.Keys.ToArray();
            for (var i = 0; i < selectedIndices.Keys.Count; i++)
            {
                var insertId = -1;
                query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam0 = _cmd.Parameters.Add("?pos", MySqlDbType.Int32); oParam0.Value = keys[i];
                        var oParam1 = _cmd.Parameters.Add("?val", MySqlDbType.VarChar); oParam1.Value = selectedIndices[keys[i]];
                        _cmd.ExecuteNonQuery();
                    }
                }

                insertId = (int)_cmd.LastInsertedId;

                query = "INSERT INTO answers_stored_strings (answer_id, string_id, type) VALUES (?answer_id, ?insert_id,  \"string\")";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam0 = _cmd.Parameters.Add("?answer_id", MySqlDbType.Int32); oParam0.Value = answerId;
                        var oParam1 = _cmd.Parameters.Add("?insert_id", MySqlDbType.Int32); oParam1.Value = insertId;
                        _cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Inserted strings!");
            }
            
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override int[] readAnswerIndex(int questionId, int sessionId)
    {
        var query = string.Empty;
        var val = new List<int>();

        try
        {
            // get the last given answer (row) for given session and question
            query = "SELECT pos FROM store_strings INNER JOIN (SELECT string_id FROM answers_stored_strings WHERE answer_id = (SELECT id FROM store_answers WHERE question_id = " +
                " ?questionId AND user_answer_id = (SELECT id FROM user_answers WHERE session_id = ?sessionId ORDER BY id DESC LIMIT 1) " +
                "ORDER BY id DESC LIMIT 1)) AS answerId ON (answerId.string_id = store_strings.id)";

            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionId", MySqlDbType.Int32); oParam0.Value = questionId;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (!_rdr.IsDBNull(_rdr.GetOrdinal("pos")))
                            {
                                val.Add(int.Parse(_rdr["pos"].ToString()));
                            }
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return val.ToArray();
    }

    public override Dictionary<int,string> readAnswer(string questionName, int sessionId)
    {
        var query = string.Empty;
        var result = new Dictionary<int, string>();

        try
        {
            // get the last given answer (row) for given session and question
            query = "SELECT pos,val FROM store_strings INNER JOIN (SELECT string_id FROM answers_stored_strings WHERE answer_id = (SELECT id FROM store_answers WHERE question_id = " +
                " (SELECT id FROM questions WHERE name = ?questionName) AND user_answer_id = (SELECT id FROM user_answers WHERE session_id = ?sessionId ORDER BY id DESC LIMIT 1) " +
                "ORDER BY id DESC LIMIT 1)) AS answerId ON (answerId.string_id = store_strings.id)";

            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionName", MySqlDbType.VarChar); oParam0.Value = questionName;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (!_rdr.IsDBNull(_rdr.GetOrdinal("pos")))
                            {
                                result.Add(int.Parse(_rdr["pos"].ToString()), _rdr["val"].ToString());
                            }
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override void AddExperiment(string experimentName)
    {
        TryInsert1Value(experimentName, "experiment", "experiment_name");
    }

    public override void removeSession(int sessionId)
    {
        try
        {
            var query = "DELETE FROM sessions WHERE session_id =?sessionId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Removed session");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }


    

    public override void AddScene(SceneEntry scene)
    {
        TryInsert1Value(scene.Name, "scene", "scene_name");
        //TODO process curtain information
    }

    public override void RemoveScene(SceneEntry scene)
    {
        //TODO Process curtain information
        try
        {
            var query = "DELETE FROM scene WHERE scene_name = ?sceneName";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?sceneName", MySqlDbType.VarChar); oParam0.Value = scene.Name;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Added scene");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void SetExperimentSceneOrder(string experimentName, SceneEntry[] scenes)
    {
        //TODO Process curtain information
        for (var i = 0; i < scenes.Length; i++)
        {
            try
            {
                var query = "Insert INTO experiment_scene_order (scenes_id, experiment_id, experiment_order) VALUES" +
                    "((SELECT id FROM scene WHERE scene_name = ?sceneName),(SELECT id FROM experiment WHERE experiment_name = ?experimentName),?orderNumber)";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam0 = _cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                        var oParam1 = _cmd.Parameters.Add("?sceneName", MySqlDbType.VarChar); oParam1.Value = scenes[i].Name;
                        var oParam2 = _cmd.Parameters.Add("?orderNumber", MySqlDbType.Int32); oParam2.Value = i;
                        _cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Added scene "+i+" to order");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }
           
    }

     /// <summary>
    /// Remove the saved scene order of the experiment
    /// </summary>
    /// <param name="experimentName"> Name of the experiment</param>
    public override void RemoveExperimentSceneOrder(string experimentName) {
        try
        {
            var query = "DELETE FROM experiment_scene_order WHERE experiment_id = (SELECT id FROM experiment WHERE experiment_name = ?experimentName)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Removed scenes order of experiment");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddSession(string experimentName, string subjectId)
    {
        try
        {
            var query = "INSERT INTO sessions(experiment_id, subject_id)" +
                "VALUES ((SELECT id FROM experiment WHERE experiment_name = ?experimentName), ?subjectId)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    var oParam1 = _cmd.Parameters.Add("?subjectId", MySqlDbType.VarChar); oParam1.Value = subjectId;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Created new session for " + subjectId);



        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddLabchartFileName(int sessionId, string fileName)
    {
        try
        {
            var query = "UPDATE sessions SET labchart_file = ?labchart_file WHERE session_id = ?sessionId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    var oParam1 = _cmd.Parameters.Add("?labchart_file", MySqlDbType.VarChar); oParam1.Value = fileName;

                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted file path!");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddLabchartStartTime(int sessionId, string timestamp)
    {
        try
        {
            var query = "UPDATE sessions SET labchart_timestamp = ?labchart_timestamp WHERE session_id = ?sessionId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    var oParam1 = _cmd.Parameters.Add("?labchart_timestamp", MySqlDbType.VarChar); oParam1.Value = timestamp;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Updated Labchart start timestamp!");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override string GetLabChartStartTime(int sessionId)
    {
        var result = "";

        try
        {
            var query = "SELECT * FROM sessions WHERE session_id = ?sessionId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            result = _rdr["labchart_timestamp"].ToString();
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return result;
    }

    public override int GetNextSessionId()
    {
        var query = string.Empty;
        var nextId = -1;

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT AUTO_INCREMENT FROM information_schema.tables WHERE table_name = 'sessions' AND table_schema = DATABASE( );";
            _cmd = new MySqlCommand(query, _con);
            var result = _cmd.ExecuteScalar();
            if (result != null) nextId = int.Parse(result.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return nextId;
    }

    public override void CreateUserAnswer(int sessionId, string questionnaireName)
    {
        try
        {
            var query = "INSERT INTO user_answers(session_id, questionnaire_id) VALUES(?session_id,(SELECT id FROM questionnaires WHERE name = ?questionnaireName))";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam1 = _cmd.Parameters.Add("?session_id", MySqlDbType.VarChar); oParam1.Value = sessionId;
                    var oParam2 = _cmd.Parameters.Add("?questionnaireName", MySqlDbType.VarChar); oParam2.Value = questionnaireName;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Created entry in user_answers!");

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override List<SceneEntry> GetExperimentScenes(int experimentId)
    {
        var query = string.Empty;
        var result = new List<SceneEntry>();
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT scene_name FROM (SELECT * FROM experiment_scene_order WHERE experiment_id = ?experimentId) AS ex_order INNER JOIN scene ON ex_order.scenes_id = scene.id ORDER BY experiment_order ASC";

            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?experimentId", MySqlDbType.Int32); oParam0.Value = experimentId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                    {
                        while (_rdr.Read())
                        {
                            //TODO read out curtain information from database as well
                            result.Add(new SceneEntry(_rdr["scene_name"].ToString(),false));
                        }
                        _rdr.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }


    public override void InsertQuestion(QuestionData question)
    {
        try
        {
            // FIRST: insert question into the question_table - use nextID to identify values
            var query = "INSERT INTO questions (name, question, type) " +
                           "VALUES (?name, ?question, ?type)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?name", MySqlDbType.VarChar);
                    oParam0.Value = question.QuestionName;
                    var oParam1 = _cmd.Parameters.Add("?question", MySqlDbType.VarChar);
                    oParam1.Value = question.QuestionText;
                    var oParam2 = _cmd.Parameters.Add("?type", MySqlDbType.Int32);
                    oParam2.Value = question.QuestionType;                  

                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted question:" + question.QuestionName);

            var questionId = (int)_cmd.LastInsertedId;            
            
            // SECOND: insert vals
            if (question.Vals != null)
            {
                for (var i = 0; i <= question.Vals.Length-1; i++)
                {
                    query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                    if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            var oParam0 = _cmd.Parameters.Add("?pos", MySqlDbType.Int32);
                            oParam0.Value = i;
                            var oParam1 = _cmd.Parameters.Add("?val", MySqlDbType.VarChar);
                            oParam1.Value = question.Vals[i].ToString();
                            _cmd.ExecuteNonQuery();
                        }
                    }
                    query = "INSERT INTO questions_stored_strings (questions_id, string_id, type) VALUES (?question_id, (SELECT LAST_INSERT_ID()), \"float\")";
                    if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            var oParam0 = _cmd.Parameters.Add("?question_id", MySqlDbType.Int32); oParam0.Value = questionId;
                            _cmd.ExecuteNonQuery();
                        }
                    }
                }
                Debug.Log("Inserted vals!");
            }

            // THIRD: insert labels
            if (question.Labels != null)
            {
                for (var i = 0; i <= question.Labels.Length-1; i++)
                {
                    query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                    if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            var oParam0 = _cmd.Parameters.Add("?pos", MySqlDbType.Int32);
                            oParam0.Value = i;
                            var oParam1 = _cmd.Parameters.Add("?val", MySqlDbType.VarChar);
                            oParam1.Value = question.Labels[i];
                            _cmd.ExecuteNonQuery();
                        }
                    }
                    query = "INSERT INTO questions_stored_strings (questions_id, string_id, type) VALUES (?question_id, (SELECT LAST_INSERT_ID()),  \"string\" )";
                    if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            var oParam0 = _cmd.Parameters.Add("?question_id", MySqlDbType.Int32); oParam0.Value = questionId;
                            _cmd.ExecuteNonQuery();
                        }
                    }
                }
                Debug.Log("Inserted label!");
            }

            // Add question to question set
            query = "INSERT INTO question_question_sets (question_set_id, question_id) " +
            "VALUES ((SELECT id FROM question_sets WHERE name = ?setName),?question_id)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?question_id", MySqlDbType.VarChar); oParam0.Value = questionId;
                    var oParam1 = _cmd.Parameters.Add("?setName", MySqlDbType.VarChar); oParam1.Value = question.QuestionSet;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted relation between question and question set!");

            // Add the ouput coding to the database
            if (question.Output != null)
            {
                for (var i = 0; i <= question.Output.Length-1; i++)
                {
                    query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                    if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            var oParam0 = _cmd.Parameters.Add("?pos", MySqlDbType.Int32);
                            oParam0.Value = i;
                            var oParam1 = _cmd.Parameters.Add("?val", MySqlDbType.VarChar);
                            oParam1.Value = question.Output[i].ToString();
                            _cmd.ExecuteNonQuery();
                        }
                    }
                    query = "INSERT INTO questions_coded_output (questions_id, string_id) VALUES (?question_id, (SELECT LAST_INSERT_ID()))";
                    if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            var oParam0 = _cmd.Parameters.Add("?question_id", MySqlDbType.Int32); oParam0.Value = questionId;
                            _cmd.ExecuteNonQuery();
                        }
                    }
                }
                Debug.Log("Inserted ouput coding!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(question.QuestionName + " caused: ");
            Debug.LogError(ex.ToString() );
        }
    }

    public override bool CreateQuestionSet(string name)
    {
        var success = false;
        try
        {
            var query = "INSERT INTO question_sets (name) " +
                "VALUES (?name)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?name", MySqlDbType.VarChar); oParam0.Value = name;
                    _cmd.ExecuteNonQuery();
                }
            }
            success = true;
            Debug.Log("Inserted question set!");
        }
        catch (Exception ex)
        {
            success = false;
            Debug.LogError(ex.ToString());
        }
        return success;

    }

    public override List<string> GetAllQuestionnaireNames()
    {
        var questionnaireNames = new List<string>();
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            const string query = " SELECT name FROM questionnaires";

            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                    {
                        while (_rdr.Read())
                        {
                            if (_rdr["name"] != null)
                            {
                                questionnaireNames.Add(_rdr["name"].ToString());
                            }
                        }
                        _rdr.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return questionnaireNames;
    }

    public override void AddJumps(Question q, string questionSetName)
    {
        if (q.Jumps != null)
        {
            // First insert the jumps
            try
            {
                for (var i = 0; i < q.Jumps.Count; i++)
                {
                    var jump = q.Jumps[i];
                    var query = "INSERT INTO jumps (question_set, origin_id, dest_id) " +
                                   "VALUES ((SELECT id FROM question_sets WHERE name = ?setName),((SELECT tmp.id FROM (SELECT qs.id,qs.name,qset.name AS qsetName FROM questions AS qs " +
                                   "INNER JOIN question_question_sets AS qqs ON qs.id = qqs.question_id " +
                                   "INNER JOIN question_sets AS qset ON  qqs.question_set_id = qset.id) AS tmp " +
                                   "WHERE  tmp.qsetName = ?setName AND tmp.name = ?origin_name)),((SELECT tmp.id FROM (SELECT qs.id,qs.name,qset.name AS qsetName FROM questions AS qs " +
                                   "INNER JOIN question_question_sets AS qqs ON qs.id = qqs.question_id " +
                                   "INNER JOIN question_sets AS qset ON  qqs.question_set_id = qset.id) AS tmp " +
                                   "WHERE  tmp.qsetName = ?setName AND tmp.name = ?dest_name)))";
                    if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            var oParam0 = _cmd.Parameters.Add("?setName", MySqlDbType.VarChar);
                            oParam0.Value = questionSetName;
                            var oParam1 = _cmd.Parameters.Add("?origin_name", MySqlDbType.VarChar);
                            oParam1.Value = q.Name;
                            var oParam2 = _cmd.Parameters.Add("?dest_name", MySqlDbType.VarChar);
                            oParam2.Value = jump.Destination;
                            _cmd.ExecuteNonQuery();
                        }
                    }
                    Debug.Log("Inserted question jump!");

                    // Insert conditions (for each jump add entries for each option and the assigned boolean)
                    //      If conditions has 2 possible jumps and 2 choices (2x2 matrix) we get 4 entries (two for each jump id)

                    var jumpId = (int) _cmd.LastInsertedId;
                    var conditions = jump.Activator.ToCharArray();

                    if (conditions.Length > 1)
                    {
                        for (var j = 0; j < conditions.Length; j++)
                        {
                            query = "INSERT INTO jump_conditions (jump_id, option_id, assign) " +
                                    "VALUES (" + jumpId + "," + j + "," + (conditions[j] == 'T') + ")";
                            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                            using (_con)
                            {
                                using (_cmd = new MySqlCommand(query, _con))
                                {
                                    _cmd.ExecuteNonQuery();
                                }
                            }
                            Debug.Log("Inserted question jump condition!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex + " " + questionSetName + " " + q.Name);

            }
        }
    }

    public override string GetJumpCondition(int jumpId)
    {
        var condition = "";
        try
        {
            var query = "SELECT * FROM jump_conditions WHERE jump_id = ?jumpId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?jumpId", MySqlDbType.Int32); oParam0.Value = jumpId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            condition += bool.Parse(_rdr["assign"].ToString())?"T":"F";
                        }
                    _rdr.Dispose();
                }
            }
            if (condition.Length == 0)
            {
                condition = "*";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return condition;
    }

    public override bool[,] GetJumpConditions(List<int> jumpIds)
    {
        // Get both dimensions
        var sizeI = jumpIds.Count;
        var sizeJ = 0;

        var query = string.Empty;

        try
        {
            query = "SELECT COUNT(*) FROM jump_conditions WHERE jump_id = ?jumpId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?jumpId", MySqlDbType.Int32); oParam0.Value = jumpIds[0];
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            sizeJ = int.Parse(_rdr["COUNT(*)"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        if (sizeJ > 0)
        {
            var conds = new bool[sizeI, sizeJ];

            for (var i = 0; i < jumpIds.Count; i++)
            {
                try
                {
                    query = "SELECT * FROM jump_conditions WHERE jump_id = ?jumpId";
                    if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            var oParam0 = _cmd.Parameters.Add("?jumpId", MySqlDbType.Int32); oParam0.Value = jumpIds[0];
                            _rdr = _cmd.ExecuteReader();
                            if (_rdr.HasRows)
                                while (_rdr.Read())
                                {
                                    var j = int.Parse(_rdr["option_id"].ToString());
                                    conds[i, j] = bool.Parse(_rdr["assign"].ToString());
                                }
                            _rdr.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
            }
            return conds;
        }
        else
        {
            return null;
        }
    }

    public override int GetQuestionIdByName(string name)
    {
        var id = -1;
        try
        {
            var query = "SELECT id  FROM questions WHERE name = ?questionName";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionName", MySqlDbType.VarChar); oParam0.Value = name;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            id = int.Parse(_rdr["id"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return id;
    }

    public override string GetQuestionNameById(int id)
    {
        var name = "";
        try
        {
            var query = "SELECT name  FROM questions WHERE id = ?questionId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionId", MySqlDbType.VarChar); oParam0.Value = id;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            name = _rdr["name"].ToString();
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return name;
    }

    public override int GetQuestionSetId(string questionSetName)
    {
        var query = string.Empty;
        var result = -1;
        try
        {
            query = "SELECT id FROM question_sets WHERE name = ?questionSetName";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionSetName", MySqlDbType.VarChar); oParam0.Value = questionSetName;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (!_rdr.IsDBNull(_rdr.GetOrdinal("id")))
                                result = int.Parse(_rdr["id"].ToString());
                            else
                                result = -2;
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override List<int> GetQuestionsOfSet(string questionSetName)
    {
        var query = string.Empty;

        var questionInSet = new List<int>();

        try
        {
            query = "SELECT question_id  FROM question_question_sets WHERE question_set_id = (SELECT id FROM question_sets WHERE name = ?questionSetName)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionSetName", MySqlDbType.VarChar); oParam0.Value = questionSetName;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            questionInSet.Add(int.Parse(_rdr["question_id"].ToString()));
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return questionInSet;
    }

    public override List<int> GetJumpIds(int questionId)
    {
        var query = string.Empty;

        var jumpIds = new List<int>();

        try
        {
            query = "SELECT id  FROM jumps WHERE origin_id = ?questionId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionId", MySqlDbType.Int32); oParam0.Value = questionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            jumpIds.Add(int.Parse(_rdr["id"].ToString()));
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return jumpIds;
    }

    public override int GetJumpDest(int jumpId)
    {
        var query = string.Empty;

        var destId = 0;

        try
        {
            query = "SELECT dest_id  FROM jumps WHERE id = ?jumpId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?jumpId", MySqlDbType.Int32); oParam0.Value = jumpId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (!_rdr.IsDBNull(_rdr.GetOrdinal("dest_id")))
                                destId = int.Parse(_rdr["dest_id"].ToString());
                            else
                                destId = -1;
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return destId;
    }

    public override void AddQuestionnaire(string name)
    {
        TryInsert1Value(name,"questionnaires","name");
    }

    public override void SetupQuestionnaire(string questionnaireName, string questionSetName)
    {
        try
        {
            var query = "INSERT INTO questionnaire_question_sets (questionnaire_id, question_set_id) " +
                "VALUES ((SELECT id FROM questionnaires WHERE name = ?questionnaireName), (SELECT id FROM question_sets WHERE name = ?question_set))";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionnaireName", MySqlDbType.VarChar); oParam0.Value = questionnaireName;
                    var oParam1 = _cmd.Parameters.Add("?question_set", MySqlDbType.VarChar); oParam1.Value = questionSetName;
                    _cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override List<string> GetQuestionSets(string questionnaireName)
    {
        var query = string.Empty;

        var qsNames = new List<string>();

        try
        {
            query = "SELECT name FROM (SELECT * FROM questionnaire_question_sets  WHERE questionnaire_id =" +
                "(SELECT id FROM questionnaires WHERE name = ?name) GROUP BY id ASC)" +
                "AS qsi INNER JOIN question_sets ON qsi.question_set_id = question_sets.id ORDER BY question_sets.id";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?name", MySqlDbType.VarChar); oParam0.Value = questionnaireName;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            qsNames.Add(_rdr["name"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return qsNames;
    }

    public override List<string> GetQuestionSets(int questionnaireId)
    {
        var query = string.Empty;

        var qsNames = new List<string>();

        try
        {
            query = "SELECT name FROM (SELECT * FROM questionnaire_question_sets  WHERE questionnaire_id = ?questionnaireId)" +
                "AS qsi INNER JOIN question_sets ON qsi.question_set_id = question_sets.id";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionnaireId", MySqlDbType.Int32); oParam0.Value = questionnaireId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            qsNames.Add(_rdr["name"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return qsNames;
    }

    public override object[] GetQuestionVars(int internalQuestionId)
    {
        var result = new object[3];
        try
        {
            var query = "SELECT *  FROM questions WHERE id = ?internalQuestionId";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            //questionName
                            result[0] = _rdr["name"].ToString();
                            //question_text                            
                            result[1] = _rdr["question"].ToString();
                            //type
                            result[2] = int.Parse(_rdr["type"].ToString());
                        }
                    else return null;
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override int[] GetQuestionVals(int internalQuestionId)
    {
        int[] vals = null;
        try
        {
            var floatCount = 0;
            var query = "SELECT Count(*) FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type =  \"float\" ";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (!_rdr.IsDBNull(_rdr.GetOrdinal("Count(*)"))) floatCount = int.Parse(_rdr["Count(*)"].ToString());
                        }
                    _rdr.Dispose();
                }
            }

            if (floatCount > 0)
            {

                vals = new int[floatCount];

                var i = 0;

                query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type =  \"float\" )" +
                        " AS questions_stored_string ON (questions_stored_string.string_id = store_strings.id)";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {                    
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam0 = _cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                        _rdr = _cmd.ExecuteReader();
                        if (_rdr.HasRows)
                            while (_rdr.Read())
                            {
                                if (!_rdr.IsDBNull(_rdr.GetOrdinal("val"))) vals[i] = int.Parse(_rdr["val"].ToString());
                                i++;
                            }
                        _rdr.Dispose();
                    }
                }


            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return vals;
    }

    public override string[] GetQuestionLabels(int internalQuestionId)
    {
        string[] labels = null;
        try
        {
            var stringsCount = 0;

            var query = "SELECT Count(*) FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type = \"string\"";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (!_rdr.IsDBNull(_rdr.GetOrdinal("Count(*)"))) stringsCount = int.Parse(_rdr["Count(*)"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
            if (stringsCount > 0)
            {
                labels = new string[stringsCount];

                var i = 0;

                query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type =  \"string\" )" +
                        " AS questions_stored_string ON (questions_stored_string.string_id = store_strings.id)";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam0 = _cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                        _rdr = _cmd.ExecuteReader();
                        if (_rdr.HasRows)
                            while (_rdr.Read())
                            {
                                if (!_rdr.IsDBNull(_rdr.GetOrdinal("val"))) labels[i] = _rdr["val"].ToString();
                                i++;
                            }
                        _rdr.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return labels;
    }

    public override string GetQuestionsSetName(int internalQuestionId)
    {
        var questionSetName = "";
        try
        {
            var query = "SELECT name FROM question_sets WHERE id = (SELECT question_set_id FROM question_question_sets WHERE question_id = ?internalQuestionId )";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            questionSetName = _rdr["name"].ToString();
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return questionSetName;
    }

    public override int[] GetQuestionOutput(int internalQuestionId)
    {
        int[] output = null;
        try
        {
            var outputCodeCount = 0;
            // Get output codes from database
            var query = "SELECT Count(*) FROM questions_coded_output WHERE questions_id = (SELECT id FROM questions WHERE id = ?internalQuestionId )";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (!_rdr.IsDBNull(_rdr.GetOrdinal("Count(*)"))) outputCodeCount = int.Parse(_rdr["Count(*)"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
            if (outputCodeCount > 0)
            {
                output = new int[outputCodeCount];

                var i = 0;

                query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_coded_output WHERE questions_id = " +
                        " (SELECT id FROM questions WHERE id = ?internalQuestionId )) AS questions_code ON (questions_code.string_id = store_strings.id)";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam0 = _cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                        _rdr = _cmd.ExecuteReader();
                        if (_rdr.HasRows)
                            while (_rdr.Read())
                            {
                                if (!_rdr.IsDBNull(_rdr.GetOrdinal("val"))) output[i] = int.Parse(_rdr["val"].ToString());
                                i++;
                            }
                        _rdr.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return output;
    }

    public override string[] GetSessionData(int sessionId)
    {
        var dataNumber = 4;

        var result = new string[dataNumber];
        var query = string.Empty;

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT session_id, subject_id, labchart_timestamp, labchart_file FROM sessions WHERE session_id = ?sessionId";

            using (_cmd = new MySqlCommand(query, _con))
            {
                var oParam0 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                _rdr = _cmd.ExecuteReader();
                if (_rdr.HasRows)
                    while (_rdr.Read())
                    {
                        result[0] = _rdr["session_id"].ToString();
                        result[1] = _rdr["subject_id"].ToString();
                        result[2] = _rdr["labchart_timestamp"].ToString();
                        result[3] = _rdr["labchart_file"].ToString();
                    }
                _rdr.Dispose();
            }
           
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return result;
    }

    public override string[][] GetAllSessionsData(string experimentName)
    {
        var dataNumber = 4;
        var result = new string[dataNumber][];

        var sessionIds = new List<string>[dataNumber];
        for (var i = 0; i < dataNumber; i++) sessionIds[i] = new List<string>();
        var query = string.Empty;

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT session_id, subject_id, labchart_timestamp, labchart_file FROM sessions LEFT JOIN (SELECT id FROM experiment WHERE id = (SELECT id FROM experiment WHERE experiment_name = ?experimentName))" +
                "AS selected_experiment ON sessions.experiment_id = selected_experiment.id";

            using (_cmd = new MySqlCommand(query, _con))
            {
                var oParam0 = _cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                _rdr = _cmd.ExecuteReader();
                if (_rdr.HasRows)
                    while (_rdr.Read())
                    {
                        sessionIds[0].Add(_rdr["session_id"].ToString());
                        sessionIds[1].Add(_rdr["subject_id"].ToString());
                        sessionIds[2].Add(_rdr["labchart_timestamp"].ToString());
                        sessionIds[3].Add(_rdr["labchart_file"].ToString());
                    }
                _rdr.Dispose();
            }

            for (var j = 0; j < dataNumber; j++)
                result[j] = new string[sessionIds[0].Count];

            for (var i = 0; i < sessionIds[0].Count; i++)
                for (var j = 0; j < dataNumber; j++)
                    result[j][i] = sessionIds[j][i];

            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return result;
    }

    public override int getExperimentId(int sessionId)
    {
        var query = string.Empty;
        var result = -1;
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = " SELECT experiment_id FROM SESSIONS WHERE session_id = ?sessionId";


            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                    {
                        while (_rdr.Read())
                        {
                            result = int.Parse(_rdr["experiment_id"].ToString());
                        }
                        _rdr.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override int getExperimentId(string experimentName)
    {
        var query = string.Empty;
        var result = -1;
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = " SELECT id FROM experiment WHERE experiment_name = ?experimentName";


            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                    {
                        while (_rdr.Read())
                        {
                            result = int.Parse(_rdr["id"].ToString());
                        }
                        _rdr.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override int[] GetAnsweredQuestionnaireIds(int[] sessionsIds)
    {
        var query = string.Empty;
        var questionnaireIds = new List<int>();

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();

            query = "SELECT DISTINCT (questionnaire_id) AS questionnaire_id FROM user_answers WHERE session_id = " + sessionsIds[0];
            for (var i = 1; i < sessionsIds.Length; i++)
            {
                query += " OR session_id = " + sessionsIds[i];
            }

            using (_cmd = new MySqlCommand(query, _con))
            {
                _rdr = _cmd.ExecuteReader();
                if (_rdr.HasRows)
                    while (_rdr.Read())
                    {
                        questionnaireIds.Add(int.Parse(_rdr["questionnaire_id"].ToString()));
                    }
                _rdr.Dispose();
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return questionnaireIds.ToArray();
    }

    public override List<int>[] GetAnswerIds(int[] questionnaireIds, int[] sessionsIds)
    {
        var query = string.Empty;
        var answerIDs = new List<int>[questionnaireIds.Length];
        for (var i = 0; i < questionnaireIds.Length; i++)
            answerIDs[i] = new List<int>();

        for (var i = 0; i < questionnaireIds.Length; i++)
        {
            var questionnaireId = questionnaireIds[i];

            try
            {
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();

                query = "SELECT id FROM questions INNER JOIN (" +
                            "SELECT DISTINCT(question_id)  FROM store_answers INNER JOIN (SELECT id FROM user_answers WHERE questionnaire_id = " + questionnaireId +
                            " AND session_id = " + questionnaireIds[0];
                for (var j = 1; j < questionnaireIds.Length; j++)
                {
                    query += " OR session_id = " + questionnaireIds[j];
                }
                query += " ) AS user_answers_id ON (user_answers_id.id = store_answers.user_answer_id))" +
                    "AS question_internal_ids ON (question_internal_ids.question_id = questions.id)";

                using (_cmd = new MySqlCommand(query, _con))
                {
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            answerIDs[i].Add(int.Parse(_rdr["id"].ToString()));
                        }
                    _rdr.Dispose();
                }

            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        return answerIDs;
    }

    public override List<string> GetDataOrigins()
    {
        var result = new List<string>();
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            var query = "SELECT * FROM data_origin";   
            using (_cmd = new MySqlCommand(query, _con))
            {
                _rdr = _cmd.ExecuteReader();
                if (_rdr.HasRows)
                    while (_rdr.Read())
                    {
                        result.Add(_rdr["device_name"].ToString());
                    }
                _rdr.Dispose();
            }          
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override void AddDataOrigin(string originName)
    {
        // Add a sensor (only if it hasn't been added yet)
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            var query = "SELECT * FROM data_origin WHERE device_name = ?originName";
            _cmd = new MySqlCommand(query, _con);
            var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
            var result = _cmd.ExecuteScalar();
            if (result == null)
            {
                query = "INSERT INTO data_origin (device_name) VALUES (?originName)";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam1 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam1.Value = originName;
                        _cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Inserted data origin!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void RemoveDataOrigin(string originName)
    {
        // Add a sensor (only if it hasn't been added yet)
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            var query = "SELECT * FROM data_origin WHERE device_name = ?originName";
            _cmd = new MySqlCommand(query, _con);
            var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
            var result = _cmd.ExecuteScalar();
            if (result != null)
            {
                query = "DELETE FROM data_origin WHERE device_name = ?originName";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam1 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam1.Value = originName;
                        _cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Removed data origin!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddDataOutput(string originName, string outputDescription)
    {
        // Add a particular output of a sensor (only if it hasn't been added yet)
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            var query = "SELECT * FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND  description = ?outputDescription";
            _cmd = new MySqlCommand(query, _con);
            var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
            var oParam1 = _cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
            var result = _cmd.ExecuteScalar();
            if (result == null)
            {

                query = "INSERT INTO data_description (device_id, description) VALUES ((SELECT id FROM data_origin WHERE device_name = ?originName), ?outputDescription)";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                        oParam1 = _cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                        _cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Inserted data_description!");
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddDataUnit(string originName, string outputDescription, string unitName)
    {
        // adds a data unit if it has not been added yet
        try
        {
            var query = "SELECT * FROM data_units WHERE description_id = (SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName ) AND"
                           + " description = ?outputDescription) AND unit = ?unitName";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            object result = null;
            using (_cmd = new MySqlCommand(query, _con))
            {
                var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                var oParam1 = _cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                var oParam2 = _cmd.Parameters.Add("?unitName", MySqlDbType.VarChar); oParam2.Value = unitName;
                result = _cmd.ExecuteScalar();
            }
            if (result == null)
            {

                query = "INSERT INTO data_units (description_id, unit) VALUES ((SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND"
                           + " description = ?outputDescription), ?unitName)";
                if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                        var oParam1 = _cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                        var oParam2 = _cmd.Parameters.Add("?unitName", MySqlDbType.VarChar); oParam2.Value = unitName;
                        _cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Inserted units!");

            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddSensorData(string originName, string outputDescription, string value, string time, int sessionId)
    {
        try
        {
            var query = "INSERT INTO sensor_data (data_description_id, session_id, value,time) VALUES" +
                       "((SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND"
                          + " description = ?outputDescription),?sessionId,?value,?time)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                    var oParam2 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam2.Value = sessionId;
                    var oParam3 = _cmd.Parameters.Add("?value", MySqlDbType.VarChar); oParam3.Value = value;
                    var oParam4 = _cmd.Parameters.Add("?time", MySqlDbType.VarChar); oParam4.Value = time;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted 1D Sensor data!");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddSensorData(string originName, string outputDescription, string valueX, string valueY, string valueZ, string time, int sessionId)
    {
        try
        {
            var query = "INSERT INTO sensor_data_3d (data_description_id, session_id, x, y, z, time) VALUES" +
                       "((SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND"
                          + " description = ?outputDescription),?sessionId,?valueX, ?valueY, ?valueZ,?time)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                    var oParam2 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam2.Value = sessionId;
                    var oParam3 = _cmd.Parameters.Add("?valueX", MySqlDbType.VarChar); oParam3.Value = valueX;
                    var oParam4 = _cmd.Parameters.Add("?valueY", MySqlDbType.VarChar); oParam4.Value = valueY;
                    var oParam5 = _cmd.Parameters.Add("?valueZ", MySqlDbType.VarChar); oParam5.Value = valueZ;
                    var oParam6 = _cmd.Parameters.Add("?time", MySqlDbType.VarChar); oParam6.Value = time;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted 3D Sensor data!");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddSystemData(String originName, String outputDescription, String value, String time, int sessionId)
    {
        try
        {
            var query = "INSERT INTO system_data (data_description_id, session_id, value,time) VALUES" +
                       "((SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND"
                          + " description = ?outputDescription),?sessionId,?value,?time)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                    var oParam2 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam2.Value = sessionId;
                    var oParam3 = _cmd.Parameters.Add("?value", MySqlDbType.VarChar); oParam3.Value = value;
                    var oParam4 = _cmd.Parameters.Add("?time", MySqlDbType.VarChar); oParam4.Value = time;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted 1D Sensor data!");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override Dictionary<DateTime, string[]> Get3DMeasuredDataByTime(string originName, string description, int sessionId)
    {
        var query = string.Empty;
        var result = new Dictionary<DateTime, string[]>();

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT * FROM"
                        + "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)"
                        + "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)"
                        + "AS data_w_desc INNER JOIN (sensor_data_3d)ON(sensor_data_3d.data_description_id = data_w_desc.id) WHERE session_id = ?sessionId AND description = ?description";
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    var oParam2 = _cmd.Parameters.Add("?description", MySqlDbType.VarChar); oParam2.Value = description;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            var timeString = _rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (result.ContainsKey(aDate))
                            {
                                if (_rdr["unit"].ToString().Length > 0)
                                {
                                    result[aDate] = new string[3];
                                    result[aDate][0] = _rdr["x"].ToString() + " " + _rdr["unit"].ToString();
                                    result[aDate][1] = _rdr["y"].ToString() + " " + _rdr["unit"].ToString();
                                    result[aDate][2] = _rdr["z"].ToString() + " " + _rdr["unit"].ToString();
                                }
                                else
                                {
                                    result[aDate] = new string[3];
                                    result[aDate][0] = _rdr["x"].ToString();
                                    result[aDate][1] = _rdr["y"].ToString();
                                    result[aDate][2] = _rdr["z"].ToString();
                                }
                            }
                            else
                            {
                                if (_rdr["unit"].ToString().Length > 0)
                                {
                                    var tmp = new string[3];
                                    tmp[0] = _rdr["x"].ToString() + " " + _rdr["unit"].ToString();
                                    tmp[1] = _rdr["y"].ToString() + " " + _rdr["unit"].ToString();
                                    tmp[2] = _rdr["z"].ToString() + " " + _rdr["unit"].ToString();
                                    result.Add(aDate, tmp);
                                }
                                else
                                {
                                    var tmp = new string[3];
                                    tmp[0] = _rdr["x"].ToString();
                                    tmp[1] = _rdr["y"].ToString();
                                    tmp[2] = _rdr["z"].ToString();
                                    result.Add(aDate, tmp);
                                }
                            }
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override Dictionary<DateTime, Dictionary<string, string>> GetMeasuredDataByTime(string originName, int sessionId)
    {
        var query = string.Empty;
        var result = new Dictionary<DateTime, Dictionary<string, string>>();

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            var timeString = _rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (result.ContainsKey(aDate))
                            {
                                if (_rdr["unit"].ToString().Length > 0)
                                    result[aDate].Add(_rdr["description"].ToString(), _rdr["value"].ToString() + " " + _rdr["unit"].ToString());
                                else
                                    result[aDate].Add(_rdr["description"].ToString(), _rdr["value"].ToString());
                            }
                            else
                            {
                                var tmp = new Dictionary<string, string>();
                                if (_rdr["unit"].ToString().Length > 0)
                                    tmp.Add(_rdr["description"].ToString(), _rdr["value"].ToString() + " " + _rdr["unit"].ToString());
                                else
                                    tmp.Add(_rdr["description"].ToString(), _rdr["value"].ToString());
                                result.Add(aDate, tmp);
                            }
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override Dictionary<string, Dictionary<string, List<DateTime>>> GetMeasuredDataByName(string originName, int sessionId)
    {

        var query = string.Empty;
        var result = new Dictionary<string, Dictionary<string, List<DateTime>>>();

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            var description = _rdr["description"].ToString();

                            if (result.ContainsKey(description))
                            {
                                var timeString = _rdr["time"].ToString();
                                var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                                if (result[description].ContainsKey(_rdr["value"].ToString()))
                                {
                                    result[description][_rdr["value"].ToString()].Add(aDate);
                                }
                                else
                                {
                                    var dateList = new List<DateTime>();
                                    dateList.Add(aDate);
                                    result[description].Add(_rdr["value"].ToString(), dateList);
                                }
                            }
                            else
                            {
                                var timeString = _rdr["time"].ToString();
                                var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                                var dateList = new List<DateTime>();
                                var tmp = new Dictionary<string, List<DateTime>>();
                                dateList.Add(aDate);
                                tmp.Add(_rdr["value"].ToString() + " " + _rdr["unit"].ToString(), dateList);
                                result.Add(description, tmp);
                            }
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override Dictionary<DateTime, Dictionary<string, string>> GetSystemDataByTime(string originName, string description, int sessionId)
    {
        var query = string.Empty;
        var result = new Dictionary<DateTime, Dictionary<string, string>>();

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId AND description = ?description";
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    var oParam2 = _cmd.Parameters.Add("?description", MySqlDbType.VarChar); oParam2.Value = description;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            var timeString = _rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (result.ContainsKey(aDate))
                            {
                                if (_rdr["unit"].ToString().Length > 0)
                                    result[aDate].Add(_rdr["description"].ToString(), _rdr["value"].ToString() + " " + _rdr["unit"].ToString());
                                else
                                    result[aDate].Add(_rdr["description"].ToString(), _rdr["value"].ToString());
                            }
                            else
                            {
                                var tmp = new Dictionary<string, string>();
                                if (_rdr["unit"].ToString().Length > 0)
                                    tmp.Add(_rdr["description"].ToString(), _rdr["value"].ToString() + " " + _rdr["unit"].ToString());
                                else
                                    tmp.Add(_rdr["description"].ToString(), _rdr["value"].ToString());
                                result.Add(aDate, tmp);
                            }
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override List<string>[] GetSystemData(string originName, int sessionId)
    {

        var query = string.Empty;
        var result = new List<string>[3];

        result[0] = new List<string>();
        result[1] = new List<string>();
        result[2] = new List<string>();

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId ORDER BY time";
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            result[0].Add(_rdr["description"].ToString());
                            result[1].Add(_rdr["value"].ToString());
                            result[2].Add(_rdr["time"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override List<string>[] GetSystemData(string originName, string description, int sessionId)
    {
        var query = string.Empty;
        var result = new List<string>[2];
        result[0] = new List<string>();
        result[1] = new List<string>();

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId AND description = ?description";
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    var oParam2 = _cmd.Parameters.Add("?description", MySqlDbType.VarChar); oParam2.Value = description;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            result[0].Add(_rdr["value"].ToString());
                            result[1].Add(_rdr["time"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override List<string>[] GetMeasurmentsDataAsString(string originName, int sessionId)
    {
        var query = string.Empty;
        var result = new List<string>[2];
        result[0] = new List<string>();
        result[1] = new List<string>();

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = "SELECT * FROM" +
                     "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                     "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                     "AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (_rdr["value"].ToString().Length > 0)
                                if (_rdr["unit"].ToString().Length > 0)
                                {                                    
                                    result[0].Add(_rdr["time"].ToString());
                                    result[1].Add(_rdr["description"].ToString() + " " + _rdr["value"].ToString() + " " + _rdr["unit"].ToString());
                                }
                                else
                                {
                                    result[0].Add(_rdr["time"].ToString());
                                    result[1].Add(_rdr["description"].ToString() + " " + _rdr["value"].ToString());
                                }
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }


    public override void CreateExperimentParameter(string experimentName, string parameterDescription)
    {
        try
        {
            var query = "INSERT INTO experiment_parameter(experiment_id, parameter_description)" +
                "VALUES((SELECT id FROM experiment WHERE experiment_name = ?experimentName), ?parameterDescription)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    var oParam1 = _cmd.Parameters.Add("?parameterDescription", MySqlDbType.VarChar); oParam1.Value = parameterDescription;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Added experiment parameter");
        }
        catch (Exception ex)
        {
            if (ex.ToString().StartsWith("MySql.Data.MySqlClient.MySqlException: Duplicate entry"))
            {
                Debug.LogWarning(ex.ToString());
            }
            else
            {
                Debug.LogError(ex.ToString());
            }
        }
    }

    public override void RemoveExperimentParameter(string parameterName, string experimentName)
    {
        try
        {
            var query = "DELETE FROM experiment_parameter WHERE experiment_id = (SELECT id FROM experiment WHERE experiment_name = ?experimentName) AND parameter_description = ?parameterName";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?parameterName", MySqlDbType.VarChar); oParam0.Value = parameterName;
                    var oParam1 = _cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam1.Value = experimentName;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Removed scenes order of experiment");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    
    }

    public override List<string> GetExperimentParameters(string experimentName)
    {
        var result = new List<string>();

        try
        {
            var query = "SELECT parameter_description FROM experiment_parameter WHERE experiment_id = (SELECT id FROM experiment WHERE experiment_name = ?experimentName)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            result.Add(_rdr["parameter_description"].ToString());
                        }
                    _rdr.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return result;
    }

    public override void LogSessionParameter(int sessionId, string parameterDescription, string parameterValue)
    {
        try
        {
            var query = "INSERT INTO session_parameter_values(session_id, experiment_parameter_id, value) " +
                "VALUES(?sessionId, (SELECT id FROM experiment_parameter WHERE parameter_description = ?parameterDescription), ?parameterValue)";
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    var oParam1 = _cmd.Parameters.Add("?parameterDescription", MySqlDbType.VarChar); oParam1.Value = parameterDescription;
                    var oParam2 = _cmd.Parameters.Add("?parameterValue", MySqlDbType.VarChar); oParam2.Value = parameterValue;
                    _cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted session parameters!");

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override string GetSessionParameter(int sessionId, string parameterName)
    {
        var result = "";

        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            var query = "SELECT value FROM session_parameter_values WHERE session_id = ?sessionId AND "
                + "experiment_parameter_id = (SELECT id FROM experiment_parameter WHERE parameter_description = ?parameterName AND experiment_id = (SELECT experiment_id FROM sessions WHERE session_id = ?sessionId))";
            using (_cmd = new MySqlCommand(query, _con))
            {
                var oParam0 = _cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                var oParam1 = _cmd.Parameters.Add("?parameterName", MySqlDbType.VarChar); oParam1.Value = parameterName;
                _rdr = _cmd.ExecuteReader();
                if (_rdr.HasRows)
                    while (_rdr.Read())
                    {
                        result = _rdr["value"].ToString();
                    }
                _rdr.Dispose();

            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override bool CheckSchemaExists(string schemaName)
    {
        var query = string.Empty;
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = " SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = ?schemaName";


            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?schemaName", MySqlDbType.VarChar); oParam0.Value = schemaName;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                    {
                        while (_rdr.Read())
                        {
                            if (_rdr["SCHEMA_NAME"] != null)
                            {
                                _rdr.Dispose();
                                return true;
                            }

                        }
                        _rdr.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return false;
    }

    public override bool CheckQuestionnaireExists(string questionnaireName)
    {
        var query = string.Empty;
        try
        {
            if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
            query = " SELECT name FROM questionnaires WHERE name = ?questionnaireName ;";

            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var oParam0 = _cmd.Parameters.Add("?questionnaireName", MySqlDbType.VarChar); oParam0.Value = questionnaireName;
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                    {
                        while (_rdr.Read())
                        {
                            if (_rdr["name"] != null)
                            {
                                _rdr.Dispose();
                                return true;
                            }

                        }
                        _rdr.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return false;
    }

    public override void CreateSchema()
    {
        if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
        var txt = (TextAsset)Resources.Load("Setup_EVE_DB", typeof(TextAsset));
        var script = new MySqlScript(_con, txt.text);
        script.Delimiter = "$$";
        script.Execute();
        _con.Close();
    }

    public override void DropSchema()
    {
        if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
        var query = "DROP DATABASE IF EXISTS EVE";
        _cmd = new MySqlCommand(query, _con);
        _cmd.ExecuteNonQuery();
    }

    private bool IsInserted(string value, string table, string variable)
    {
        if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
        var query = "SELECT * FROM " + table + " WHERE " + variable + " = ?nameToCompare";
        _cmd = new MySqlCommand(query, _con);
        var oParam0 = _cmd.Parameters.Add("?nameToCompare", MySqlDbType.VarChar); oParam0.Value = value;
        return _cmd.ExecuteScalar() != null;
    }
    private void Insert1Value(string value, string table, string variable)
    {
        var query = "INSERT INTO " + table + " (" + variable + ") VALUES (?valueToAdd)";
        if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
        using (_con)
        {
            using (_cmd = new MySqlCommand(query, _con))
            {
                var oParam0 = _cmd.Parameters.Add("?valueToAdd", MySqlDbType.VarChar);
                oParam0.Value = value;
                _cmd.ExecuteNonQuery();
            }
        }
        Debug.Log(value + " added  to " + table + ".");
    }

    private bool TryInsert1Value(string value, string table, string variable)
    {
        var inserted = true;
        try
        {
            if (!IsInserted(value, table, variable))
            {
                Insert1Value(value, table, variable);
            }
        }
        catch (Exception ex)
        {
            inserted = false;
            Debug.LogError(ex.ToString());
        }
        return inserted;
    }

    private void Insert3Value(string[] values, string table, string[] variables)
    {
        if (variables.Length != 3 || values.Length != 3)
        {
            Debug.LogError("Tried to insert the wrong number of values");
            return;
        }
        var vars = variables[0] + ", " + variables[1] + "," + variables[2];
        var query = "INSERT INTO " + table + " (" + vars + ") VALUES (?value0ToAdd, ?value1ToAdd, ?value2ToAdd)";
        if (!_con.State.Equals(ConnectionState.Open)) _con.Open();
        using (_con)
        {
            using (_cmd = new MySqlCommand(query, _con))
            {
                var oParam0 = _cmd.Parameters.Add("?value0ToAdd", MySqlDbType.VarChar);
                oParam0.Value = values[0];
                var oParam1 = _cmd.Parameters.Add("?value1ToAdd", MySqlDbType.VarChar);
                oParam1.Value = values[1];
                var oParam2 = _cmd.Parameters.Add("?value2ToAdd", MySqlDbType.VarChar);
                oParam2.Value = values[2];
                _cmd.ExecuteNonQuery();
            }
        }
        Debug.Log(values + " added  to " + table + ".");
    }

}
