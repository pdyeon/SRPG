using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour
{
    private void Start()
    {
        newPosition = this.transform.position;
    }
    Vector3 newPosition;

    Vector3 currentVelocity;
    float smoothTime = 0.5f;

    public void OnUnitMoved( Hex oldHex, Hex newHex )
    {
        this.transform.position = oldHex.PositionFromCamera();
        newPosition = newHex.PositionFromCamera();
        currentVelocity = Vector3.zero;

        if(Vector3.Distance(this.transform.position, newPosition) > 2)
        {
            this.transform.position = newPosition;
        }
    }

    private void Update()
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref currentVelocity, smoothTime);
    }
}
