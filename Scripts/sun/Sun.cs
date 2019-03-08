using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour {

	int numberOfParticles = 1;
	Vector3[] vert;
	Vector3[] normals;
	Vector2[] uv;
	int[] trians;
	Mesh mesh;
	SingleParticle[] particles;
	GameObject cam;
	private static System.Random randGen;
	Vector3 curAxis = new Vector3(0,1,0);
	public int seed = 100500;
	public float particleSpeed = 0.05f;
	public float radius = 500f;

	public Vector3 worldToLocal(Vector3 vec){
		return	transform.worldToLocalMatrix.MultiplyPoint(vec);
	}
	public Vector3 localToWorld(Vector3 vec){
		return	transform.localToWorldMatrix.MultiplyPoint(vec);
	}		
		
	
	// Use this for initialization
	void Start () {
			gameObject.AddComponent<MeshFilter>();
			gameObject.AddComponent<MeshRenderer>();
			gameObject.GetComponent<MeshRenderer>().material = GameObject.Find("SunMat").GetComponent<MeshRenderer>().material;		
		
		randGen = new System.Random(seed);
		cam = GameObject.Find("Camera");

		vert = new Vector3[4*numberOfParticles];
		uv = new Vector2[4*numberOfParticles];	
		normals =  new Vector3[4*numberOfParticles];
		trians = new int[6*numberOfParticles];	
		particles = new SingleParticle[numberOfParticles];

		//ставлю точки полигоны треугольники и т.п.
		
		for(int i = 0; i < numberOfParticles; i++){
			int index = i * 4;
			vert[index + 0] = new Vector3(-1f,0f,-1f);
			vert[index + 1] = new Vector3(1f,0f,-1f);
			vert[index + 2] = new Vector3(-1f,0f,1f);
			vert[index + 3] = new Vector3(1f,0f,1f);
			
			uv[index + 0] = new Vector2(0f,0f);
			uv[index + 1] = new Vector2(1f,0f);
			uv[index + 2] = new Vector2(0f,1f);
			uv[index + 3] = new Vector2(1f,1f);
			
			normals[index + 0] = new Vector3(0f,1f,0f);
			normals[index + 1] = new Vector3(0f,1f,0f);
			normals[index + 2] = new Vector3(0f,1f,0f);
			normals[index + 3] = new Vector3(0f,1f,0f);
			
			trians[i * 6 + 0] = index + 0;
			trians[i * 6 + 1] = index + 2;
			trians[i * 6 + 2] = index + 1;
			trians[i * 6 + 3] = index + 1;
			trians[i * 6 + 4] = index + 2;
			trians[i * 6 + 5] = index + 3;
			
			particles[i] = new SingleParticle();
			// распределяю облачка по сфере случайным образом
			Vector3 position = new Vector3(0,0,0);
			position.Normalize();
			position = position * radius;
			particles[i].position =position;

		}
		
		mesh = GetComponent<MeshFilter>().mesh;
	    mesh.Clear();
		mesh.vertices = vert;
		mesh.uv = uv;
		mesh.triangles = trians;
		mesh.normals = normals;
		mesh.RecalculateBounds();	
	}
	
	// Update is called once per frame
	void Update () {

		
		
		Vector3 camPosition = worldToLocal(cam.transform.position);// позиция камеры
		camPosition.Normalize();
		Quaternion magic = Quaternion.FromToRotation(curAxis,camPosition);
		float size = radius;
		//смещение точек каждого квада от позиции квада
		Vector3 v0 = magic * SingleParticle.point0 * size;
		Vector3 v1 = magic * SingleParticle.point1 * size;
		Vector3 v2 = magic * SingleParticle.point2 * size;
		Vector3 v3 = magic * SingleParticle.point3 * size;
		
		for(int i = 0; i < numberOfParticles; i++){
			
			int index = i * 4;
			
			vert[index + 0] = v0 + particles[i].position * radius;
			vert[index + 1] = v1 + particles[i].position * radius;
			vert[index + 2] = v2 + particles[i].position * radius;
			vert[index + 3] = v3 + particles[i].position * radius;
			
			normals[index + 0] = particles[i].position;
			normals[index + 1] = particles[i].position;
			normals[index + 2] = particles[i].position;
			normals[index + 3] = particles[i].position;
		}
		mesh.vertices = vert;
		mesh.RecalculateBounds();
		

	}	
}
