using UnityEngine;
using System.Collections;

public class SingleCloud : MonoBehaviour {
	int numberOfParticles = 500;

	Vector3[] vert;
	Vector3[] normals;
	Vector2[] uv;
	int[] trians;
	Mesh mesh;
	SingleParticle[] particles;
	GameObject cam;
	private static System.Random randGen;
	//радиус облака
	public float radius = 300f;
	public float height;
	Vector3 curAxis = new Vector3(0,1,0);
	public int seed = 100500;
	public float planetRadius;
	public float particleSpeed = 0.00005f;
	
	
	
	public Vector3 worldToLocal(Vector3 vec){
		return	transform.worldToLocalMatrix.MultiplyPoint(vec);
	}
	public Vector3 localToWorld(Vector3 vec){
		return	transform.localToWorldMatrix.MultiplyPoint(vec);
	}		
	
	// Use this for initialization
	void Start () {

		randGen = new System.Random(seed);
		cam = GameObject.Find("Camera");
		radius = randGen.Next((int)(planetRadius * 0.1f),(int)(planetRadius * 0.25f));
		vert = new Vector3[4*numberOfParticles];
		uv = new Vector2[4*numberOfParticles];	
		normals =  new Vector3[4*numberOfParticles];
		trians = new int[6*numberOfParticles];	
		particles = new SingleParticle[numberOfParticles];
		radius = radius * 1.1f;
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
			Vector3 position = new Vector3(randGen.Next(-500,500),randGen.Next(-500,500),randGen.Next(-500,500));
			position.Normalize();
			position = position * radius * randGen.Next(0,100) * 0.01f;
			particles[i].position =position;
			particles[i].velocity = new Vector3(randGen.Next(-100,100),randGen.Next(-100,100),randGen.Next(-100,100));
			
			particles[i].velocity*= particleSpeed;
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
		
		// есть точка центр планеты
		// есть позиция частицы в облаке в сферическом представлении
		// есть z частицы внутри облака если спроецировать позицию частицы на сферу с радиусом 1
		// и умножить это число на радиус плюс высоту облака плюс позицию частицы то облако обернется вокруг сферы
		//равно как можно и применить масштаб
		// есть радиус и высота и так просто оборачиваем вокруг сферы чья поверхность проходит через центр координат
		// а сам центр располагается по оси игрек где то в отрицательной части
		
		Vector3 centerOfPlanet = new Vector3(0f,-radius-height,0f);
		
		
		Vector3 camPosition = worldToLocal(cam.transform.position);// позиция камеры
		camPosition.Normalize();
		Quaternion magic = Quaternion.FromToRotation(curAxis,camPosition);
		float size = radius*0.2f;
		//смещение точек каждого квада от позиции квада
		Vector3 v0 = magic * SingleParticle.point0 * size;
		Vector3 v1 = magic * SingleParticle.point1 * size;
		Vector3 v2 = magic * SingleParticle.point2 * size;
		Vector3 v3 = magic * SingleParticle.point3 * size;
		
		for(int i = 0; i < numberOfParticles; i++){
			
			particles[i].position += particles[i].velocity;
			
			
			
			// если частица пытается свалить от центра дальше чем следует то ее скорость меняется постепенно на такую
			//которая вернет ее к центру облака
			
			if(particles[i].position.magnitude > 0.95f * radius) {
				//Debug.Log("ouch");
				// надо поменять направление скорости частицы;
				particles[i].velocity += -particleSpeed * particles[i].position;
			}
			else{
				particles[i].velocity += particleSpeed * new Vector3(randGen.Next(-100,100),randGen.Next(-100,100),randGen.Next(-100,100));
			}
			//фэйковая деформированная позиция точки
			Vector3 fakePos = particles[i].position;
			float yHeight = fakePos.y;
			
			fakePos -= centerOfPlanet;
			fakePos.Normalize();
			fakePos *= radius+height+yHeight;
			fakePos.y *= 0.1f;
			
			
			int index = i * 4;
			
			vert[index + 0] = v0 + fakePos;
			vert[index + 1] = v1 + fakePos;
			vert[index + 2] = v2 + fakePos;
			vert[index + 3] = v3 + fakePos;
		}
		mesh.vertices = vert;
		mesh.RecalculateBounds();
		

	}
}
