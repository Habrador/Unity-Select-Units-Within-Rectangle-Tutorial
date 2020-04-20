using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSquare : MonoBehaviour
{
    //Add all units in the scene to this array
    public GameObject[] allUnits;
    //The selection square we draw when we drag the mouse to select units
    public RectTransform selectionSquareTrans;
    //To test the square's corners
    public Transform sphere1;
    public Transform sphere2;
    public Transform sphere3;
    public Transform sphere4;
    //The materials
    public Material normalMaterial;
    public Material highlightMaterial;
    public Material selectedMaterial;

    //All currently selected units
    [System.NonSerialized]
    public List<GameObject> selectedUnits = new List<GameObject>();

    //We have hovered above this unit, so we can deselect it next update
    //and dont have to loop through all units
    private GameObject highlightThisUnit;

    //To determine if we are clicking with left mouse or holding down left mouse
    private float delay = 0.3f;
    private float clickTime = 0f;
    //The start and end coordinates of the square we are making
    private Vector3 squareStartPos;
    private Vector3 squareEndPos;
    //If it was possible to create a square
    private bool hasCreatedSquare;
    //The selection squares 4 corner positions
    private Vector3 TL, TR, BL, BR;



    void Start()
    {
        //Deactivate the square selection image
        selectionSquareTrans.gameObject.SetActive(false);
    }



    void Update()
    {
        //Select one or several units by clicking or draging the mouse
        SelectUnits();

        //Highlight by hovering with mouse above a unit which is not selected
        HighlightUnit();
    }



    //Select units with click or by draging the mouse
    void SelectUnits()
    {
        //Are we clicking with left mouse or holding down left mouse
        bool isClicking = false;
        bool isHoldingDown = false;

        //Click the mouse button
        if (Input.GetMouseButtonDown(0))
        {
            clickTime = Time.time;

            //We dont yet know if we are drawing a square, but we need the first coordinate in case we do draw a square
            RaycastHit hit;
            //Fire ray from camera to the ground plane
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200f, 1 << 9))
            {
                //The corner position of the square
                squareStartPos = hit.point;
            }
        }
        //Release the mouse button
        else if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - clickTime <= delay)
            {
                isClicking = true;
            }

            //Select all units within the square if we have created a square
            if (hasCreatedSquare)
            {
                hasCreatedSquare = false;

                //Deactivate the square selection image
                selectionSquareTrans.gameObject.SetActive(false);

                //Clear the list with selected unit
                selectedUnits.Clear();

                //Select the units
                for (int i = 0; i < allUnits.Length; i++)
                {
                    GameObject currentUnit = allUnits[i];

                    //Is this unit within the square
                    if (IsWithinPolygon(currentUnit.transform.position))
                    {
                        currentUnit.GetComponent<MeshRenderer>().material = selectedMaterial;

                        selectedUnits.Add(currentUnit);
                    }
                    //Otherwise deselect the unit if it's not in the square
                    else
                    {
                        currentUnit.GetComponent<MeshRenderer>().material = normalMaterial;
                    }
                }
            }

        }
        //Holding down the mouse button
        else if (Input.GetMouseButton(0))
        {
            if (Time.time - clickTime > delay)
            {
                isHoldingDown = true;
            }
        }


        //Select one unit with left mouse and deselect all units with left mouse by clicking on what's not a unit
        if (isClicking)
        {
            //Deselect all units
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                selectedUnits[i].GetComponent<MeshRenderer>().material = normalMaterial;
            }

            //Clear the list with selected units
            selectedUnits.Clear();

            //Try to select a new unit
            RaycastHit hit;
            //Fire ray from camera
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200f))
            {
                //Did we hit a friendly unit?
                if (hit.collider.CompareTag("Friendly"))
                {
                    GameObject activeUnit = hit.collider.gameObject;
                    //Set this unit to selected
                    activeUnit.GetComponent<MeshRenderer>().material = selectedMaterial;
                    //Add it to the list of selected units, which is now just 1 unit
                    selectedUnits.Add(activeUnit);
                }
            }
        }


        //Drag the mouse to select all units within the square
        if (isHoldingDown)
        {
            //Activate the square selection image
            if (!selectionSquareTrans.gameObject.activeInHierarchy)
            {
                selectionSquareTrans.gameObject.SetActive(true);
            }

            //Get the latest coordinate of the square
            squareEndPos = Input.mousePosition;

            //Display the selection with a GUI image
            DisplayRectangle();

            //Highlight the units within the selection square, but don't select the units
            //We select them when we have released the mouse button
            if (hasCreatedSquare)
            {
                for (int i = 0; i < allUnits.Length; i++)
                {
                    GameObject currentUnit = allUnits[i];

                    //Is this unit within the square
                    if (IsWithinPolygon(currentUnit.transform.position))
                    {
                        currentUnit.GetComponent<MeshRenderer>().material = highlightMaterial;
                    }
                    //Otherwise deactivate
                    else
                    {
                        currentUnit.GetComponent<MeshRenderer>().material = normalMaterial;
                    }
                }
            }
        }
    }



    //Highlight a unit when mouse is above it
    void HighlightUnit()
    {
        //Change material on the latest unit we highlighted
        if (highlightThisUnit != null)
        {
            //But make sure the unit we want to change material on is not selected
            bool isSelected = false;
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                if (selectedUnits[i] == highlightThisUnit)
                {
                    isSelected = true;
                    break;
                }
            }

            if (!isSelected)
            {
                highlightThisUnit.GetComponent<MeshRenderer>().material = normalMaterial;
            }

            highlightThisUnit = null;
        }

        //Fire a ray from the mouse position to get the unit we want to highlight
        RaycastHit hit;
        //Fire ray from camera
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200f))
        {
            //Did we hit a friendly unit?
            if (hit.collider.CompareTag("Friendly"))
            {
                //Get the object we hit
                GameObject currentObj = hit.collider.gameObject;

                //Highlight this unit if it's not selected
                bool isSelected = false;
                for (int i = 0; i < selectedUnits.Count; i++)
                {
                    if (selectedUnits[i] == currentObj)
                    {
                        isSelected = true;
                        break;
                    }
                }

                if (!isSelected)
                {
                    highlightThisUnit = currentObj;

                    highlightThisUnit.GetComponent<MeshRenderer>().material = highlightMaterial;
                }
            }
        }
    }



    //Is a unit within a polygon determined by 4 corners
    bool IsWithinPolygon(Vector3 unitPos)
    {
        bool isWithinPolygon = false;

        //The polygon forms 2 triangles, so we need to check if a point is within any of the triangles
        //Triangle 1: TL - BL - TR
        if (IsWithinTriangle(unitPos, TL, BL, TR))
        {
            return true;
        }

        //Triangle 2: TR - BL - BR
        if (IsWithinTriangle(unitPos, TR, BL, BR))
        {
            return true;
        }

        return isWithinPolygon;
    }



    //Is a point within a triangle
    //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
    bool IsWithinTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        bool isWithinTriangle = false;

        //Need to set z -> y because of other coordinate system
        float denominator = ((p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z));

        float a = ((p2.z - p3.z) * (p.x - p3.x) + (p3.x - p2.x) * (p.z - p3.z)) / denominator;
        float b = ((p3.z - p1.z) * (p.x - p3.x) + (p1.x - p3.x) * (p.z - p3.z)) / denominator;
        float c = 1 - a - b;

        //The point is within the triangle if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }



    //Display the selection with a GUI rectangle
    void DisplayRectangle()
    {
        //The start position of the rectangle is in 3d space, or the first coordinate will move
        //as we move the camera which is not what we want
        Vector3 squareStartScreen = Camera.main.WorldToScreenPoint(squareStartPos);

        squareStartScreen.z = 0f;

        //Calculate the middle position of the rectangle
        Vector3 middle = (squareStartScreen + squareEndPos) / 2f;

        //Set the middle position of the GUI rectangle
        selectionSquareTrans.position = middle;

        //Calculate the size of the rectangle
        float sizeX = Mathf.Abs(squareStartScreen.x - squareEndPos.x);
        float sizeY = Mathf.Abs(squareStartScreen.y - squareEndPos.y);

        //Set the size of the square
        selectionSquareTrans.sizeDelta = new Vector2(sizeX, sizeY);

        //The problem is that the corners in the 2d rectangle is not the same as in 3d space
        //To get corners, we have to fire 4 rays from the screen and see where they hit the ground
        float halfSizeX = sizeX * 0.5f;
        float halfSizeY = sizeY * 0.5f;

        TL = new Vector3(middle.x - halfSizeX, middle.y + halfSizeY, 0f);
        TR = new Vector3(middle.x + halfSizeX, middle.y + halfSizeY, 0f);
        BL = new Vector3(middle.x - halfSizeX, middle.y - halfSizeY, 0f);
        BR = new Vector3(middle.x + halfSizeX, middle.y - halfSizeY, 0f);

        //From screen to world
        RaycastHit hit;
        int i = 0;
        //Fire ray from camera
        if (Physics.Raycast(Camera.main.ScreenPointToRay(TL), out hit, 200f, 1 << 9))
        {
            TL = hit.point;
            i++;
        }
        if (Physics.Raycast(Camera.main.ScreenPointToRay(TR), out hit, 200f, 1 << 9))
        {
            TR = hit.point;
            i++;
        }
        if (Physics.Raycast(Camera.main.ScreenPointToRay(BL), out hit, 200f, 1 << 9))
        {
            BL = hit.point;
            i++;
        }
        if (Physics.Raycast(Camera.main.ScreenPointToRay(BR), out hit, 200f, 1 << 9))
        {
            BR = hit.point;
            i++;
        }

        //Could we create a square?
        hasCreatedSquare = false;

        //We could find 4 points
        if (i == 4)
        {
            //Display the corners for debug
            //sphere1.position = TL;
            //sphere2.position = TR;
            //sphere3.position = BL;
            //sphere4.position = BR;

            hasCreatedSquare = true;
        }
    }
}
