using System.Collections.Generic;
using System.Data;
using EVE.Scripts.Utils.Mysql;
using MySql.Data.MySqlClient;
using UnityEngine;

namespace Assets.EVE.Scripts.Utils
{
    public static class MysqlUtils {

        /// <summary>
        /// Assert that the database is currently open and if not reopen connection.
        /// </summary>
        /// <remarks>
        /// Does not throw an error.
        /// </remarks>
        /// <param name="connection">Database connection</param>
        public static void ReconnectIfNecessary(MySqlConnection connection)
        {
            if (connection.State != ConnectionState.Open) connection.Open();
        }

        /// <summary>
        /// Execute the command and returns a result column.
        /// </summary>
        /// <param name="command">Query to be executed.</param>
        /// <param name="columnName">Column to be returned.</param>
        /// <param name="parameters">Optional: command parameters</param>
        /// <param name="errorId">Optional: Return error code</param>
        /// <returns>Integer values in query result column</returns>
        public static List<int> ExecuteAndGetInts(MySqlCommand command, string columnName, MysqlParameter[] parameters = null, int errorId = -1)
        {
            var results = new List<int>();
            if (parameters != null)
            {
                AddParameters(command,parameters);
            }
            var rdr = command.ExecuteReader();
            if (rdr.HasRows)
                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(rdr.GetOrdinal(columnName)))
                    {
                        results.Add(int.Parse(rdr[columnName].ToString()));
                    }
                    else
                    {
                        results.Add(errorId);
                    }
                }
            rdr.Dispose();
            return results;
        }

        /// <summary>
        /// Execute the command and returns a dictionary.
        /// </summary>
        /// <param name="command">Query to be executed.</param>
        /// <param name="keyName">Column containing key.</param>
        /// <param name="valName">Column containing value.</param>
        /// <param name="parameters">Optional: command parameters</param>
        /// <returns>Integer values in query result column</returns>
        public static Dictionary<int,string> ExecuteAndGetIntDictionary(MySqlCommand command, string keyName, string valName, MysqlParameter[] parameters = null)
        {
            var results = new Dictionary<int,string>();
            if (parameters != null)
            {
                AddParameters(command,parameters);
            }
            var rdr = command.ExecuteReader();
            if (rdr.HasRows)
                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(rdr.GetOrdinal(keyName)))
                    {
                        results.Add(int.Parse(rdr[keyName].ToString()), rdr[valName].ToString());
                    }
                }
            rdr.Dispose();
            return results;
        }

        /// <summary>
        /// Execute the command and returns a single integer.
        /// </summary>
        /// <param name="command">Query to be executed.</param>
        /// <param name="parameters">Optional: Parameters for command.</param>
        /// <param name="errorId">Optional: change error id.</param>
        /// <returns>Integer value in query result</returns>
        public static int ExecuteAndGetInt(MySqlCommand command, MysqlParameter[] parameters = null, int errorId = -1)
        {
            if (parameters != null)
            {
                AddParameters(command,parameters);
            }
            var result = errorId;
            var scalar = command.ExecuteScalar();
            if (scalar != null && scalar.ToString().Length>0) result = int.Parse(scalar.ToString());
            
            return result;
        }
        
        
        /// <summary>
        /// Execute the command and returns a result column.
        /// </summary>
        /// <param name="command">Query to be executed.</param>
        /// <param name="columnName">Column to be returned.</param>
        /// <returns>String values in query result column</returns>
        public static List<string> ExecuteAndGetStrings(MySqlCommand command, string columnName, MysqlParameter[] parameters = null)
        {
            var results = new List<string>();
            if (parameters != null)
            {
                AddParameters(command,parameters);
            }
            var rdr = command.ExecuteReader();
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    if (rdr[columnName] != null)
                    {
                        results.Add(rdr[columnName].ToString());
                    }
                }
            }
            rdr.Dispose();
            return results;
        }
        
        /// <summary>
        /// Execute the command and returns a single integer.
        /// </summary>
        /// <param name="command">Query to be executed.</param>
        /// <returns>Integer value in query result</returns>
        public static string ExecuteAndGetString(MySqlCommand command, MysqlParameter[] parameters = null)
        {
            string result = null;
            if (parameters != null)
            {
                AddParameters(command,parameters);
            }
            var scalar = command.ExecuteScalar();
            if (scalar != null) result = scalar.ToString();
            return result;
        }

        /// <summary>
        /// Adds parameters to a command.
        /// </summary>
        /// <param name="command">Command to be parametrised.</param>
        /// <param name="parameters">Parameters to be added.</param>
        /// <param name="types">Database data type of parameters</param>
        /// <param name="values">Values to be stored in parametrised command.</param>
        public static void AddParameters(MySqlCommand command, MysqlParameter[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var tempParameter = command.Parameters.Add(parameters[i].name, parameters[i].type);
                tempParameter.Value = parameters[i].value;
            }
        }
        
        /// <summary>
        /// Adds a single parameters to a command.
        /// </summary>
        /// <param name="command">Command to be parametrised.</param>
        /// <param name="parameters">Parameters to be added.</param>
        /// <param name="types">Database data type of parameters</param>
        /// <param name="values">Values to be stored in parametrised command.</param>
        public static void AddParameter(MySqlCommand command, MysqlParameter parameters)
        {
            var tempParameter = command.Parameters.Add(parameters.name, parameters.type);
            tempParameter.Value = parameters.value;
        }
        
        /// <summary>
        /// Executes a non-query with parameters.
        /// </summary>
        /// <param name="command">Non-query command.</param>
        /// <param name="parameters">Parameters to be added.</param>
        /// <param name="types">Database data type of parameters.</param>
        /// <param name="values">Values for parameters.</param>
        public static void ExecuteWithParameters(MySqlCommand command, MysqlParameter[] parameters)
        {
            AddParameters(command, parameters);
            command.ExecuteNonQuery();
        }
        
        /// <summary>
        /// Executes a non-query with a single parameter.
        /// </summary>
        /// <param name="command">Non-query command.</param>
        /// <param name="parameters">Parameters to be added.</param>
        /// <param name="types">Database data type of parameters.</param>
        /// <param name="values">Values for parameters.</param>
        public static void ExecuteWithParameter(MySqlCommand command, MysqlParameter parameter)
        {
            AddParameter(command, parameter);
            command.ExecuteNonQuery();
        }
    }
}
