using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Assets.EVE.Scripts.XML;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire2
{
    public class QuestionnaireFactory
    {

        private readonly LoggingManager _log;
        private ExperimentSettings _settings;

        public QuestionnaireFactory(LoggingManager log, ExperimentSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        public void WriteQuestionnairesToDb(IEnumerable<string> names)
        {
            var questionnaires = names.Select(ReadQuestionnaireToXml).ToList();
            foreach (var questionnaire in questionnaires)
            {
                foreach (var questionnaireQuestionSet in questionnaire.QuestionSets)
                {
                    WriteQuestionSetToDb(questionnaireQuestionSet);
                }

                // Create a questionnaire entry in DB
                _log.addQuestionnaire(questionnaire.Name);
                // Then for each question set of the questionnaire add an entry into DB (q_q_sets table)
                foreach (var questionnaireQuestionSet in questionnaire.QuestionSets)
                {
                    _log.setupQuestionnaire(questionnaire.Name, questionnaireQuestionSet);
                }
            }
        }

        public List<Questionnaire> ReadQuestionnairesFromDb(IEnumerable<string> names)
        {
            var questionnaires = new List<Questionnaire>();
            foreach (var name in names)
            {
                var questionnaire = new Questionnaire(name);
                var sets = _log.GetQuestionSets(name);
                questionnaire.QuestionSets = sets;
                questionnaires.Add(questionnaire);
            }
            return questionnaires;
        }

        public List<QuestionSet> ReadQuestionSetsFromDb(IEnumerable<Questionnaire> questionnaires)
        {
            var qSets = new List<QuestionSet>();
            foreach (var questionnaire in questionnaires)
            {
                foreach (var t in questionnaire.QuestionSets)
                {
                    var qSet = new QuestionSet(t);
                    qSet.LoadFromDatabase(_log);
                    qSets.Add(qSet);
                }
            }
            return qSets;
        }

        public void ExportAllQuestionnairesToXml()
        {
            var questionnaireNames = _log.GetQuestionnaireNames();
            var questionnaires = ReadQuestionnairesFromDb(questionnaireNames);
            var questionSets = ReadQuestionSetsFromDb(questionnaires);
            foreach (var questionnaire in questionnaires)
            {
                WriteQuestionnaireToXml(questionnaire,"Export/" + questionnaire.Name);
            }
            foreach (var questionSet in questionSets)
            {
                WriteQuestionSetToXml(questionSet,"Export/" + questionSet.Name);
            }
        }

        public void ImportAllQuestionnairesFromXml()
        {
            var questionnaireNames = _settings.QuestionnaireSettings.Questionnaires;
            WriteQuestionnairesToDb(questionnaireNames);
            //var questionnaires = questionnaireNames.Select(ReadQuestionnaireToXml).ToList();
        }

        private void WriteQuestionSetToDb(string questionSet)
        {
            bool qsCreated = _log.CreateQuestionSet(questionSet);
            if (qsCreated)
            {
                var qs = ReadQuestionSetFromXml(questionSet);
                qs.WriteToDatabase(_log);
            }
        }

        public QuestionSet ReadQuestionSetFromXml(string name)
        {
            TextAsset ta = Resources.Load<TextAsset>("QuestionSets/" + name);
            var xmlSerializer = new XmlSerializer(typeof(QuestionSet));
            Assets.EVE.Scripts.Questionnaire2.QuestionSet questionSet;
            using (var stream = new StringReader(ta.text))
            {
                questionSet = (QuestionSet)xmlSerializer.Deserialize(stream);
            }
            return questionSet;
        }

        public void WriteQuestionSetToXml(QuestionSet questionSet, string fileName)
        {
            string folderPath = Application.dataPath + "/Experiment/Resources/QuestionSets/" + fileName;
            var xmlSerializer = new XmlSerializer(typeof(QuestionSet));
            using (var stream = new FileStream(folderPath, FileMode.Create))
            {
                using (var sw = new StreamWriter(stream, Encoding.UTF8))
                {
                    xmlSerializer.Serialize(sw, questionSet);
                }
            }
            Debug.Log("Wrote settings to " + folderPath);
        }

        

        public void WriteQuestionnaireToXml(Questionnaire questionSet, string fileName)
        {
            string folderPath = Application.dataPath + "/Experiment/Resources/Questionnaires/" + fileName + ".xml";
            var xmlSerializer = new XmlSerializer(typeof(Questionnaire));
            using (var stream = new FileStream(folderPath, FileMode.Create))
            {
                using (var sw = new StreamWriter(stream, Encoding.UTF8))
                {
                    xmlSerializer.Serialize(sw, questionSet);
                }
            }
            Debug.Log("Wrote settings to " + folderPath);
        }

        public Questionnaire ReadQuestionnaireToXml(string fileName)
        {
            TextAsset ta = Resources.Load<TextAsset>("Questionnaires/" + fileName);
            var xmlSerializer = new XmlSerializer(typeof(Questionnaire));
            Questionnaire questionSet;
            using (var stream = new StringReader(ta.text))
            {
                questionSet = (Questionnaire)xmlSerializer.Deserialize(stream);
            }
            return questionSet;
        }
    }
}
