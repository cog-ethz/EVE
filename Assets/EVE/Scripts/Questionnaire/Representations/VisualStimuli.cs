using System.Collections;
using System.Collections.Generic;
using Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Assets.EVE.Scripts.Questionnaire.Representations
{
    public class VisualStimuli : Representation
    {
        public Questions.VisualStimuli Question;
        
        public GameObject FixationSceen, DecisionScreen, ExpositionScreen;


        private bool _fixation, _decide, _inDecision, _decided, _firstStimulus, _secondStimulus;

        private int[] _randomisationOrder;
        private int _currentIndex;


        private Dictionary<string, string> _experimentParameters;
        private LaunchManager _launchManager;
        private LoggingManager _log;


        private VideoClip[] _vidLocs;
        private Sprite[] _imgLocs;
        private RawImage _rawImage;
        private VideoPlayer _videoPlayer;
        private QuestionnaireSystem _qSystem;
        private int _currentDecision;

        // Use this for initialization
        void Awake ()
        {
            _launchManager = GameObject
                .FindGameObjectWithTag("LaunchManager")
                .GetComponent<LaunchManager>();
            _log = _launchManager.GetLoggingManager();
            _experimentParameters = _launchManager.SessionParameters;
        }

        void Update()
        {
            if (_inDecision)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    Debug.Log("1 key was pressed.");
                    Question.RetainAnswer(_currentIndex,"1");
                    _log.insertLiveMeasurement("User_Choice", "User Choice", null, "1");
                    _log.insertLiveMeasurement("LabChart", "Event", null, "User Choice: 1");
                    _decided = true;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    Debug.Log("2 key was pressed.");
                    Question.RetainAnswer(_currentIndex,"2");
                    _log.insertLiveMeasurement("User_Choice", "User Choice", null, "2");
                    _log.insertLiveMeasurement("LabChart", "Event", null, "User Choice: 2");
                    _decided = true;
                }

                if (_decided)
                {
                    _decided = false;
                    _decide = false;
                    if (_currentIndex == _randomisationOrder.Length)
                    {
                        Question.SetIsAnswered(true);
                        _qSystem.GoToNextQuestion();
                    }
                    if (!Question.IsAnswered())
                    {
                        _decide = false;
                        StartCoroutine(Question.Configuration.SeparatorFirst
                            ? SwitchToFixation(Question.Times.Fixation)
                            : SwitchToNext(Question.Times.Exposition));
                    }
                    _inDecision = false;
                }
            }
        }

        public override void InitialiseRepresentation(QuestionnaireSystem qSystem)
        {
            _qSystem = qSystem;
            _rawImage = ExpositionScreen.GetComponentInChildren<RawImage>();
            _videoPlayer = ExpositionScreen.GetComponentInChildren<VideoPlayer>();

            _currentIndex = 0;
            _currentDecision = 0;
            _randomisationOrder = Question.RandomisationOrder(_experimentParameters).ToArray();

            if (Question.Configuration.Type == Type.Image)
            {
                _imgLocs = new Sprite[Question.Stimuli.Count];
                for (var i = 0; i< Question.Stimuli.Count; i++)
                {
                    _imgLocs[i] = Resources.Load<Sprite>(Question.Stimuli[i]);
                }
            }
            else
            {
                _vidLocs = new VideoClip[Question.Stimuli.Count];
                for (var i = 0; i < Question.Stimuli.Count; i++)
                {
                    _vidLocs[i] = Resources.Load<VideoClip>(Question.Stimuli[i]);
                }
            }

            if (Question.Configuration.Type == Type.Image)
            {
                _rawImage.texture = _imgLocs[_randomisationOrder[_currentIndex]].texture;
            }
            else
            {
                _videoPlayer.clip = _vidLocs[_randomisationOrder[_currentIndex]];
            }

            StartCoroutine(Question.Configuration.SeparatorFirst
                ? SwitchToFixation(Question.Times.Fixation)
                : SwitchToNext(Question.Times.Exposition));
        }


        private IEnumerator SwitchToFixation(float time)
        {
            _fixation = true;
            UpdateVisibility();

			if (Question.Configuration.SeparatorFirst && !_firstStimulus)
			{
				_firstStimulus = true;
			}
			else
			{
				_secondStimulus = true;
				_firstStimulus = false;

				_currentIndex++;

				if (Question.Configuration.Type == Type.Image)
				{
					_rawImage.texture = _imgLocs[_randomisationOrder[_currentIndex]].texture;
				}
				else
				{
					_videoPlayer.clip = _vidLocs[_randomisationOrder[_currentIndex]];
				}
			}

            var index = Question.Configuration.SeparatorFirst ? _currentIndex : _currentDecision;
            _log.insertLiveMeasurement("Fixation"
                , "Event"
                , null
                , "Start " + index);

            Debug.Log("Remain in Fixation for " + time + " sec");
            _log.insertLiveMeasurement("LabChart", "Event", null, "Fixation: " + index);
            yield return new WaitForSeconds(time);
            
            _fixation = false;
            StartCoroutine(SwitchToNext(Question.Times.Exposition));
        }

        private IEnumerator SwitchToNext(float time)
        {
            UpdateVisibility();

			_log.insertLiveMeasurement("Video", "Event", null, "Start " + Question.Stimuli[_randomisationOrder[_currentIndex]]);
			_log.insertLiveMeasurement("LabChart", "Event", null, "Video: " + Question.Stimuli[_randomisationOrder[_currentIndex]]);
            Debug.Log("Remain in next scene for " + time + " sec");
            yield return new WaitForSeconds(time);

            if (Question.Configuration.Choice == Choice.None
                && Question.Configuration.Separator == Separator.None
                && Question.Configuration.Randomisation == Randomisation.None)
            {
                Question.SetIsAnswered(true);
                _qSystem.GoToNextQuestion();
            }
            else
            {
                StartCoroutine(_secondStimulus
                    ? SwitchToDecision(Question.Times.Decision)
                    : SwitchToFixation(Question.Times.Fixation));
            }
        }

        private IEnumerator SwitchToDecision(float time)
        {
            _secondStimulus = false;
            _log.insertLiveMeasurement("Decision", "Event", null, "Start " + _currentDecision);
            _log.insertLiveMeasurement("LabChart", "Event", null, "Decision: " + _currentDecision);
            _decide = true;
            UpdateVisibility();


            
            _currentIndex++;
            if (_currentIndex < _randomisationOrder.Length)
            {
                if (Question.Configuration.Type == Type.Image)
                {
                    _rawImage.texture = _imgLocs[_randomisationOrder[_currentIndex]].texture;
                }
                else
                {
                    _videoPlayer.clip = _vidLocs[_randomisationOrder[_currentIndex]];
                }
            }
            _currentDecision++;
            Debug.Log("Remain in Decision for " + time + " sec");
            yield return new WaitForSeconds(time);

            //This is buggy because it should only be executed when no decision was made at all
            //currently it triggers after the wait time is over without knowing how many new stimuli have been shown inbetween
            /*if (!_decided)
            {
                _decide = false;
                Question.RetainAnswer(_currentIndex, "-1");
                if (_currentIndex == _randomisationOrder.Length)
                {
                    Question.SetIsAnswered(true);
                    _qSystem.GoToNextQuestion();
                }
                if (!Question.IsAnswered())
                {
                    StartCoroutine(Question.Configuration.SeparatorFirst
                        ? SwitchToFixation(Question.Times.Fixation)
                        : SwitchToNext(Question.Times.Exposition));
                }
            }
            else
            {
                _decided = false;
            }*/
        }

        private void UpdateVisibility()
        {
            FixationSceen.SetActive(false);
            DecisionScreen.SetActive(false);
            ExpositionScreen.SetActive(false);
            
            if (_fixation)
            {
                FixationSceen.SetActive(true);
            }
            else if (_decide)
            {
                _inDecision = true;
                DecisionScreen.SetActive(true);
            }
            else
            {
                ExpositionScreen.SetActive(true);
            }
        }
    }
}
