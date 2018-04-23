using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Obsolete("This class has not been updated to the new GUI system")]
public class PopupQuestionnaire : MonoBehaviour {

	//style
	public GUISkin 		skin;
    public GUISkin leftAllignedSkin;

	public Texture 		backgroundTexture;
	public float 		fadeSpeed;
	
	private float 		menuX;
	private float 		menuY;
	public  float 		textureWidth;
	public  float 		textureHeight;
	
	private bool  		fadingIn	= true;
	private bool  		fadingOut	= false;
	private float 		alpha		= 0;
	private float 		lerpTime	= 0;

	private int 		windowWidth	 = 800;
	private int 		windowHeight = 600;

	//answer vars
	//private QuestionnaireBuilder builder;
	//public  int 				 questionnaireID;
    private QuestionSet currentQuestionSet;

    private string questionSetName;
	private int 				 totalQuestions;
	private int 				 currentQuestion;
	private Stack<int>			 previousQuestion;

	//focus control
	private int 				 currentRowSelected = 1;
	private int 				 currentColumnSelected = 1;
	private bool 				 confirmButtonPressed = false;
	private bool 				 axisInUseLeftRight = false;
	private bool 				 axisInUseTopBottom = false;

	//others
	private bool pleaseAnswerHint 	= false;
	private bool finished 			= false;
	public  bool readyToUseParams   = false;

	public  Camera 		cam;
	public  GameObject 	player;
    //public FirstPersonController mouseLookComponent1;
    //public FirstPersonController mouseLookComponent2;
	//private	BlurEffect  blur;
	private LoggingManager log;
	//private TakeGlobalParameters parameters;

	
	void Start () {
		//Load Builder
        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        log = launchManager.GetLoggingManager();

		//builder = new QuestionnaireBuilder (log);

		//Load Questionnaire
		//questionCollection = builder.LoadQuestionnaire ( questionCollectionID );

        // Load question set
        /*questionSetName = GameObject.Find("Main Camera").GetComponent<TakeGlobalParameters>().questionSetName;
        log.setQuestionnaireName(GameObject.Find("Main Camera").GetComponent<TakeGlobalParameters>().questionnaireName);
        currentQuestionSet = GameObject.Find("Main Camera").GetComponent<TakeGlobalParameters>().questionnaire[questionSetName];*/

        //Load other
        totalQuestions = currentQuestionSet.getPageAmount();
        currentQuestion = 0;
        previousQuestion = new Stack<int>();

		//Game Objects
		//mouseLookComponent1 = player.GetComponent<FirstPersonController>();
		//mouseLookComponent2 = cam.GetComponent<FirstPersonController>();
		//blur = cam.GetComponent<BlurEffect> ();
		//blur.enabled = false;
	}

	void OnGUI() {
		if (IsInsideMenuArea () && !finished) {
			if (fadingIn)  {
				StartFadingIn ();	// controls the transparency of the drawn elements
			}
			DisplayQuestionnaire ();
			//if(log != null) log.setQuestionnaireID (questionnaireID);	// questionnaire ID will be available conly after the Manager set it
		} else {
			if(fadingOut)  {
				StartFadingOut();
				DisplayQuestionnaire ();
			}
		}
	}

	// -----------------------------------------
	//			 joystick control
	//------------------------------------------
	void Update(){
		if (IsInsideMenuArea ()) {

			//---------------------------
			// 		normal input
			//--------------------------

			//toggle up and down
			if (Input.GetAxisRaw ("MainTopBottom") != 0) {
				if (axisInUseTopBottom == false) {
					if (Input.GetAxisRaw ("MainTopBottom") == 1) {
						currentRowSelected++;
						//if(!parameters.replay)
                            log.LogInput("row++");
					}
					if (Input.GetAxisRaw ("MainTopBottom") == -1) {
						currentRowSelected--;
						//if(!parameters.replay)
                            log.LogInput("row--");
					}
					axisInUseTopBottom = true;
				}
			} else
				axisInUseTopBottom = false;

			//toggle left and right
			if (Input.GetAxisRaw ("MainLeftRight") != 0) {
				if (axisInUseLeftRight == false) {
					if (Input.GetAxisRaw ("MainLeftRight") == 1) {
						currentColumnSelected++;
						//if(!parameters.replay)
                            log.LogInput("col++");
					}
					if (Input.GetAxisRaw ("MainLeftRight") == -1) {
						currentColumnSelected--;
						//if(!parameters.replay)
                            log.LogInput("col++");
					}
					axisInUseLeftRight = true;
				}
			} else
				axisInUseLeftRight = false;

			//confirm, next, etc
			if (Input.GetButtonDown ("Back")) {
				confirmButtonPressed = true;
				//if(!parameters.replay)
                    log.LogInput("confirm");
			}

			//return
			if (Input.GetButtonDown ("Thumb")) {
				pleaseAnswerHint = false;

				if (currentQuestion > 0) {
					currentQuestion = previousQuestion.Pop ();
					currentRowSelected = 1;
					currentColumnSelected = 1;
				}
				//if(!parameters.replay)
                    log.LogInput("thumb");
			}

			//---------------------------
			// 		programmatical input
			//--------------------------
			/*if( parameters.fake_input ) {
				if( parameters.input == "row++" ) currentRowSelected++;
				if( parameters.input == "row--" ) currentRowSelected--;
				if( parameters.input == "col++" ) currentColumnSelected++;
				if( parameters.input == "col--" ) currentColumnSelected--;
				if( parameters.input == "confirm" ) confirmButtonPressed = true;
				if( parameters.input == "thumb" ) {
					pleaseAnswerHint = false;					
					if (currentQuestion > 0) {
						currentQuestion = previousQuestion.Pop ();
						currentRowSelected = 1;
						currentColumnSelected = 1;
					}
				}

				parameters.fake_input = false;
			}*/
		}
	}

	// -----------------------------------------
	//			 mouse motion enable/disable
	//------------------------------------------
	public void StopAllMotion() {
		//savedTimeScale = Time.timeScale;	// save the time at which we pause
		//Time.timeScale = 0;				// no movement
		
		// disable any rotation of the camera
		//mouseLookComponent1.enabled = false;
		//mouseLookComponent2.enabled = false;

		//disable movement
		player.GetComponent<CharacterController>().enabled = false;
		//player.GetComponent<CharacterMotor>().enabled = false;
	}
	
	public void ContinueMotion(){
		//Time.timeScale = savedTimeScale;	// start game again at the time at which we stopped	
		
		// reenable camera rotation
		//mouseLookComponent1.enabled = true;
		//mouseLookComponent2.enabled = true;

		//enable movement
		player.GetComponent<CharacterController>().enabled = true;
		//player.GetComponent<CharacterMotor>().enabled = true;
	}

	// -----------------------------------------
	//			 distance check
	//------------------------------------------
	public float DistanceToPlayer() {
		return (transform.position - player.transform.position).magnitude;
	}
	
	public bool IsInsideMenuArea() {
		return ( DistanceToPlayer() < GetComponent<SphereCollider>().radius );
	}

	// -----------------------------------------
	//			 display questionnaire
	//------------------------------------------
	//this one will be different from the full screen questionnaire, since it needs to be joystick usable

	private void DisplayQuestionnaire(){
		//draw the background
		int top 	= (Screen.height - (int) textureHeight) / 2;
		int left 	= (Screen.width - (int) textureWidth) / 2;
		GUI.DrawTexture (new Rect (left, top, textureWidth, textureHeight), backgroundTexture);

		//draw questions
		GUILayout.BeginArea( new Rect( (Screen.width - windowWidth)/2, (Screen.height - windowHeight)/2, windowWidth, windowHeight) );				
		
		if (currentQuestion >= 0 && currentQuestion < totalQuestions) {
			//Show question number
			skin.label.fontSize = skin.label.fontSize + 4;
			skin.label.fontStyle = FontStyle.Bold;
			//GUILayout.Label ( "Question " + (currentQuestion + 1).ToString() + " of " + totalQuestions.ToString() + "\n\n", skin.label );
			GUILayout.Label ( "Question " + (currentQuestion + 1).ToString() + "\n\n", skin.label );
			skin.label.fontSize = skin.label.fontSize - 4;
			skin.label.fontStyle = FontStyle.Normal;
			
			//Show Questions for this Page
			//for the PopUpQuestionnaire there should only be questions with focus control -> so its a SimpleQuestion
            Question q = (Question)currentQuestionSet.getQuestion(currentQuestion);
            // FIXME displayWithFocusControl is not part of Question class (root class of all questions)
			//q.displayWithFocusControl(currentRowSelected, currentColumnSelected, confirmButtonPressed);

			//NoAnswer
			if(pleaseAnswerHint) {
				skin.label.fontStyle = FontStyle.Bold;
				skin.label.normal.textColor = new Color(1,0,0,1);
				GUI.Label (new Rect(windowWidth/2 - 140, windowHeight - 100, 280, 50), "Please answer all questions!", skin.label);
				skin.label.normal.textColor = new Color(0,0,0,1);
				skin.label.fontStyle = FontStyle.Normal;
			}

			//continue button
			GUILayout.BeginHorizontal();
			GUI.SetNextControlName( "continue" );
			GUI.Button( new Rect( windowWidth/2 - 100, windowHeight - 70, 200, 30) , "Continue", skin.label );
			GUILayout.EndHorizontal();

			bool lastRow;
			if(q.GetRows() == 1) lastRow = mod(currentRowSelected-1, q.GetColumns()+1) == q.GetColumns();
			else 				 lastRow = mod(currentRowSelected-1, q.GetRows()+1) == q.GetRows();

			if( lastRow && confirmButtonPressed ) {
				if (q.AnswerGiven()){
					//if(!parameters.replay)
                        q.SaveAnswer();
					previousQuestion.Push(currentQuestion);
                    currentQuestion += currentQuestionSet.getJumpSize(currentQuestion);
					currentRowSelected = 1;
					currentColumnSelected = 1;
					pleaseAnswerHint = false;
				} else {
					pleaseAnswerHint = true;
				}

				if( currentQuestion == totalQuestions ) {
					finished = true;
					ContinueMotion();
				}
			}

			// Set focus for gamepad control, if row is last one, focuse continue button
			if( lastRow ) GUI.FocusControl("continue");

			confirmButtonPressed = false;	//already used to display question -> set back
			
		} /*else {
			GUILayout.Label ( "End of questionnaire.", skin.label );

			//GUILayout.BeginHorizontal();
			GUI.SetNextControlName( "finish" );
			GUI.Button( new Rect( windowWidth/2 - 100, windowHeight - 100, 200, 30) , "Continue", skin.label );
			GUI.FocusControl("finish");
			//GUILayout.EndHorizontal();

			if( confirmButtonPressed ) {
				finished = true;
				ContinueMotion();
			}
		}*/
		
		GUILayout.EndArea();
	}

	// -----------------------------------------
	//			fading in and out functions
	//------------------------------------------
	
	private void FadeToMenu()
	{
		lerpTime += fadeSpeed * Time.deltaTime;
		alpha = Mathf.Lerp (alpha, 1f, lerpTime);
		GUI.color = new Color(1,1,1,alpha);
		//blur.iterations = (int) (alpha * 9) % 10 + 1;
	}
	
	private void FadeToClear()
	{
		lerpTime += fadeSpeed * Time.deltaTime;
		alpha = Mathf.Lerp (alpha, 0, lerpTime);
		GUI.color = new Color(1,1,1,alpha);
		//blur.iterations = (int) (alpha * 9) % 10 + 1;
	}
	
	private void StartFadingIn()
	{
		//blur.enabled = true;
		FadeToMenu();
		
		if (GUI.color.a >= 0.95f) {
			GUI.color = new Color(1,1,1,1);
			fadingIn = false;
			fadingOut = true;
			lerpTime = 0;
			StopAllMotion();
		}
	}
	
	private void StartFadingOut()
	{
		FadeToClear();
		
		if (GUI.color.a <= 0.05f) {
			GUI.color = new Color(1,1,1,0);
			//blur.enabled = false;
			fadingIn = true;
			fadingOut = false;
			lerpTime = 0;
			ContinueMotion();
		}
	}

	// -----------------------------------------
	//			other helpful stuff
	//------------------------------------------

	//a correct modulo function
	int mod(int a, int b) {
		return a - b * Mathf.FloorToInt((float) a / (float) b);
	}
}
