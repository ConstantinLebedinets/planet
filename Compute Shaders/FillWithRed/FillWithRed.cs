using System;
using UnityEngine;

public class FillWithRed : MonoBehaviour
{
    public GameObject plane_A;
    public GameObject plane_B;
    public GameObject plane_C;

    public ComputeShader computeShader;

    RenderTexture renderTexture_A;
    RenderTexture renderTexture_B;
    RenderTexture renderTexture_C;

    int kernelIndex_KernelFunction_A;
    int kernelIndex_KernelFunction_B;
    int kernelIndex_KernelFunction_C;

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
    ThreadSize kernelThreadSize_KernelFunction_B;
    ThreadSize kernelThreadSize_KernelFunction_C;

    void Start()
    {
        // enableRandomWrite を有効にして、
        // 書き込み可能な状態のテクスチャを生成することが重要です。

        this.renderTexture_A = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        this.renderTexture_B = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        this.renderTexture_C = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);

        this.renderTexture_A.enableRandomWrite = true;
        this.renderTexture_B.enableRandomWrite = true;
        this.renderTexture_C.enableRandomWrite = true;

        this.renderTexture_A.Create();
        this.renderTexture_B.Create();
        this.renderTexture_C.Create();

        // カーネルのインデックスと、スレッドのサイズを保存します。

        this.kernelIndex_KernelFunction_A = this.computeShader.FindKernel("KernelFunction_A");
        this.kernelIndex_KernelFunction_B = this.computeShader.FindKernel("KernelFunction_B");
        this.kernelIndex_KernelFunction_C = this.computeShader.FindKernel("KernelFunction_C");


        uint threadSizeX, threadSizeY, threadSizeZ;

        this.computeShader.GetKernelThreadGroupSizes
            (this.kernelIndex_KernelFunction_A,
             out threadSizeX, out threadSizeY, out threadSizeZ);

        this.kernelThreadSize_KernelFunction_A
            = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

        this.computeShader.GetKernelThreadGroupSizes
            (this.kernelIndex_KernelFunction_B,
             out threadSizeX, out threadSizeY, out threadSizeZ);

        this.kernelThreadSize_KernelFunction_B
            = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);


        this.computeShader.GetKernelThreadGroupSizes
            (this.kernelIndex_KernelFunction_C,
             out threadSizeX, out threadSizeY, out threadSizeZ);

        this.kernelThreadSize_KernelFunction_C
            = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);
        // ComputeShader で計算した結果を保存するためのバッファとして、ここではテクスチャを設定します。

        this.computeShader.SetTexture
            (this.kernelIndex_KernelFunction_A, "textureBuffer", this.renderTexture_A);
        this.computeShader.SetTexture
            (this.kernelIndex_KernelFunction_B, "textureBuffer", this.renderTexture_B);
        this.computeShader.SetTexture
            (this.kernelIndex_KernelFunction_C, "textureBuffer", this.renderTexture_C);
    }

    void Update()
    {
        // Dispatch メソッドで ComputeShader を実行します。
        // ここで、グループ数は、"解像度 / スレッド数" で算出します。
        // これですべてのピクセルが処理されます。
        // 
        // 例えば 512 / 8 = 64 です。各グループは 64 ピクセルずつ平行に処理します。
        // またグループスレッドは、64 / 8 = 8 ピクセルずつ並行に処理します。

        this.computeShader.Dispatch(this.kernelIndex_KernelFunction_A,
                                    this.renderTexture_A.width / this.kernelThreadSize_KernelFunction_A.x,
                                    this.renderTexture_A.height / this.kernelThreadSize_KernelFunction_A.y,
                                    this.kernelThreadSize_KernelFunction_A.z);

        this.computeShader.Dispatch(this.kernelIndex_KernelFunction_B,
                                    this.renderTexture_B.width / this.kernelThreadSize_KernelFunction_B.x,
                                    this.renderTexture_B.height / this.kernelThreadSize_KernelFunction_B.y,
                                    this.kernelThreadSize_KernelFunction_B.z);

        this.computeShader.Dispatch(this.kernelIndex_KernelFunction_C,
                                    this.renderTexture_C.width / this.kernelThreadSize_KernelFunction_C.x,
                                    this.renderTexture_C.height / this.kernelThreadSize_KernelFunction_C.y,
                                    this.kernelThreadSize_KernelFunction_C.z);


        // 実行して得られた結果をテクスチャとして設定します。

        plane_A.GetComponent<Renderer>().material.mainTexture = this.renderTexture_A;
        plane_B.GetComponent<Renderer>().material.mainTexture = this.renderTexture_B;
        plane_C.GetComponent<Renderer>().material.mainTexture = this.renderTexture_C;

    }
}