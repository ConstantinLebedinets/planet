using UnityEngine;
using System.Collections;

public class NewPlanet : MonoBehaviour {
	
	GameObject cam;
	Vector3 camPosition;
	GameObject child0,child1,child2;
	public Cubemap heightMap;

	// Use this for initialization
	void Start () {
		
		cam = GameObject.Find("Camera");
		//gameObject.transform.eulerAngles = new Vector3(0,0,0);
		Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
		
		child0 = new GameObject("child0");
		child0.AddComponent<MeshFilter>();
		child0.AddComponent<MeshRenderer>();
		child0.transform.position = gameObject.transform.position;
		child0.transform.parent = gameObject.transform;		
		child0.GetComponent<MeshFilter>().mesh = mesh;
		child0.GetComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().material;
		//child0.transform.localScale = new Vector3(1,1,1);
		child0.transform.eulerAngles = new Vector3(0,90,0);
		
		child1 = new GameObject("child1");
		child1.AddComponent<MeshFilter>();
		child1.AddComponent<MeshRenderer>();
		child1.transform.position = gameObject.transform.position;
		child1.transform.parent = gameObject.transform;		
		child1.GetComponent<MeshFilter>().mesh = mesh;
		child1.GetComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().material;
		//child0.transform.localScale = new Vector3(1,1,1);
		child1.transform.eulerAngles = new Vector3(0,180,0);		
		
		child2 = new GameObject("child2");
		child2.AddComponent<MeshFilter>();
		child2.AddComponent<MeshRenderer>();
		child2.transform.position = gameObject.transform.position;
		child2.transform.parent = gameObject.transform;		
		child2.GetComponent<MeshFilter>().mesh = mesh;
		child2.GetComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().material;
		//child0.transform.localScale = new Vector3(1,1,1);
		child2.transform.eulerAngles = new Vector3(0,270,0);		
		
		
		
		gameObject.transform.up = cam.transform.position - gameObject.transform.position;
		int width = 256;
		heightMap = new Cubemap(width,TextureFormat.ARGB32,false);
			for( int i = 0 ; i<width;i++){
				for( int j = 0 ; j<width;j++){
				heightMap.SetPixel(CubemapFace.NegativeX,i,j, new Color(1,0,0,1));
				heightMap.SetPixel(CubemapFace.NegativeY,i,j, new Color(1,1,0,1));
				heightMap.SetPixel(CubemapFace.NegativeZ,i,j, new Color(1,0,1,1));
				heightMap.SetPixel(CubemapFace.PositiveX,i,j, new Color(1,1,0.5f,1));
				heightMap.SetPixel(CubemapFace.PositiveY,i,j, new Color(1,1,0.75f,1));
				heightMap.SetPixel(CubemapFace.PositiveZ,i,j, new Color(1,1,1,1));
			}}
		heightMap.Apply();
		
		
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.up = cam.transform.position - gameObject.transform.position;
		
		gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Scale", gameObject.transform.localScale.x);
	}
	
	void OnGUI(){
		//GUI.DrawTexture(new Rect(10,5,256,256),);
		//GUI.TextArea(new Rect(10,5,199,20) , " time: " + cam.transform.position.ToString());
		//GUI.TextArea(new Rect(10,25,199,20) , " time: " + cam.transform.localToWorldMatrix.MultiplyPoint(cam.transform.position).ToString());
	}	
	
}