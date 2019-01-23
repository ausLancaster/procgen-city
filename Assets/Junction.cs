
using UnityEngine;

public class Junction : MonoBehaviour {

    public void SetColor(Color col)
    {
        GetComponent<MeshRenderer>().material.color = col;
    }
}
