using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire.Representations
{
    public abstract class Representation : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public abstract void InitialiseRepresentation(QuestionnaireManager qSystem);
    }
}
