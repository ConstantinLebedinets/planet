using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise3d : MonoBehaviour
{

    public GameObject plane_A;
    public ComputeShader computeShader;
    RenderTexture renderTexture_A;
    int kernelIndex_KernelFunction_A;
    ComputeBuffer heights;
    float[] heightsData;

    public Cubemap map;

    struct ThreadSize
    {
        public int x;
        public int y;
        public int z;

        public ThreadSize(uint x, uint y, uint z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }

    }

    ThreadSize kernelThreadSize_KernelFunction_A;
    //мои функции
    private static System.Random randGen;
    private static int[] randData = new int[512];
    private ComputeBuffer randDataBuffer;
    //создание массива из которого считаются случайные числа

    private void init(int seed)
    {
        randGen = new System.Random(seed);
        for (int i = 0; i < 511; i++) randData[i] = randGen.Next(0, 255);

        randDataBuffer = new ComputeBuffer(512, sizeof(int), ComputeBufferType.Default);
        randDataBuffer.SetData(randData);
    }
    // Start is called before the first frame update
    void Start()
    {
        initilizeCubemap();
        

        //вот это не работает
        Material skyboxMaterial = new Material(Shader.Find("Skybox/Cubemap"));
        //RenderSettings.skybox.mainTexture = this.map;
        skyboxMaterial.SetTexture("_Cube", this.map);
        RenderSettings.skybox = skyboxMaterial;

        gameObject.transform.position+= new Vector3(0, 0, 4);

        GameObject upperFace = UpperFace(new Vector3(0,0,0));//1
        GameObject westFace = WestFace(new Vector3(0, 0, 0));
        GameObject yeastFace = YeastFace(new Vector3(0, 0, 0));
        GameObject bottomFace = BottomFace(new Vector3(0, 0, 0));//1
        GameObject southFace = SouthFace(new Vector3(0, 0, 0));//1
        GameObject northFace = NorthFace(new Vector3(0, 0, 0));//1
        //

    }

    // Update is called once per frame
    void Update()
    {
        /* this.computeShader.Dispatch(this.kernelIndex_KernelFunction_A,
                                     this.renderTexture_A.width / this.kernelThreadSize_KernelFunction_A.x,
                                     this.renderTexture_A.height / this.kernelThreadSize_KernelFunction_A.y,
                                     this.kernelThreadSize_KernelFunction_A.z);
          this.heights.GetData(this.heightsData);*/
        plane_A.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;
        //rotation of planet
        Vector3 angle = gameObject.transform.eulerAngles;
        angle.y += 0.1f;
        gameObject.transform.eulerAngles = angle;
    }

        void OnGUI()
    {
        
        GUI.TextArea(new Rect(10, 85, 230, 60), " Height at: 0.0 in texbuffer: "+this.heightsData[0]);// + this.renderTexture_A.getPixel(0,0));
    }


    GameObject YeastFace(Vector3 pos)
    {

        GameObject child0 = new GameObject("YeastFace");
        child0.AddComponent<MeshFilter>();
        child0.AddComponent<MeshRenderer>();
        child0.transform.position = gameObject.transform.position + pos;
        child0.transform.parent = gameObject.transform;

        Vector3[] vertices;
        Vector2[] UVs;
        Vector4[] tangs;
        int[] triangles;

        Mesh mesh = new Mesh();

        int meshWidth = 128;
        int meshHeight = 128;
        int vertNumber = meshWidth * meshHeight;
        int triansCount = (meshWidth - 1) * (meshHeight - 1) * 2;
        vertices = new Vector3[vertNumber];
        UVs = new Vector2[vertNumber];
        tangs = new Vector4[meshHeight * meshWidth]; // массив касательных



        Vector3 sizeScale = new Vector3(1.0f, 2.0f / meshWidth, 2.0f / meshHeight);
        Vector2 uvScale = new Vector2(1.0f / meshWidth, 1.0f / meshHeight);

        int index;
        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = 0; y < meshHeight; y++)
            {
                index = y * meshWidth + x;  // получаем индекс

                float val = this.heightsData[8 * x + y * meshWidth * 64 + 2 * 1024 * 1024];//noise value from compute shader

                Vector3 vertex = new Vector3(1, x* 2.0f / meshWidth, y* 2.0f / meshHeight); // задаём вершину
                vertices[index] = Vector3.Normalize( new Vector3(0, -1, -1) + vertex)*(1+0.3f*val); // вкладываем вершину в массив и соотносим по размерам
                Vector2 cur_uv = new Vector2(y, x); // задаём uv координату
                UVs[index] = Vector2.Scale(cur_uv, uvScale); // вкладываем uv координату в массив и соотносим по размерам
                                                             /*
                                                             Vector*.Scale(Vector* a, Vector* b) перемножает вектора по координатам соответственно.
                                                             То есть, например, Vector3.Scale(a=(1, 2, 1), b=(2, 3, 1)) вернёт новый вектор (2, 6, 1)
                                                             */

                /* Расчитываем касательную: этот вектор идёт с предыдущей вершины
                 к следующей вдоль оси X. Вектор касательной нужен нам в том случае, если мы
                 будем применять отражающие шейдеры к мешу.
                 W координата касательной всегда должна быть равна либо -1, либо 1, так как
                 бинормаль расчитывается путём умножения нормали на W координату касательной */

                Vector3 leftV = new Vector3(x - 1, 0, y);
                Vector3 rightV = new Vector3(x + 1, 0, y);
                Vector3 tang = Vector3.Scale(sizeScale, rightV - leftV).normalized;
                tangs[index] = new Vector4(tang.x, tang.y, tang.z, 1);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = UVs;


        // Создаём те самые треугольники
        index = 0;
        triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
        for (int y = 0; y < meshHeight - 1; y++)
        {
            for (int x = 0; x < meshWidth - 1; x++)
            {
                // создаём полигон
                triangles[index++] = (y * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;
                triangles[index++] = ((y + 1) * meshWidth) + x;

                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;
                triangles[index++] = ((y + 1) * meshWidth) + x + 1;
            }
        }

        // Даём знать как вершины соединены между собой
        mesh.triangles = triangles;
        // Авторасчёт нормалей, основываясь на меше
        mesh.RecalculateNormals();
        // Касательные нужно назначать обязательно после расчёта или перерасчёта нормалей
        mesh.tangents = tangs;

        child0.GetComponent<MeshFilter>().mesh = mesh;
        child0.GetComponent<MeshRenderer>().material = plane_A.GetComponent<Renderer>().material;
        //child0.transform.localScale = new Vector3(1,1,1);
        child0.transform.eulerAngles = new Vector3(0, 90, 0);
        child0.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;

        return child0;
    }


    GameObject WestFace(Vector3 pos)
    {

        GameObject child0 = new GameObject("WestFace");
        child0.AddComponent<MeshFilter>();
        child0.AddComponent<MeshRenderer>();
        child0.transform.position = gameObject.transform.position + pos;
        child0.transform.parent = gameObject.transform;

        Vector3[] vertices;
        Vector2[] UVs;
        Vector4[] tangs;
        int[] triangles;

        Mesh mesh = new Mesh();

        int meshWidth = 128;
        int meshHeight = 128;
        int vertNumber = meshWidth * meshHeight;
        int triansCount = (meshWidth - 1) * (meshHeight - 1) * 2;
        vertices = new Vector3[vertNumber];
        UVs = new Vector2[vertNumber];
        tangs = new Vector4[meshHeight * meshWidth]; // массив касательных



        Vector3 sizeScale = new Vector3(1.0f, 2.0f / meshWidth, 2.0f / meshHeight);
        Vector2 uvScale = new Vector2(1.0f / meshWidth, 1.0f / meshHeight);

        int index;
        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = 0; y < meshHeight; y++)
            {
                index = y * meshWidth + x;  // получаем индекс

                float val = this.heightsData[8 * x + y * meshWidth * 64 + 5 * 1024 * 1024];//noise value from compute shader

                Vector3 vertex = new Vector3(-1, x * 2.0f / meshWidth, y * 2.0f / meshHeight); // задаём вершину
                vertices[index] = Vector3.Normalize(new Vector3(0, -1, -1) + vertex) * (1 + 0.3f * val); // вкладываем вершину в массив и соотносим по размерам

                Vector2 cur_uv = new Vector2(y, x); // задаём uv координату
                UVs[index] = Vector2.Scale(cur_uv, uvScale); // вкладываем uv координату в массив и соотносим по размерам
                                                             /*
                                                             Vector*.Scale(Vector* a, Vector* b) перемножает вектора по координатам соответственно.
                                                             То есть, например, Vector3.Scale(a=(1, 2, 1), b=(2, 3, 1)) вернёт новый вектор (2, 6, 1)
                                                             */

                /* Расчитываем касательную: этот вектор идёт с предыдущей вершины
                 к следующей вдоль оси X. Вектор касательной нужен нам в том случае, если мы
                 будем применять отражающие шейдеры к мешу.
                 W координата касательной всегда должна быть равна либо -1, либо 1, так как
                 бинормаль расчитывается путём умножения нормали на W координату касательной */

                Vector3 leftV = new Vector3(x - 1, 0, y);
                Vector3 rightV = new Vector3(x + 1, 0, y);
                Vector3 tang = Vector3.Scale(sizeScale, rightV - leftV).normalized;
                tangs[index] = new Vector4(tang.x, tang.y, tang.z, 1);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = UVs;


        // Создаём те самые треугольники
        index = 0;
        triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
        for (int y = 0; y < meshHeight - 1; y++)
        {
            for (int x = 0; x < meshWidth - 1; x++)
            {
                // создаём полигон
                triangles[index++] = (y * meshWidth) + x;
                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;

                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = ((y + 1) * meshWidth) + x + 1;
                triangles[index++] = (y * meshWidth) + x + 1;
            }
        }

        // Даём знать как вершины соединены между собой
        mesh.triangles = triangles;
        // Авторасчёт нормалей, основываясь на меше
        mesh.RecalculateNormals();
        // Касательные нужно назначать обязательно после расчёта или перерасчёта нормалей
        mesh.tangents = tangs;

        child0.GetComponent<MeshFilter>().mesh = mesh;
        child0.GetComponent<MeshRenderer>().material = plane_A.GetComponent<Renderer>().material;
        //child0.transform.localScale = new Vector3(1,1,1);
        child0.transform.eulerAngles = new Vector3(0, 90, 0);
        child0.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;

        return child0;
    }


    GameObject NorthFace(Vector3 pos)
    {

        GameObject child0 = new GameObject("NorthFace");
        child0.AddComponent<MeshFilter>();
        child0.AddComponent<MeshRenderer>();
        child0.transform.position = gameObject.transform.position + pos;
        child0.transform.parent = gameObject.transform;

        Vector3[] vertices;
        Vector2[] UVs;
        Vector4[] tangs;
        int[] triangles;

        Mesh mesh = new Mesh();

        int meshWidth = 128;
        int meshHeight = 128;
        int vertNumber = meshWidth * meshHeight;
        int triansCount = (meshWidth - 1) * (meshHeight - 1) * 2;
        vertices = new Vector3[vertNumber];
        UVs = new Vector2[vertNumber];
        tangs = new Vector4[meshHeight * meshWidth]; // массив касательных



        Vector3 sizeScale = new Vector3(2.0f / meshWidth, 2.0f / meshHeight, 1.0f);
        Vector2 uvScale = new Vector2(1.0f / meshWidth, 1.0f / meshHeight);

        int index;
        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = 0; y < meshHeight; y++)
            {
                index = y * meshWidth + x;  // получаем индекс

                float val = this.heightsData[8 * x + y * meshWidth * 64 + 4 * 1024 * 1024];//noise value from compute shader

                Vector3 vertex = new Vector3(x * 2.0f / meshWidth, y * 2.0f / meshHeight, 1); // задаём вершину
                vertices[index] = Vector3.Normalize(new Vector3(-1, -1, 0) + vertex) * (1 + 0.3f * val); // вкладываем вершину в массив и соотносим по размерам
                Vector2 cur_uv = new Vector2(y, x); // задаём uv координату
                UVs[index] = Vector2.Scale(cur_uv, uvScale); // вкладываем uv координату в массив и соотносим по размерам
                                                             /*
                                                             Vector*.Scale(Vector* a, Vector* b) перемножает вектора по координатам соответственно.
                                                             То есть, например, Vector3.Scale(a=(1, 2, 1), b=(2, 3, 1)) вернёт новый вектор (2, 6, 1)
                                                             */

                /* Расчитываем касательную: этот вектор идёт с предыдущей вершины
                 к следующей вдоль оси X. Вектор касательной нужен нам в том случае, если мы
                 будем применять отражающие шейдеры к мешу.
                 W координата касательной всегда должна быть равна либо -1, либо 1, так как
                 бинормаль расчитывается путём умножения нормали на W координату касательной */

                Vector3 leftV = new Vector3(x - 1, 0, y);
                Vector3 rightV = new Vector3(x + 1, 0, y);
                Vector3 tang = Vector3.Scale(sizeScale, rightV - leftV).normalized;
                tangs[index] = new Vector4(tang.x, tang.y, tang.z, 1);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = UVs;


        // Создаём те самые треугольники
        index = 0;
        triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
        for (int y = 0; y < meshHeight - 1; y++)
        {
            for (int x = 0; x < meshWidth - 1; x++)
            {
                // создаём полигон
                triangles[index++] = (y * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;
                triangles[index++] = ((y + 1) * meshWidth) + x;

                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;
                triangles[index++] = ((y + 1) * meshWidth) + x + 1;
            }
        }

        // Даём знать как вершины соединены между собой
        mesh.triangles = triangles;
        // Авторасчёт нормалей, основываясь на меше
        mesh.RecalculateNormals();
        // Касательные нужно назначать обязательно после расчёта или перерасчёта нормалей
        mesh.tangents = tangs;

        child0.GetComponent<MeshFilter>().mesh = mesh;
        child0.GetComponent<MeshRenderer>().material = plane_A.GetComponent<Renderer>().material;
        //child0.transform.localScale = new Vector3(1,1,1);
        child0.transform.eulerAngles = new Vector3(0, 90, 0);
        child0.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;

        return child0;
    }


    GameObject SouthFace(Vector3 pos)
    {

        GameObject child0 = new GameObject("SouthFace");
        child0.AddComponent<MeshFilter>();
        child0.AddComponent<MeshRenderer>();
        child0.transform.position = gameObject.transform.position + pos;
        child0.transform.parent = gameObject.transform;

        Vector3[] vertices;
        Vector2[] UVs;
        Vector4[] tangs;
        int[] triangles;

        Mesh mesh = new Mesh();

        int meshWidth = 128;
        int meshHeight = 128;
        int vertNumber = meshWidth * meshHeight;
        int triansCount = (meshWidth - 1) * (meshHeight - 1) * 2;
        vertices = new Vector3[vertNumber];
        UVs = new Vector2[vertNumber];
        tangs = new Vector4[meshHeight * meshWidth]; // массив касательных



        Vector3 sizeScale = new Vector3(2.0f / meshWidth, 2.0f / meshHeight, 1.0f);
        Vector2 uvScale = new Vector2(1.0f / meshWidth, 1.0f / meshHeight);

        int index;
        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = 0; y < meshHeight; y++)
            {
                index = y * meshWidth + x;  // получаем индекс

                float val = this.heightsData[8 * x + y * meshWidth * 64 + 1 * 1024 * 1024];//noise value from compute shader
                Vector3 vertex = new Vector3(x * 2.0f / meshWidth, y * 2.0f / meshHeight, -1); // задаём вершину
                vertices[index] = Vector3.Normalize(new Vector3(-1, -1, 0) + vertex) * (1 + 0.3f * val); // вкладываем вершину в массив и соотносим по размерам

                Vector2 cur_uv = new Vector2(y, x); // задаём uv координату
                UVs[index] = Vector2.Scale(cur_uv, uvScale); // вкладываем uv координату в массив и соотносим по размерам
                                                             /*
                                                             Vector*.Scale(Vector* a, Vector* b) перемножает вектора по координатам соответственно.
                                                             То есть, например, Vector3.Scale(a=(1, 2, 1), b=(2, 3, 1)) вернёт новый вектор (2, 6, 1)
                                                             */

                /* Расчитываем касательную: этот вектор идёт с предыдущей вершины
                 к следующей вдоль оси X. Вектор касательной нужен нам в том случае, если мы
                 будем применять отражающие шейдеры к мешу.
                 W координата касательной всегда должна быть равна либо -1, либо 1, так как
                 бинормаль расчитывается путём умножения нормали на W координату касательной */

                Vector3 leftV = new Vector3(x - 1, 0, y);
                Vector3 rightV = new Vector3(x + 1, 0, y);
                Vector3 tang = Vector3.Scale(sizeScale, rightV - leftV).normalized;
                tangs[index] = new Vector4(tang.x, tang.y, tang.z, 1);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = UVs;


        // Создаём те самые треугольники
        index = 0;
        triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
        for (int y = 0; y < meshHeight - 1; y++)
        {
            for (int x = 0; x < meshWidth - 1; x++)
            {
                // создаём полигон
                triangles[index++] = (y * meshWidth) + x;
                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;

                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = ((y + 1) * meshWidth) + x + 1;
                triangles[index++] = (y * meshWidth) + x + 1;
            }
        }

        // Даём знать как вершины соединены между собой
        mesh.triangles = triangles;
        // Авторасчёт нормалей, основываясь на меше
        mesh.RecalculateNormals();
        // Касательные нужно назначать обязательно после расчёта или перерасчёта нормалей
        mesh.tangents = tangs;

        child0.GetComponent<MeshFilter>().mesh = mesh;
        child0.GetComponent<MeshRenderer>().material = plane_A.GetComponent<Renderer>().material;
        //child0.transform.localScale = new Vector3(1,1,1);
        child0.transform.eulerAngles = new Vector3(0, 90, 0);
        child0.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;

        return child0;
    }


    GameObject BottomFace( Vector3 pos)
    {

        GameObject child0 = new GameObject("BottomFace");
        child0.AddComponent<MeshFilter>();
        child0.AddComponent<MeshRenderer>();
        child0.transform.position = gameObject.transform.position + pos;
        child0.transform.parent = gameObject.transform;

        Vector3[] vertices;
        Vector2[] UVs;
        Vector4[] tangs;
        int[] triangles;

        Mesh mesh = new Mesh();

        int meshWidth = 128;
        int meshHeight = 128;
        int vertNumber = meshWidth * meshHeight;
        int triansCount = (meshWidth - 1) * (meshHeight - 1) * 2;
        vertices = new Vector3[vertNumber];
        UVs = new Vector2[vertNumber];
        tangs = new Vector4[meshHeight * meshWidth]; // массив касательных



        Vector3 sizeScale = new Vector3(2.0f / meshWidth, 1.0f, 2.0f / meshHeight);
        Vector2 uvScale = new Vector2(1.0f / meshWidth, 1.0f / meshHeight);

        int index;
        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = 0; y < meshHeight; y++)
            {
                index = y * meshWidth + x;  // получаем индекс

                float val = this.heightsData[8 * x + y * meshWidth * 64 + 3 * 1024 * 1024];//noise value from compute shader

                Vector3 vertex = new Vector3(x * 2.0f / meshWidth, -1, y * 2.0f / meshHeight); // задаём вершину
                vertices[index] = Vector3.Normalize(new Vector3(-1, 0, -1) + vertex) * (1 + 0.3f * val); // вкладываем вершину в массив и соотносим по размерам
                Vector2 cur_uv = new Vector2(y, x); // задаём uv координату
                UVs[index] = Vector2.Scale(cur_uv, uvScale); // вкладываем uv координату в массив и соотносим по размерам
                                                             /*
                                                             Vector*.Scale(Vector* a, Vector* b) перемножает вектора по координатам соответственно.
                                                             То есть, например, Vector3.Scale(a=(1, 2, 1), b=(2, 3, 1)) вернёт новый вектор (2, 6, 1)
                                                             */

                /* Расчитываем касательную: этот вектор идёт с предыдущей вершины
                 к следующей вдоль оси X. Вектор касательной нужен нам в том случае, если мы
                 будем применять отражающие шейдеры к мешу.
                 W координата касательной всегда должна быть равна либо -1, либо 1, так как
                 бинормаль расчитывается путём умножения нормали на W координату касательной */

                Vector3 leftV = new Vector3(x - 1, 0, y);
                Vector3 rightV = new Vector3(x + 1, 0, y);
                Vector3 tang = Vector3.Scale(sizeScale, rightV - leftV).normalized;
                tangs[index] = new Vector4(tang.x, tang.y, tang.z, 1);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = UVs;


        // Создаём те самые треугольники
        index = 0;
        triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
        for (int y = 0; y < meshHeight - 1; y++)
        {
            for (int x = 0; x < meshWidth - 1; x++)
            {
                // создаём полигон
                triangles[index++] = (y * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;
                triangles[index++] = ((y + 1) * meshWidth) + x;

                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;
                triangles[index++] = ((y + 1) * meshWidth) + x + 1;
            }
        }

        // Даём знать как вершины соединены между собой
        mesh.triangles = triangles;
        // Авторасчёт нормалей, основываясь на меше
        mesh.RecalculateNormals();
        // Касательные нужно назначать обязательно после расчёта или перерасчёта нормалей
        mesh.tangents = tangs;

        child0.GetComponent<MeshFilter>().mesh = mesh;
        child0.GetComponent<MeshRenderer>().material = plane_A.GetComponent<Renderer>().material;
        //child0.transform.localScale = new Vector3(1,1,1);
        child0.transform.eulerAngles = new Vector3(0, 90, 0);
        child0.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;

        return child0;
    }

    GameObject UpperFace(Vector3 pos)
    {

        GameObject child0 = new GameObject("UpperFace");
        child0.AddComponent<MeshFilter>();
        child0.AddComponent<MeshRenderer>();
        child0.transform.position = gameObject.transform.position + pos;
        child0.transform.parent = gameObject.transform;

        Vector3[] vertices;
        Vector2[] UVs;
        Vector4[] tangs;
        int[] triangles;

        Mesh mesh = new Mesh();

        int meshWidth = 128;
        int meshHeight = 128;
        int vertNumber = meshWidth * meshHeight;
        int triansCount = (meshWidth - 1) * (meshHeight - 1) * 2;
        vertices = new Vector3[vertNumber];
        UVs = new Vector2[vertNumber];
        tangs = new Vector4[meshHeight * meshWidth]; // массив касательных



        Vector3 sizeScale = new Vector3(2.0f / meshWidth, 1.0f, 2.0f / meshHeight);
        Vector2 uvScale = new Vector2(1.0f / meshWidth, 1.0f / meshHeight);

        int index;
        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = 0; y < meshHeight; y++)
            {
                index = y * meshWidth + x;  // получаем индекс

                float val = this.heightsData[8 * x + y * meshWidth * 64];//noise value from compute shader
                Vector3 vertex = new Vector3(x * 2.0f / meshWidth, 1, y * 2.0f / meshHeight); // задаём вершину
                vertices[index] = Vector3.Normalize(new Vector3(-1, 0, -1) + vertex) * (1 + 0.3f * val); // вкладываем вершину в массив и соотносим по размерам
                Vector2 cur_uv = new Vector2(y, x); // задаём uv координату
                UVs[index] = Vector2.Scale(cur_uv, uvScale); // вкладываем uv координату в массив и соотносим по размерам
                                                             /*
                                                             Vector*.Scale(Vector* a, Vector* b) перемножает вектора по координатам соответственно.
                                                             То есть, например, Vector3.Scale(a=(1, 2, 1), b=(2, 3, 1)) вернёт новый вектор (2, 6, 1)
                                                             */

                /* Расчитываем касательную: этот вектор идёт с предыдущей вершины
                 к следующей вдоль оси X. Вектор касательной нужен нам в том случае, если мы
                 будем применять отражающие шейдеры к мешу.
                 W координата касательной всегда должна быть равна либо -1, либо 1, так как
                 бинормаль расчитывается путём умножения нормали на W координату касательной */

                Vector3 leftV = new Vector3(x - 1, 0, y);
                Vector3 rightV = new Vector3(x + 1, 0, y);
                Vector3 tang = Vector3.Scale(sizeScale, rightV - leftV).normalized;
                tangs[index] = new Vector4(tang.x, tang.y, tang.z, 1);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = UVs;


        // Создаём те самые треугольники
        index = 0;
        triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
        for (int y = 0; y < meshHeight - 1; y++)
        {
            for (int x = 0; x < meshWidth - 1; x++)
            {
                // создаём полигон
                triangles[index++] = (y * meshWidth) + x;
                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = (y * meshWidth) + x + 1;

                triangles[index++] = ((y + 1) * meshWidth) + x;
                triangles[index++] = ((y + 1) * meshWidth) + x + 1;
                triangles[index++] = (y * meshWidth) + x + 1;
            }
        }

        // Даём знать как вершины соединены между собой
        mesh.triangles = triangles;
        // Авторасчёт нормалей, основываясь на меше
        mesh.RecalculateNormals();
        // Касательные нужно назначать обязательно после расчёта или перерасчёта нормалей
        mesh.tangents = tangs;

        child0.GetComponent<MeshFilter>().mesh = mesh;
        child0.GetComponent<MeshRenderer>().material = plane_A.GetComponent<Renderer>().material;
        //child0.transform.localScale = new Vector3(1,1,1);
        child0.transform.eulerAngles = new Vector3(0, 90, 0);
        child0.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;

        return child0;
    }

    void initilizeCubemap()
    {

        //создаем числа в случайном массиве
        this.init(125);


        this.renderTexture_A = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        this.renderTexture_A.enableRandomWrite = true;
        this.renderTexture_A.Create();

        this.kernelIndex_KernelFunction_A = this.computeShader.FindKernel("KernelFunction_A");

        uint threadSizeX, threadSizeY, threadSizeZ;

        this.computeShader.GetKernelThreadGroupSizes
            (this.kernelIndex_KernelFunction_A,
             out threadSizeX, out threadSizeY, out threadSizeZ);

        this.kernelThreadSize_KernelFunction_A
            = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

        this.computeShader.SetTexture
            (this.kernelIndex_KernelFunction_A, "textureBuffer", this.renderTexture_A);

        //ставлю массив с случайной датой

        this.computeShader.SetBuffer(this.kernelIndex_KernelFunction_A, "randData", randDataBuffer);
        //this.computeShader.SetInts("randData", randData);

        this.heights = new ComputeBuffer(1024 * 1024 * 6, sizeof(float));
        this.heightsData = new float[1024 * 1024 * 6];
        this.computeShader.SetBuffer(this.kernelIndex_KernelFunction_A, "heights", this.heights);

        this.map = new Cubemap(1024, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, 0);
        this.computeShader.Dispatch(this.kernelIndex_KernelFunction_A,
                                   this.renderTexture_A.width / this.kernelThreadSize_KernelFunction_A.x,
                                   this.renderTexture_A.height / this.kernelThreadSize_KernelFunction_A.y,
                                   this.kernelThreadSize_KernelFunction_A.z);
        this.heights.GetData(this.heightsData);

        for (int x = 0; x < 1024; x++)
        {
            for (int y = 0; y < 1024; y++)
            {
                int index = x + y * 1024;
                float val = this.heightsData[index];
                this.map.SetPixel(CubemapFace.NegativeX, x, y, new Color(val, val, val, 1));

                index += 1024 * 1024;
                val = this.heightsData[index];
                this.map.SetPixel(CubemapFace.NegativeY, x, y, new Color(val, val, val, 1));

                index += 1024 * 1024;
                val = this.heightsData[index];
                this.map.SetPixel(CubemapFace.NegativeZ, x, y, new Color(val, val, val, 1));

                index += 1024 * 1024;
                val = this.heightsData[index];
                this.map.SetPixel(CubemapFace.PositiveX, x, y, new Color(val, val, val, 1));

                index += 1024 * 1024;
                val = this.heightsData[index];
                this.map.SetPixel(CubemapFace.PositiveY, x, y, new Color(val, val, val, 1));

                index += 1024 * 1024;
                val = this.heightsData[index];
                this.map.SetPixel(CubemapFace.PositiveZ, x, y, new Color(val, val, val, 1));
            }
        }
        this.map.Apply();


    }
}
