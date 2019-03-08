using UnityEngine;
using System.Collections;
using System;

public class Noise3D : MonoBehaviour {
	
		private static System.Random randGen; 
		private static int[] randData = new int[512];
	
	public static void init ( int seed) {
		randGen = new System.Random(seed);
		for(int i=0; i<511; i++) randData[i] = randGen.Next(0,255); 
	}
	//сплайн для апроксимации
 	private static float fade(float t) {
 		return t*t*t*(t*(t*6-15)+10);
	}	
	
	 //выдает по координате значение из фэйкового бесконечного массива
	private static int rand(int x, int y, int z){
		int x0 =(256+(x%256))%256;
		int y0 =(256+(y%256))%256;
		int z0 =(256+(z%256))%256;	
		return randData[ x0 + randData[ y0 + randData[ z0 ] ] ]; 	
	}
	public static float noise(Vector3 pos){
		int x0 = Mathf.FloorToInt(pos.x);
		int y0 = Mathf.FloorToInt(pos.y);
		int z0 = Mathf.FloorToInt(pos.z);
		//смещение внутри ячейки
		float dx = pos.x-x0;
		float dy = pos.y-y0;
		float dz = pos.z-z0;
		//читаю 8 точек из фэйкового рандома
		int x000 = rand(x0,y0,z0);
		int x001 = rand(x0,y0,z0+1);
		int x010 = rand(x0,y0+1,z0);
		int x011 = rand(x0,y0+1,z0+1);
		int x100 = rand(x0+1,y0,z0);
		int x101 = rand(x0+1,y0,z0+1);
		int x110 = rand(x0+1,y0+1,z0);
		int x111 = rand(x0+1,y0+1,z0+1);	
		//смешиваю
		float dif00 = Mathf.Lerp(x000,x001,fade(dz));
		float dif01 = Mathf.Lerp(x010,x011,fade(dz));
		float dif10 = Mathf.Lerp(x100,x101,fade(dz));
		float dif11 = Mathf.Lerp(x110,x111,fade(dz));
		
		float diff0 = Mathf.Lerp(dif00,dif01,fade(dy));
		float diff1 = Mathf.Lerp(dif10,dif11,fade(dy));
		
		return Mathf.Lerp(diff0,diff1,fade(dx))*0.0039215686274509803921568627451f;
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
}
