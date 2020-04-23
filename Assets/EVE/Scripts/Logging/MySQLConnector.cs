using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MySql.Data.MySqlClient;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.XML.XMLHelper;
using EVE.Scripts.Utils.Mysql;
using MysqlUtils = Assets.EVE.Scripts.Utils.MysqlUtils;

public class MySqlConnector : DatabaseConnector
{

    // MySQL instance specific items
    private MySqlConnection _con = null; // connection object
    private MySqlCommand _cmd = null; // command object
    private MySqlDataReader _rdr = null;
    private MySqlConnectionStringBuilder _sb  = null;


    public override int ConnectToServer(string server, string database, string user, string password)
    {
        var returnId = 0;
        try
        {
            _sb = new MySqlConnectionStringBuilder
            {
                Server = server, Database = database, UserID = user, Password = password
            };
            _con = new MySqlConnection(_sb.ConnectionString);
            _con.Open();
            Debug.Log("Connection State: " + _con.State);
            _con.Close();
        }
        catch (MySqlException mysqlEx)
        {
            Debug.LogError(mysqlEx.ToString());
            switch (mysqlEx.Number)
            {
                //http://dev.mysql.com/doc/refman/5.0/en/error-messages-server.html
                case 1042: // Unable to connect to any of the specified MySQL hosts (Check Server,Port)
                    returnId = -2;
                    break;
                case 0: // Access denied (Check DB name,username,password)
                    if (mysqlEx.Message.Contains("Unknown database"))
                        returnId = -4;
                    else
                        returnId = -3;
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            returnId = -1;
        }
        return returnId;
        
    }

    public override int ConnectToServer(string server, string user, string password)
    {
        var returnId = 0;
        try
        {
            _sb = new MySqlConnectionStringBuilder
            {
                Server = server, UserID = user, Password = password
            };
            _con = new MySqlConnection(_sb.ConnectionString);
            _con.Open();
            Debug.Log("Connection State: " + _con.State);
            _con.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            returnId = -1;
        }
        return returnId;
    }

    public override void InsertAnswer(string questionName, string questionSetName, string questionnaireName, int sessionId, Dictionary<int,string> selectedIndices)
    {
        if (selectedIndices == null) return;
        
        var 
            query = "INSERT INTO store_answers (question_id, user_answer_id) " +
                    "VALUES ((SELECT tmp.id FROM (SELECT qs.id,qs.name,qset.name AS qsetName FROM questions AS qs " +
                    "INNER JOIN question_question_sets AS qqs ON qs.id = qqs.question_id " +
                    "INNER JOIN question_sets AS qset ON  qqs.question_set_id = qset.id) AS tmp " +
                    "WHERE  tmp.qsetName = ?setName AND tmp.name = ?question_name), (SELECT id FROM user_answers WHERE session_id = ?session_id && questionnaire_id = " +
                    "(SELECT id FROM questionnaires WHERE name = ?questionnaire_name) ORDER BY id DESC LIMIT 1))";
        var answerId = -1;

        try
        {
            // FIRST: insert answer into the store_answers
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new[]
                    {
                        new MysqlParameter("?question_name",MySqlDbType.VarChar,questionName),
                        new MysqlParameter("?session_id",MySqlDbType.Int32,sessionId),
                        new MysqlParameter("?setName",MySqlDbType.VarChar,questionSetName),
                        new MysqlParameter("?questionnaire_name",MySqlDbType.VarChar,questionnaireName)
                    });
                }
            }

            answerId = (int)_cmd.LastInsertedId;

            // ============ string values ==============
            
            var keys = selectedIndices.Keys.ToArray();
            for (var i = 0; i < selectedIndices.Keys.Count; i++)
            {
                var insertId = -1;
                query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                MysqlUtils.ReconnectIfNecessary(_con);
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        MysqlUtils.ExecuteWithParameters(_cmd,new[]
                        {
                            new MysqlParameter("?pos",MySqlDbType.Int32,keys[i]),
                            new MysqlParameter("?val",MySqlDbType.VarChar,selectedIndices[keys[i]])
                        });
                    }
                }
                insertId = (int)_cmd.LastInsertedId;

                query = "INSERT INTO answers_stored_strings (answer_id, string_id, type) VALUES (?answer_id, ?insert_id,  \"string\")";
                MysqlUtils.ReconnectIfNecessary(_con);
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        _cmd.Prepare();
                        MysqlUtils.ExecuteWithParameters(_cmd,new[]
                        {
                            new MysqlParameter("?answer_id",MySqlDbType.Int32,answerId),
                            new MysqlParameter("?insert_id",MySqlDbType.Int32,insertId)
                        });
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

            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    val = MysqlUtils.ExecuteAndGetInts(_cmd, "pos",new[]
                    {
                        new MysqlParameter("?questionId",MySqlDbType.Int32,questionId),
                        new MysqlParameter("?sessionId",MySqlDbType.Int32,sessionId)
                    });
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

            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    result = MysqlUtils.ExecuteAndGetIntDictionary(_cmd,"pos","val",new[]
                    {
                        new MysqlParameter("?questionName",MySqlDbType.VarChar,questionName),
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId)
                    });
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

    public override void RemoveSession(int sessionId)
    {
        try
        {
            var query = "DELETE FROM sessions WHERE session_id =?sessionId";
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameter(_cmd,new MysqlParameter("?sessionId",MySqlDbType.Int32,sessionId));
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
        var query = "DELETE FROM scene WHERE scene_name = ?sceneName";
        
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameter(_cmd,new MysqlParameter("?sceneName",MySqlDbType.VarChar,scene.Name));
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
        var query = "Insert INTO experiment_scene_order (scenes_id, experiment_id, experiment_order) VALUES" +
                    "((SELECT id FROM scene WHERE scene_name = ?sceneName),(SELECT id FROM experiment WHERE experiment_name = ?experimentName),?orderNumber)";

        try
        {
            for (var i = 0; i < scenes.Length; i++)
            {
                MysqlUtils.ReconnectIfNecessary(_con);
                using (_con)
                {
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        MysqlUtils.ExecuteWithParameters(_cmd, new[]
                        {
                            new MysqlParameter("?experimentName", MySqlDbType.VarChar, experimentName),
                            new MysqlParameter("?sceneName", MySqlDbType.VarChar, scenes[i].Name),
                            new MysqlParameter("?orderNumber", MySqlDbType.Int32, i)
                        });
                    }
                }
                Debug.Log("Added scene " + i + " to order");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameter(_cmd,new MysqlParameter("?experimentName",MySqlDbType.VarChar,experimentName));
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new[]
                    {
                        new MysqlParameter("?experimentName",MySqlDbType.VarChar,experimentName),
                        new MysqlParameter("?subjectId",MySqlDbType.VarChar,subjectId)
                    });
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new[]
                    {
                        new MysqlParameter("?sessionId",MySqlDbType.Int32,sessionId),
                        new MysqlParameter("?labchart_file",MySqlDbType.VarChar,fileName)
                    });
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new[]
                    {
                        new MysqlParameter("?sessionId",MySqlDbType.Int32,sessionId),
                        new MysqlParameter("?labchart_timestamp", MySqlDbType.VarChar,timestamp)
                    });
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    result = MysqlUtils.ExecuteAndGetStrings(_cmd, "labchart_timestamp",new []
                    {
                        new MysqlParameter("?sessionId", MySqlDbType.Int32,sessionId)
                    })[0];
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
        
        var query = "SELECT max(session_id) FROM EVE.sessions;";
        MysqlUtils.ReconnectIfNecessary(_con);
        using (_con)
        {
            try
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var maxId = -1;
                    maxId = MysqlUtils.ExecuteAndGetInt(_cmd);
                    if (maxId < 0) maxId = 0;
                    Debug.Log("Next session will be " + maxId);
                    return maxId + 1;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        return -1;
    }

    public override void CreateUserAnswer(int sessionId, string questionnaireName)
    {
        try
        {
            var query = "INSERT INTO EVE.user_answers(session_id, questionnaire_id) SELECT ?session_id, id FROM EVE.questionnaires WHERE name = ?questionnaireName";
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?session_id", MySqlDbType.VarChar,sessionId),
                        new MysqlParameter("?questionnaireName", MySqlDbType.VarChar,questionnaireName)
                    });
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
            MysqlUtils.ReconnectIfNecessary(_con);
            query = "SELECT scene_name FROM (SELECT * FROM EVE.experiment_scene_order WHERE experiment_id = 78) AS ex_order INNER JOIN EVE.scene ON ex_order.scenes_id = scene.id ORDER BY experiment_order ASC";

            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameters(_cmd,new []{new MysqlParameter("?experimentId", MySqlDbType.Int32,experimentId)});
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?name", MySqlDbType.VarChar,question.QuestionName),
                        new MysqlParameter("?question", MySqlDbType.VarChar,question.QuestionText), 
                        new MysqlParameter("?type", MySqlDbType.Int32,question.QuestionType) 
                    });
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
                    MysqlUtils.ReconnectIfNecessary(_con);
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            MysqlUtils.ExecuteWithParameters(_cmd,new []
                            {
                                new MysqlParameter("?pos", MySqlDbType.Int32,i), 
                                new MysqlParameter("?val", MySqlDbType.VarChar,question.Vals[i].ToString()), 
                            });
                        }
                    }
                    query = "INSERT INTO questions_stored_strings (questions_id, string_id, type) VALUES (?question_id, (SELECT LAST_INSERT_ID()), \"float\")";
                    MysqlUtils.ReconnectIfNecessary(_con);
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            MysqlUtils.ExecuteWithParameter(_cmd,new MysqlParameter("?question_id", MySqlDbType.Int32, questionId));
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
                    MysqlUtils.ReconnectIfNecessary(_con);
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            MysqlUtils.ExecuteWithParameters(_cmd,new []
                            {
                                new MysqlParameter("?pos", MySqlDbType.Int32,i), 
                                new MysqlParameter("?val", MySqlDbType.VarChar,question.Labels[i])
                            });
                        }
                    }
                    query = "INSERT INTO questions_stored_strings (questions_id, string_id, type) VALUES (?question_id, (SELECT LAST_INSERT_ID()),  \"string\" )";
                    MysqlUtils.ReconnectIfNecessary(_con);
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            MysqlUtils.ExecuteWithParameter(_cmd,new MysqlParameter("?question_id", MySqlDbType.Int32, questionId));
                        }
                    }
                }
                Debug.Log("Inserted label!");
            }

            // Add question to question set
            query = "INSERT INTO question_question_sets (question_set_id, question_id) " +
            "VALUES ((SELECT id FROM question_sets WHERE name = ?setName),?question_id)";
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd, new[]
                    {
                        new MysqlParameter("?question_id", MySqlDbType.VarChar, questionId),
                        new MysqlParameter("?setName", MySqlDbType.VarChar, question.QuestionSet)
                    });
                }
            }
            Debug.Log("Inserted relation between question and question set!");

            // Add the ouput coding to the database
            if (question.Output != null)
            {
                for (var i = 0; i <= question.Output.Length-1; i++)
                {
                    query = "INSERT INTO store_strings (pos, val) VALUES (?pos, ?val)";
                    MysqlUtils.ReconnectIfNecessary(_con);
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            MysqlUtils.ExecuteWithParameters(_cmd,new []
                            {
                                new MysqlParameter("?pos", MySqlDbType.Int32,i), 
                                new MysqlParameter("?val", MySqlDbType.VarChar,question.Output[i].ToString()), 
                            });
                        }
                    }
                    query = "INSERT INTO questions_coded_output (questions_id, string_id) VALUES (?question_id, (SELECT LAST_INSERT_ID()))";
                    MysqlUtils.ReconnectIfNecessary(_con);
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            MysqlUtils.ExecuteWithParameter(_cmd,new MysqlParameter("?question_id", MySqlDbType.Int32, questionId));
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameter(_cmd,new MysqlParameter("?name", MySqlDbType.VarChar, name));
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
            MysqlUtils.ReconnectIfNecessary(_con);
            const string query = " SELECT name FROM questionnaires";

            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    questionnaireNames = MysqlUtils.ExecuteAndGetStrings(_cmd, "name");
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
            var query = "INSERT INTO jumps (question_set, origin_id, dest_id) " +
                        "VALUES ((SELECT id FROM question_sets WHERE name = ?setName),((SELECT tmp.id FROM (SELECT qs.id,qs.name,qset.name AS qsetName FROM questions AS qs " +
                        "INNER JOIN question_question_sets AS qqs ON qs.id = qqs.question_id " +
                        "INNER JOIN question_sets AS qset ON  qqs.question_set_id = qset.id) AS tmp " +
                        "WHERE  tmp.qsetName = ?setName AND tmp.name = ?origin_name)),((SELECT tmp.id FROM (SELECT qs.id,qs.name,qset.name AS qsetName FROM questions AS qs " +
                        "INNER JOIN question_question_sets AS qqs ON qs.id = qqs.question_id " +
                        "INNER JOIN question_sets AS qset ON  qqs.question_set_id = qset.id) AS tmp " +
                        "WHERE  tmp.qsetName = ?setName AND tmp.name = ?dest_name)))";
            // First insert the jumps
            try
            {
                for (var i = 0; i < q.Jumps.Count; i++)
                {
                    var jump = q.Jumps[i];
                    MysqlUtils.ReconnectIfNecessary(_con);
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            MysqlUtils.ExecuteWithParameters(_cmd,new []
                            {
                                new MysqlParameter("?setName", MySqlDbType.VarChar, questionSetName), 
                                new MysqlParameter("?origin_name", MySqlDbType.VarChar, q.Name), 
                                new MysqlParameter("?dest_name", MySqlDbType.VarChar, jump.Destination), 
                            });
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
                            MysqlUtils.ReconnectIfNecessary(_con);
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameter(_cmd,new MysqlParameter("?jumpId", MySqlDbType.Int32, jumpId));
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

        var query = "SELECT COUNT(*) FROM jump_conditions WHERE jump_id = ?jumpId";

        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameter(_cmd,new MysqlParameter("?jumpId", MySqlDbType.Int32, jumpIds[0]));
                    sizeJ = MysqlUtils.ExecuteAndGetInts(_cmd, "COUNT(*)")[0];
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
                    MysqlUtils.ReconnectIfNecessary(_con);
                    using (_con)
                    {
                        using (_cmd = new MySqlCommand(query, _con))
                        {
                            MysqlUtils.AddParameter(_cmd,new MysqlParameter("?jumpId", MySqlDbType.Int32, jumpIds[0]));
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    id = MysqlUtils.ExecuteAndGetInts(_cmd, "id",new []
                    {
                        new MysqlParameter("?questionName", MySqlDbType.VarChar, name)
                    })[0];
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
        const string query = "SELECT name  FROM questions WHERE id = ?questionId";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    name = MysqlUtils.ExecuteAndGetStrings(_cmd, "name",new []
                    {
                        new MysqlParameter("?questionId", MySqlDbType.Int32, id)
                    })[0];
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var results = MysqlUtils.ExecuteAndGetInts(_cmd, "id", new []
                    {
                        new MysqlParameter("?questionSetName", MySqlDbType.VarChar, questionSetName)
                    },-2);
                    if (results != null && results.Count>0)
                    {
                        return results[0];
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

    public override List<int> GetQuestionsOfSet(string questionSetName)
    {
        var query = string.Empty;

        var questionInSet = new List<int>();

        try
        {
            query = "SELECT question_id  FROM question_question_sets WHERE question_set_id = (SELECT id FROM question_sets WHERE name = ?questionSetName)";
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameter(_cmd,new MysqlParameter("?questionSetName", MySqlDbType.VarChar, questionSetName));
                    questionInSet = MysqlUtils.ExecuteAndGetInts(_cmd, "question_id");
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    jumpIds = MysqlUtils.ExecuteAndGetInts(_cmd, "id",new []
                    {
                        new MysqlParameter("?questionId", MySqlDbType.Int32, questionId)
                    });
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    destId = MysqlUtils.ExecuteAndGetInts(_cmd, "dest_id",new []
                    {
                        new MysqlParameter("?jumpId", MySqlDbType.Int32, jumpId)
                    })[0];
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?questionnaireName", MySqlDbType.VarChar, questionnaireName),
                        new MysqlParameter("?question_set", MySqlDbType.VarChar, questionSetName), 
                    });
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
        var query = "SELECT name FROM (SELECT * FROM EVE.questionnaire_question_sets WHERE questionnaire_id = (SELECT id FROM EVE.questionnaires WHERE name = ?name) ORDER BY id ASC) AS qqs INNER JOIN EVE.question_sets qs ON qqs.question_set_id = qs.id ORDER BY qs.id;";
        var qsNames = new List<string>();
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    _cmd.Prepare();
                    qsNames = MysqlUtils.ExecuteAndGetStrings(_cmd, "name", new []
                    {
                        new MysqlParameter("?name", MySqlDbType.VarChar, questionnaireName)
                    });
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
            query = "SELECT name FROM (SELECT * FROM questionnaire_question_sets  WHERE questionnaire_id = ?questionnaireId) " + 
                    "AS qsi INNER JOIN question_sets ON qsi.question_set_id = question_sets.id";
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    qsNames = MysqlUtils.ExecuteAndGetStrings(_cmd, "name",new []
                    {
                        new MysqlParameter("?questionnaireId", MySqlDbType.Int32, questionnaireId)
                    });
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameter(_cmd, new MysqlParameter("?internalQuestionId", MySqlDbType.Int32, internalQuestionId));
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
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    floatCount = MysqlUtils.ExecuteAndGetInts(_cmd, "Count(*)",new []
                    {
                        new MysqlParameter("?internalQuestionId", MySqlDbType.Int32, internalQuestionId)
                    })[0];
                }
            }

            if (floatCount > 0)
            {

                vals = new int[floatCount];

                var i = 0;

                query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type =  \"float\" )" +
                        " AS questions_stored_string ON (questions_stored_string.string_id = store_strings.id)";
                MysqlUtils.ReconnectIfNecessary(_con);
                using (_con)
                {                    
                    using (_cmd = new MySqlCommand(query, _con))
                    {
                        MysqlUtils.AddParameter(_cmd, new MysqlParameter("?internalQuestionId", MySqlDbType.Int32, internalQuestionId));
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
        const string query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_stored_strings WHERE questions_id = ?internalQuestionId AND type =  \"string\" ) AS questions_stored_string ON (questions_stored_string.string_id = store_strings.id)";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var result = MysqlUtils.ExecuteAndGetStrings(_cmd, "val", new[]
                    {
                        new MysqlParameter("?internalQuestionId", MySqlDbType.Int32, internalQuestionId)
                    });
                    return result.ToArray();
                }
            }
            
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override string GetQuestionsSetName(int internalQuestionId)
    {
        const string query = "SELECT name FROM question_sets WHERE id = (SELECT question_set_id FROM question_question_sets WHERE question_id = ?internalQuestionId )";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var questionSetName = MysqlUtils.ExecuteAndGetStrings(_cmd, "name",new []
                    {
                        new MysqlParameter("?internalQuestionId", MySqlDbType.Int32, internalQuestionId)
                    })[0];
                    return questionSetName;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override int[] GetQuestionOutput(int internalQuestionId)
    {
        const string query = "SELECT val FROM store_strings INNER JOIN (SELECT string_id FROM questions_coded_output WHERE questions_id = (SELECT id FROM questions WHERE id = ?internalQuestionId )) AS questions_code ON (questions_code.string_id = store_strings.id)";

        try
        {
            // Get output codes from database
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var result = MysqlUtils.ExecuteAndGetInts(_cmd, "val", new[]
                    {
                        new MysqlParameter("?internalQuestionId", MySqlDbType.Int32, internalQuestionId)
                    });
                    return result.ToArray();
                }
            }
            
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override string[] GetSessionData(int sessionId)
    {
        const string query = "SELECT session_id, subject_id, labchart_timestamp, labchart_file FROM sessions WHERE session_id = ?sessionId";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameter(_cmd, new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId));
                    var result = new string[4];
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
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override string[][] GetAllSessionsData(string experimentName)
    {
        const string query = "SELECT session_id, subject_id, labchart_timestamp, labchart_file FROM sessions LEFT JOIN (SELECT id FROM experiment WHERE id = (SELECT id FROM experiment WHERE experiment_name = ?experimentName)) AS selected_experiment ON sessions.experiment_id = selected_experiment.id";

        const int dataNumber = 4;
        var sessionIds = new List<string>[dataNumber];
        for (var i = 0; i < dataNumber; i++) sessionIds[i] = new List<string>();
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameter(_cmd,
                        new MysqlParameter("?experimentName", MySqlDbType.VarChar, experimentName));
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
            }

            var result = new string[dataNumber][];
            for (var j = 0; j < dataNumber; j++)
                result[j] = sessionIds[j].ToArray();

            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override int getExperimentId(int sessionId)
    {
        const string query = " SELECT experiment_id FROM SESSIONS WHERE session_id = ?sessionId";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var result = MysqlUtils.ExecuteAndGetInts(_cmd, "experiment_id", new []
                    {
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId)
                    })[0];
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return -1;
    }

    public override int getExperimentId(string experimentName)
    {
        const string query = " SELECT id FROM experiment WHERE experiment_name = ?experimentName";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var result = MysqlUtils.ExecuteAndGetInts(_cmd, "id", new []
                    {
                        new MysqlParameter("?experimentName", MySqlDbType.VarChar, experimentName)
                    })[0];
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return -1;
    }

    public override int[] GetAnsweredQuestionnaireIds(int[] sessionsIds)
    {
        var questionnaireIds = new List<int>();
        
        var query = "SELECT DISTINCT (questionnaire_id) AS questionnaire_id FROM user_answers WHERE session_id = " + sessionsIds[0];
        for (var i = 1; i < sessionsIds.Length; i++)
        {
            query += " OR session_id = " + sessionsIds[i];
        }

        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_cmd = new MySqlCommand(query, _con))
            {
                questionnaireIds = MysqlUtils.ExecuteAndGetInts(_cmd, "questionnaire_id");
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
            query = "SELECT id FROM questions INNER JOIN (" +
                    "SELECT DISTINCT(question_id)  FROM store_answers INNER JOIN (SELECT id FROM user_answers WHERE questionnaire_id = " + questionnaireId +
                    " AND session_id = " + questionnaireIds[0];
            for (var j = 1; j < questionnaireIds.Length; j++)
            {
                query += " OR session_id = " + questionnaireIds[j];
            }
            query += " ) AS user_answers_id ON (user_answers_id.id = store_answers.user_answer_id))" +
                     "AS question_internal_ids ON (question_internal_ids.question_id = questions.id)";
            try
            {
                MysqlUtils.ReconnectIfNecessary(_con);
                using (_cmd = new MySqlCommand(query, _con))
                {
                    answerIDs[i] = MysqlUtils.ExecuteAndGetInts(_cmd,"id");
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
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            const string query = "SELECT * FROM data_origin";   
            using (_cmd = new MySqlCommand(query, _con))
            {
                var result = MysqlUtils.ExecuteAndGetStrings(_cmd, "device_name");
                return result;
            }          
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override void AddDataOrigin(string originName)
    {
        // Add a sensor (only if it hasn't been added yet)
        TryInsert1Value(originName,"data_origin","device_name");
    }

    public override void RemoveDataOrigin(string originName)
    {
        
        const string selectQuery = "SELECT * FROM data_origin WHERE device_name = ?originName";
        const string deleteQuery = "DELETE FROM data_origin WHERE device_name = ?originName";
        // Remove a sensor
        try
        {
            string result;
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(selectQuery, _con))
                {
                    result = MysqlUtils.ExecuteAndGetString(_cmd,
                        new[] {new MysqlParameter("?originName", MySqlDbType.VarChar, originName)});
                }
            }
            if (result != null)
            {
                MysqlUtils.ReconnectIfNecessary(_con);
                using (_con)
                {
                    using (_cmd = new MySqlCommand(deleteQuery, _con))
                    {
                        MysqlUtils.ExecuteWithParameter(_cmd,
                            new MysqlParameter("?originName", MySqlDbType.VarChar, originName));
                    }
                }
                Debug.Log("Removed data origin: [" + originName + "]");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddDataOutput(string originName, string outputDescription)
    {
        const string insertQuery = "INSERT INTO data_description (device_id, description) VALUES ((SELECT id FROM data_origin WHERE device_name = ?originName), ?outputDescription)";
        const string selectQuery = "SELECT * FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND  description = ?outputDescription";
        // Add a particular output of a sensor (only if it hasn't been added yet)
        try
        {
            string result;
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(selectQuery, _con))
                {
                    result = MysqlUtils.ExecuteAndGetString(_cmd, new[]
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName),
                        new MysqlParameter("?outputDescription", MySqlDbType.VarChar, outputDescription),
                    });
                }
            }
            if (result == null)
            {
                MysqlUtils.ReconnectIfNecessary(_con);
                using (_con)
                {
                    using (_cmd = new MySqlCommand(insertQuery, _con))
                    {
                        MysqlUtils.ExecuteWithParameters(_cmd,new []
                        {
                            new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                            new MysqlParameter("?outputDescription", MySqlDbType.VarChar, outputDescription), 
                        });
                    }
                }
                Debug.Log("Inserted data_description: [" + originName + ", " + outputDescription + "]");
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddDataUnit(string originName, string outputDescription, string unitName)
    {
        const string selectQuery = "SELECT * FROM data_units WHERE description_id = (SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName ) AND description = ?outputDescription) AND unit = ?unitName";
        const string insertQuery = "INSERT INTO data_units (description_id, unit) VALUES ((SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND description = ?outputDescription), ?unitName)";

        // adds a data unit if it has not been added yet
        try
        {
            string result;
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(selectQuery, _con))
                {
                    result = MysqlUtils.ExecuteAndGetString(_cmd, new[]
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName),
                        new MysqlParameter("?outputDescription", MySqlDbType.VarChar, outputDescription),
                        new MysqlParameter("?unitName", MySqlDbType.VarChar, unitName)
                    });
                }
            }
            if (result == null)
            {
                MysqlUtils.ReconnectIfNecessary(_con);
                using (_con)
                {
                    using (_cmd = new MySqlCommand(insertQuery, _con))
                    {
                        MysqlUtils.ExecuteWithParameters(_cmd,new []
                        {
                            new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                            new MysqlParameter("?outputDescription", MySqlDbType.VarChar, outputDescription),
                            new MysqlParameter("?unitName", MySqlDbType.VarChar, unitName) 
                        });
                    }
                }
                Debug.Log("Inserted unit data: [" + originName + ", " + outputDescription + ", " + unitName + "]");
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
            const string query = "INSERT INTO sensor_data (data_description_id, session_id, value,time) VALUES ((SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND description = ?outputDescription),?sessionId,?value,?time)";
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?outputDescription", MySqlDbType.VarChar, outputDescription), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId), 
                        new MysqlParameter("?value", MySqlDbType.VarChar, value),
                        new MysqlParameter("?time", MySqlDbType.VarChar, time) 
                    });
                }
            }
            Debug.Log("Inserted Sensor data: [" + originName + ", " + outputDescription + ", " + value + "]");
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
            const string query = "INSERT INTO sensor_data_3d (data_description_id, session_id, x, y, z, time) VALUES ((SELECT id FROM data_description WHERE device_id = (SELECT id FROM data_origin WHERE device_name = ?originName) AND description = ?outputDescription),?sessionId,?valueX, ?valueY, ?valueZ,?time)";
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?outputDescription", MySqlDbType.VarChar, outputDescription), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId), 
                        new MysqlParameter("?valueX", MySqlDbType.VarChar, valueX),
                        new MysqlParameter("?valueY", MySqlDbType.VarChar, valueY),
                        new MysqlParameter("?valueZ", MySqlDbType.VarChar, valueZ),
                        new MysqlParameter("?time", MySqlDbType.VarChar, time) 
                    });
                }
            }
            Debug.Log("Inserted Sensor data: [" + originName + ", " + outputDescription + ", " + valueX + ", " + valueY + ", " + valueZ + "]");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override void AddSystemData(String originName, String outputDescription, String value, String time, int sessionId)
    {
        value = value.Length > 45 ? value.Substring(0, 45) : value;
        try
        {
            const string query = "INSERT INTO EVE.system_data (data_description_id, session_id, value,time) SELECT id,?sessionId,?value,?time FROM EVE.data_description WHERE device_id = (SELECT id FROM EVE.data_origin WHERE device_name = ?originName) AND  description = ?outputDescription";
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?outputDescription", MySqlDbType.VarChar, outputDescription), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId), 
                        new MysqlParameter("?value", MySqlDbType.VarChar, value),
                        new MysqlParameter("?time", MySqlDbType.VarChar, time) 
                    });
                }
            }
            Debug.Log("Inserted System data: [" + originName + ", " + outputDescription + ", " + value + "]");
        }
        catch (Exception ex)
        {            
            Debug.Log("Tried insert System data: [" + originName + ", " + outputDescription + ", " + value + "]");
            Debug.LogError(ex.ToString());
        }
    }

    public override Dictionary<DateTime, string[]> Get3DMeasuredDataByTime(string originName, string description, int sessionId)
    {
        const string query = "SELECT * FROM (SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin) ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName) AS data_w_desc INNER JOIN (sensor_data_3d)ON(sensor_data_3d.data_description_id = data_w_desc.id) WHERE session_id = ?sessionId AND description = ?description";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId), 
                        new MysqlParameter("?description", MySqlDbType.VarChar, description)
                    });
                    var result = new Dictionary<DateTime, string[]>();
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            var timeString = _rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (!result.ContainsKey(aDate))
                            {
                                result.Add(aDate, null);
                            }
                            if (_rdr["unit"].ToString().Length > 0)
                            {
                                result[aDate] = new[] {_rdr["x"] + " " + _rdr["unit"], _rdr["y"] + " " + _rdr["unit"], _rdr["z"] + " " + _rdr["unit"]};
                            }
                            else
                            {
                                result[aDate] = new[] {_rdr["x"].ToString(), _rdr["y"].ToString(), _rdr["z"].ToString()};
                            }
                        }
                    _rdr.Dispose();
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override Dictionary<DateTime, Dictionary<string, string>> GetMeasuredDataByTime(string originName, int sessionId)
    {
        const string query = "SELECT * FROM (SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin) ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName) AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId)
                    });
                    var result = new Dictionary<DateTime, Dictionary<string, string>>();
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            var timeString = _rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (!result.ContainsKey(aDate))
                            {
                                result.Add(aDate, new Dictionary<string, string>());
                            }
                            if (_rdr["unit"].ToString().Length > 0)
                                result[aDate].Add(_rdr["description"].ToString(), _rdr["value"].ToString() + " " + _rdr["unit"].ToString());
                            else
                                result[aDate].Add(_rdr["description"].ToString(), _rdr["value"].ToString());
                        }
                    _rdr.Dispose();
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override Dictionary<string, Dictionary<string, List<DateTime>>> GetMeasuredDataByName(string originName, int sessionId)
    {

        const string query = "SELECT * FROM (SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin) ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName) AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId)
                    });
                    var result = new Dictionary<string, Dictionary<string, List<DateTime>>>();

                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            var description = _rdr["description"].ToString();
                            var dateTime = DateTime.ParseExact(_rdr["time"].ToString(), "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            
                            if (!result.ContainsKey(description))
                            {
                                result.Add(description, new Dictionary<string, List<DateTime>>());
                            }
                            if (!result[description].ContainsKey(_rdr["value"].ToString()))
                            {
                                result[description].Add(_rdr["value"].ToString(), new List<DateTime>());
                            }
                            result[description][_rdr["value"].ToString()].Add(dateTime);
                        }
                    _rdr.Dispose();
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override Dictionary<DateTime, Dictionary<string, string>> GetSystemDataByTime(string originName, string description, int sessionId)
    {
        const string query = "SELECT * FROM (SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin) ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName) AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId AND description = ?description";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId), 
                        new MysqlParameter("?description", MySqlDbType.VarChar, description)
                    });
                    var result = new Dictionary<DateTime, Dictionary<string, string>>();
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            var timeString = _rdr["time"].ToString();
                            var aDate = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("de-DE"));
                            if (!result.ContainsKey(aDate))
                            {
                                result.Add(aDate, new Dictionary<string, string>());
                            }
                            if (_rdr["unit"].ToString().Length > 0)
                                result[aDate].Add(_rdr["description"].ToString(), _rdr["value"].ToString() + " " + _rdr["unit"].ToString());
                            else
                                result[aDate].Add(_rdr["description"].ToString(), _rdr["value"].ToString());
                        }
                    _rdr.Dispose();
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override List<string>[] GetSystemData(string originName, int sessionId)
    {
        const string query = "SELECT * FROM (SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin) ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName) AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId ORDER BY time";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId)
                    });
                    var result = new List<string>[3];
                    result[0] = new List<string>();
                    result[1] = new List<string>();
                    result[2] = new List<string>();
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            result[0].Add(_rdr["description"].ToString());
                            result[1].Add(_rdr["value"].ToString());
                            result[2].Add(_rdr["time"].ToString());
                        }
                    _rdr.Dispose();
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override List<string>[] GetSystemData(string originName, string description, int sessionId)
    {
        const string query = "SELECT * FROM (SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin) ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName) AS data_w_desc INNER JOIN (system_data)ON(system_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId AND description = ?description";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId), 
                        new MysqlParameter("?description", MySqlDbType.VarChar, description)
                    });
                    var result = new List<string>[2];
                    result[0] = new List<string>();
                    result[1] = new List<string>();

                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            result[0].Add(_rdr["value"].ToString());
                            result[1].Add(_rdr["time"].ToString());
                        }
                    _rdr.Dispose();
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override List<string>[] GetMeasurementsDataAsString(string originName, int sessionId)
    {
        const string query = "SELECT * FROM (SELECT output_w_unit.id, description, device_name, unit FROM(SELECT output_w_sensor.id, description, device_name FROM data_description AS output_w_sensor INNER JOIN(data_origin) ON(data_origin.id = output_w_sensor.device_id)) AS output_w_unit LEFT JOIN(data_units) ON(data_units.description_id = output_w_unit.id) WHERE output_w_unit.device_name = ?originName) AS data_w_desc INNER JOIN (sensor_data)ON(sensor_data.data_description_id = data_w_desc.id) WHERE session_id =?sessionId";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameters(_cmd,new []
                    {
                        new MysqlParameter("?originName", MySqlDbType.VarChar, originName), 
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId)
                    });
                    
                    var result = new List<string>[4];
                    result[0] = new List<string>();
                    result[1] = new List<string>();
                    result[2] = new List<string>();
                    result[3] = new List<string>();
                    
                    _rdr = _cmd.ExecuteReader();
                    if (_rdr.HasRows)
                        while (_rdr.Read())
                        {
                            if (_rdr["value"].ToString().Length > 0)
                            {
                                result[0].Add(_rdr["time"].ToString());
                                result[1].Add(_rdr["description"].ToString());
                                result[2].Add(_rdr["value"].ToString());
                                if (_rdr["unit"].ToString().Length > 0)
                                {
                                    result[3].Add(_rdr["unit"].ToString());
                                }
                            }
                        }
                    _rdr.Dispose();
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }


    public override void CreateExperimentParameter(string experimentName, string parameterDescription)
    {
        const string query = "INSERT INTO experiment_parameter(experiment_id, parameter_description) VALUES((SELECT id FROM experiment WHERE experiment_name = ?experimentName), ?parameterDescription)";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?experimentName", MySqlDbType.VarChar, experimentName), 
                        new MysqlParameter("?parameterDescription", MySqlDbType.VarChar, parameterDescription)
                    });
                }
            }
            Debug.Log("Removed parameter [" + parameterDescription + "] from experiment " + experimentName);
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
        const string query = "DELETE FROM experiment_parameter WHERE experiment_id = (SELECT id FROM experiment WHERE experiment_name = ?experimentName) AND parameter_description = ?parameterName";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?parameterName", MySqlDbType.VarChar, parameterName), 
                        new MysqlParameter("?experimentName", MySqlDbType.VarChar, experimentName)
                    });
                }
            }
            Debug.Log("Removed parameter [" + parameterName + "] from experiment " + experimentName);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override List<string> GetExperimentParameters(string experimentName)
    {
        const string query = "SELECT parameter_description FROM experiment_parameter WHERE experiment_id = (SELECT id FROM experiment WHERE experiment_name = ?experimentName)";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.AddParameter(_cmd,new MysqlParameter("?experimentName", MySqlDbType.VarChar, experimentName));
                    return MysqlUtils.ExecuteAndGetStrings(_cmd, "parameter_description");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override void LogSessionParameter(int sessionId, string parameterDescription, string parameterValue)
    {
        const string query = "INSERT INTO session_parameter_values(session_id, experiment_parameter_id, value) VALUES(?sessionId, (SELECT id FROM experiment_parameter WHERE parameter_description = ?parameterDescription), ?parameterValue)";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    MysqlUtils.ExecuteWithParameters(_cmd,new []
                    {
                        new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId),
                        new MysqlParameter("?parameterDescription", MySqlDbType.VarChar, parameterDescription), 
                        new MysqlParameter("?parameterValue", MySqlDbType.VarChar, parameterValue)
                    });
                }
            }
            Debug.Log("Inserted session " + sessionId + " parameters: [" + parameterDescription + ", " + parameterValue + "]");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public override string GetSessionParameter(int sessionId, string parameterName)
    {
        const string query = "SELECT value FROM session_parameter_values WHERE session_id = ?sessionId AND experiment_parameter_id = (SELECT id FROM experiment_parameter WHERE parameter_description = ?parameterName AND experiment_id = (SELECT experiment_id FROM sessions WHERE session_id = ?sessionId))";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_cmd = new MySqlCommand(query, _con))
            {
                var result = MysqlUtils.ExecuteAndGetStrings(_cmd, "value",new []
                {
                    new MysqlParameter("?sessionId", MySqlDbType.Int32, sessionId),
                    new MysqlParameter("?parameterName", MySqlDbType.VarChar, parameterName)
                })[0];
                return result;
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }

    public override bool CheckSchemaExists(string schemaName)
    {
        const string query = " SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = ?schemaName";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var result = MysqlUtils.ExecuteAndGetString(_cmd,new[]
                    {
                        new MysqlParameter("?schemaName", MySqlDbType.VarChar, schemaName)
                    });
                    return result != null;
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
        const string query = " SELECT name FROM questionnaires WHERE name = ?questionnaireName ;";
        try
        {
            MysqlUtils.ReconnectIfNecessary(_con);
            using (_con)
            {
                using (_cmd = new MySqlCommand(query, _con))
                {
                    var result = MysqlUtils.ExecuteAndGetString(_cmd,new[]
                    {
                        new MysqlParameter("?questionnaireName", MySqlDbType.VarChar, questionnaireName)
                    });
                    return result != null;
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
        var txt = (TextAsset)Resources.Load("Setup_EVE_DB", typeof(TextAsset));
        MysqlUtils.ReconnectIfNecessary(_con);
        using (_con)
        {
            var script = new MySqlScript(_con, txt.text) {Delimiter = "$$"};
            script.Execute();
        }
    }

    public override void DropSchema()
    {
        const string query = "DROP DATABASE IF EXISTS EVE";
        MysqlUtils.ReconnectIfNecessary(_con);
        using (_con)
        {
            using (_cmd = new MySqlCommand(query, _con))
            {
                _cmd.ExecuteNonQuery();
            }
        }
    }

    private bool IsInserted(string value, string table, string variable)
    {
        var query = "SELECT * FROM "+table+" WHERE "+variable+" = ?nameToCompare";
        MysqlUtils.ReconnectIfNecessary(_con);
        using (_con)
        {
            using (_cmd = new MySqlCommand(query, _con))
            {
                var result = MysqlUtils.ExecuteAndGetString(_cmd,new[]
                {
                    new MysqlParameter("?nameToCompare", MySqlDbType.VarChar, value)
                });
                return result != null;
            }
        }
    }
    private void Insert1Value(string value, string table, string variable)
    {
        var query = "INSERT INTO "+table+" ("+variable+") VALUES (?valueToAdd)";
        MysqlUtils.ReconnectIfNecessary(_con);
        using (_con)
        {
            using (_cmd = new MySqlCommand(query, _con))
            {
                MysqlUtils.ExecuteWithParameters(_cmd, new []
                {
                    new MysqlParameter("?valueToAdd", MySqlDbType.VarChar, value)
                });
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

    private void Insert3Values(string[] values, string table, string[] variables)
    {
        if (variables.Length != 3 || values.Length != 3)
        {
            Debug.LogError("Tried to insert the wrong number of values");
            return;
        }
        var query = "INSERT INTO " + table + " ("+variables[0]+", "+variables[1]+", "+variables[2]+") VALUES (?value0ToAdd, ?value1ToAdd, ?value2ToAdd)";
        MysqlUtils.ReconnectIfNecessary(_con);
        using (_con)
        {
            using (_cmd = new MySqlCommand(query, _con))
            {
                MysqlUtils.ExecuteWithParameters(_cmd,new []
                {
                    new MysqlParameter("?value0ToAdd", MySqlDbType.VarChar, values[0]), 
                    new MysqlParameter("?value1ToAdd", MySqlDbType.VarChar, values[1]), 
                    new MysqlParameter("?value2ToAdd", MySqlDbType.VarChar,values[2]) 
                });
            }
        }
        Debug.Log(values + " added  to " + table + ".");
    }
}
