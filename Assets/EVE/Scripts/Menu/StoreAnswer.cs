using System.Collections;
using System.Collections.Generic;
using Assets.EVE.Scripts.Questionnaire2;
using UnityEngine;
using UnityEngine.UI;

public class StoreAnswer : MonoBehaviour
{

    private int _pos = -1;
    private bool _displayableTextField;
    private bool _currentlyDisplayed;

    private Assets.EVE.Scripts.Questionnaire2.Questions.Question q;
    private InputField input;

    void Awake()
    {
        q = GameObject.Find("QuestionPlaceholder")
            .GetComponent<QuestionStorage>()
            .GetQuestion();
    }

    public void SetActiveDisableTextField(bool enabled)
    {
        _displayableTextField = enabled;
        if (_displayableTextField)
        {
            input = gameObject.transform.parent.parent.Find("InputField").GetComponent<InputField>();
        }
    }

    public void SetPositionOffset(int offset)
    {
        _pos = offset;
    }

    public void SetAnswerString(string internalAnswer)
    {
        if (_pos == -1)
        {
            q.RetainAnswer(internalAnswer);
        }
        else
        {
            q.RetainAnswer(_pos, internalAnswer);
        }
        Debug.Log("Passed answer " + internalAnswer + " at " + _pos);
    }

    public void OnClickPassOnNumber()
    {
        if (_pos > -1)
        {
            q.RetainAnswer(_pos);
            Debug.Log("Passed number " + _pos);
        }
        if (_displayableTextField)
        {
            //switch whether the text can be edited based on whether button is selected
            _currentlyDisplayed = !_currentlyDisplayed;
            input.interactable = _currentlyDisplayed;
        }
    }
}
