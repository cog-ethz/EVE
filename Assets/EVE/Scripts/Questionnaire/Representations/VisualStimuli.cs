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


        private bool _fixation, _decide, _inDecision, _decided, _secondStimuli;

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
                    _decided = true;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    Debug.Log("2 key was pressed.");
                    Question.RetainAnswer(_currentIndex,"2");
                    _log.insertLiveMeasurement("User_Choice", "User Choice", null, "2");
                    _decided = true;
                }

                if (_decided)
                {
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

            StartCoroutine(Question.Configuration.SeparatorFirst
                ? SwitchToFixation(Question.Times.Fixation)
                : SwitchToNext(Question.Times.Exposition));
        }


        private IEnumerator SwitchToFixation(float time)
        {
            _fixation = true;
            UpdateVisibility();
            _log.insertLiveMeasurement("Fixation"
                , "Event"
                , null
                , "Start " + _currentDecision);

            Debug.Log("Remain in Fixation for " + time + " sec");
            yield return new WaitForSeconds(time);

            _secondStimuli = true;
            _currentIndex++;
            _fixation = false;
            StartCoroutine(SwitchToNext(Question.Times.Exposition));
        }

        private IEnumerator SwitchToNext(float time)
        {
            UpdateVisibility();
            if (Question.Configuration.Type == Type.Image)
            {
                _rawImage.texture = _imgLocs[_randomisationOrder[_currentIndex]].texture;
            }
            else
            {
                _videoPlayer.clip = _vidLocs[_randomisationOrder[_currentIndex]];
                _videoPlayer.Play();
            }

            _log.insertLiveMeasurement("Video", "Event", null, "Start " + Question.Stimuli[_currentIndex]);
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
                StartCoroutine(_secondStimuli
                    ? SwitchToDecision(Question.Times.Decision)
                    : SwitchToFixation(Question.Times.Fixation));
            }
        }

        private IEnumerator SwitchToDecision(float time)
        {
            _secondStimuli = false;
            _log.insertLiveMeasurement("Decision", "Event", null, "Start " + _currentDecision);
            _decide = true;
            UpdateVisibility();

            _currentIndex++;
            _currentDecision++;
            Debug.Log("Remain in Decision for " + time + " sec");
            yield return new WaitForSeconds(time);

            if (!_decided)
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
            }
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
