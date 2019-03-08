using UnityEngine;
using System.Collections;


public static class MagicData {
	
	/*
	 * получает запрос в виде вектора, оно как то там считает и выдает высоту где этот вектор пересекает сферу с радиусом 1
	*/
	
	static int width = 200;
	
	static float [ , , ] data;
	static Vector3 [] face;
	
	static public void Init(){

		data = new float [6,width+1,width+1];
			//грань по x+
			for(int i=0; i<=width; i++){
				for(int j=0; j<=width; j++){			
					Vector3 pos = new Vector3(width*0.5f,i-width*0.5f,j-width*0.5f);
					pos.Normalize();
					data[0,i,j] = getHeight(pos);}}
			//грань по x-
			for(int i=0; i<=width; i++){
				for(int j=0; j<=width; j++){			
					Vector3 pos = new Vector3(-width*0.5f,i-width*0.5f,j-width*0.5f);
					pos.Normalize();
					data[1,i,j] = getHeight(pos);}}		
			//грань по y+
			for(int i=0; i<=width; i++){
				for(int j=0; j<=width; j++){			
					Vector3 pos = new Vector3(i-width*0.5f,width*0.5f,j-width*0.5f);
					pos.Normalize();
					float height = getHeight(pos);
					data[2,i,j] = getHeight(pos);}}
			//грань по y-
			for(int i=0; i<=width; i++){
				for(int j=0; j<=width; j++){			
					Vector3 pos = new Vector3(i-width*0.5f,-width*0.5f,j-width*0.5f);
					pos.Normalize();
					data[3,i,j] = getHeight(pos);}}	
			//грань по z+
			for(int i=0; i<=width; i++){
				for(int j=0; j<=width; j++){			
					Vector3 pos = new Vector3(i-width*0.5f,j-width*0.5f,width*0.5f);
					pos.Normalize();
					data[4,i,j] = getHeight(pos);}}
			//грань по z-
			for(int i=0; i<=width; i++){
				for(int j=0; j<=width; j++){			
					Vector3 pos = new Vector3(i-width*0.5f,j-width*0.5f,-width*0.5f);
					pos.Normalize();
					data[5,i,j] = getHeight(pos);}}	
		//нормали  куба
		face = new Vector3[6];
		
		face[0] = new Vector3 (1,0,0);
		face[1] = new Vector3 (-1,0,0);
		face[2] = new Vector3 (0,1,0);
		face[3] = new Vector3 (0,-1,0);
		face[4] = new Vector3 (0,0,1);
		face[5] = new Vector3 (0,0,-1);
		
	}
	

	
	static private float fade(float t) {
 		return t*t*t*(t*(t*6-15)+10);
	}
	static public float getHeight(Vector3 pos){
		int numberOfOctaves = 6;
		float sum = 0f;		
		for(int i = 1; i<=numberOfOctaves; i++){
			sum = sum + Mathf.Pow((2f * Noise3D.noise( pos * i * i ) - 1f),1f)/i;
		}

		return 1f+0.06f*sum;
	}
	static public float Height ( Vector3 vec){
	
		vec.Normalize();
		
		float x = Mathf.Abs(vec.x);
		float y = Mathf.Abs(vec.y);
		float z = Mathf.Abs(vec.z);
		//выделение грани
		// нужно вектор обрезать по плоскостям куба и далее по точке пересечения определить нужную ячейку массива с картой высот
		//очевидно что минимальный угол между вектором и нормалью каждой плоскости даст ту плоскость которую пересекает вектор

		int max = 0;
		float saved = -2f;
		//нахожу максимальное значение
		for(int i=0; i<6;i++){
			//сравниваю нормаль грани с полученным вектором
			float angle = Vector3.Dot(vec,face[i]);
			if(angle>saved){
				saved = angle;
				max = i;
			}
		}
		
		int i0 = -1 , j0 = -1 ;
		float dx = 0, dy = 0;
		//если угол 0 то 1 если 180 то -1, соответственно самое большое число покажет нужную грань		
		//ошибка в определении количества элементов массива width+1 или сколько там
		switch (max)
		{
		    case 0:
				dx = width*0.5f*(vec.y/x+1f);
				dy = width*0.5f*(vec.z/x+1f);
				i0 = Mathf.FloorToInt(dx);
				j0 = Mathf.FloorToInt(dy);			
		        break;
		    case 1:				
				dx = width*0.5f*(vec.y/x+1f);
				dy = width*0.5f*(vec.z/x+1f);
				i0 = Mathf.FloorToInt(dx);
				j0 = Mathf.FloorToInt(dy);	
		        break;
			case 2:
				dx = width*0.5f*(vec.x/y+1f);
				dy = width*0.5f*(vec.z/y+1f);
				i0 = Mathf.FloorToInt(dx);
				j0 = Mathf.FloorToInt(dy);	
		        break;
			case 3:
				dx = width*0.5f*(vec.x/y+1f);
				dy = width*0.5f*(vec.z/y+1f);
				i0 = Mathf.FloorToInt(dx);
				j0 = Mathf.FloorToInt(dy);
		        break;
			case 4:
				dx = width*0.5f*(vec.x/z+1f);
				dy = width*0.5f*(vec.y/z+1f);
				i0 = Mathf.FloorToInt(dx);
				j0 = Mathf.FloorToInt(dy);	
		        break;
			case 5:
				dx = width*0.5f*(vec.x/z+1f);
				dy = width*0.5f*(vec.y/z+1f);
				i0 = Mathf.FloorToInt(dx);
				j0 = Mathf.FloorToInt(dy);				
		        break;}

		//значение высоты в выбранной клетке
		//if(i0 == width)i0--;
		//if(j0 == width)j0--;
		i0 = i0 % width;
		j0 = j0 % width;
		float v00 = data[max,i0,j0];
		float v10 = data[max,i0+1,j0];
		float v01 = data[max,i0,j0+1];
		float v11 = data[max,i0+1,j0+1];
		//апроксимация значения в выбранной клетке
		float diff0 = Mathf.Lerp(v00,v01,fade(dy-j0));
		float diff1 = Mathf.Lerp(v10,v11,fade(dy-j0));
		
		return Mathf.Lerp(diff0,diff1,fade(dx-i0));	
	}	
}
