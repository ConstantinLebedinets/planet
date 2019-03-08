using UnityEngine;
using System.Collections;

public class MeshPlanet : MonoBehaviour {
	GameObject cam;
	Mesh mesh;
	Vector3 camPosition;
	Vector3[] vert;
	Vector3[] vertOld;
	public float radius =5.0f;
	float coordCorrection;
	int width = 200;
	Vector3 pos;
	//Vector3[] normals;
	Vector2[] uv;	
	int[] trians;
	Vector3 curAxis = new Vector3(0,1,0);
	//HeightMapStore one;
//	MagicData two;
	Quaternion magic;
	
	float camDistance;	
	GameObject ocean, cloudSystem, cloud, trees;
	Vector3 camPositionOfRenderedWorld;
	
	//**********************************
	//какой то глюк с позицией планеты, если не в 0.0 то возвращает непонятно что
	//****************************************
	
	private void setVertices(){

		camPosition = worldToLocal(cam.transform.position);// позиция камеры		
		magic = Quaternion.FromToRotation(curAxis,camPosition);
		//определяю магию для плавного нарастания детализации
		camDistance = Vector3.Magnitude(camPosition);//   Mathf.Sqrt(camPosition.x*camPosition.x+camPosition.y*camPosition.y+camPosition.z*camPosition.z);	
		float min = 0.8f;
		float max = 2.5f;
		
		if(camDistance>=1.75f*radius){
			coordCorrection = max;
		}
		else{
			if(camDistance<=radius){
				coordCorrection = min;
			}
			else{			
				float bebe = Mathf.Pow( (camDistance/radius - 0.75f),0.9f);			
				coordCorrection = Mathf.Lerp(min,max,bebe);		
			}
		}

			//ставлю точки	
		
		float texCoorScale = 200f;
		for(int i = 0; i < vertOld.Length; i++){			
			//есть сохраненная позиция в меше которую легко повернуть к камере
			Vector3 turnedPos = vertOld[i];
			turnedPos.x *=coordCorrection;
			turnedPos.z *=coordCorrection;
			
			turnedPos = magic * turnedPos;
			turnedPos.Normalize();
			float height = MagicData.Height(turnedPos);
			vert[i] = turnedPos*radius* height;
				Vector3 pos = turnedPos;
					float xCoor = Mathf.Abs(pos.x);
					float yCoor = Mathf.Abs(pos.y);
					float zCoor = Mathf.Abs(pos.z);
					if(xCoor>=yCoor && xCoor>=zCoor){uv[i] = new Vector2(pos.y/pos.x,pos.z/pos.x);}
					if(yCoor>xCoor && yCoor>=zCoor){uv[i] = new Vector2(pos.x/pos.y,pos.z/pos.y);}
					if(zCoor>yCoor && zCoor>xCoor){uv[i] = new Vector2(pos.y/pos.z,pos.x/pos.z);}
			uv[i]*=texCoorScale;
		}
		

	}	
	
	
	// Use this for initialization
	void Start () {
		
		cam = GameObject.Find("Camera");
		//gameObject.transform.eulerAngles = new Vector3(0,0,0);
		mesh = gameObject.GetComponent<MeshFilter>().mesh;
		
		//сохраняю старые позиции точек

		vertOld = mesh.vertices;
		
		//подымаю от нуля позиции точек до 1
		for(int i = 0; i < vertOld.Length; i++)vertOld[i]+=new Vector3(0,1,0);	
		
		vert = new Vector3[vertOld.Length];
		uv = new Vector2[vertOld.Length];
		Noise3D.init(125);		
		
		MagicData.Init();

		
		CreateOcean();
		CreateClouds();
		CreateTrees();

		
	}
	
	//********************************************************
	// trees
	//********************************************************
	void CreateTrees(){
		
		trees = new GameObject("TreeSystem");
		trees.transform.position = gameObject.transform.position;
		trees.transform.parent  = gameObject.transform;
		trees.AddComponent<TreeSystem>();
		trees.GetComponent<TreeSystem>().radius = radius;
		trees.GetComponent<TreeSystem>().planet = gameObject;
		trees.GetComponent<TreeSystem>().cam = cam;
	}
	//********************************************************
	// clouds
	//********************************************************	
	void CreateClouds(){
		cloudSystem = new GameObject("CloudSystem");
		cloudSystem.transform.position = gameObject.transform.position;
		cloudSystem.transform.parent = gameObject.transform;
		cloudSystem.AddComponent<CloudSystem>();
		cloudSystem.GetComponent<CloudSystem>().planet = gameObject;
		cloudSystem.GetComponent<CloudSystem>().radius = radius;		
	}
	//********************************************************
	// ocean
	//********************************************************		
	void CreateOcean(){
		//Создание океана
		ocean = new GameObject("Ocean");
		ocean.AddComponent<MeshFilter>();
		ocean.AddComponent<MeshRenderer>();
		ocean.AddComponent<Ocean>();
		ocean.GetComponent<Ocean>().radius = radius;
		ocean.transform.position = gameObject.transform.position;
		ocean.transform.parent = gameObject.transform;
		ocean.GetComponent<MeshRenderer>().material = GameObject.Find("WaterMat").GetComponent<MeshRenderer>().material;

	}
	
	
	// Update is called once per frame
	void Update () {
				
		//обновить меши если камера сместилась более чем на 5 единиц чего то там
		Vector3 currentCamPosition = worldToLocal(cam.transform.position);
		float distance = Vector3.Magnitude(currentCamPosition - camPositionOfRenderedWorld);
		if(distance >= 0.03f * radius) {		
			camPositionOfRenderedWorld = currentCamPosition;
			setVertices();	      
			//апдейт поверхности	
			mesh.vertices = vert;
			mesh.uv = uv;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}
		
		
		//вращение планеты
		Vector3 angle = gameObject.transform.eulerAngles;
		angle.y+=0.01f;
		gameObject.transform.eulerAngles = angle;
		//camHandle();
		
	}
	
	
	public Vector3 worldToLocal(Vector3 vec){
		return	transform.worldToLocalMatrix.MultiplyPoint(vec);
	}
	public Vector3 localToWorld(Vector3 vec){
		return	transform.localToWorldMatrix.MultiplyPoint(vec);
	}	
	void OnGUI(){
		
		/*
		GUI.TextArea(new Rect(10,5,99,20) , " time: " + Time.time.ToString());
		
		GUI.TextArea(new Rect(10,25,230,20) , " Cam position: " + camPosition.ToString());
		float heightAtThisPoint = two.Height(camPosition);
		Vector3 posit = camPosition;
		posit.Normalize();
		posit = posit * heightAtThisPoint * radius;
		GUI.TextArea(new Rect(10,45,230,20) , " Height at: " + posit.ToString());
		GUI.TextArea(new Rect(10,65,230,20) , " dist: " + camPosition.magnitude.ToString());
		GUI.TextArea(new Rect(10,85,230,20) , " height: " + posit.magnitude.ToString());*/
	}

	
}
