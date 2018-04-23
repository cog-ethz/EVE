using UnityEngine;
using System.Collections;

public class MakeItSpin : MonoBehaviour {

	public float speed = 200;
	private float oldY = 1f;
	private float offset = 1f;
	private float floatingDist = 0.25f;

	void Start() {
		oldY = transform.position.y;
		transform.position += new Vector3 (0.0f, Random.Range (-floatingDist, floatingDist), 0.0f);
		offset = Time.deltaTime * 0.25f;
	}

	void Update () {
		transform.Rotate(Vector3.up, speed * Time.deltaTime * Random.Range(0.0f,1.0f));
		transform.Rotate(Vector3.right, speed * Time.deltaTime * Random.Range(0.0f,1.0f));
		transform.Rotate(Vector3.forward, speed * Time.deltaTime * Random.Range(0.0f,1.0f));
		if (Mathf.Abs(oldY - transform.position.y)>floatingDist-0.03){
			offset *= -1f;
		}
		float offsetSpeed = (floatingDist - Mathf.Abs (oldY - transform.position.y) / floatingDist);
		offsetSpeed = offsetSpeed < 0.5f ? 0.5f : offsetSpeed;
		transform.position += new Vector3 (0.0f, offsetSpeed*offset, 0.0f);
	}
}
