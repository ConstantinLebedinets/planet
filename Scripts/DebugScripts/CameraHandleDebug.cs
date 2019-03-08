using UnityEngine;
using System.Collections;

public class CameraHandleDebug : MonoBehaviour {

	// начальный угол вращения по оси y
    public float yRotation = 5.0F;
 	public float xRotation = 5.0F;
	public float sensitivity = 2.0F;
	public float speed = 0.1f;
	// Use this for initialization
    void Start() {

    }
	
    void Update() {
        // получаем значение оси ввода "Horizontal" и прибавляем его к значению вращения
        yRotation -= sensitivity * Input.GetAxis ("Mouse Y");
		xRotation += sensitivity * Input.GetAxis ("Mouse X");
        // устанавливаем вращение объекта
        transform.eulerAngles = new Vector3(yRotation, xRotation, 0);
		
		//движение камеры
		float delta=speed;
	    float dx=0.0f;
	    float dy=0.0f;
	    float dz=0.0f;

		if(Input.GetKey(KeyCode.A))dx-=delta;
        if(Input.GetKey(KeyCode.D))dx+=delta;
        if(Input.GetKey(KeyCode.S))dz-=delta;
        if(Input.GetKey(KeyCode.W))dz +=delta;
        

            Vector3 move = new Vector3(dx, 0, dz);
		move = transform.rotation * move;
		transform.localPosition += move;
    }
    // при инициализации объекта выводим значения его вращений вокруг каждой оси в консоль
    void Awake() {

    }

}
