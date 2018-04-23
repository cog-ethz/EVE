using UnityEngine;

public class GuidanceArrow : MonoBehaviour
{

    private Transform target;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target);
    }

    public void setTarget(Transform aTarget)
    {
        target = aTarget;
    }

    public Transform getTarget()
    {
        return target;
    }
}
