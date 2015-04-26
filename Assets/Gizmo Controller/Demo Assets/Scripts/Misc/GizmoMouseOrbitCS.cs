using UnityEngine;
using System.Collections;

public class GizmoMouseOrbitCS : MonoBehaviour {
	public Transform target;
	public GizmoControllerCS GC;
	public float distance = 6.0f;
	public float moveSpeed = 500.0f;
	
	public float xSpeed = 250.0f;
	public float ySpeed = 120.0f;
	
	public float yMinLimit = -20.0f;
	public float yMaxLimit = 88.0f;
	
	public float zoomSpeed = 10.0f;
	public float zoomNearLimit = 1.0f;
	public float zoomFarLimit = 200.0f;
	
	private float x = 0.0f;
	private float y = 0.0f;
	private float z = 0.0f;
	
	private float moveMultiplier = 0.2f;
	
	private bool ignoreHotControl = false;
	private bool isEnabled = true;
	
	private bool downButtonPressed = false;
	private bool leftButtonPressed = false;
	private bool rightButtonPressed = false;
	private bool upButtonPressed = false;

	// Use this for initialization
	void Start () {
		resetCamera();
	}
	
	void resetCamera(){
		if(target){
			distance = 6;
			x = 180;
			y = 15;
			
			target.transform.position = Vector3.zero;
			
			Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
			Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
			
			transform.rotation = rotation;
			transform.position = position;
		}
	}//resetCamera
	
	void setEnabled(bool enabled){
		isEnabled = enabled;
	}//setEnabled
	
	void zoom(float val){
		distance += val * Time.deltaTime;
		distance = Mathf.Clamp(distance, zoomNearLimit, zoomFarLimit);
		ignoreHotControl = true;
	}//zoom
	
	void rotate(float val){
		x += (val* Time.deltaTime) * xSpeed * 0.02f;
		ignoreHotControl = true;
	}//rotate
	
	void move(string dir){
		switch(dir){
		case "up":
			upButtonPressed = true;
			break;
			
		case "down":
			downButtonPressed = true;
			break;
			
		case "left":
			leftButtonPressed = true;
			break;
			
		case "right":
			rightButtonPressed = true;
			break;
		}//switch
		
		ignoreHotControl = true;
	}//move
	
	void LateUpdate () {
		//Here we check if the Gizmo Controller is assigned.
		//If it is and the user is currently dragging an axis handle
		//then we don't want to be moving the camera around.
		if(GC != null){
			if(GC.IsDraggingAxis()){
				return;
			}
		}

		if(GUIUtility.hotControl != 0 && !ignoreHotControl)
			return;
		
		if(!isEnabled)
			return;
		
		if (target) {
			
			/*Rotation*/
			
			if(target != null && Input.GetAxis("Mouse ScrollWheel") != 0){
				distance += Input.GetAxis("Mouse ScrollWheel");
				distance = Mathf.Clamp(distance, zoomNearLimit, zoomFarLimit);
			}//if    	
			
			if(Input.GetAxis("Fire1") != 0){
				x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
				y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
				
				y = ClampAngle(y, yMinLimit, yMaxLimit);
			}//if
			
			Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
			Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
			
			transform.rotation = rotation;
			transform.position = position;			
			
			/*Movement*/
			Vector3 dir;
			Vector3 move;
			Vector3 targetPos = target.transform.position;
			Vector3 pos = transform.position;
			if(Input.GetKey("up") || upButtonPressed){
				dir = transform.position - targetPos;				
				move = (dir.normalized) * moveSpeed * Time.deltaTime;
				
				
				if(Input.GetKey(KeyCode.LeftShift)){
					pos.x -= move.x*moveMultiplier;
					pos.z -= move.z*moveMultiplier;
					
					targetPos.x -= move.x*moveMultiplier;
					targetPos.z -= move.z*moveMultiplier;
				}
				else{
					pos.x -= move.x;
					pos.z -= move.z;
					targetPos.x -= move.x;
					targetPos.z -= move.z;
				}
				
				if(upButtonPressed)
					upButtonPressed = false;
			}
			
			if(Input.GetKey("down") || downButtonPressed){
				//Debug.Log("Moving Camera");
				dir = transform.position - target.transform.position;
				
				move = (dir.normalized) * moveSpeed * Time.deltaTime;
				
				
				if(Input.GetKey(KeyCode.LeftShift)){
					pos.x += move.x*moveMultiplier;
					pos.z += move.z*moveMultiplier;
					
					targetPos.x += move.x*moveMultiplier;
					targetPos.z += move.z*moveMultiplier;
				}
				else{
					pos.x += move.x;
					pos.z += move.z;
					targetPos.x += move.x;
					targetPos.z += move.z;
				}
				
				if(downButtonPressed)
					downButtonPressed = false;
			}
			
			if(Input.GetKey("left") || leftButtonPressed){
				//Debug.Log("Moving Camera");
				dir = transform.position - target.transform.position;				
				move = -(transform.right) * moveSpeed * Time.deltaTime;
				
				if(Input.GetKey(KeyCode.LeftShift)){
					pos.x += move.x*moveMultiplier;
					pos.z += move.z*moveMultiplier;
					
					targetPos.x += move.x*moveMultiplier;
					targetPos.z += move.z*moveMultiplier;
				}
				else{
					pos.x += move.x;
					pos.z += move.z;
					targetPos.x += move.x;
					targetPos.z += move.z;
				}
				
				if(leftButtonPressed)
					leftButtonPressed = false;
				
			}
			
			if(Input.GetKey("right") || rightButtonPressed){
				//Debug.Log("Moving Camera");
				dir = transform.position - target.transform.position;
				move = transform.right * moveSpeed * Time.deltaTime;
				
				
				if(Input.GetKey(KeyCode.LeftShift)){
					pos.x += move.x*moveMultiplier;
					pos.z += move.z*moveMultiplier;
					
					targetPos.x += move.x*moveMultiplier;
					targetPos.z += move.z*moveMultiplier;
				}
				else{
					pos.x += move.x;
					pos.z += move.z;
					targetPos.x += move.x;
					targetPos.z += move.z;
				}
				
				if(rightButtonPressed)
					rightButtonPressed = false;
			}
			
			if(Input.GetMouseButton(1)){
				Vector3 hMove = transform.right * (moveSpeed*2.0f) * Time.deltaTime;
				Vector3 vMove = transform.up *  (moveSpeed*2.0f) * Time.deltaTime;
				
				pos.x += hMove.x*Input.GetAxis("Mouse X");
				pos.z += hMove.z*Input.GetAxis("Mouse X");
				
				pos.y += vMove.y*Input.GetAxis("Mouse Y");
				
				targetPos.x += hMove.x*Input.GetAxis("Mouse X");
				targetPos.z += hMove.z*Input.GetAxis("Mouse X");
				
				targetPos.y += vMove.y*Input.GetAxis("Mouse Y");
				
			}

			target.transform.position = targetPos;
			transform.position = pos;
			
			
		}//if
		
		if(ignoreHotControl)
			ignoreHotControl = false;
	}//LateUpdate

	public static float ClampAngle (float angle, float min, float max) {
		if (angle < -360.0f)
			angle += 360.0f;
		if (angle > 360.0f)
			angle -= 360.0f;

		return Mathf.Clamp (angle, min, max);
	}//ClampAngle

}