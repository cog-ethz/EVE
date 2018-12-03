using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using MySql.Data.MySqlClient;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.Questionnaire.Questions;

public class MySQLConnector : DatabaseConnector
{

    // MySQL instance specific items
    private MySqlConnection con = null; // connection object
    private MySqlCommand cmd = null; // command object
    private MySqlDataReader rdr = null;

    public MySQLConnector()
    {
        
    }

    public override void ConnectToServer(string server, string database, string user, string password)
    {
        var constr = "Server=" + server + ";Database=" + database + ";User ID=" + user + ";Password=" + password + ";Pooling=true";
        con = new MySqlConnection(constr);
        con.Open();
        Debug.Log("Connection State: " + con.State);
    }

    public override void ConnectToServer(string server, string user, string password)
    {
        var constr = "Server=" + server + ";User ID=" + user + ";Password=" + password + ";Pooling=true";
        con = new MySqlConnection(constr);
        con.Open();
        Debug.Log("Connection State: " + con.State);
    }

    public override void InsertAnswer(string questionName, string questionSetName, string questionnaireName, int sessionId, KeyValuePair<int,string>[] selectedIndeces)
    {
        var query = string.Empty;
        var answerID = -1;

        try
        {
            // FIRST: insert answer into the store_answers
            query = "INSERT INTO store_answers (question_id, user_answer_id) " +
                "VALUES ((SELECT tmp.id FROM (SELECT qs.id,qs.name,qset.name AS qsetName FROM questions AS qs " +
                "INNER JOIN question_question_sets AS qqs ON qs.id = qqs.question_id " +
                "INNER JOIN question_sets AS qset ON  qqs.question_set_id = qset.id) AS tmp " +
                "WHERE  tmp.qsetName = ?setName AND tmp.name = ?question_name), (SELECT id FROM user_answers WHERE session_id = ?session_id && questionnaire_id = " +
                "(SELECT id FROM questionnaires WHERE name = ?questionnaire_name) ORDER BY id DESC LIMIT 1))";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?question_name", MySqlDbType.VarChar); oParam0.Value = questionName;
                    var oParam1 = cmd.Parameters.Add("?session_id", MySqlDbType.Int32); oParam1.Value = sessionId;
                    var oParam2 = cmd.Parameters.Add("?setName", MySqlDbType.VarChar); oParam2.Value = questionSetName;
                    var oParam3 = cmd.Parameters.Add("?questionnaire_name", MySqlDbType.VarChar); oParam3.Value = questionnaireName;

                    cmd.ExecuteNonQuery();
                }
            }

            answerID = (int)cmd.LastInsertedId;

            // ============ string values ==============
            if (selectedIndeces != null)
            {
                for (var i = 0; i < selectedIndeces.Length; i++)
                {
                    var insert_id = -1;
                    query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?pos", MySqlDbType.Int32); oParam0.Value = selectedIndeces[i].Key;
                            var oParam1 = cmd.Parameters.Add("?val", MySqlDbType.VarChar); oParam1.Value = selectedIndeces[i].Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    insert_id = (int)cmd.LastInsertedId;

                    query = "INSERT INTO answers_stored_strings (answer_id, string_id, type) VALUES (?answer_id, ?insert_id,  \"string\")";
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?answer_id", MySqlDbType.Int32); oParam0.Value = answerID;
                            var oParam1 = cmd.Parameters.Add("?insert_id", MySqlDbType.Int32); oParam1.Value = insert_id;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Debug.Log("Inserted strings!");
                }
            }
        }
        catch (System.Exception ex)
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

            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionId", MySqlDbType.Int32); oParam0.Value = questionId;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(rdr.GetOrdinal("pos")))
                            {
                                val.Add(int.Parse(rdr["pos"].ToString()));
                            }
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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

            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionName", MySqlDbType.VarChar); oParam0.Value = questionName;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(rdr.GetOrdinal("pos")))
                            {
                                result.Add(int.Parse(rdr["pos"].ToString()), rdr["val"].ToString());
                            }
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Removed session");
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }


    

    public override void AddScene(string sceneName)
    {
        TryInsert1Value(sceneName, "scene", "scene_name");
    }

    public override void RemoveScene(string sceneName)
    {
        try
        {
            var query = "DELETE FROM scene WHERE scene_name = ?sceneName";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?sceneName", MySqlDbType.VarChar); oParam0.Value = sceneName;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Added scene");
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void SetExperimentSceneOrder(string experimentName, string[] scenes)
    {
        for (var i = 0; i < scenes.Length; i++)
        {
            try
            {
                var query = "Insert INTO experiment_scene_order (scenes_id, experiment_id, experiment_order) VALUES" +
                    "((SELECT id FROM scene WHERE scene_name = ?sceneName),(SELECT id FROM experiment WHERE experiment_name = ?experimentName),?orderNumber)";
                if (!con.State.Equals(ConnectionState.Open)) con.Open();
                using (con)
                {
                    using (cmd = new MySqlCommand(query, con))
                    {
                        var oParam0 = cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                        var oParam1 = cmd.Parameters.Add("?sceneName", MySqlDbType.VarChar); oParam1.Value = scenes[i];
                        var oParam2 = cmd.Parameters.Add("?orderNumber", MySqlDbType.Int32); oParam2.Value = i;
                        cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Added scene "+i+" to order");
            }
            catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Removed scenes order of experiment");
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    var oParam1 = cmd.Parameters.Add("?subjectId", MySqlDbType.VarChar); oParam1.Value = subjectId;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Created new session for " + subjectId);



        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddLabchartFileName(int sessionId, string fileName)
    {
        try
        {
            var query = "UPDATE sessions SET labchart_file = ?labchart_file WHERE session_id = ?sessionId";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    var oParam1 = cmd.Parameters.Add("?labchart_file", MySqlDbType.VarChar); oParam1.Value = fileName;

                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted file path!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddLabchartStartTime(int sessionId, string timestamp)
    {
        try
        {
            var query = "UPDATE sessions SET labchart_timestamp = ?labchart_timestamp WHERE session_id = ?sessionId";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    var oParam1 = cmd.Parameters.Add("?labchart_timestamp", MySqlDbType.VarChar); oParam1.Value = timestamp;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Updated Labchart start timestamp!");
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            result = rdr["labchart_timestamp"].ToString();
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return result;
    }

    public override int GetNextSessionId()
    {
        var query = string.Empty;
        var nextID = -1;

        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT AUTO_INCREMENT FROM information_schema.tables WHERE table_name = 'sessions' AND table_schema = DATABASE( );";
            cmd = new MySqlCommand(query, con);
            var result = cmd.ExecuteScalar();
            if (result != null) nextID = int.Parse(result.ToString());
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return nextID;
    }

    public override void CreateUserAnswer(int sessionId, string questionnaireName)
    {
        try
        {
            var query = "INSERT INTO user_answers(session_id, questionnaire_id) VALUES(?session_id,(SELECT id FROM questionnaires WHERE name = ?questionnaireName))";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam1 = cmd.Parameters.Add("?session_id", MySqlDbType.VarChar); oParam1.Value = sessionId;
                    var oParam2 = cmd.Parameters.Add("?questionnaireName", MySqlDbType.VarChar); oParam2.Value = questionnaireName;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Created entry in user_answers!");

        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override List<string> GetExperimentScenes(int experimentId)
    {
        var query = string.Empty;
        var result = new List<string>();
        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT scene_name FROM (SELECT * FROM experiment_scene_order WHERE experiment_id = ?experimentId) AS ex_order INNER JOIN scene ON ex_order.scenes_id = scene.id ORDER BY experiment_order ASC";

            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?experimentId", MySqlDbType.Int32); oParam0.Value = experimentId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            result.Add(rdr["scene_name"].ToString());
                        }
                        rdr.Dispose();
                    }
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?name", MySqlDbType.VarChar);
                    oParam0.Value = question.QuestionName;
                    var oParam1 = cmd.Parameters.Add("?question", MySqlDbType.VarChar);
                    oParam1.Value = question.QuestionText;
                    var oParam2 = cmd.Parameters.Add("?type", MySqlDbType.Int32);
                    oParam2.Value = question.QuestionType;                  

                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted question:" + question.QuestionName);

            var questionID = (int)cmd.LastInsertedId;            
            
            // SECOND: insert vals
            if (question.Vals != null)
            {
                for (var i = 0; i <= question.Vals.Length-1; i++)
                {
                    query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?pos", MySqlDbType.Int32);
                            oParam0.Value = i;
                            var oParam1 = cmd.Parameters.Add("?val", MySqlDbType.VarChar);
                            oParam1.Value = question.Vals[i].ToString();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    query = "INSERT INTO questions_stored_strings (questions_id, string_id, type) VALUES (?question_id, (SELECT LAST_INSERT_ID()), \"float\")";
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?question_id", MySqlDbType.Int32); oParam0.Value = questionID;
                            cmd.ExecuteNonQuery();
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
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?pos", MySqlDbType.Int32);
                            oParam0.Value = i;
                            var oParam1 = cmd.Parameters.Add("?val", MySqlDbType.VarChar);
                            oParam1.Value = question.Labels[i];
                            cmd.ExecuteNonQuery();
                        }
                    }
                    query = "INSERT INTO questions_stored_strings (questions_id, string_id, type) VALUES (?question_id, (SELECT LAST_INSERT_ID()),  \"string\" )";
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?question_id", MySqlDbType.Int32); oParam0.Value = questionID;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                Debug.Log("Inserted label!");
            }

            // Add question to question set
            query = "INSERT INTO question_question_sets (question_set_id, question_id) " +
            "VALUES ((SELECT id FROM question_sets WHERE name = ?setName),?question_id)";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?question_id", MySqlDbType.VarChar); oParam0.Value = questionID;
                    var oParam1 = cmd.Parameters.Add("?setName", MySqlDbType.VarChar); oParam1.Value = question.QuestionSet;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted relation between question and question set!");

            // Add the ouput coding to the database
            if (question.Output != null)
            {
                for (var i = 0; i <= question.Output.Length-1; i++)
                {
                    query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?pos", MySqlDbType.Int32);
                            oParam0.Value = i;
                            var oParam1 = cmd.Parameters.Add("?val", MySqlDbType.VarChar);
                            oParam1.Value = question.Output[i].ToString();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    query = "INSERT INTO questions_coded_output (questions_id, string_id) VALUES (?question_id, (SELECT LAST_INSERT_ID()))";
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?question_id", MySqlDbType.Int32); oParam0.Value = questionID;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                Debug.Log("Inserted ouput coding!");
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?name", MySqlDbType.VarChar); oParam0.Value = name;
                    cmd.ExecuteNonQuery();
                }
            }
            success = true;
            Debug.Log("Inserted question set!");
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            const string query = " SELECT name FROM questionnaires";

            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            if (rdr["name"] != null)
                            {
                                questionnaireNames.Add(rdr["name"].ToString());
                            }
                        }
                        rdr.Dispose();
                    }
                }
            }
        }
        catch (System.Exception ex)
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
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?setName", MySqlDbType.VarChar);
                            oParam0.Value = questionSetName;
                            var oParam1 = cmd.Parameters.Add("?origin_name", MySqlDbType.VarChar);
                            oParam1.Value = q.Name;
                            var oParam2 = cmd.Parameters.Add("?dest_name", MySqlDbType.VarChar);
                            oParam2.Value = jump.Destination;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Debug.Log("Inserted question jump!");

                    // Insert conditions (for each jump add entries for each option and the assigned boolean)
                    //      If conditions has 2 possible jumps and 2 choices (2x2 matrix) we get 4 entries (two for each jump id)

                    var jumpId = (int) cmd.LastInsertedId;
                    var conditions = jump.Activator.ToCharArray();

                    if (conditions.Length > 1)
                    {
                        for (var j = 0; j < conditions.Length; j++)
                        {
                            query = "INSERT INTO jump_conditions (jump_id, option_id, assign) " +
                                    "VALUES (" + jumpId + "," + j + "," + (conditions[j] == 'T') + ")";
                            if (!con.State.Equals(ConnectionState.Open)) con.Open();
                            using (con)
                            {
                                using (cmd = new MySqlCommand(query, con))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            Debug.Log("Inserted question jump condition!");
                        }
                    }
                }
            }
            catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?jumpId", MySqlDbType.Int32); oParam0.Value = jumpId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            condition += bool.Parse(rdr["assign"].ToString())?"T":"F";
                        }
                    rdr.Dispose();
                }
            }
            if (condition.Length == 0)
            {
                condition = "*";
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?jumpId", MySqlDbType.Int32); oParam0.Value = jumpIds[0];
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            sizeJ = int.Parse(rdr["COUNT(*)"].ToString());
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
                    if (!con.State.Equals(ConnectionState.Open)) con.Open();
                    using (con)
                    {
                        using (cmd = new MySqlCommand(query, con))
                        {
                            var oParam0 = cmd.Parameters.Add("?jumpId", MySqlDbType.Int32); oParam0.Value = jumpIds[0];
                            rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                                while (rdr.Read())
                                {
                                    var j = int.Parse(rdr["option_id"].ToString());
                                    conds[i, j] = bool.Parse(rdr["assign"].ToString());
                                }
                            rdr.Dispose();
                        }
                    }
                }
                catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionName", MySqlDbType.VarChar); oParam0.Value = name;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            id = int.Parse(rdr["id"].ToString());
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionId", MySqlDbType.VarChar); oParam0.Value = id;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            name = rdr["name"].ToString();
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return name;
    }

    public override List<int> GetQuestionsOfSet(string questionSetName)
    {
        var query = string.Empty;

        var questionInSet = new List<int>();

        try
        {
            query = "SELECT question_id  FROM question_question_sets WHERE question_set_id = (SELECT id FROM question_sets WHERE name = ?questionSetName)";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionSetName", MySqlDbType.VarChar); oParam0.Value = questionSetName;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            questionInSet.Add(int.Parse(rdr["question_id"].ToString()));
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionId", MySqlDbType.Int32); oParam0.Value = questionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            jumpIds.Add(int.Parse(rdr["id"].ToString()));
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return jumpIds;
    }

    public override int GetJumpDest(int jumpId)
    {
        var query = string.Empty;

        var destID = 0;

        try
        {
            query = "SELECT dest_id  FROM jumps WHERE id = ?jumpId";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?jumpId", MySqlDbType.Int32); oParam0.Value = jumpId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(rdr.GetOrdinal("dest_id")))
                                destID = int.Parse(rdr["dest_id"].ToString());
                            else
                                destID = -1;
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return destID;
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionnaireName", MySqlDbType.VarChar); oParam0.Value = questionnaireName;
                    var oParam1 = cmd.Parameters.Add("?question_set", MySqlDbType.VarChar); oParam1.Value = questionSetName;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?name", MySqlDbType.VarChar); oParam0.Value = questionnaireName;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            qsNames.Add(rdr["name"].ToString());
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionnaireId", MySqlDbType.Int32); oParam0.Value = questionnaireId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            qsNames.Add(rdr["name"].ToString());
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            //questionName
                            result[0] = rdr["name"].ToString();
                            //question_text                            
                            result[1] = rdr["question"].ToString();
                            //type
                            result[2] = int.Parse(rdr["type"].ToString());
                        }
                    else return null;
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            var float_count = 0;
            var query = "SELECT Count(*) FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type =  \"float\" ";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(rdr.GetOrdinal("Count(*)"))) float_count = int.Parse(rdr["Count(*)"].ToString());
                        }
                    rdr.Dispose();
                }
            }

            if (float_count > 0)
            {

                vals = new int[float_count];

                var i = 0;

                query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type =  \"float\" )" +
                        " AS questions_stored_string ON (questions_stored_string.string_id = store_strings.id)";
                if (!con.State.Equals(ConnectionState.Open)) con.Open();
                using (con)
                {                    
                    using (cmd = new MySqlCommand(query, con))
                    {
                        var oParam0 = cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                        rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                            while (rdr.Read())
                            {
                                if (!rdr.IsDBNull(rdr.GetOrdinal("val"))) vals[i] = int.Parse(rdr["val"].ToString());
                                i++;
                            }
                        rdr.Dispose();
                    }
                }


            }
        }
        catch (System.Exception ex)
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
            var strings_count = 0;

            var query = "SELECT Count(*) FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type = \"string\"";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(rdr.GetOrdinal("Count(*)"))) strings_count = int.Parse(rdr["Count(*)"].ToString());
                        }
                    rdr.Dispose();
                }
            }
            if (strings_count > 0)
            {
                labels = new string[strings_count];

                var i = 0;

                query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type =  \"string\" )" +
                        " AS questions_stored_string ON (questions_stored_string.string_id = store_strings.id)";
                if (!con.State.Equals(ConnectionState.Open)) con.Open();
                using (con)
                {
                    using (cmd = new MySqlCommand(query, con))
                    {
                        var oParam0 = cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                        rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                            while (rdr.Read())
                            {
                                if (!rdr.IsDBNull(rdr.GetOrdinal("val"))) labels[i] = rdr["val"].ToString();
                                i++;
                            }
                        rdr.Dispose();
                    }
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            questionSetName = rdr["name"].ToString();
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            var output_code_count = 0;
            // Get output codes from database
            var query = "SELECT Count(*) FROM questions_coded_output WHERE questions_id = (SELECT id FROM questions WHERE id = ?internalQuestionId )";
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(rdr.GetOrdinal("Count(*)"))) output_code_count = int.Parse(rdr["Count(*)"].ToString());
                        }
                    rdr.Dispose();
                }
            }
            if (output_code_count > 0)
            {
                output = new int[output_code_count];

                var i = 0;

                query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_coded_output WHERE questions_id = " +
                        " (SELECT id FROM questions WHERE id = ?internalQuestionId )) AS questions_code ON (questions_code.string_id = store_strings.id)";
                if (!con.State.Equals(ConnectionState.Open)) con.Open();
                using (con)
                {
                    using (cmd = new MySqlCommand(query, con))
                    {
                        var oParam0 = cmd.Parameters.Add("?internalQuestionId", MySqlDbType.Int32); oParam0.Value = internalQuestionId;
                        rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                            while (rdr.Read())
                            {
                                if (!rdr.IsDBNull(rdr.GetOrdinal("val"))) output[i] = int.Parse(rdr["val"].ToString());
                                i++;
                            }
                        rdr.Dispose();
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return output;
    }

    public override string[] GetSessionData(int sessionID)
    {
        var data_number = 4;

        var result = new string[data_number];
        var query = string.Empty;

        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT session_id, subject_id, labchart_timestamp, labchart_file FROM sessions WHERE session_id = ?sessionId";

            using (cmd = new MySqlCommand(query, con))
            {
                var oParam0 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionID;
                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                    while (rdr.Read())
                    {
                        result[0] = rdr["session_id"].ToString();
                        result[1] = rdr["subject_id"].ToString();
                        result[2] = rdr["labchart_timestamp"].ToString();
                        result[3] = rdr["labchart_file"].ToString();
                    }
                rdr.Dispose();
            }
           
            return result;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return result;
    }

    public override string[][] GetAllSessionsData(string experimentName)
    {
        var data_number = 4;
        var result = new string[data_number][];

        var session_ids = new List<string>[data_number];
        for (var i = 0; i < data_number; i++) session_ids[i] = new List<string>();
        var query = string.Empty;

        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT session_id, subject_id, labchart_timestamp, labchart_file FROM sessions LEFT JOIN (SELECT id FROM experiment WHERE id = (SELECT id FROM experiment WHERE experiment_name = ?experimentName))" +
                "AS selected_experiment ON sessions.experiment_id = selected_experiment.id";

            using (cmd = new MySqlCommand(query, con))
            {
                var oParam0 = cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                    while (rdr.Read())
                    {
                        session_ids[0].Add(rdr["session_id"].ToString());
                        session_ids[1].Add(rdr["subject_id"].ToString());
                        session_ids[2].Add(rdr["labchart_timestamp"].ToString());
                        session_ids[3].Add(rdr["labchart_file"].ToString());
                    }
                rdr.Dispose();
            }

            for (var j = 0; j < data_number; j++)
                result[j] = new string[session_ids[0].Count];

            for (var i = 0; i < session_ids[0].Count; i++)
                for (var j = 0; j < data_number; j++)
                    result[j][i] = session_ids[j][i];

            return result;
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = " SELECT experiment_id FROM SESSIONS WHERE session_id = ?sessionId";


            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            result = int.Parse(rdr["experiment_id"].ToString());
                        }
                        rdr.Dispose();
                    }
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = " SELECT id FROM experiment WHERE experiment_name = ?experimentName";


            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            result = int.Parse(rdr["id"].ToString());
                        }
                        rdr.Dispose();
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return result;
    }

    public override int[] GetAnsweredQuestionnaireIds(int[] sessionsIds)
    {
        var query = string.Empty;
        var questionnaire_ids = new List<int>();

        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();

            query = "SELECT DISTINCT (questionnaire_id) AS questionnaire_id FROM user_answers WHERE session_id = " + sessionsIds[0];
            for (var i = 1; i < sessionsIds.Length; i++)
            {
                query += " OR session_id = " + sessionsIds[i];
            }

            using (cmd = new MySqlCommand(query, con))
            {
                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                    while (rdr.Read())
                    {
                        questionnaire_ids.Add(int.Parse(rdr["questionnaire_id"].ToString()));
                    }
                rdr.Dispose();
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return questionnaire_ids.ToArray();
    }

    public override List<int>[] GetAnswerIds(int[] questionnaireIds, int[] sessionsIds)
    {
        var query = string.Empty;
        var answer_IDs = new List<int>[questionnaireIds.Length];
        for (var i = 0; i < questionnaireIds.Length; i++)
            answer_IDs[i] = new List<int>();

        for (var i = 0; i < questionnaireIds.Length; i++)
        {
            var questionnaireId = questionnaireIds[i];

            try
            {
                if (!con.State.Equals(ConnectionState.Open)) con.Open();

                query = "SELECT id FROM questions INNER JOIN (" +
                            "SELECT DISTINCT(question_id)  FROM store_answers INNER JOIN (SELECT id FROM user_answers WHERE questionnaire_id = " + questionnaireId +
                            " AND session_id = " + questionnaireIds[0];
                for (var j = 1; j < questionnaireIds.Length; j++)
                {
                    query += " OR session_id = " + questionnaireIds[j];
                }
                query += " ) AS user_answers_id ON (user_answers_id.id = store_answers.user_answer_id))" +
                    "AS question_internal_ids ON (question_internal_ids.question_id = questions.id)";

                using (cmd = new MySqlCommand(query, con))
                {
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            answer_IDs[i].Add(int.Parse(rdr["id"].ToString()));
                        }
                    rdr.Dispose();
                }

            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        return answer_IDs;
    }

    public override List<string> GetDataOrigins()
    {
        var result = new List<string>();
        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            var query = "SELECT * FROM data_origin";   
            using (cmd = new MySqlCommand(query, con))
            {
                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                    while (rdr.Read())
                    {
                        result.Add(rdr["device_name"].ToString());
                    }
                rdr.Dispose();
            }          
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            var query = "SELECT * FROM data_origin WHERE device_name = ?originName";
            cmd = new MySqlCommand(query, con);
            var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
            var result = cmd.ExecuteScalar();
            if (result == null)
            {
                query = "INSERT INTO data_origin (device_name) VALUES (?originName)";
                if (!con.State.Equals(ConnectionState.Open)) con.Open();
                using (con)
                {
                    using (cmd = new MySqlCommand(query, con))
                    {
                        var oParam1 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam1.Value = originName;
                        cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Inserted data origin!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void RemoveDataOrigin(string originName)
    {
        // Add a sensor (only if it hasn't been added yet)
        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            var query = "SELECT * FROM data_origin WHERE device_name = ?originName";
            cmd = new MySqlCommand(query, con);
            var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                query = "DELETE FROM data_origin WHERE device_name = ?originName";
                if (!con.State.Equals(ConnectionState.Open)) con.Open();
                using (con)
                {
                    using (cmd = new MySqlCommand(query, con))
                    {
                        var oParam1 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam1.Value = originName;
                        cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Removed data origin!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddDataOutput(string originName, string outputDescription)
    {
        // Add a particular output of a sensor (only if it hasn't been added yet)
        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            var query = "SELECT * FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND  description = ?outputDescription";
            cmd = new MySqlCommand(query, con);
            var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
            var oParam1 = cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
            var result = cmd.ExecuteScalar();
            if (result == null)
            {

                query = "INSERT INTO data_description (device_id, description) VALUES ((SELECT id FROM data_origin WHERE device_name = ?originName), ?outputDescription)";
                if (!con.State.Equals(ConnectionState.Open)) con.Open();
                using (con)
                {
                    using (cmd = new MySqlCommand(query, con))
                    {
                        oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                        oParam1 = cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                        cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Inserted data_description!");
            }

        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            object result = null;
            using (cmd = new MySqlCommand(query, con))
            {
                var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                var oParam1 = cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                var oParam2 = cmd.Parameters.Add("?unitName", MySqlDbType.VarChar); oParam2.Value = unitName;
                result = cmd.ExecuteScalar();
            }
            if (result == null)
            {

                query = "INSERT INTO data_units (description_id, unit) VALUES ((SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND"
                           + " description = ?outputDescription), ?unitName)";
                if (!con.State.Equals(ConnectionState.Open)) con.Open();
                using (con)
                {
                    using (cmd = new MySqlCommand(query, con))
                    {
                        var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                        var oParam1 = cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                        var oParam2 = cmd.Parameters.Add("?unitName", MySqlDbType.VarChar); oParam2.Value = unitName;
                        cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("Inserted units!");

            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                    var oParam2 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam2.Value = sessionId;
                    var oParam3 = cmd.Parameters.Add("?value", MySqlDbType.VarChar); oParam3.Value = value;
                    var oParam4 = cmd.Parameters.Add("?time", MySqlDbType.VarChar); oParam4.Value = time;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted 1D Sensor data!");
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                    var oParam2 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam2.Value = sessionId;
                    var oParam3 = cmd.Parameters.Add("?valueX", MySqlDbType.VarChar); oParam3.Value = valueX;
                    var oParam4 = cmd.Parameters.Add("?valueY", MySqlDbType.VarChar); oParam4.Value = valueY;
                    var oParam5 = cmd.Parameters.Add("?valueZ", MySqlDbType.VarChar); oParam5.Value = valueZ;
                    var oParam6 = cmd.Parameters.Add("?time", MySqlDbType.VarChar); oParam6.Value = time;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted 3D Sensor data!");
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?outputDescription", MySqlDbType.VarChar); oParam1.Value = outputDescription;
                    var oParam2 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam2.Value = sessionId;
                    var oParam3 = cmd.Parameters.Add("?value", MySqlDbType.VarChar); oParam3.Value = value;
                    var oParam4 = cmd.Parameters.Add("?time", MySqlDbType.VarChar); oParam4.Value = time;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted 1D Sensor data!");
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT * FROM"
                        + "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)"
                        + "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)"
                        + "AS data_w_desc INNER JOIN (sensor_data_3d)ON(sensor_data_3d.data_description_id = data_w_desc.id) WHERE session_id = ?sessionId AND description = ?description";
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    var oParam2 = cmd.Parameters.Add("?description", MySqlDbType.VarChar); oParam2.Value = description;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            var timeString = rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (result.ContainsKey(aDate))
                            {
                                if (rdr["unit"].ToString().Length > 0)
                                {
                                    result[aDate] = new string[3];
                                    result[aDate][0] = rdr["x"].ToString() + " " + rdr["unit"].ToString();
                                    result[aDate][1] = rdr["y"].ToString() + " " + rdr["unit"].ToString();
                                    result[aDate][2] = rdr["z"].ToString() + " " + rdr["unit"].ToString();
                                }
                                else
                                {
                                    result[aDate] = new string[3];
                                    result[aDate][0] = rdr["x"].ToString();
                                    result[aDate][1] = rdr["y"].ToString();
                                    result[aDate][2] = rdr["z"].ToString();
                                }
                            }
                            else
                            {
                                if (rdr["unit"].ToString().Length > 0)
                                {
                                    var tmp = new string[3];
                                    tmp[0] = rdr["x"].ToString() + " " + rdr["unit"].ToString();
                                    tmp[1] = rdr["y"].ToString() + " " + rdr["unit"].ToString();
                                    tmp[2] = rdr["z"].ToString() + " " + rdr["unit"].ToString();
                                    result.Add(aDate, tmp);
                                }
                                else
                                {
                                    var tmp = new string[3];
                                    tmp[0] = rdr["x"].ToString();
                                    tmp[1] = rdr["y"].ToString();
                                    tmp[2] = rdr["z"].ToString();
                                    result.Add(aDate, tmp);
                                }
                            }
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            var timeString = rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (result.ContainsKey(aDate))
                            {
                                if (rdr["unit"].ToString().Length > 0)
                                    result[aDate].Add(rdr["description"].ToString(), rdr["value"].ToString() + " " + rdr["unit"].ToString());
                                else
                                    result[aDate].Add(rdr["description"].ToString(), rdr["value"].ToString());
                            }
                            else
                            {
                                var tmp = new Dictionary<string, string>();
                                if (rdr["unit"].ToString().Length > 0)
                                    tmp.Add(rdr["description"].ToString(), rdr["value"].ToString() + " " + rdr["unit"].ToString());
                                else
                                    tmp.Add(rdr["description"].ToString(), rdr["value"].ToString());
                                result.Add(aDate, tmp);
                            }
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            var description = rdr["description"].ToString();

                            if (result.ContainsKey(description))
                            {
                                var timeString = rdr["time"].ToString();
                                var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                                if (result[description].ContainsKey(rdr["value"].ToString()))
                                {
                                    result[description][rdr["value"].ToString()].Add(aDate);
                                }
                                else
                                {
                                    var dateList = new List<DateTime>();
                                    dateList.Add(aDate);
                                    result[description].Add(rdr["value"].ToString(), dateList);
                                }
                            }
                            else
                            {
                                var timeString = rdr["time"].ToString();
                                var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                                var dateList = new List<DateTime>();
                                var tmp = new Dictionary<string, List<DateTime>>();
                                dateList.Add(aDate);
                                tmp.Add(rdr["value"].ToString() + " " + rdr["unit"].ToString(), dateList);
                                result.Add(description, tmp);
                            }
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId AND description = ?description";
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    var oParam2 = cmd.Parameters.Add("?description", MySqlDbType.VarChar); oParam2.Value = description;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            var timeString = rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (result.ContainsKey(aDate))
                            {
                                if (rdr["unit"].ToString().Length > 0)
                                    result[aDate].Add(rdr["description"].ToString(), rdr["value"].ToString() + " " + rdr["unit"].ToString());
                                else
                                    result[aDate].Add(rdr["description"].ToString(), rdr["value"].ToString());
                            }
                            else
                            {
                                var tmp = new Dictionary<string, string>();
                                if (rdr["unit"].ToString().Length > 0)
                                    tmp.Add(rdr["description"].ToString(), rdr["value"].ToString() + " " + rdr["unit"].ToString());
                                else
                                    tmp.Add(rdr["description"].ToString(), rdr["value"].ToString());
                                result.Add(aDate, tmp);
                            }
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId ORDER BY time";
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            result[0].Add(rdr["description"].ToString());
                            result[1].Add(rdr["value"].ToString());
                            result[2].Add(rdr["time"].ToString());
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT * FROM" +
                    "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                    "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                    "AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId AND description = ?description";
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    var oParam2 = cmd.Parameters.Add("?description", MySqlDbType.VarChar); oParam2.Value = description;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            result[0].Add(rdr["value"].ToString());
                            result[1].Add(rdr["time"].ToString());
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = "SELECT * FROM" +
                     "(SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin)" +
                     "ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName)" +
                     "AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?originName", MySqlDbType.VarChar); oParam0.Value = originName;
                    var oParam1 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam1.Value = sessionId;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            if (rdr["value"].ToString().Length > 0)
                                if (rdr["unit"].ToString().Length > 0)
                                {                                    
                                    result[0].Add(rdr["time"].ToString());
                                    result[1].Add(rdr["description"].ToString() + " " + rdr["value"].ToString() + " " + rdr["unit"].ToString());
                                }
                                else
                                {
                                    result[0].Add(rdr["time"].ToString());
                                    result[1].Add(rdr["description"].ToString() + " " + rdr["value"].ToString());
                                }
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    var oParam1 = cmd.Parameters.Add("?parameterDescription", MySqlDbType.VarChar); oParam1.Value = parameterDescription;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Added experiment parameter");
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?parameterName", MySqlDbType.VarChar); oParam0.Value = parameterName;
                    var oParam1 = cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam1.Value = experimentName;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Removed scenes order of experiment");
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?experimentName", MySqlDbType.VarChar); oParam0.Value = experimentName;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                        while (rdr.Read())
                        {
                            result.Add(rdr["parameter_description"].ToString());
                        }
                    rdr.Dispose();
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                    var oParam1 = cmd.Parameters.Add("?parameterDescription", MySqlDbType.VarChar); oParam1.Value = parameterDescription;
                    var oParam2 = cmd.Parameters.Add("?parameterValue", MySqlDbType.VarChar); oParam2.Value = parameterValue;
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Inserted session parameters!");

        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override string GetSessionParameter(int sessionId, string parameterName)
    {
        var result = "";

        try
        {
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            var query = "SELECT value FROM session_parameter_values WHERE session_id = ?sessionId AND "
                + "experiment_parameter_id = (SELECT id FROM experiment_parameter WHERE parameter_description = ?parameterName AND experiment_id = (SELECT experiment_id FROM sessions WHERE session_id = ?sessionId))";
            using (cmd = new MySqlCommand(query, con))
            {
                var oParam0 = cmd.Parameters.Add("?sessionId", MySqlDbType.Int32); oParam0.Value = sessionId;
                var oParam1 = cmd.Parameters.Add("?parameterName", MySqlDbType.VarChar); oParam1.Value = parameterName;
                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                    while (rdr.Read())
                    {
                        result = rdr["value"].ToString();
                    }
                rdr.Dispose();

            }

        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = " SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = ?schemaName";


            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?schemaName", MySqlDbType.VarChar); oParam0.Value = schemaName;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            if (rdr["SCHEMA_NAME"] != null)
                            {
                                rdr.Dispose();
                                return true;
                            }

                        }
                        rdr.Dispose();
                    }
                }
            }
        }
        catch (System.Exception ex)
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
            if (!con.State.Equals(ConnectionState.Open)) con.Open();
            query = " SELECT name FROM questionnaires WHERE name = ?questionnaireName ;";

            using (con)
            {
                using (cmd = new MySqlCommand(query, con))
                {
                    var oParam0 = cmd.Parameters.Add("?questionnaireName", MySqlDbType.VarChar); oParam0.Value = questionnaireName;
                    rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            if (rdr["name"] != null)
                            {
                                rdr.Dispose();
                                return true;
                            }

                        }
                        rdr.Dispose();
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return false;
    }

    public override void CreateSchema()
    {
        if (!con.State.Equals(ConnectionState.Open)) con.Open();
        var txt = (TextAsset)Resources.Load("Setup_EVE_DB", typeof(TextAsset));
        var script = new MySqlScript(con, txt.text);
        script.Delimiter = "$$";
        script.Execute();
        con.Close();
    }

    public override void DropSchema()
    {
        if (!con.State.Equals(ConnectionState.Open)) con.Open();
        var query = "DROP DATABASE IF EXISTS EVE";
        cmd = new MySqlCommand(query, con);
        cmd.ExecuteNonQuery();
    }

    private bool IsInserted(string value, string table, string variable)
    {
        if (!con.State.Equals(ConnectionState.Open)) con.Open();
        var query = "SELECT * FROM " + table + " WHERE " + variable + " = ?nameToCompare";
        cmd = new MySqlCommand(query, con);
        var oParam0 = cmd.Parameters.Add("?nameToCompare", MySqlDbType.VarChar); oParam0.Value = value;
        return cmd.ExecuteScalar() != null;
    }
    private void Insert1Value(string value, string table, string variable)
    {
        var query = "INSERT INTO " + table + " (" + variable + ") VALUES (?valueToAdd)";
        if (!con.State.Equals(ConnectionState.Open)) con.Open();
        using (con)
        {
            using (cmd = new MySqlCommand(query, con))
            {
                var oParam0 = cmd.Parameters.Add("?valueToAdd", MySqlDbType.VarChar);
                oParam0.Value = value;
                cmd.ExecuteNonQuery();
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
        if (!con.State.Equals(ConnectionState.Open)) con.Open();
        using (con)
        {
            using (cmd = new MySqlCommand(query, con))
            {
                var oParam0 = cmd.Parameters.Add("?value0ToAdd", MySqlDbType.VarChar);
                oParam0.Value = values[0];
                var oParam1 = cmd.Parameters.Add("?value1ToAdd", MySqlDbType.VarChar);
                oParam1.Value = values[1];
                var oParam2 = cmd.Parameters.Add("?value2ToAdd", MySqlDbType.VarChar);
                oParam2.Value = values[2];
                cmd.ExecuteNonQuery();
            }
        }
        Debug.Log(values + " added  to " + table + ".");
    }

}
