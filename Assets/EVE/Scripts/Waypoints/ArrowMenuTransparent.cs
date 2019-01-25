using UnityEngine;

public class ArrowMenuTransparent : MonoBehaviour {

	public float 		FadeSpeed;
    public Color Color;

    private bool  		_fadingIn;
	private bool  		_fadingOut;
	private float 		_alpha;
	private float _lerpTime;
    private Transform _flashingObject, _player;
	

	void Awake () {
	    _flashingObject = transform.Find("arrow");
	    Color = _flashingObject.transform.GetComponent<Renderer>().material.color;
	    _flashingObject.transform.GetComponent<Renderer>().material.color = new Color(Color.r, Color.g, Color.b,0f);
	    

        // Fading Parameters
	    _fadingIn = true;
	    _fadingOut = false;
	    _alpha = 0;
	    _lerpTime = 0;

	}


	void OnGUI () {

	    if (_player == null && Camera.main != null)
	    {
	        _player = Camera.main.transform.parent;
        }
	    else
	    {
	        if (IsInsideMenuArea())
	        {
	            if (_fadingIn) StartFadingIn(); // controls the transparency of the drawn elements
	        }
	        else
	        {
	            if (_fadingOut)
	            {
	                StartFadingOut();
	            }
	        }
        }

		
		
	}

	// -----------------------------------------
	//			 public functions 
	//------------------------------------------

	public float DistanceToPlayer() {
		return (transform.position - _player.transform.position).magnitude;
	}

	public bool IsInsideMenuArea() {
		return ( DistanceToPlayer() < GetComponent<SphereCollider>().radius );
	}
    // -----------------------------------------
    //			fading in and out functions
    //------------------------------------------

    private void FadeToMenu()
	{
	    _lerpTime += FadeSpeed * Time.deltaTime;
	    _alpha = Mathf.Lerp (_alpha, 1f, _lerpTime);
	    Color = _flashingObject.transform.GetComponent<Renderer>().material.color;
	    _flashingObject.transform.GetComponent<Renderer>().material.color = new Color(Color.r, Color.g, Color.b, _alpha);
	}

	private void FadeToClear()
	{
	    _lerpTime += FadeSpeed * Time.deltaTime;
	    _alpha = Mathf.Lerp (_alpha, 0, _lerpTime);
	    Color = _flashingObject.transform.GetComponent<Renderer>().material.color;
	    _flashingObject.transform.GetComponent<Renderer>().material.color = new Color(Color.r, Color.g, Color.b, _alpha);
	}

    private void StartFadingIn()
	{
		FadeToMenu();
		
		if (_alpha >= 0.95f) {
		    _alpha = 1f;
		    Color = _flashingObject.transform.GetComponent<Renderer>().material.color;
		    _flashingObject.transform.GetComponent<Renderer>().material.color = new Color(Color.r, Color.g, Color.b,1f);
		    _fadingIn = false;
		    _fadingOut = true;
		    _lerpTime = 0;
		}
	}

    private void StartFadingOut()
	{
		FadeToClear();
		
		if (_alpha <= 0.05f) {
		    _alpha = 0f;
		    Color = _flashingObject.transform.GetComponent<Renderer>().material.color;
		    _flashingObject.transform.GetComponent<Renderer>().material.color = new Color(Color.r, Color.g, Color.b,0f);
		    _fadingIn = true;
		    _fadingOut = false;
		    _lerpTime = 0;
		}
	}
}
