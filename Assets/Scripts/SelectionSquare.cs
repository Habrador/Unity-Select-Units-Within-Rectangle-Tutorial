using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSquare : MonoBehaviour
{
    //Add all units in the scene to this array
    public GameObject[] allUnits;
    //The selection square we draw when we drag the mouse to select units
    public RectTransform selectionSquareTrans;
 
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

    //The start and end coordinates of the rectangle we are making
    private Vector3 rectangleStartPos;
    //If it was possible to create a rectangle
    private bool hasCreatedRectangle;
    //The selection squares 4 corner positions
    private Vector3 TL, TR, BL, BR;

    //The ground plane on which the units are located
    //It's faster and simpler to raycast to this plane than using a plane GameObject
    //because a Plane is infinite so we can be outside of the map and still select units 
    //on the map
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);



    void Start()
    {
        //Deactivate the square selection image
        selectionSquareTrans.gameObject.SetActive(false);
    }



    void Update()
    {
        //Select one or several units by clicking or draging the mouse
        SelectUnits();

        //Highlight a single unit by hovering with mouse above a unit which is not selected
        HighlightUnit();
    }



    //Select units with click or by draging the mouse
    void SelectUnits()
    {
        //Have we clicked with left mouse or are we holding down left mouse
        bool hasClicked = false;
        bool isHoldingDown = false;
        bool hasReleased = false;

        //Press down left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            clickTime = Time.time;

            //We dont yet know if we are drawing a rectangle, but we need the first coordinate in case we do draw a rectangle
            rectangleStartPos = Input.mousePosition;
        }
        //Release left mouse button
        else if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - clickTime <= delay)
            {
                hasClicked = true;
            }

            hasReleased = true;
        }
        //Hold down left mouse button
        else if (Input.GetMouseButton(0))
        {
            if (Time.time - clickTime > delay)
            {
                isHoldingDown = true;
            }
        }


        //Select one unit and/or deselect currently selected units by clicking on what's not a unit
        if (hasClicked)
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
            //Display the rectangle with a GUI image
            DisplayRectangle();

            //Highlight the units within the selection square, but don't select the units
            //We select them when we have released the mouse button
            if (hasCreatedRectangle)
            {
                for (int i = 0; i < allUnits.Length; i++)
                {
                    GameObject currentUnit = allUnits[i];

                    //Is this unit within the rectangle
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


        //We have released the mouse button and should select the units within the rectangle
        if (hasReleased)
        {
            //Select all units within the rectangle if we have created a rectangle
            if (hasCreatedRectangle)
            {
                hasCreatedRectangle = false;

                //Deactivate the square selection image
                selectionSquareTrans.gameObject.SetActive(false);

                //Clear the list with selected unit
                selectedUnits.Clear();

                //Select the units that are within the rectangle
                for (int i = 0; i < allUnits.Length; i++)
                {
                    GameObject currentUnit = allUnits[i];

                    //Is this unit within the rectangle
                    if (IsWithinPolygon(currentUnit.transform.position))
                    {
                        currentUnit.GetComponent<MeshRenderer>().material = selectedMaterial;

                        selectedUnits.Add(currentUnit);
                    }
                    //Otherwise deselect the unit if it's not in the rectangle
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



    //Is a point within a triangle in xz space
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
        //Activate the border image
        if (!selectionSquareTrans.gameObject.activeInHierarchy)
        {
            selectionSquareTrans.gameObject.SetActive(true);
        }

        //Get the a corner coordinate of the rectangle, which is where the mouse currently is
        Vector3 rectangleEndPos = Input.mousePosition;

        //Calculate the middle position of the rectangle by using the two corners we have
        Vector3 middle = (rectangleStartPos + rectangleEndPos) / 2f;

        //Set the middle position of the GUI rectangle
        selectionSquareTrans.position = middle;

        //Calculate the size of the rectangle
        float sizeX = Mathf.Abs(rectangleStartPos.x - rectangleEndPos.x);
        float sizeY = Mathf.Abs(rectangleStartPos.y - rectangleEndPos.y);

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
        Ray rayTL = Camera.main.ScreenPointToRay(TL);
        Ray rayTR = Camera.main.ScreenPointToRay(TR);
        Ray rayBL = Camera.main.ScreenPointToRay(BL);
        Ray rayBR = Camera.main.ScreenPointToRay(BR);

        float distanceToPlane = 0f;
        
        //Fire ray from camera
        if (groundPlane.Raycast(rayTL, out distanceToPlane))
        {
            TL = rayTL.GetPoint(distanceToPlane);
        }
        if (groundPlane.Raycast(rayTR, out distanceToPlane))
        {
            TR = rayTR.GetPoint(distanceToPlane);
        }
        if (groundPlane.Raycast(rayBL, out distanceToPlane))
        {
            BL = rayBL.GetPoint(distanceToPlane);
        }
        if (groundPlane.Raycast(rayBR, out distanceToPlane))
        {
            BR = rayBR.GetPoint(distanceToPlane);
        }

        hasCreatedRectangle = true;
    }



    //Debug the rectangles corners in world space
    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(TL, 1f);
        //Gizmos.DrawSphere(TR, 1f);
        //Gizmos.DrawSphere(BL, 1f);
        //Gizmos.DrawSphere(BR, 1f);
    }
}
