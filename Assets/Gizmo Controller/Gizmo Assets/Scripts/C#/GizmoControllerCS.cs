using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class GizmoControllerCS : MonoBehaviour
{
    public enum GIZMO_MODE { TRANSLATE, ROTATE, SCALE }
    enum GIZMO_AXIS { NONE, X, Y, Z }

    public float RotationSpeed = 250.0f;

    #region Public Variables
    //Snap settings
    public float SnappedRotationSpeed = 500.0f;
    public float SnappedScaleMultiplier = 4.0f;
    public bool Snapping = false;
    public float MoveSnapIncrement = 1;
    public float AngleSnapIncrement = 5;
    public float ScaleSnapIncrement = 1;

    public Texture2D[] GizmoControlButtonImages;
    public int InteractableLayer = 9;
    public int LayerID = 8;
    public float gizmoDefaultScale = 2f;
    public Camera ActiveCamera = null;

    //Mode Settings
    public bool AllowTranslate = true;
    public bool AllowRotate = true;
    public bool AllowScale = true;
    private bool TranslateInLocalSpace = false; //Local Space Translation has issues
    public bool ShowGizmoControlWindow = true;

    //Shortcut Keys
    public bool EnableShortcutKeys = true;
    public string TranslateShortcutKey = "1";
    public string RotateShortcutKey = "2";
    public string ScaleShortcutKey = "3";
    public string SnapShortcutKey = "s";

    public Toggle translateToggle;
    public Toggle rotateToggle;
    public Toggle scaleToggle;

    #endregion

    #region Private Variables

    private GIZMO_MODE _mode;
    public Transform SelectedObject { get; private set; }
    private bool _showGizmo = false;

    private GIZMO_AXIS _activeAxis = GIZMO_AXIS.NONE;
    private Transform _selectedAxis = null;

    private Vector3 _lastIntersectPosition = Vector3.zero;
    private Vector3 _currIntersectPosition = Vector3.zero;

    private bool _draggingAxis = false;
    private Vector3 _translationDelta = Vector3.zero;
    private Vector3 initialScale = Vector3.zero;
    private float axisLength = 70;

    private float _rotationSnapDelta = 0;
    private Vector3 _scaleSnapDelta = Vector3.zero;
    private Vector3 _moveSnapDelta = Vector3.zero;

    private bool _ignoreRaycast = false;

    private float _XRotationDisplayValue = 0;
    private float _YRotationDisplayValue = 0;
    private float _ZRotationDisplayValue = 0;

    private Vector2 _controlWinPosition = Vector2.zero;
    private int _controlWinWidth = 105;
    private int _controlWinHeight = 160;

    private GUISpinnerCS AxisSpinner = new GUISpinnerCS();
    private GUISpinnerCS SnapSpinner = new GUISpinnerCS();

    #endregion

    void Start()
    {
        Hide();
    }//Start

    //Returns true if currently dragging the gizmo with the mouse
    public bool IsDraggingAxis()
    {
        return _draggingAxis;
    }//IsDragging

    //Returns true if the mouse is currently hovering over one of the gizmo axis controls
    public bool IsOverAxis()
    {
        if (_selectedAxis != null)
            return true;

        return false;
    }//IsOverAxis

    //Make the gizmo visible
    //Pass in GIZMO_MODE value which will set the current mode of the gizmo control
    public void Show()
    {
        if (SelectedObject == null)
            return;

        foreach (Transform t in transform)
        {
            if (t.GetComponent<Renderer>())
                t.GetComponent<Renderer>().enabled = true;

            if (t.GetComponent<Collider>())
                t.GetComponent<Collider>().isTrigger = false;
        }//for

        SetMode(_mode);
        _showGizmo = true;
    }//Show

    //Hides the gizmo
    public void Hide()
    {
        foreach (Transform t in transform)
        {
            if (t.GetComponent<Renderer>())
                t.GetComponent<Renderer>().enabled = false;

            if (t.GetComponent<Collider>())
                t.GetComponent<Collider>().isTrigger = true;
        }//for

        SelectedObject = null;
        _selectedAxis = null;
        _showGizmo = false;
    }//Hide

    //Returns true if the Gizmo control is currently not visible
    public bool IsHidden()
    {
        return !_showGizmo;
    }//IsHidden

    //Set's the X/Y position in screen coordinates of the gizmo control window
    public void SetControlWinPosition(Vector2 position)
    {
        _controlWinPosition = position;
    }//SetControlWinPosition

    //Toggles snap mode on and off
    public void ToggleSnapping()
    {
        Snapping = !Snapping;
    }//ToggleSnapping

    //Set's snapping mode
    // Pass in boolean True=Snap on, False=Snap off
    public void SetSnapping(bool snap)
    {
        Snapping = snap;
    }//SetSnapping

    //Returns the active GIZMO_MODE
    public GIZMO_MODE GetMode()
    {
        return _mode;
    }//GetMode

    public void ResetTransformToSelectedObject()
    {
        if (SelectedObject == null)
            return;

        transform.position = SelectedObject.position;
        if (_mode != GIZMO_MODE.TRANSLATE)
            transform.rotation = SelectedObject.rotation;
    }//ResetPositionToSelectedObject

    //Set's the allowable modes for the Gizmo
    //move - Can use move/translate mode
    //rotate - Can use rotate mode
    //scale - Can use scale mode
    public void SetAllowedModes(bool move, bool rotate, bool scale)
    {
        AllowTranslate = move;
        AllowRotate = rotate;
        AllowScale = scale;
    }//SetAllowsModes

    public void ResetTransformations(bool resetPosition)
    {
        if (resetPosition)
        {
            transform.position = Vector3.zero;
        }//if

        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (SelectedObject != null)
        {
            SelectedObject.transform.position = transform.position;
            SelectedObject.transform.rotation = transform.rotation;
            SelectedObject.transform.localScale = Vector3.one;
        }//if

        _XRotationDisplayValue = 0;
        _YRotationDisplayValue = 0;
        _ZRotationDisplayValue = 0;
    }//ResetTransformations

    #region OnGUI
    void OnGUI()
    {
        if (SelectedObject == null)
            return;

        if (_draggingAxis && _mode == GIZMO_MODE.ROTATE)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            screenPos.y = Screen.height - screenPos.y;
            float RotationValue = 0.0f;
            switch (_activeAxis)
            {
                case GIZMO_AXIS.X: RotationValue = _XRotationDisplayValue; break;
                case GIZMO_AXIS.Y: RotationValue = _YRotationDisplayValue; break;
                case GIZMO_AXIS.Z: RotationValue = _ZRotationDisplayValue; break;
            }//switch

            if (Snapping)
                GUI.Label(new Rect(screenPos.x, screenPos.y, 140, 20), "Snap Rotation Amt: " + Math.Round(RotationValue));
            else
                GUI.Label(new Rect(screenPos.x, screenPos.y, 120, 20), "Rotation Amt: " + Math.Round(RotationValue, 1));
        }//if	

        if (_showGizmo && ShowGizmoControlWindow)
        {
            if (AllowTranslate || AllowRotate || AllowScale)
            {
                Rect ControlRect = new Rect(_controlWinPosition.x, _controlWinPosition.y, _controlWinWidth, _controlWinHeight);
                if (ControlRect.Contains(Event.current.mousePosition))
                    GUI.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                else
                    GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.8f);
                DrawGizmoControls(ControlRect);
                GUI.color = Color.white;
            }//if
        }//if
    }//OnGUI

    void DrawGizmoControls(Rect groupRect)
    {
        GUI.Box(groupRect, "Gizmo Control");
        Rect innerRect = new Rect(groupRect.x + 4, groupRect.y + 20, groupRect.width - 8, groupRect.height - 24);
        GUI.BeginGroup(innerRect);

        if (GizmoControlButtonImages.Length != 0)
        {
            GIZMO_MODE tmpMode = (GIZMO_MODE)GUI.Toolbar(new Rect(0, 0, innerRect.width, 20), (int)_mode, GizmoControlButtonImages);
            if (GUI.changed)
            {
                GUI.changed = false;
                if ((AllowTranslate && tmpMode == GIZMO_MODE.TRANSLATE) ||
                    AllowRotate && tmpMode == GIZMO_MODE.ROTATE ||
                    AllowScale && tmpMode == GIZMO_MODE.SCALE)
                    SetMode(tmpMode);
            }//if
        }//if
        else
        {
            int CtrlBtnWidth = (int)innerRect.width / 3;
            int CtrlBtnOffset = 0;
            if (AllowTranslate && GUI.Button(new Rect(CtrlBtnOffset, 0, CtrlBtnWidth, 20), "P")) { SetMode(GIZMO_MODE.TRANSLATE); }
            CtrlBtnOffset += CtrlBtnWidth;

            if (AllowRotate && GUI.Button(new Rect(CtrlBtnOffset, 0, CtrlBtnWidth, 20), "R")) { SetMode(GIZMO_MODE.ROTATE); }
            CtrlBtnOffset += CtrlBtnWidth;

            if (AllowScale && GUI.Button(new Rect(CtrlBtnOffset, 0, CtrlBtnWidth, 20), "S")) { SetMode(GIZMO_MODE.SCALE); }
        }//else

        switch (_mode)
        {
            case GIZMO_MODE.TRANSLATE:
                if (AllowTranslate)
                {
                    Vector3 guiTranslation = new Vector3(AxisSpinner.Draw(new Rect(0.0f, 25.0f, innerRect.width, 20.0f), "X:", transform.position.x),
                                                        AxisSpinner.Draw(new Rect(0, 47, innerRect.width, 20), "Y:", transform.position.y),
                                                        AxisSpinner.Draw(new Rect(0, 69, innerRect.width, 20), "Z:", transform.position.z));
                    transform.position = guiTranslation;
                    SelectedObject.transform.position = transform.position;
                    Snapping = GUI.Toggle(new Rect(0, 94, innerRect.width, 20), Snapping, "Snap Mode");
                    MoveSnapIncrement = SnapSpinner.Draw(new Rect(0, 114, innerRect.width, 20), "Snap By:", MoveSnapIncrement, 1);
                }//if
                else
                {
                    GUI.Label(new Rect(0, 40, innerRect.width, 40), "Disabled");
                }//else
                break;

            case GIZMO_MODE.ROTATE:
                if (AllowRotate)
                {
                    if (_draggingAxis)
                    {
                        AxisSpinner.Draw(new Rect(0, 25, innerRect.width, 20), "X:", transform.rotation.eulerAngles.x);
                        AxisSpinner.Draw(new Rect(0, 47, innerRect.width, 20), "Y:", transform.rotation.eulerAngles.y);
                        AxisSpinner.Draw(new Rect(0, 69, innerRect.width, 20), "Z:", transform.rotation.eulerAngles.z);
                    }//if
                    else
                    {
                        Vector3 guiAngles = new Vector3(AxisSpinner.Draw(new Rect(0, 25, innerRect.width, 20), "X:", transform.rotation.eulerAngles.x),
                                                        AxisSpinner.Draw(new Rect(0, 47, innerRect.width, 20), "Y:", transform.rotation.eulerAngles.y),
                                                        AxisSpinner.Draw(new Rect(0, 69, innerRect.width, 20), "Z:", transform.rotation.eulerAngles.z));
                        transform.rotation = Quaternion.Euler(guiAngles);
                        if (GUI.changed || SelectedObject.transform.rotation != transform.rotation)
                        {
                            GUI.changed = false;
                            SelectedObject.transform.rotation = transform.rotation;
                        }//if
                    }//if

                    Snapping = GUI.Toggle(new Rect(0, 94, innerRect.width, 20), Snapping, "Snap Mode");
                    AngleSnapIncrement = SnapSpinner.Draw(new Rect(0, 114, innerRect.width, 20), "Snap By:", AngleSnapIncrement, 1);
                }//if
                else
                {
                    GUI.Label(new Rect(0, 40, innerRect.width, 40), "Disabled");
                }//else
                break;

            case GIZMO_MODE.SCALE:
                if (AllowScale)
                {
                    Vector3 guiScale = new Vector3(AxisSpinner.Draw(new Rect(0, 25, innerRect.width, 20), "X:", SelectedObject.localScale.x),
                                                    AxisSpinner.Draw(new Rect(0, 47, innerRect.width, 20), "Y:", SelectedObject.localScale.y),
                                                    AxisSpinner.Draw(new Rect(0, 69, innerRect.width, 20), "Z:", SelectedObject.localScale.z));
                    SelectedObject.localScale = guiScale;
                    Snapping = GUI.Toggle(new Rect(0, 94, innerRect.width, 20), Snapping, "Snap Mode");
                    ScaleSnapIncrement = SnapSpinner.Draw(new Rect(0, 114, innerRect.width, 20), "Snap By:", ScaleSnapIncrement, 1);
                }//if
                else
                {
                    GUI.Label(new Rect(0, 40, innerRect.width, 40), "Disabled");
                }//else
                break;
        }//switch

        GUI.EndGroup();
    }//DrawGizmoControls
    #endregion OnGUI
    void SetSelectedAxisObject(Transform axisObject)
    {
        if (_selectedAxis != null)
        {
            if (axisObject == null)
            {
                _selectedAxis.SendMessage("SetAxisColor", GizmoAxisHandleCS.AXIS_COLOR.NORMAL);
            }//if
            else if (_selectedAxis.name != axisObject.name)
            {
                _selectedAxis.SendMessage("SetAxisColor", GizmoAxisHandleCS.AXIS_COLOR.NORMAL);
            }//if
        }//if

        _selectedAxis = axisObject;

        if (_selectedAxis != null)
        {
            _selectedAxis.SendMessage("SetAxisColor", GizmoAxisHandleCS.AXIS_COLOR.HOVER);
        }//if
    }//SetSelectedAxisObject

    public void SetDragHilight(bool SetDrag)
    {
        if (_selectedAxis == null)
            return;

        if (SetDrag)
            _selectedAxis.SendMessage("SetAxisColor", GizmoAxisHandleCS.AXIS_COLOR.DRAG);
        else
            _selectedAxis.SendMessage("SetAxisColor", GizmoAxisHandleCS.AXIS_COLOR.HOVER);
    }//SetDragHihlight

    void Update()
    {
        if (EnableShortcutKeys)
        {
            if (Input.GetKeyUp(TranslateShortcutKey))
            {
                if (AllowTranslate)
                {
                    SetMode(GIZMO_MODE.TRANSLATE);
                    translateToggle.isOn = true;
                }
            }//if

            if (Input.GetKeyUp(RotateShortcutKey))
            {
                if (AllowRotate)
                {
                    SetMode(GIZMO_MODE.ROTATE);
                    rotateToggle.isOn = true;
                }
            }//if

            if (Input.GetKeyUp(ScaleShortcutKey))
            {
                if (AllowScale)
                {
                    SetMode(GIZMO_MODE.SCALE);
                    scaleToggle.isOn = true;
                }
            }//if
            // disabled snapping cause UI does not support it
            //if (Input.GetKeyUp(SnapShortcutKey))
            //{
            //    ToggleSnapping();
            //}//if
        }//if

        if (!_showGizmo || SelectedObject == null || GameController.IsConnected == false)
            return;

        AxisSpinner.Update();
        SnapSpinner.Update();

        //Scale Gizmo relative to the the distance from the camera for consistant sizing
        float distance = Mathf.Abs(Vector3.Distance(Camera.main.transform.position, transform.position));
        transform.localScale = Vector3.one * gizmoDefaultScale * (distance / 8.0f);

        if (UIController.OverUI())
            return;

        int layerMask = 1 << LayerID;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            int validHitMask = 1 << LayerID | 1 << InteractableLayer;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, validHitMask) == false && _showGizmo)
            {
                SelectedObject.GetComponent<InteractableObject>().DoClearSelection();
                Hide();
            }
        }
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) && !_ignoreRaycast)
        {
            if (!_draggingAxis)
            {   
                if (hit.transform.name.Contains("X"))
                    _activeAxis = GIZMO_AXIS.X;

                if (hit.transform.name.Contains("Y"))
                    _activeAxis = GIZMO_AXIS.Y;

                if (hit.transform.name.Contains("Z"))
                    _activeAxis = GIZMO_AXIS.Z;

                SetSelectedAxisObject(hit.transform);
            }//if

            if (Input.GetMouseButtonUp(0))
            {
                _activeAxis = GIZMO_AXIS.NONE;
                _lastIntersectPosition = Vector3.zero;
                _draggingAxis = false;
                SetDragHilight(false);
            }//if		
        }//if
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Set this to true if Mouse button was down so we aren't handling gizmo transformations when draging the mouse
                //over the gizmo when we don't mean to be interacting with it
                _ignoreRaycast = true;
                _activeAxis = GIZMO_AXIS.NONE;
            }//if

            if (!_draggingAxis)
                SetSelectedAxisObject(null);
        }//else

        if (Input.GetMouseButtonUp(0))
        {
            _ignoreRaycast = false;
        }//if	

        if ((_activeAxis != GIZMO_AXIS.NONE && Input.GetMouseButtonUp(0)) || (Input.GetMouseButtonDown(0) && !_draggingAxis))
        {
            _activeAxis = GIZMO_AXIS.NONE;
            _lastIntersectPosition = Vector3.zero;
            _currIntersectPosition = _lastIntersectPosition;
            _rotationSnapDelta = 0;
            _scaleSnapDelta = Vector3.zero;
            _moveSnapDelta = Vector3.zero;
            _XRotationDisplayValue = 0;
            _YRotationDisplayValue = 0;
            _ZRotationDisplayValue = 0;
            _draggingAxis = false;
            SetDragHilight(false);
        }//if

        if (_activeAxis != GIZMO_AXIS.NONE && Input.GetMouseButton(0))
        {
            Vector3 objPos = transform.position;
            Vector3 objMovement = Vector3.zero;
            Plane plane;
            float hitDistance = 0.0f;
            float MouseXDelta = Input.GetAxis("Mouse X");
            float MouseYDelta = Input.GetAxis("Mouse Y");
            float snapValue;
            if(!_draggingAxis)
            {
                _draggingAxis = true;
                SetDragHilight(true);

                initialScale = SelectedObject.transform.localScale;
                Vector3 referenceVec = (_activeAxis == GIZMO_AXIS.X) ? transform.right : (_activeAxis == GIZMO_AXIS.Y ? transform.up : transform.forward);
                Vector2 vecScreenSpace = (Camera.main.worldToCameraMatrix * referenceVec).normalized;
                Vector3 pivot = Camera.main.WorldToScreenPoint(transform.position);
                pivot.z = 0;
                _currIntersectPosition = Input.mousePosition;
                _currIntersectPosition.z = 0;
                _translationDelta = Vector3.Project(_currIntersectPosition - pivot, vecScreenSpace);

                axisLength = _translationDelta.magnitude;
            }

            _currIntersectPosition = _lastIntersectPosition;
            switch (_activeAxis)
            {
                case GIZMO_AXIS.X:
                    switch (_mode)
                    {
                        case GIZMO_MODE.TRANSLATE:
                            #region X_Translate
                            //Vector3 xPlaneVec = new Vector3(0.0f, Camera.main.transform.position.y - transform.position.y, Camera.main.transform.position.z - transform.position.z);
                            //xPlaneVec.Normalize();
                            plane = new Plane(Vector3.forward, transform.position);
                            hitDistance = 0;
                            if (plane.Raycast(ray, out hitDistance))
                            {
                                _currIntersectPosition = GetIntersectPoint(hitDistance, ray);

                                if (_lastIntersectPosition != Vector3.zero)
                                {
                                    _translationDelta = _currIntersectPosition - _lastIntersectPosition;
                                    if (Snapping && MoveSnapIncrement > 0)
                                    {
                                        _moveSnapDelta.x += _translationDelta.x;
                                        snapValue = Mathf.Round(_moveSnapDelta.x / MoveSnapIncrement) * MoveSnapIncrement;
                                        _moveSnapDelta.x -= snapValue;

                                        objMovement = (TranslateInLocalSpace ? transform.right : Vector3.right) * snapValue;

                                        objPos += objMovement;
                                    }//if
                                    else
                                    {
                                        objMovement = (TranslateInLocalSpace ? transform.right : Vector3.right) * _translationDelta.x;
                                        objPos += objMovement;
                                    }//else

                                    transform.position = objPos;
                                    SelectedObject.transform.position = transform.position;
                                }//if						
                            }//if					

                            _lastIntersectPosition = _currIntersectPosition;
                            break;
                        #endregion X_Translate
                        case GIZMO_MODE.ROTATE:
                            #region X_Rotate
                            var rotXDelta = MouseXDelta * Time.deltaTime;

                            if (Snapping && AngleSnapIncrement > 0)
                            {
                                rotXDelta *= SnappedRotationSpeed;
                                _rotationSnapDelta += rotXDelta;

                                snapValue = Mathf.Round(_rotationSnapDelta / AngleSnapIncrement) * AngleSnapIncrement;
                                _rotationSnapDelta -= snapValue;

                                _XRotationDisplayValue += snapValue;
                                transform.rotation *= Quaternion.AngleAxis(snapValue, Vector3.right);
                                SelectedObject.transform.rotation = transform.rotation;
                            }//if
                            else
                            {
                                rotXDelta *= RotationSpeed;
                                _XRotationDisplayValue += rotXDelta;
                                transform.Rotate(Vector3.right * rotXDelta);
                                SelectedObject.transform.rotation = transform.rotation;
                            }//else


                            _XRotationDisplayValue = ClampRotationAngle((float)_XRotationDisplayValue);
                            break;
                        #endregion X_Rotate
                        case GIZMO_MODE.SCALE:
                            #region X_Scale
                            _currIntersectPosition = Input.mousePosition;
                            _currIntersectPosition.z = 0;

                            Vector2 rightScreenSpace = (Camera.main.worldToCameraMatrix * transform.right).normalized;
                            Vector3 pivot = Camera.main.WorldToScreenPoint(transform.position);
                            pivot.z = 0;
                            _translationDelta = Vector3.Project(_currIntersectPosition - pivot, rightScreenSpace);
                            float sign = Mathf.Sign(Vector3.Dot(rightScreenSpace, _translationDelta.normalized));

                            if (_translationDelta != Vector3.zero)
                            {
                                if (Snapping && ScaleSnapIncrement > 0)
                                {
                                    float scaleSnapIncrementPixels = ScaleSnapIncrement * axisLength / initialScale.x;

                                    snapValue = Mathf.Round((_translationDelta.magnitude * sign - axisLength) / scaleSnapIncrementPixels) * ScaleSnapIncrement;

                                    objMovement = Vector3.right * snapValue;
                                    SelectedObject.transform.localScale = initialScale + objMovement;
                                }
                                else
                                {
                                    objMovement = Vector3.right * (_translationDelta.magnitude / axisLength * initialScale.x) * sign;
                                    Vector3 baseScale = new Vector3(0f, SelectedObject.transform.localScale.y, SelectedObject.transform.localScale.z);
                                    SelectedObject.transform.localScale = baseScale + objMovement;
                                }
                            }					

                            break;
                        #endregion X_Scale
                    }//switch
                    break;

                case GIZMO_AXIS.Y:
                    switch (_mode)
                    {
                        case GIZMO_MODE.TRANSLATE:
                            #region Y_Translate
                            plane = new Plane(Vector3.forward, transform.position);
                            hitDistance = 0;

                            if (plane.Raycast(ray, out hitDistance))
                            {

                                _currIntersectPosition = GetIntersectPoint(hitDistance, ray);

                                if (_lastIntersectPosition != Vector3.zero)
                                {
                                    _translationDelta = _currIntersectPosition - _lastIntersectPosition;
                                    if (Snapping && MoveSnapIncrement > 0)
                                    {
                                        _moveSnapDelta.y += _translationDelta.y;
                                        snapValue = Mathf.Round(_moveSnapDelta.y / MoveSnapIncrement) * MoveSnapIncrement;
                                        _moveSnapDelta.y -= snapValue;

                                        //objMovement = Vector3.up * snapValue;			
                                        objMovement = (TranslateInLocalSpace ? transform.up : Vector3.up) * snapValue;
                                        objPos += objMovement;
                                    }//if
                                    else
                                    {
                                        //objMovement = Vector3.up * _translationDelta.y;
                                        objMovement = (TranslateInLocalSpace ? transform.up : Vector3.up) * _translationDelta.y;
                                        objPos += objMovement;
                                    }//else

                                    transform.position = objPos;
                                    SelectedObject.transform.position = transform.position;
                                }//if							
                            }//if						

                            _lastIntersectPosition = _currIntersectPosition;
                            break;
                        #endregion Y_Translate
                        case GIZMO_MODE.ROTATE:
                            #region Y_Rotate
                            var rotYDelta = MouseXDelta * Time.deltaTime;

                            if (Snapping && AngleSnapIncrement > 0)
                            {
                                rotYDelta *= SnappedRotationSpeed;
                                _rotationSnapDelta += rotYDelta;

                                snapValue = Mathf.Round(_rotationSnapDelta / AngleSnapIncrement) * AngleSnapIncrement;
                                _rotationSnapDelta -= snapValue;

                                _YRotationDisplayValue += snapValue;
                                transform.rotation *= Quaternion.AngleAxis(snapValue, Vector3.up);
                                SelectedObject.transform.rotation = transform.rotation;
                            }//if
                            else
                            {
                                rotYDelta *= RotationSpeed;
                                _YRotationDisplayValue += rotYDelta;
                                transform.Rotate(Vector3.up * rotYDelta);
                                SelectedObject.transform.rotation = transform.rotation;
                            }//else

                            _YRotationDisplayValue = ClampRotationAngle((float)_YRotationDisplayValue);
                            break;
                            #endregion Y_Rotate
                        case GIZMO_MODE.SCALE:
                            #region Y_Scale
                            _currIntersectPosition = Input.mousePosition;
                            _currIntersectPosition.z = 0;
                            if(_lastIntersectPosition != Vector3.zero)
                            {
                                Vector2 upScreenSpace = (Camera.main.worldToCameraMatrix * transform.up).normalized;

                                _translationDelta = Vector3.Project(_currIntersectPosition - Camera.main.WorldToScreenPoint(transform.position), upScreenSpace);
                                float sign = Mathf.Sign(Vector3.Dot(upScreenSpace, _translationDelta.normalized));
                                if (_translationDelta != Vector3.zero)
                                {
                                    if (Snapping && ScaleSnapIncrement > 0)
                                    {
                                        float scaleSnapIncrementPixels = ScaleSnapIncrement * axisLength / initialScale.y;

                                        snapValue = Mathf.Round((_translationDelta.magnitude * sign - axisLength) / scaleSnapIncrementPixels) * ScaleSnapIncrement;

                                        objMovement = Vector3.up * snapValue;
                                        SelectedObject.transform.localScale = initialScale + objMovement;
                                    }
                                    else
                                    {
                                        objMovement = Vector3.up * (_translationDelta.magnitude / axisLength * initialScale.y) * sign;
                                        Vector3 baseScale = new Vector3(SelectedObject.transform.localScale.x, 0f, SelectedObject.transform.localScale.z);
                                        SelectedObject.transform.localScale = baseScale + objMovement;
                                    }
                                }					
                            }	
                            _lastIntersectPosition = _currIntersectPosition;
                            break;
                            #endregion Y_Scale
                    }//switch
                    break;

                case GIZMO_AXIS.Z:
                    switch (_mode)
                    {
                        case GIZMO_MODE.TRANSLATE:
                            #region Z_Translate
                            plane = new Plane(Vector3.up, transform.position);
                            hitDistance = 0;

                            if (plane.Raycast(ray, out hitDistance))
                            {

                                _currIntersectPosition = GetIntersectPoint(hitDistance, ray);

                                if (_lastIntersectPosition != Vector3.zero)
                                {
                                    _translationDelta = _currIntersectPosition - _lastIntersectPosition;
                                    if (Snapping && MoveSnapIncrement > 0)
                                    {
                                        _moveSnapDelta.z += _translationDelta.z;
                                        snapValue = Mathf.Round(_moveSnapDelta.z / MoveSnapIncrement) * MoveSnapIncrement;
                                        _moveSnapDelta.z -= snapValue;

                                        //objMovement = Vector3.forward * snapValue;			
                                        objMovement = (TranslateInLocalSpace ? transform.forward : Vector3.forward) * snapValue;
                                        objPos += objMovement;
                                    }//if
                                    else
                                    {
                                        //objMovement = Vector3.forward * _translationDelta.z;
                                        objMovement = (TranslateInLocalSpace ? transform.forward : Vector3.forward) * _translationDelta.z;
                                        objPos += objMovement;
                                    }//else

                                    transform.position = objPos;
                                    SelectedObject.transform.position = transform.position;
                                }//if							
                            }//if						

                            _lastIntersectPosition = _currIntersectPosition;
                            break;
                            #endregion Z_Translate
                        case GIZMO_MODE.ROTATE:
                            #region Z_Rotate
                            var rotZDelta = MouseXDelta * Time.deltaTime;

                            if (Snapping && AngleSnapIncrement > 0)
                            {
                                rotZDelta *= SnappedRotationSpeed;
                                _rotationSnapDelta += rotZDelta;

                                snapValue = Mathf.Round(_rotationSnapDelta / AngleSnapIncrement) * AngleSnapIncrement;
                                _rotationSnapDelta -= snapValue;

                                _ZRotationDisplayValue += snapValue;
                                transform.rotation *= Quaternion.AngleAxis(snapValue, Vector3.forward);
                                SelectedObject.transform.rotation = transform.rotation;
                            }//if
                            else
                            {
                                rotZDelta *= RotationSpeed;
                                _ZRotationDisplayValue += rotZDelta;

                                transform.Rotate(Vector3.forward * rotZDelta);
                                SelectedObject.transform.rotation = transform.rotation;
                            }//else

                            _ZRotationDisplayValue = ClampRotationAngle((float)_ZRotationDisplayValue);
                            break;
                            #endregion Z_Rotate
                        case GIZMO_MODE.SCALE:
                            #region Z_Scale
                            _currIntersectPosition = Input.mousePosition;
                            _currIntersectPosition.z = 0;
                            if(_lastIntersectPosition != Vector3.zero)
                            {
                                Vector2 forwardScreenSpace = (Camera.main.worldToCameraMatrix * transform.forward).normalized;

                                _translationDelta = Vector3.Project(_currIntersectPosition - Camera.main.WorldToScreenPoint(transform.position), forwardScreenSpace);
                                float sign = Mathf.Sign(Vector3.Dot(forwardScreenSpace, _translationDelta.normalized));
                                if (_translationDelta != Vector3.zero)
                                {
                                    if (Snapping && ScaleSnapIncrement > 0)
                                    {
                                        float scaleSnapIncrementPixels = ScaleSnapIncrement * axisLength / initialScale.z;

                                        snapValue = Mathf.Round((_translationDelta.magnitude * sign - axisLength) / scaleSnapIncrementPixels) * ScaleSnapIncrement;

                                        objMovement = Vector3.forward * snapValue;
                                        SelectedObject.transform.localScale = initialScale + objMovement;
                                    }
                                    else
                                    {
                                        objMovement = Vector3.forward * (_translationDelta.magnitude / axisLength * initialScale.z) * sign;
                                        Vector3 baseScale = new Vector3(SelectedObject.transform.localScale.x, SelectedObject.transform.localScale.y, 0f);
                                        SelectedObject.transform.localScale = baseScale + objMovement;
                                    }
                                }					
                            }	
                            _lastIntersectPosition = _currIntersectPosition;
                            break;
                            #endregion Z_Scale
                    }//switch
                    break;
            }//switch
        }//if	
    }//Update

    //Sets the currently selected object transform
    //Pass in GameObject Transform
    public void SetSelectedObject(Transform ObjectTransform)
    {
        if (ObjectTransform == null)
            return;

        transform.position = ObjectTransform.transform.position;
        SelectedObject = ObjectTransform;

        SetMode(_mode);

        _XRotationDisplayValue = 0;
        _YRotationDisplayValue = 0;
        _ZRotationDisplayValue = 0;
    }//ObjectTransform

    //Set's the active GIZMO_MODE
    //GIZMO_MODE.TRANSLATE, GIZMO_MODE.ROTATE, GIZMO_MODE.SCALE
    public void SetMode(GIZMO_MODE mode)
    {
        _mode = mode;

        if (SelectedObject == null) return;

        switch (_mode)
        {
            case GIZMO_MODE.TRANSLATE:
                transform.rotation = TranslateInLocalSpace ? SelectedObject.transform.rotation : Quaternion.identity;
                break;

            case GIZMO_MODE.ROTATE:
                transform.rotation = SelectedObject.transform.rotation;
                break;

            case GIZMO_MODE.SCALE:
                transform.rotation = SelectedObject.transform.rotation;
                break;
        }//switch

        foreach (Transform t in transform)
        {
            if (t.name.Contains("Pivot"))
                continue;

            switch (_mode)
            {
                case GIZMO_MODE.TRANSLATE:
                    if (t.name.Contains("Translate") || t.name.Contains("XY") || t.name.Contains("XZ") || t.name.Contains("YZ"))
                    {
                        t.GetComponent<Renderer>().enabled = true;
                        t.gameObject.layer = LayerID;
                    }//if
                    else
                    {
                        t.GetComponent<Renderer>().enabled = false;
                        t.gameObject.layer = 2;
                    }//else
                    break;

                case GIZMO_MODE.ROTATE:
                    if (t.name.Contains("Rotate"))
                    {
                        t.GetComponent<Renderer>().enabled = true;
                        t.gameObject.layer = LayerID;
                    }//if
                    else
                    {
                        t.GetComponent<Renderer>().enabled = false;
                        t.gameObject.layer = 2;
                    }//else
                    break;

                case GIZMO_MODE.SCALE:
                    if (t.name.Contains("Scale") || t.name.Contains("XY") || t.name.Contains("XZ") || t.name.Contains("YZ"))
                    {
                        t.GetComponent<Renderer>().enabled = true;
                        t.gameObject.layer = LayerID;
                    }//if
                    else
                    {
                        t.GetComponent<Renderer>().enabled = false;
                        t.gameObject.layer = 2;
                    }//else
                    break;
            }//switch
        }//for
    }//SetMode

    float ClampRotationAngle(float value)
    {
        if (value > 360) { value -= 360; }
        if (value < 0) { value += 360; }

        return value;
    }//ClampRotation

    Vector3 GetIntersectPoint(float HitDistance, Ray ray)
    {
        var ReturnPoint = Vector3.zero;

        if (Camera.main.orthographic)
            ReturnPoint = ray.origin;
        else
            ReturnPoint = ray.direction * HitDistance;

        return ReturnPoint;
    }//GetintersectPoint
}
