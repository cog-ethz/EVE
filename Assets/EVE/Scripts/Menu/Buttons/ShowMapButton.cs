using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMapButton : MonoBehaviour {
    private int internalEnvNumber;
    private int internalSessionID;
    private PopUpEvaluationMap internalMap;
    private string internalEnvName;
    private LoggingManager log;
    private GameObject _map;

    public void onClickShowMapButton()
    {
        string[][] envNameMatrix = new string [1][];
        envNameMatrix[0] = new string[1];
        string[] sceneNames  = log.getListOfEnvironments(internalSessionID);
        envNameMatrix[0][0] = sceneNames[internalEnvNumber];

        internalMap = GameObject.Find("EvaluationMap").GetComponent<PopUpEvaluationMap>(); 
        internalMap.SetupMaps(envNameMatrix);
        internalMap.OpenPopUpMap(createXYZfloat(internalSessionID, internalEnvNumber), internalEnvName);
    }

    public void setupButton()
    {
        log = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>().LoggingManager;
    }
    

    public void setEnvNumber(int i)
    {
        internalEnvNumber = i;
    }

    public void setEnvName(string i)
    {
        internalEnvName = i;
    }

    public void setSessionID(int i)
    {
        internalSessionID = i;
    }

    public void setMap(PopUpEvaluationMap map)
    {
        internalMap = map;
    }

    private float[][] createXYZfloat(int sessionID, int sceneID)
    {
        string[][] xyz = createXYZ(sessionID, sceneID);

        int rows = xyz.Length - 1;
        float[][] result = new float[rows][];
        for (int i = 0; i < rows; i++)
            result[i] = new float[6];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < 6; j++)
                result[i][j] = float.Parse(xyz[i + 1][j]);

        return result;
    }

    private string[][] createXYZ(int sessionID, int sceneID)
    {
        List<float>[] xyz_table = log.getXYZ(sessionID, sceneID);
        int rows = xyz_table[0].Count + 1;
        string[][] result_table = new string[rows][];
        for (int i = 0; i < rows; i++)
            result_table[i] = new string[6];

        result_table[0][0] = "Pos X";
        result_table[0][1] = "Pos Y";
        result_table[0][2] = "Pos Z";
        result_table[0][3] = "View X";
        result_table[0][4] = "View Y";
        result_table[0][5] = "View Z";

        for (int i = 0; i < xyz_table[0].Count; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                result_table[i + 1][j] = xyz_table[j][i].ToString();
            }
        }

        return result_table;
    }





}
