using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSquare : MonoBehaviour
{
    //Public drags

    //Add all units in the scene to this array
    public GameObject[] allUnits;
    //The selection square we draw when we drag the mouse to select units
    public RectTransform selectionSquareTrans;
 
    //The materials
    public Material normalMaterial;
    public Material highlightMaterial;
    public Material selectedMaterial;

    //The camera we are using
    public Camera myCamera;


    //Private

    //All currently selected units
    private List<GameObject> selectedUnits = new List<GameObject>();
    //All currently highlighted unit (are within the rectangle while dragging left mouse)
    private List<GameObject> highlightedUnits = new List<GameObject>();
    //If the mouse is above a unit, it should also be highlightet
    //But we also have to un-highlight it the mouse is no longer above the unit
    private GameObject previouslyHighlightedUnit;

    //To determine if we are clicking with left mouse or holding down left mouse
    private float delay = 0.3f;
    private float timeWhenPressedLeftMouseButton = 0f;

    //The start and end coordinates of the rectangle we are making in screen space
    private Vector3 rectangleStartPos;
    //The selection rectangle's 4 corner positions in world space
    private Vector3 TL, TR, BL, BR;

    //The ground plane on which the units are located
    //It's faster and simpler to raycast to this plane than using a finite ground plane 
    //because this plane is infinite so we dont have to care if we are outside of the ground plane
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    //Have we clicked with left mouse or are we holding down left mouse
    private bool hasClicked, isHoldingDown, hasReleased, isHovering = false;



    void Start()
    {
        //Deactivate the square selection image
        selectionSquareTrans.gameObject.SetActive(false);
    }



    void Update()
    {
        //Have we clicked with left mouse or are we holding down left mouse
        CheckInput();
    
        //Select (or highlight) one or several units with the mouse
        InteractWithUnits();
    }



    //Have we clicked with left mouse or are we holding down left mouse
    private void CheckInput()
    {
        hasClicked = false;
        isHoldingDown = false;
        hasReleased = false;
        isHovering = false;

        //Press down left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            timeWhenPressedLeftMouseButton = Time.time;

            //We dont yet know if we are drawing a rectangle, but we need the first coordinate in case we do draw a rectangle
            rectangleStartPos = Input.mousePosition;
        }
        //Release left mouse button
        else if (Input.GetMouseButtonUp(0))
        {
            //If we didn't hold down the left mouse button long enough, we say it's a click
            if (Time.time - timeWhenPressedLeftMouseButton <= delay)
            {
                hasClicked = true;
            }

            hasReleased = true;
        }
        //Hold down left mouse button
        else if (Input.GetMouseButton(0))
        {
            if (Time.time - timeWhenPressedLeftMouseButton > delay)
            {
                isHoldingDown = true;
            }
        }
        else 
        {
            isHovering = true;
        }
    }



    //Select units with click or by dragging left mouse button
    void InteractWithUnits()
    {
        //If we have clicked we want to: 
        //- deselect all units
        //- select a single unit if we click on it
        if (hasClicked)
        {        
            //Deselect all units
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                selectedUnits[i].GetComponent<MeshRenderer>().sharedMaterial = normalMaterial;
            }

            //Clear the list with selected units
            selectedUnits.Clear();


            //Try to select a new unit
            //Fire ray from camera
            if (Physics.Raycast(myCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 200f))
            {
                //Did we hit a friendly unit?
                if (hit.collider.CompareTag("Friendly"))
                {
                    GameObject activeUnit = hit.collider.gameObject;
                    
                    //Set this unit to selected
                    activeUnit.GetComponent<MeshRenderer>().sharedMaterial = selectedMaterial;
                    
                    //Add it to the list of selected units, which is now just 1 unit
                    selectedUnits.Add(activeUnit);
                }
            }
        }


        //If we drag the mouse we want to select all units within the rectangle
        if (isHoldingDown)
        {
            //Display the rectangle with a GUI image
            //will also generate the corners of the polygon we use to test if a unit should be selected
            GenerateDisplayRectangleAndSelectionPolygon();

            //Highlight the units within the selection rectangle, but don't select the units
            //We select them when we have released the mouse button
            highlightedUnits.Clear();

            foreach (GameObject unit in allUnits)
            {
                //Is this unit within the rectangle
                if (IsWithinPolygon(unit.transform.position))
                {
                    unit.GetComponent<MeshRenderer>().sharedMaterial = highlightMaterial;

                    highlightedUnits.Add(unit);
                }
                //Otherwise deselect
                else
                {
                    unit.GetComponent<MeshRenderer>().sharedMaterial = normalMaterial;
                }
            }
        }


        //We have released the mouse button and should select the units within the rectangle (if we created a rectangle)
        if (hasReleased)
        {
            //Deactivate the rectangle selection image
            selectionSquareTrans.gameObject.SetActive(false);

            //Test if we should select highlighted units?
            if (highlightedUnits.Count > 0)
            {
                //Clear the list with currently selected unit so we can select new units
                selectedUnits.Clear();

                //Select the units that are currently highlighted
                foreach (GameObject unit in highlightedUnits)
                {
                    unit.GetComponent<MeshRenderer>().sharedMaterial = selectedMaterial;

                    selectedUnits.Add(unit);
                }

                highlightedUnits.Clear();
            }
        }


        //We are hovering with the mouse, so we might want to highlight a unit
        ResetTryHighlightUnit();

        if (isHovering)
        {
            TryHighlightUnit();
        }

        //Debug.Log($"Highlighted units: {highlightedUnits.Count}. Selected units: {selectedUnits.Count}");
    }



    //Highlight a single unit when mouse is above it
    void TryHighlightUnit()
    {        
        //Fire a ray from the camera to see if mouse is above a unit 
        if (Physics.Raycast(myCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 200f))
        {
            //Did we hit a friendly unit?
            if (hit.collider.CompareTag("Friendly"))
            {
                //Get the object we hit
                GameObject currentObj = hit.collider.gameObject;

                //Highlight this unit if it's not selected
                if (!selectedUnits.Contains(currentObj))
                {
                    currentObj.GetComponent<MeshRenderer>().sharedMaterial = highlightMaterial;

                    previouslyHighlightedUnit = currentObj;
                }
            }
        }
    }


    //Will un-highlight a previously highlighted unit, or it will remain highlighted
    private void ResetTryHighlightUnit()
    {
        //Un-highlight
        if (previouslyHighlightedUnit != null)
        {
            //But we cant un-highlight it if we also selected it
            if (!selectedUnits.Contains(previouslyHighlightedUnit))
            {
                previouslyHighlightedUnit.GetComponent<MeshRenderer>().sharedMaterial = normalMaterial;

                previouslyHighlightedUnit = null;
            }
        }
    }



    //Is a unit within a polygon with 4 corners
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



    //Display the selection with a UI rectangle
    //We will also use this UI rectangle to generate a 4-corner-polygon in world space
    void GenerateDisplayRectangleAndSelectionPolygon()
    {
        //Activate the border image
        if (!selectionSquareTrans.gameObject.activeInHierarchy)
        {
            selectionSquareTrans.gameObject.SetActive(true);
        }

        //Get a corner coordinate of the rectangle, which is where the mouse currently is
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
        //These 4 corners will form a polygon, and we will see if a unit is within this polygon 
        float halfSizeX = sizeX * 0.5f;
        float halfSizeY = sizeY * 0.5f;

        Vector3 TL_screenSpace = new Vector3(middle.x - halfSizeX, middle.y + halfSizeY, 0f);
        Vector3 TR_screenSpace = new Vector3(middle.x + halfSizeX, middle.y + halfSizeY, 0f);
        Vector3 BL_screenSpace = new Vector3(middle.x - halfSizeX, middle.y - halfSizeY, 0f);
        Vector3 BR_screenSpace = new Vector3(middle.x + halfSizeX, middle.y - halfSizeY, 0f);

        //From screen to world
        Ray rayTL = myCamera.ScreenPointToRay(TL_screenSpace);
        Ray rayTR = myCamera.ScreenPointToRay(TR_screenSpace);
        Ray rayBL = myCamera.ScreenPointToRay(BL_screenSpace);
        Ray rayBR = myCamera.ScreenPointToRay(BR_screenSpace);

        float distanceToPlane = 0f;
        
        //Fire ray from camera to get the corners in world space
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

        //hasCreatedRectangle = true;
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
