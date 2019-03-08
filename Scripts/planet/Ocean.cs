using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ocean : MonoBehaviour {

	
	public int numOctave = 8;
	public float radius =5.0f;
	float coordCorrection;
	int width = 32;
	Vector3 pos;
	Vector3[] vert;
	Vector3[] normals;
	Vector2[] uv;	
	int[] trians;
	Mesh mesh;
	GameObject cam;
	Vector3 curAxis = new Vector3(0,1,0);
//	public MagicData two;
	Vector3 camPositionOfRenderedWorld;	
	
	public Vector3 worldToLocal(Vector3 vec){
		return	transform.worldToLocalMatrix.MultiplyPoint(vec);
	}
	
	
	private void setVertices(){

		Vector3 axis = worldToLocal(cam.transform.position);// позиция камеры
		
		Quaternion magic = Quaternion.FromToRotation(curAxis,axis);
		//определяю магию для плавного нарастания детализации
		float distance = Mathf.Sqrt(axis.x*axis.x+axis.y*axis.y+axis.z*axis.z);	
		float min = 0.004f;
		float max = 0.06f;
		
		if(distance>=2.0f*radius){
			coordCorrection = max;
		}
		else{
			if(distance<=radius){
				coordCorrection = min;
			}
			else{
				float bebe = Mathf.Pow( (distance/radius - 1),0.9f);
				
				coordCorrection = bebe*max + (1-bebe)*min;
				
			}
		}

			//ставлю точки		
			float cosin = 0.86602540378443864676372317075294f;
			float coorCorecX = cosin * coordCorrection;
			float coorCorecY = 0.5f * coordCorrection;
		
			float waveScale = 0.0002f;
			for(int i = 0; i<=width; i++){
				for( int j = 0; j<=width; j++){				
					float xCoor = (i-j) * coorCorecX;
					float zCoor = (i+j-width) * coorCorecY;
					float yCoor = 0;
					pos.Set(xCoor,yCoor+1,zCoor);
				
					
					pos =  magic * pos * 1000 ;
					float xWave = Mathf.Sin(Time.time + pos.x);
					float yWave = Mathf.Sin(1.5f * Time.time + pos.y);
					float zWave = Mathf.Sin(Time.time + pos.z);
					Vector3 wave = new Vector3(xWave,yWave,zWave);
					pos.Normalize();
					vert[i*(width+1)+j] = (pos + wave * waveScale) * radius ;
					//
					xCoor = Mathf.Abs(pos.x);
					yCoor = Mathf.Abs(pos.y);
					zCoor = Mathf.Abs(pos.z);
					if(xCoor>=yCoor && xCoor>=zCoor){uv[i*(width+1)+j] = new Vector2(10*pos.y/pos.x,10*pos.z/pos.x);}
					if(yCoor>xCoor && yCoor>=zCoor){uv[i*(width+1)+j] = new Vector2(10*pos.x/pos.y,10*pos.z/pos.y);}
					if(zCoor>yCoor && zCoor>xCoor){uv[i*(width+1)+j] = new Vector2(10*pos.y/pos.z,10*pos.x/pos.z);}
					
			}}	
	}
	

	
	public void Create(){
		
		cam = GameObject.Find("Camera");
		
		vert = new Vector3[(width+1)*(width+1)];
		normals = new Vector3[(width+1)*(width+1)];
		uv = new Vector2[(width+1)*(width+1)];			
			
			setVertices();

			trians = new int[3*2*width*width];
			int trianIndex =0;
			for(int i = 0; i<width; i++){
				for( int j = 0; j<width; j++){
					int v0 = i*(width+1)+j;
					int v1 = v0+1;
					int v2 = v0+width+1;
					int v3 = v2+1;
				float d0 = -i+j+0.5f*width;
				float d1 = -i+j-0.5f*width;
				if(d0>=0 && d1<0){
					trians[trianIndex+0]=v0;
					trians[trianIndex+1]=v1;
					trians[trianIndex+2]=v3;
					trianIndex+=3;
				}
				if(d0>0 && d1<=0){
					trians[trianIndex+0]=v0;
					trians[trianIndex+1]=v3;
					trians[trianIndex+2]=v2;
					trianIndex+=3;
				}
			}}

	        mesh = GetComponent<MeshFilter>().mesh;
	        mesh.Clear();
			mesh.vertices = vert;
			mesh.uv = uv;
			mesh.triangles = trians;
			mesh.RecalculateNormals();
	}	
	// Use this for initialization
	void Start () {

		Noise3D.init(15);
		Create();
		camPositionOfRenderedWorld = worldToLocal(cam.transform.position);
		

	}
	
	// Update is called once per frame
	void Update () {
		
		//обновить меши если камера сместилась более чем на 5 единиц чего то там
		Vector3 currentCamPosition = worldToLocal(cam.transform.position);
		float distance = Vector3.Magnitude(currentCamPosition - camPositionOfRenderedWorld);
		if(distance >= 5.0f) {	
			setVertices();	      
			mesh.vertices = vert;
			mesh.uv = uv;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}
		
		Vector3 angle = gameObject.transform.eulerAngles;
		angle.y+=0.01f;
		//gameObject.transform.eulerAngles = angle;

	
		//gameObject.transform.Rotate(
		/*for(int i =0; i<100000; i++){
		one.Height(new Vector3(1,2,1));}*/
	
	}
}
