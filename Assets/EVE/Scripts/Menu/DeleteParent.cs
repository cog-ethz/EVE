using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteParent : MonoBehaviour
{

    public void deleteParentObject(GameObject deleteButton)
    {
        Destroy(deleteButton.transform.parent.gameObject);
    }



}