using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise3d : MonoBehaviour
{

    public GameObject plane_A;
    public ComputeShader computeShader;
    RenderTexture renderTexture_A;
    int kernelIndex_KernelFunction_A;

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
        //создаем числа в случайном массиве
        this.init(125);


        this.renderTexture_A = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
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

        this.computeShader.SetBuffer(this.kernelIndex_KernelFunction_A,"randData", randDataBuffer);
        //this.computeShader.SetInts("randData", randData);
    }

    // Update is called once per frame
    void Update()
    {
        this.computeShader.Dispatch(this.kernelIndex_KernelFunction_A,
                                   this.renderTexture_A.width / this.kernelThreadSize_KernelFunction_A.x,
                                   this.renderTexture_A.height / this.kernelThreadSize_KernelFunction_A.y,
                                   this.kernelThreadSize_KernelFunction_A.z);

        plane_A.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;
    }


    void OnGUI()
    {
        GUI.TextArea(new Rect(10, 85, 230, 60), " Height at: 0.0 in texbuffer: ");
    }
}
