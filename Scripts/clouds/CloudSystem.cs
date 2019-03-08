using UnityEngine;
using System.Collections;

public class CloudSystem : MonoBehaviour {
	
	
	public GameObject planet;

	GameObject[] clouds;
	float[] height;
	Vector3[] velocity;
	int numberOfClouds = 10;
	float cloudSpeed = 0.82f;
	//радиус планеты
	public float radius;
	private static System.Random randGen; 
	
	// Use this for initialization
	void Start () {
		
		randGen = new System.Random(100500);		
		clouds = new GameObject[numberOfClouds];
		height = new float[numberOfClouds];
		velocity = new Vector3[numberOfClouds];
		planet = GameObject.Find("Planet");
		for( int i = 0 ; i < numberOfClouds ; i++ ) {
			string name = "Cloud"+i;
			clouds[i] = new GameObject(name);
			clouds[i].transform.parent = gameObject.transform;
			clouds[i].AddComponent<MeshFilter>();
			clouds[i].AddComponent<MeshRenderer>();
			clouds[i].AddComponent<SingleCloud>();
			clouds[i].GetComponent<SingleCloud>().seed = i;
			clouds[i].GetComponent<SingleCloud>().planetRadius = radius;
			clouds[i].GetComponent<MeshRenderer>().material = GameObject.Find("CloudMat").GetComponent<MeshRenderer>().material;
			
			//раскидывание по поверхности сферы этих облака
			Vector3 position = new Vector3(randGen.Next(-1000,1000), randGen.Next(-200, 200), randGen.Next(-1000, 1000));
			position.Normalize();

            height[i] = randGen.Next(200, 300);
			position *= radius + height[i];
			clouds[i].GetComponent<SingleCloud>().height = height[i];
			clouds[i].transform.localPosition = position;
			position.Normalize();
			clouds[i].transform.up = position;

		}
		
	
		
	}
	
	// Update is called once per frame
	void Update () {
		
			
		for( int i = 0 ; i < numberOfClouds ; i++ ) {
			velocity[i] += 0.0001f * new Vector3(randGen.Next(-100,100),randGen.Next(-100,100),randGen.Next(-100,100));
			velocity[i].Normalize();
			
			Vector3 position = clouds[i].transform.localPosition + velocity[i] * cloudSpeed;
			position.Normalize();
			clouds[i].transform.localPosition = position * (radius + height[i]);
			Vector3 up = clouds[i].transform.position - planet.transform.position;
			up.Normalize();
			clouds[i].transform.up = up;	
			clouds[i].transform.rotation = Quaternion.FromToRotation(new Vector3(0,1,0),up);
			
		}
	}
}
