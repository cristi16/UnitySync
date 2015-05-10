#pragma strict

var rotationSpeed : float = 90.0f;


function Update () {
transform.Rotate (0, rotationSpeed * Time.deltaTime, 0);
}