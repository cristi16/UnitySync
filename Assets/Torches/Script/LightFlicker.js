
var minFlickerSpeed : float = 0.3;
var maxFlickerSpeed : float = 0.01;
var minLightIntensity : float = 0.0;
var maxLightIntensity : float = 0.0;

private var Light_Temp = 0.3;
private var randomizer : float = 0.0;


public var waiting : boolean = false;

private var randomPosition : Vector3;

var moveDelay : float = 1;

var minX : float = 0;
var maxX : float = 0;
var minY : float = 0;
var maxY : float = 0;
var minZ : float = 0;
var maxZ : float = 0;


function Awake(){
initiate_intensity();
}

function Update () 
{
    if (waiting == false)
    {
        SendMessage("offsetPos");
    }
        else
        {
        GetComponent.<Light>().transform.localPosition = Vector3.Slerp (transform.localPosition, randomPosition, Time.deltaTime*1);
        }
}



function initiate_intensity(){
flicker();
}



function flicker(){
randomizer = Random.Range(minLightIntensity, maxLightIntensity);
r = Random.Range(minFlickerSpeed, maxFlickerSpeed);
////////////////////////////////////////  lerp yield

var temp = r;
var t : float = 0;
	while(t < 1.0){
	t += Time.deltaTime / temp;
	GetComponent.<Light>().intensity = Mathf.Lerp(Light_Temp, randomizer,t);
	yield;
	}	
///////////////////////////////////////
yield WaitForSeconds (r);
Light_Temp = randomizer;

initiate_intensity();

}
 

function offsetPos()
{
    randomPosition = Vector3 (Random.Range( minX,maxX ), Random.Range( minY, maxY ),Random.Range( minZ, maxZ ));
    waiting = true;
    yield WaitForSeconds (moveDelay);
    waiting = false;



}

