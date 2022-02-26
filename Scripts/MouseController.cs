using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    void Start()
    {
        Update_CurrentFunc = Update_DerectModeStart;

        hexMap = GameObject.FindObjectOfType<HexMap>();

        lineRenderer = transform.GetComponentInChildren<LineRenderer>();
    }

    // Generic bookkeeping variables
    HexMap hexMap;
    Hex hexUnderMouse;
    Hex hexLastUnderMouse;
    Vector3 lastMousePosition; // From Input. mousePosition

    // Camera Dragging bookkeeping variables
    int mouseDragThreshold = 1; // Threshold of mouse movement to start a drag
    Vector3 lastMouseGroundPlanePosition;

    // Unit movement
    Unit selectedUnit = null;
    Hex[] hexPath;
    LineRenderer lineRenderer;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;

    public LayerMask LayerIDForHexTiles;

    private void Update()
    {
       hexUnderMouse = MouseToHex();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUpdateFunc();
        }

        Update_CurrentFunc();

        // Always do camera zooms (check for being over a scroll UI later)
        Update_ScrollZomm();

        lastMousePosition = Input.mousePosition;
        hexLastUnderMouse = hexUnderMouse;

        if(selectedUnit != null)
        {
            DrawPath( (hexPath != null) ? hexPath : selectedUnit.GetHexPath() );
        }
        else
        {
            DrawPath(null); // Clear the path display
        }
    }

    void DrawPath(Hex[] hexPath)
    {
        if (hexPath == null || hexPath.Length == 0)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;

        Vector3[] ps = new Vector3[hexPath.Length];

        for (int i = 0; i < hexPath.Length; i++)
        {
            GameObject hexGO = hexMap.GetHexGO(hexPath[i]);
            ps[i] = hexGO.transform.position + (Vector3.up * 0.1f);
        }

        lineRenderer.positionCount = ps.Length;
        lineRenderer.SetPositions(ps);
    }

    void CancelUpdateFunc()
    {
        Update_CurrentFunc = Update_DerectModeStart;

        // Also do cleanup of any UI stuff associated with modes
        selectedUnit = null;

        hexPath = null;
    }

    void Update_DerectModeStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Left mouse button just went down.
            // This doesn't do anything by itself
        }
        else if(Input.GetMouseButtonUp(0))
        {
            //MouseToHex();

            // TODO: Are we clicking on a hex with a unit?
            // If so, select it

            Unit[] us = hexUnderMouse.Units();

            // TODO: Implement cycling through multiple units in the same tile

            if(us.Length > 0)
            {
                selectedUnit = us[0];

                // NOTE: Selecting a unit does NOT change our mouse mode

                // Update_CurrentFunc = Update_UnitMovement;
            }
        }
        else if(selectedUnit != null && Input.GetMouseButtonDown(1))
        {
            // We have a selected unit, and we've pushed down the right
            // mouse button, so enter unit movement mode
            Update_CurrentFunc = Update_UnitMovement;
        }

        else if(Input.GetMouseButton(0) && 
            Vector3.Distance( Input.mousePosition , lastMousePosition) > mouseDragThreshold)
        {
            // Left button is being held down AND the mouse moved? That's a camera drag!
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();
        }
        else if(selectedUnit != null && Input.GetMouseButton(1))
        {

        }
    }

    Hex MouseToHex()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        int layerMask = LayerIDForHexTiles.value;

        if(Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, layerMask))
        {
            //Debug.Log(hitInfo.collider.name);

            GameObject hexGo = hitInfo.rigidbody.gameObject;

            return hexMap.GetHexFromGameObject(hexGo);
        }
        Debug.Log("Found nothing");
        return null;
    }

    Vector3 MouseToGroundPlane(Vector3 mousePos)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);

        if (mouseRay.direction.y >= 0)
        {
            //Debug.LogError("Why is mouse pointing up?");
            return Vector3.zero;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }

    void Update_UnitMovement()
    {
        if(Input.GetMouseButtonUp(1) || selectedUnit == null)
        {
            if(selectedUnit != null)
            {
                selectedUnit.SetHexPath(hexPath);
            }

            CancelUpdateFunc();
            return;
        }

        // We have a selected unit

        // Look at the hex under our mouse

        // Is this a different hex than before (or we don't already have a path)
        if(hexPath == null || hexUnderMouse != hexLastUnderMouse)
        {
            // Do a pathfinding search to that hex
            hexPath = QPath.QPath.FindPath<Hex>(hexMap, selectedUnit, selectedUnit.Hex, hexUnderMouse, Hex.CostEstimate);
        }

    }

 
    void Update_CameraDrag()
    {
        if(Input.GetMouseButtonUp(0))
        {
            CancelUpdateFunc();
            return;
        }

        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);

        Vector3 diff = lastMouseGroundPlanePosition - hitPos;

        Camera.main.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);

        

    }

    void Update_ScrollZomm()
    {
        //Zoom
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float minHeight = 30;
        float maxHeight = 300;

        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);

        Vector3 dir = hitPos - Camera.main.transform.position;

        Vector3 p = Camera.main.transform.position;

        if (scrollAmount > 0 || p.y < maxHeight)
        {
            if(scrollAmount > 0 && p.y <= minHeight)
            {
                return;
            }
            Camera.main.transform.Translate(dir * scrollAmount, Space.World);
        }

        p = Camera.main.transform.position;
        if (p.y < minHeight)
        {
            p.y = minHeight;
        }
        if (p.y > maxHeight)
        {
            p.y = maxHeight;
        }
        Camera.main.transform.position = p;
    }
}
