using UnityEngine;

public class HexCell : MonoBehaviour
{
    HexMesh hexMesh;

    void Awake()
    {
        hexMesh = GetComponent<HexMesh>();
	}

    void Start()
    {
        hexMesh.Triangulate(this.gameObject);
    }
}