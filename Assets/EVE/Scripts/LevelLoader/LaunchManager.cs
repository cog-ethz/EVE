using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Assets.EVE.Scripts.XML;
using UnityEngine.SceneManagement;
using Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.Questionnaire;
using UnityEngine.UI;

public class LaunchManager : MonoBehaviour
{
    public ExperimentSettings ExperimentSettings;
    public GameObject FPC;

    //public Boolean debug = false;

    private LoggingManager _log;
    private MenuManager _menuManager;
    private string _subjectId = "";
    private int _sessionId, _replaySessionId;
   
    //private List<String> sceneList;
    private System.Timers.Timer _delayTimer;
    private int currentScene = 0;
    private string nextScene, activeSceneName;
    private bool loadScene = false, labchartReady, initialized;

    private string filePathRoot = "";// participant files

    private Dictionary<string,string> _sessionParameters;


    public Dictionary<string, string> SessionParameters
    {
        get
        {
            return _sessionParameters;
        }
    }

    private string[] SCENESEXTENSIONS;
    private string[] SCENESQUESTIONNAIREREALNAMES;
    private string QuestionnaireName;

    // Make objects of this class a singleton
    private static LaunchManager _instance;

    void Awake()
    {
        if (_instance)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(FPC);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        ReadExperimentSettings();

        //THIS SECTION CAN TEST WRITING EXPERIMENT SETTINGS
        //ExperimentSettings.UISettings.ReferenceResolution = new Vector2(1920,1080);
        //var path = UnityEditor.EditorUtility.SaveFilePanel("Save Experiment Settings", "", "experiment_settings", "xml");
        //WriteExperimentSettings(path);


        filePathRoot = Application.persistentDataPath + "/participant_files/"; ;
        DirectoryInfo dirInf = new DirectoryInfo(filePathRoot);
        if (!dirInf.Exists)
        {
            Debug.Log("Creating subdirectory for participant data"); dirInf.Create();
            Debug.Log("Data stored at " + filePathRoot);
            dirInf.Create();
        }
        _log = new LoggingManager();
        _log.ConnectToServer(ExperimentSettings.DatabaseSettings);
        initialized = false;
        _menuManager = GameObject.Find("Canvas").GetComponent<MenuManager>();
        _sessionParameters = new Dictionary<string, string>();
        LoadSettingsIntoDB();

        var _canvas = GameObject.Find("Canvas");

        _canvas.GetComponent<CanvasScaler>().referenceResolution = ExperimentSettings.UISettings.ReferenceResolution;

        var qs = new QuestionSet("TestSet");
        /*qs.Questions.Add(new InfoScreen("example_info", "People questionnaire:Perception of people in the neighborhood"));
        qs.Questions.Add(new Assets.EVE.Scripts.Questionnaire2.Questions.TextQuestion("exercise", "What type of exercise have you done recently?"));
        qs.Questions.Add(new Assets.EVE.Scripts.Questionnaire2.Questions.TextQuestion("born_in", "Where were you born?", new List<Label>() { new Label("Country:"), new Label("City:") }));
        qs.Questions.Add(new ChoiceQuestion("yes_no", "Is this a Yes/No Question?", Choice.Single,new List<Label>() { new Label("Yes",1), new Label("No",0) },null));
        qs.Questions.Add(new ChoiceQuestion("multiple_yes_no", "Answer quickly without thinking:", Choice.Single, new List<Label>() { new Label("Do you like questionnaires?"), new Label("Have you ever been to New York?"), new Label("Coffee with Milk?"), new Label("Coffee with Sugar?"), new Label("Black Coffee?") }, new List<Label>() { new Label("Yes", 1), new Label("No", 0) }));
        qs.Questions.Add(new ChoiceQuestion("coffe_consumption", "Did you have coffee, espresso, or another beverage containing caffeine in the past 24 hours?", Choice.Single, new List<Label>() { new Label("If yes, how many hours ago", 1,true), new Label("No, I did not have any caffeine", 0) },null));
        qs.Questions.Add(new ChoiceQuestion("band_judgement", "Please check with which assessment you agree most about the bands:", Choice.Multiple, new List<Label>() { new Label("Coldplay"), new Label("Maroon5"), new Label("Nsync"), new Label("Red Hot Chili Peppers")}, new List<Label>() { new Label("Mainstream", 0), new Label("Best of their Genre", 1), new Label("Original",2)}));
        qs.Questions.Add(new ChoiceQuestion("likert_scale", "Is this a good question?", Choice.Single, null,new List<Label>() { new Label("Not at all", 0), new Label("", 1), new Label("", 2), new Label("It is okay", 3), new Label("", 4), new Label("", 5), new Label("Very Good", 6) }));
        qs.Questions.Add(new ChoiceQuestion("multi_c_text", "Which animals do you like:", Choice.Multiple, new List<Label>() { new Label("Bears", 0), new Label("Lions", 1), new Label("Frogs", 2), new Label("Axolotls", 3), new Label("Humans", 4), new Label("Bats", 5), new Label("None", 6), new Label("Other:", 7,true) }, null));
        qs.Questions.Add(new ChoiceQuestion("image_example", "Which image looks the best?", Choice.Single, new List<Label>() { new Label("This", 0, "Images/test_image"), new Label("This", 1, "Images/test_image"), new Label("This", 2, "Images/test_image") }, null));
        qs.Questions.Add(new ScaleQuestion("amuesment_scale", "How much did you feel AMUSEMENT?", Scale.Line, "Did not experience at all", "Strongest experience ever felt"));
        qs.Questions.Add(new ScaleQuestion("SAM_pleasure", "Please rate how happy-unhappy you actually felt", Scale.Pleasure, "SAD", "CHEERFUL"));
        qs.Questions.Add(new ScaleQuestion("SAM_arousal", "Please rate how excited - calm you actually felt", Scale.Arousal, "QUIET", "ACTIVE"));
        qs.Questions.Add(new ScaleQuestion("SAM_dominance", "Please rate how controlled vs. in-control you actually felt", Scale.Dominance, "DEPENDENT", "INDEPENDENT"));
        qs.Questions.Add(new Assets.EVE.Scripts.Questionnaire2.Questions.LadderQuestion("swiss_status", "Swiss comparison of social status", "At the TOP of the ladder are the people who are the best off. The lower you are, the closer you are to the people at the very bottom. Where would you place yourself on this ladder, compared to all the other people in Switzerland?"));
        qs.Questions.Add(new ChoiceQuestion("flu_med", "Have you taken any medication for cold or flu symptoms today?", Choice.Single, new List<Label>() { new Label("Yes", 1), new Label("No", 0) }, null, new List<Jump>() { new Jump("inflam_med", "FT") }));
        qs.Questions.Add(new Assets.EVE.Scripts.Questionnaire2.Questions.TextQuestion("flu_med_name", "Which medication(s)?"));
        qs.Questions.Add(new ChoiceQuestion("inflam_med", "Have you taken any anti-inflammatory medication today?", Choice.Single, new List<Label>() { new Label("Yes", 1), new Label("No", 0) }, null, new List<Jump>() { new Jump("*", "FT") }));
        qs.Questions.Add(new Assets.EVE.Scripts.Questionnaire2.Questions.TextQuestion("inflam_med_name", "Which medication(s)?"));
        */

        var qs_sam = new QuestionSet("SAM_German");
        /*qs_sam.Questions.Add(new InfoScreen("SAM_info_pleasure_DE", "&lt;b&gt;&lt;size=48&gt;SAM Fragen&lt;/size&gt;&lt;/b&gt;\\n\\nDie erste SAM Skala steht für glücklich-unglücklich und geht von Lächeln bis Stirnrunzeln.\\n\\nAn einem Extrem der glücklich bis unglücklich Skala könnten Sie sich glücklich, erfreut, zufrieden, wohl oder hoffnungsvoll fühlen. Falls Sie sich im Moment rundum wohl fühlen, könnten Sie die Figur ganz rechts auswählen. Am anderen Ende der Skala beschreibt es Sie, falls Sie sich gänzlich unglücklich, genervt, unzufrieden, melancholisch, verzweifelt oder gelangweilt fühlen. Falls Sie sich gänzlich unglücklich fühlen, könnten Sie die Figur ganz links auswählen. Falls Sie sich komplett neutral - weder glücklich noch unglücklich - fühlen, könnten Sie die Figur in der Mitte auswählen. Zudem könnten Sie die Zwischenschritte zwischen zwei Figuren auswählen."));
        qs_sam.Questions.Add(new ScaleQuestion("SAM_pleasure_DE", "Bitte beurteilen Sie, wie glücklich-unglücklich Sie sich im Moment fühlen.", Scale.Pleasure, "TRAURIG", "FRÖHLICH"));
        qs_sam.Questions.Add(new InfoScreen("SAM_info_arousal_DE", "&lt;b&gt;&lt;size=48&gt;SAM Fragen&lt;/size&gt;&lt;/b&gt;\\n\\nDie nächste SAM Skala steht für aufgeregt-ruhig.\\n\\nAn einem Extrem der Skala könnten Sie sich stimuliert, aufgeregt, rasend, nervös, hellwach oder aufgeweckt fühlen. Falls Sie sich im Moment sehr aufgeregt fühlen, könnten Sie die Figur ganz rechts auswählen. Am anderen Ende der Skala fühlen Sie sich komplett entspannt, ruhig, träge, matt, müde oder unerregt. Sie könnten angeben, dass Sie sich komplett ruhig fühlen, indem Sie die Figur ganz links auswählen. Falls Sie weder sehr aufgeregt, noch sehr ruhig sind, könnten Sie die Figur in der Mitte auswählen. Zudem könnten Sie die Zwischenschritte zwischen zwei Figuren auswählen."));
        qs_sam.Questions.Add(new ScaleQuestion("SAM_arousal_DE", "Bitte beurteilen Sie, wie aufgeregt-ruhig Sie sich im Moment fühlen.", Scale.Arousal, "STILL", "AKTIV"));
        qs_sam.Questions.Add(new InfoScreen("SAM_info_dominance_DE", "&lt;b&gt;&lt;size=48&gt;SAM Fragen&lt;/size&gt;&lt;/b&gt;\\n\\nDie letzte SAM Skala steht für \"in Kontrolle\"-\"von Aussen kontrolliert\".\\n\\nAn einem Extrem der Skala könnten Sie sich unwichtig, ausser Kontrolle oder eingeschüchtert fühlen, aber auch, als bestimme jemand anderes oder als ob Sie die Situation nicht handhaben könnten. Falls Sie sich im Moment so fühlen, dass Sie diese Situation nicht handhaben könnten, könnten Sie die Figur ganz links auswählen. Am anderen Ende der Skala fühlen Sie sich in Kontrolle, wichtig, als ob Sie von niemandem Hilfe bräuchten und die Situation handhaben könnten. Sie könnten angeben, dass Sie sich komplett in Kontrolle und in der Lage fühlen die Situation handzuhaben, indem Sie die Figur ganz rechts auswählen. Falls Sie weder sehr in Kontrolle, noch sehr von Aussen kontrolliert sind, könnten Sie die Figur in der Mitte auswählen. Zudem könnten Sie die Zwischenschritte zwischen zwei Figuren auswählen."));
        qs_sam.Questions.Add(new ScaleQuestion("SAM_dominance_DE", "Bitte beurteilen Sie, wie sehr \"in Kontrolle\"-\"von Aussen kontrolliert\" Sie sich im Moment fühlen.", Scale.Dominance, "ABHÄNGIG", "UNABHÄNGIG"));
        */

        var qs_his = new QuestionSet("baseline_story");
        /*qs_his.Questions.Add(new InfoScreen("story_info", "<b><size=48>Geschichte</size></b> \\n \\nIm Folgenden werden Sie eine Geschichte lesen, die zur Kalibrierung der Sensoren dient. Sie müssen nichts weiter tun, als die Geschichte aufmerksam und langsam zu lesen. Sobald die Geschichte vorbei ist, werden Sie mit Fragebögen weitermachen."));
        qs_his.Questions.Add(new InfoScreen("story_A", "Eines Tages saß vor einem ärmlichen Hause ein alter Mann mit seiner Frau, und wollten von der Arbeit ein wenig ausruhen. Da kam auf einmal ein prächtiger, mit vier Rappen bespannter Wagen herbeigefahren, aus dem ein reichgekleideter Herr stieg. Der Bauer stand auf, trat zu dem Herrn und fragte, was sein Verlangen wäre, und worin er ihm dienen könnte. Der Fremde reichte dem Alten die Hand und sagte: \"Ich wünsche nichts als einmal ein ländliches Gericht zu genießen. Bereitet mir Kartoffel, wie Ihr sie zu essen pflegt, damit will ich mich zu Euerm Tisch setzen, und sie mit Freude verzehren.\" Der Bauer lächelte und sagte: \"Ihr seid ein Graf oder Fürst, oder gar ein Herzog, vornehme Herren haben manchmal solch ein Gelüsten; Euer Wunsch soll aber erfüllt werden.\" Die Frau ging in die Küche, und sie fing an Kartoffeln zu waschen und zu reiben und wollte Klöße daraus bereiten, wie sie die Bauern essen."));
        qs_his.Questions.Add(new InfoScreen("story_B", "Während sie bei der Arbeit stand, sagte der Bauer zu dem Fremden: \"Kommt einstweilen mit mir in meinen Hausgarten, wo ich noch etwas zu schaffen habe.\" In dem Garten hatte er Löcher gegraben und wollte jetzt Bäume einsetzen. \"Habt Ihr keine Kinder,\" fragte der Fremde, \"die Euch bei der Arbeit behilflich sein könnten?\" - \"Nein,\" antwortete der Bauer; \"ich habe freilich einen Sohn gehabt,\" setzte er hinzu, \"aber der ist schon seit langer Zeit in die weite Welt gegangen. Es war ein ungeratener Junge, klug und verschlagen, aber er wollte nichts lernen und machte lauter böse Streiche; zuletzt lief er mir fort, und seitdem habe ich nichts von ihm gehört.\" Der Alte nahm ein Bäumchen, setzte es in ein Loch und stieß einen Pfahl daneben: und als er Erde hineingeschaufelt und sie festgestampft hatte, band er den Stamm unten, oben und in der Mitte mit einem Strohseil fest an den Pfahl."));
        qs_his.Questions.Add(new InfoScreen("story_C", "\"Aber sagt mir,\" sprach der Herr, \"warum bindet Ihr den krummen knorrichten Baum, der dort in der Ecke fast bis auf den Boden gebückt liegt, nicht auch an einen Pfahl wie diesen, damit er strack wächst?\" Der Alte lächelte und sagte \"Herr, Ihr redet, wie Ihrs versteht: man sieht wohl, daß Ihr Euch mit der Gärtnerei nicht abgegeben habt. Der Baum dort ist alt und verknorzt, den kann niemand mehr gerad machen: Bäume muß man ziehen, solange sie jung sind.\" - \"Es ist wie bei Euerm Sohn,\" sagte der Fremde, \"hättet Ihr den gezogen, wie er noch jung war, so wäre er nicht fortgelaufen; jetzt wird er auch hart und knorzig geworden sein.\" - \"Freilich,\" antwortete der Alte, \"es ist schon lange, seit er fortgegangen ist; er wird sich verändert haben.\" - \"Würdet Ihr ihn noch erkennen, wenn er vor Euch träte?\" fragte der Fremde. \"Am Gesicht schwerlich,\" antwortete der Bauer, \"aber er hat ein Zeichen an sich, ein Muttermal auf der Schulter, das wie eine Bohne aussieht.\""));
        qs_his.Questions.Add(new InfoScreen("story_D", "Als er dies gesagt hatte, zog der Fremde den Rock aus, entblößte seine Schulter und zeigte dem Bauer die Bohne. \"Herr Gott,\" rief der Alte, \"du bist wahrhaftig mein Sohn,\" und die Liebe zu seinem Kind regte sich in seinem Herzen. \"Aber,\" setzte er hinzu, \"wie kannst du mein Sohn sein, du bist ein großer Herr geworden und lebst in Reichtum und Überfluß! Auf welchem Weg bist du dazu gelangt?\" - \"Ach, Vater,\" erwiderte der Sohn, \"der junge Baum war an keinen Pfahl gebunden und ist krumm gewachsen: jetzt ist er zu alt; er wird nicht wieder gerad. Wie ich das alles erworben habe? Ich bin ein Dieb geworden. Aber erschreckt Euch nicht, ich bin ein Meisterdieb. Für mich gibt es weder Schloß noch Riegel: wonach mich gelüstet, das ist mein. Glaubt nicht, daß ich stehle wie ein gemeiner Dieb, ich nehme nur vom Überfluß der Reichen. Arme Leute sind sicher: ich gebe ihnen lieber, als daß ich ihnen etwas nehme."));
        qs_his.Questions.Add(new InfoScreen("story_E", "So auch, was ich ohne Mühe, List und Gewandtheit haben kann, das rühre ich nicht an.\" - \"Ach, mein Sohn,\" sagte der Vater, \"es gefällt mir doch nicht, ein Dieb bleibt ein Dieb; ich sage dir, es nimmt kein gutes Ende.\" Er führte ihn zu der Mutter, und als sie hörte, daß es ihr Sohn war, weinte sie vor Freude, als er ihr aber sagte, daß er ein Meisterdieb geworden wäre, so flossen ihr zwei Ströme über das Gesicht. Endlich sagte sie: \"Wenn er auch ein Dieb geworden ist, so ist er doch mein Sohn, und meine Augen haben ihn noch einmal gesehen."));
        */

        var qs_sti = new QuestionSet("stimuli");

        qs_sti.Questions.Add(new VisualStimuli("test_stimuli",
            "A",Separator.FixationCross, Choice.ArrowKeys, Randomisation.ExperimentParameter,
            Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli.Type.Video, "test_stimuli_order", false,20,10,30,
            new List<string>
            {
                "Videos/h_1", "Videos/h_2", "Videos/h_3", "Videos/h_4", "Videos/h_5", "Videos/h_6", "Videos/h_7",
                "Videos/l_1", "Videos/l_2", "Videos/l_3", "Videos/l_4", "Videos/l_5", "Videos/l_6", "Videos/l_7"
            }));

        var qn = new Questionnaire("energyscape_pre");


        qn.QuestionSets.Add(qs_sti.Name);
        //qn.QuestionSets.Add(qs_his.Name);

        var qf = new QuestionnaireFactory(_log,ExperimentSettings);
        //qf.WriteQuestionSetToXml(qs_sti, "TestSet.xml");
        //qf.WriteQuestionnaireToXml(qn,"energyscape_pre");
    }


    // -----------------------------------------
    //			 During one Playthrough
    //------------------------------------------
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string previousSceneName = activeSceneName;
        activeSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Scene N " + currentScene);
        if (!initialized)
        {
            initialized = true;
            return;
        }
        var sceneList = ExperimentSettings.SceneSettings.Scenes;
        if (activeSceneName == "Loader" && previousSceneName != "Evaluation" && sceneList != null)
        { //coming back from a scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
            _log.LogSceneEnd(sceneList[currentScene]);
            this.enabled = true;

            if (currentScene < sceneList.Count - 1)
            {
                currentScene++;
                loadCurrentScene();
            }
            else
            {
                _menuManager = GameObject.Find("Canvas").GetComponent<MenuManager>();
                _menuManager.ShowMenu(GameObject.Find("Finished").GetComponent<Menu>());
            }
        }
        else if (activeSceneName == "Loader" && previousSceneName == "Evaluation")
        {
            _menuManager.ShowMenu(GameObject.Find("Evaluation Menu").GetComponent<Menu>());
        }
        else if (activeSceneName == "Evaluation" && previousSceneName != "Loader")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
            this.enabled = true;
            _menuManager.setDetailsInt(_replaySessionId);
            _menuManager.ShowMenu(GameObject.Find("Evaluation Details").GetComponent<Menu>());
        }
        else if (activeSceneName == "Evaluation")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
        }
        else if (activeSceneName == "Loader")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
        }
        else
        {          
            if (!FPC.GetComponentInChildren<ReplayRoute>().isActivated())
                _log.LogSceneStart(sceneList[currentScene]);
            this.enabled = false;   
        }
    }


    // -----------------------------------------
    //			 Initialization
    //------------------------------------------	
    void Start()
    {         
        _sessionId = _log.GetCurrentSessionID();
        if (_sessionId < 0)
        {
            var databaseSetupMenu = GameObject.Find("SetupDatabaseMenu").GetComponent<Menu>();
            _menuManager.DisplayErrorMessage("Unable to connect to the database! Press ok to check the database status", databaseSetupMenu);
        }
        else
        {
            LoadSettingsIntoDB();
        }
    }


    public void setCompletedAndReset() {
        currentScene = 0;
        _log.updateParameters();
        _sessionId = _log.GetCurrentSessionID();
    }

    public void SetMenuManager(MenuManager menuManager)
    {
        _menuManager = menuManager;
    }
    
    // -----------------------------------------
    //			 Scene Loading
    //------------------------------------------	
    void Update()
    {
        //this is done in Update s.t. the loading screen has time to display
        if (loadScene)
        {
            loadScene = false;
            
            SceneManager.LoadScene(nextScene);
        }
        else if (_menuManager != null)
        {
            if (_menuManager.CheckBackEvalScene())
            {
                SceneManager.LoadScene("Evaluation");
            }

            if (_menuManager.CheckBackLoaderScene())
            {
                SceneManager.LoadScene("Loader");
            }
        }


    }

    public void loadCurrentScene()
    {
        SynchroniseSceneListWithDB();
        var sceneList = ExperimentSettings.SceneSettings.Scenes;
        LoadScene(sceneList[currentScene].Contains(".xml") ? "Questionnaire" : sceneList[currentScene]);
    }
    private void LoadScene(string scene)
    {
        var scenes = ExperimentSettings.SceneSettings.Scenes;
        if (scenes[currentScene].Contains(".xml"))
        {
            var splittedFileName = scenes[currentScene].Split('.');
            QuestionnaireName = splittedFileName[0];

            _log.CreateUserAnswer(_log.GetCurrentSessionID(), splittedFileName[0]);
            _log.setQuestionnaireName(splittedFileName[0]);
        }        
        nextScene = scene;
        loadScene = true;
    }
  
   public string getSubjectID()
    {
        return _subjectId;
    }

	//error checking before running an experiment
	//the part with the logsessionparameters is from the old implementation and is not suited for everchanging sensors
    public void startExperiment() {
        
        _subjectId = _menuManager.subjectId;

        if (_subjectId.Length < 1)
        {
            Menu originMenu = GameObject.Find("Experiment Menu").GetComponent<Menu>();
            _menuManager.DisplayErrorMessage("The Subject ID is invalid!", originMenu);
        }
        else
        {
            SynchroniseSceneListWithDB();

            var sceneList = ExperimentSettings.SceneSettings.Scenes;
            Console.Write(sceneList.Count);
            int nParameters = _menuManager.GetAttributesList().Count;
            if (sceneList.Count <= 0)
            {
                Menu originMenu = GameObject.Find("Scene Config").GetComponent<Menu>();
                _menuManager.DisplayErrorMessage("No scenes selected!", originMenu);
            }
            else
            {
                if (_sessionParameters.Count != nParameters)
                {

                    Menu originMenu = GameObject.Find("Attribute Form").GetComponent<Menu>();
                    _menuManager.DisplayErrorMessage("Session parameters are not set.",originMenu);
                }
                else
                {
                    _log.LogSession(ExperimentSettings.Name, _subjectId);
                    if (_sessionParameters.ContainsKey("Labchart File Name"))
                        _log.SetLabChartFileName(_sessionParameters["Labchart File Name"]);

                    if (_log.getSensors().Contains("HL7Server"))
                        this.gameObject.GetComponent<HL7ServerStarter>().enabled = true;
                    storeSessionParameters();
                    Menu startMenu = GameObject.Find("Start Menu").GetComponent<Menu>();
                    _menuManager.ShowMenu(startMenu);
                }
            }
        }

    }

    private void storeSessionParameters()
    {
        foreach (KeyValuePair<string, string> entry in _sessionParameters)
        {
            _log.LogSessionParameter(entry.Key, entry.Value);
        }
    }

    public int getCurrentSessionID() {
        return _sessionId;
    }

    public void setSessionId(int id)
    {
        _sessionId = id;
    }
    


    public string GetQuestionnaireName() {
        return QuestionnaireName;
    }


    public void SetExperimentName(string name)
    {
        ExperimentSettings.Name = name;
    }

    public string GetExperimentName()
    {
        return ExperimentSettings.Name;
    }

    public LoggingManager GetLoggingManager()
    {
        return _log;
    }

    public MenuManager GetMenuManager()
    {
        return _menuManager;
    }

    public Dictionary<string,string> getSessionParameters()
    {
        return _sessionParameters;
    }

    public void changeSessionsParameter(string name, string value)
    {
        if (_sessionParameters.ContainsKey(name))
            _sessionParameters.Remove(name);
        _sessionParameters.Add(name, value);
    }

    public int getReplaySessionId()
    {
        return _replaySessionId;
    }

    public void setReplaySessionId(int id)
    {
        _replaySessionId = id;
    }

    /// <summary>
    /// Writes the Experiment Settings into an XML at the
    /// specified location.
    /// </summary>
    /// <remarks>
    /// This will overwrite previous settings.
    /// </remarks>
    /// <param name="folderPath"></param>
    public void WriteExperimentSettings(string folderPath)
    {
        if (!folderPath.EndsWith(".xml"))
            if (!folderPath.EndsWith("/")) folderPath = folderPath+ "/experiment_settings.xml";
        var xmlSerializer = new XmlSerializer(typeof(ExperimentSettings));
        using (var stream = new FileStream(folderPath, FileMode.Create))
        {
            xmlSerializer.Serialize(stream, ExperimentSettings);
        }
        Debug.Log("Wrote settings to " + folderPath);
    }

    /// <summary>
    /// Loads the experiment settings from the Resources folder.
    /// </summary>
    public void ReadExperimentSettings()
    {
        TextAsset ta = Resources.Load<TextAsset>("experiment_settings");
        var xmlSerializer = new XmlSerializer(typeof(ExperimentSettings));
        using (var stream = new StringReader(ta.text))
        {
            ExperimentSettings = (ExperimentSettings)xmlSerializer.Deserialize(stream);
        }
    }

    

    public void LoadSettingsIntoDB()
    {
        var name = ExperimentSettings.Name;
        _log.LogExperiment(name);
        _log.RemoveExperimentSceneOrder(name);
        foreach (var scene in ExperimentSettings.SceneSettings.Scenes)
        {
            _log.AddScene(scene);
        }
        _log.SetExperimentSceneOrder(name, ExperimentSettings.SceneSettings.Scenes.ToArray());
        UpdateParameters();
        foreach (var sensor in ExperimentSettings.SensorSettings.Sensors)
        {
            _log.AddSensor(sensor);
        }
    }

    public void SynchroniseSceneListWithDB()
    {
        if (_log.GetCurrentSessionID() > -1)
            ExperimentSettings.SceneSettings.Scenes = new List<string>(_log.getSceneNamesInOrder(ExperimentSettings.Name));
    }

    public void SynchroniseSensorListWithDB()
    {
        if (_log.GetCurrentSessionID() > -1)
        {
            var sensors = new List<string>(_log.getSensors());
            if (sensors.Contains("Labchart"))
            {
                ExperimentSettings.SensorSettings.Labchart = true;
                sensors.Remove("Labchart");
            }
            if (sensors.Contains("HL7Server"))
            {
                ExperimentSettings.SensorSettings.H7Server = true;
                sensors.Remove("HL7Server");
            }
            ExperimentSettings.SensorSettings.Sensors = sensors;
        }
    }


    /// <summary>
    /// This method allows to establish a correspondence between the parameters
    /// described in the database and in the experiment settings. 
    /// 
    /// This should be called whenever manually experiment parameters have been
    /// added within the EVE UI or within the Experiment Settings XML file.
    /// </summary>
    private void UpdateParameters()
    {
        var existingParams = _log.GetExperimentParameters(ExperimentSettings.Name);
        var requiredParams = ExperimentSettings.ParameterSettings.Parameters;
        for (var i = 0; i < requiredParams.Count; i++)
        {
            if (!existingParams.Contains(requiredParams[i]))
            {
                _log.CreateExperimentParameter(ExperimentSettings.Name, requiredParams[i]);
            }
        }
        for (var i = 0; i < existingParams.Count; i++)
        {
            if (!requiredParams.Contains(existingParams[i]))
            {
                requiredParams.Add(existingParams[i]);
            }
        }
        _menuManager.SetActiveParameters(requiredParams);
    }
}

