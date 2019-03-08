using UnityEngine;
using System.Collections;

public class DebugCubeMap : MonoBehaviour {
		public Cubemap heightMap;
	// Use this for initialization
	void Start () {
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
		gameObject.GetComponent<MeshRenderer>().material.SetTexture("_CubeMap", heightMap);
			
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
