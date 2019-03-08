using UnityEngine;
using System.Collections;

public class CameraHandle : MonoBehaviour {
	
	
	

	public float sensitivity = 0.01F;
	public float speed = 5.4f;
    public float height_of_sight = 1.75f;
	Vector3 Up = new Vector3(0,1,0);
	Quaternion magic;
	GameObject planet;
    MeshPlanet ThisMeshPlanet;
    GameObject slave;
	GameObject camLight; // костыль когда солнце за горизонтом, а вообще пора писать нормальный свет
	
	// Use this for initialization
    void Start() {
		slave = new GameObject("Slave");
		slave.transform.position = transform.position;
		planet = GameObject.Find("Planet");
        ThisMeshPlanet = planet.GetComponent<MeshPlanet>();

        slave.transform.parent = planet.transform;
		transform.parent = slave.transform;
		magic = Quaternion.FromToRotation(Up,slave.transform.position);
		slave.transform.rotation = magic;
		
		camLight = new GameObject("CamLight");
		camLight.transform.position = transform.position;
		camLight.transform.parent = transform;
		camLight.transform.localPosition += new Vector3(0,15,0);
		camLight.AddComponent<Light>();
		camLight.GetComponent<Light>().color = Color.grey;
		camLight.GetComponent<Light>().intensity = 0.85f;
		camLight.GetComponent<Light>().range = 25.0f;
    }
	
    void Update() {
		//перемещение в точку в которой камера и выставление локальной позиции камеры в ноль
		slave.transform.position = transform.position;
		transform.localPosition = new Vector3(0,0,0);
		
		//запоминаю глобальный угол камеры
		Quaternion previous = transform.rotation;
		
		//установка верха раба в соответствии с нормалью к сфере в этой точке пространства
		Vector3 slaveUp = slave.transform.position - planet.transform.position;
		slaveUp.Normalize();
		slave.transform.up = slaveUp;
		//убираю ненужный поворот от изменения кватерниона раба
		transform.rotation = previous;

        //мауслук
        if (Input.GetMouseButton(1))
        {
            Vector3 rotation = transform.localEulerAngles;
            // получаем значение оси ввода "Horizontal" и прибавляем его к значению вращения
            rotation.x -= sensitivity * Input.GetAxis("Mouse Y");
            rotation.y += sensitivity * Input.GetAxis("Mouse X");
            rotation.z = 0f; // ибо какого то хуя бывает не ноль
                             // устанавливаем вращение объекта
            transform.localEulerAngles = rotation;
        }
        //движение камеры

        Vector3 move = new Vector3(0,0,0);	
		if(Input.GetKey(KeyCode.A))move-= transform.right;
        if(Input.GetKey(KeyCode.D))move+= transform.right;
        if(Input.GetKey(KeyCode.S))move-= transform.forward;
        if(Input.GetKey(KeyCode.W))move+= transform.forward;
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
            move += 5 * transform.forward;
            // and setting cursor position to the center
            Screen.lockCursor = true;

        }
        else { Screen.lockCursor = false; }
        transform.position += move*speed;
        camHandle();
    }


   
    void camHandle()
    {
        //запрет на перемещение внутри планеты
        Vector3 camPosition = ThisMeshPlanet.worldToLocal(transform.position);
        float camDistance = Vector3.Magnitude(camPosition);
        float heightAtThisPoint = MagicData.Height(camPosition);
        Vector3 posit = camPosition;
        posit.Normalize();
        if (camDistance < (posit * heightAtThisPoint * (ThisMeshPlanet.radius + height_of_sight)).magnitude)
        {
            transform.position = ThisMeshPlanet.localToWorld(posit * heightAtThisPoint * (ThisMeshPlanet.radius + height_of_sight));
        }
    }
    


    // при инициализации объекта выводим значения его вращений вокруг каждой оси в консоль
    void Awake() {

    }



    void OnGUI(){
		GUI.TextArea(new Rect(10,45,230,20) , " Height at: " + transform.localEulerAngles.ToString());
	}
}
