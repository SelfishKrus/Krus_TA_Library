using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class FFTOcean : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////       Public       //////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////

    public ComputeShader fftComputeShader;

    public int RTResolution;

    public struct SpectrumSettings 
    {
        public float scale;
        public float angle;
        public float spreadBlend;
        public float swell;
        public float alpha;
        public float peakOmega;
        public float gamma;
        public float shortWavesFade; 
    }

    SpectrumSettings[] spectrums = new SpectrumSettings[8];

    [System.Serializable]
    public struct DisplaySpectrumSettings 
    {
        [Range(0, 5)]
        public float scale;
        public float windSpeed;
        [Range(0.0f, 360.0f)]
        public float windDirection;
        public float fetch;
        [Range(0, 1)]
        public float spreadBlend;
        [Range(0, 1)]
        public float swell;
        public float peakEnhancement;
        public float shortWavesFade;
    }

    [Header("Spectrum Settings")]
    public float tileGlobalScale = 1f;
    public float globalScale = 1f;
    public float tessellationFactor = 10.0f;

    [Range(0, 100000)]
    public int seed = 0;

    [Range(0.0f, 0.1f)]
    public float lowCutoff = 0.0001f;

    [Range(0.1f, 9000.0f)]
    public float highCutoff = 9000.0f;

    [Range(0.0f, 20.0f)]
    public float gravity = 9.81f;

    [Range(2.0f, 20.0f)]
    public float depth = 20.0f;

    [Range(0.0f, 200.0f)]
    public float repeatTime = 200.0f;

    [Range(0.0f, 5.0f)]
    public float speed = 1.0f;

    public Vector2 lambda = new Vector2(1.0f, 1.0f);

    [Range(0.0f, 10.0f)]
    public float displacementDepthFalloff = 1.0f;

    public bool updateSpectrum = false;

    [Header("Layer One")]
    [Range(0, 2048)]
    public int lengthScale1 = 256;
    [Range(0.01f, 200.0f)]
    public float tile1 = 8.0f;
    public bool visualizeTile1 = false;
    public bool visualizeLayer1 = false;
    public bool contributeDisplacement1 = true;
    [SerializeField]
    public DisplaySpectrumSettings spectrum1;
    [SerializeField]
    public DisplaySpectrumSettings spectrum2;

    [Header("Layer Two")]
    [Range(0, 2048)]
    public int lengthScale2 = 256;
    [Range(0.01f, 200.0f)]
    public float tile2 = 8.0f;
    public bool visualizeTile2 = false;
    public bool visualizeLayer2 = false;
    public bool contributeDisplacement2 = true;
    [SerializeField]
    public DisplaySpectrumSettings spectrum3;
    [SerializeField]
    public DisplaySpectrumSettings spectrum4;

    [Header("Layer Three")]
    [Range(0, 2048)]
    public int lengthScale3 = 256;
    [Range(0.01f, 200.0f)]
    public float tile3 = 8.0f;
    public bool visualizeTile3 = false;
    public bool visualizeLayer3 = false;
    public bool contributeDisplacement3 = true;
    [SerializeField]
    public DisplaySpectrumSettings spectrum5;
    [SerializeField]
    public DisplaySpectrumSettings spectrum6;

    [Header("Layer Four")]
    [Range(0, 2048)]
    public int lengthScale4 = 256;
    [Range(0.01f, 200.0f)]
    public float tile4 = 8.0f;
    public bool visualizeTile4 = false;
    public bool visualizeLayer4 = false;
    public bool contributeDisplacement4 = true;
    [SerializeField]
    public DisplaySpectrumSettings spectrum7;
    [SerializeField]
    public DisplaySpectrumSettings spectrum8;

    public RenderTexture displacementTextures, slopeTextures, initialSpectrumTextures, pingPongTex, pingPongTex2, spectrumTextures;

    //////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////       Private       /////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////

    private Mesh mesh;
    private Material material;

    private int N, logN, threadGroupsX, threadGroupsY;
    
    private ComputeBuffer spectrumBuffer;
    
    
    //////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////       Function       ////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////
    
    RenderTexture CreateRenderTex(int width, int height, int depth, RenderTextureFormat format, bool useMips) 
    {
        RenderTexture rt = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Linear);
        rt.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.enableRandomWrite = true;
        rt.volumeDepth = depth;
        rt.useMipMap = useMips;
        rt.autoGenerateMips = false;
        rt.enableRandomWrite = true;
        rt.anisoLevel = 16;
        rt.Create();

        return rt;
    }

    void SetFFTUniforms() 
    {
        fftComputeShader.SetVector("_Lambda", lambda);
        fftComputeShader.SetFloat("_FrameTime", Time.time * speed);
        fftComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        fftComputeShader.SetFloat("_Gravity", gravity);
        fftComputeShader.SetFloat("_RepeatTime", repeatTime);
        fftComputeShader.SetInt("_N", N);
        fftComputeShader.SetInt("_Seed", seed);
        fftComputeShader.SetInt("_LengthScale0", lengthScale1);
        fftComputeShader.SetInt("_LengthScale1", lengthScale2);
        fftComputeShader.SetInt("_LengthScale2", lengthScale3);
        fftComputeShader.SetInt("_LengthScale3", lengthScale4);
        // fftComputeShader.SetFloat("_NormalStrength", normalStrength);
        // fftComputeShader.SetFloat("_FoamThreshold", foamThreshold);
        fftComputeShader.SetFloat("_Depth", depth);
        fftComputeShader.SetFloat("_LowCutoff", lowCutoff);
        fftComputeShader.SetFloat("_HighCutoff", highCutoff);
        // fftComputeShader.SetFloat("_FoamBias", foamBias);
        // fftComputeShader.SetFloat("_FoamDecayRate", foamDecayRate);
        // fftComputeShader.SetFloat("_FoamThreshold", foamThreshold);
        // fftComputeShader.SetFloat("_FoamAdd", foamAdd);
    }

    float JonswapAlpha(float fetch, float windSpeed) 
    {
        return 0.076f * Mathf.Pow(gravity * fetch / windSpeed / windSpeed, -0.22f);
    }

    float JonswapPeakFrequency(float fetch, float windSpeed) 
    {
        return 22 * Mathf.Pow(windSpeed * fetch / gravity / gravity, -0.33f);
    }

    void FillSpectrumStruct(DisplaySpectrumSettings displaySettings, ref SpectrumSettings computeSettings) 
    {
        computeSettings.scale = displaySettings.scale;
        computeSettings.angle = displaySettings.windDirection / 180 * Mathf.PI;
        computeSettings.spreadBlend = displaySettings.spreadBlend;
        computeSettings.swell = Mathf.Clamp(displaySettings.swell, 0.01f, 1);
        computeSettings.alpha = JonswapAlpha(displaySettings.fetch, displaySettings.windSpeed);
        computeSettings.peakOmega = JonswapPeakFrequency(displaySettings.fetch, displaySettings.windSpeed);
        computeSettings.gamma = displaySettings.peakEnhancement;
        computeSettings.shortWavesFade = displaySettings.shortWavesFade;
    }

    void SetSpectrumBuffers() 
    {
        FillSpectrumStruct(spectrum1, ref spectrums[0]);
        FillSpectrumStruct(spectrum2, ref spectrums[1]);
        FillSpectrumStruct(spectrum3, ref spectrums[2]);
        FillSpectrumStruct(spectrum4, ref spectrums[3]);
        FillSpectrumStruct(spectrum5, ref spectrums[4]);
        FillSpectrumStruct(spectrum6, ref spectrums[5]);
        FillSpectrumStruct(spectrum7, ref spectrums[6]);
        FillSpectrumStruct(spectrum8, ref spectrums[7]);

        spectrumBuffer.SetData(spectrums);
        fftComputeShader.SetBuffer(0, "_Spectrums", spectrumBuffer);
    }

    void InverseFFT(RenderTexture spectrumTextures) 
    {
        fftComputeShader.SetTexture(3, "_FourierTarget", spectrumTextures);
        fftComputeShader.Dispatch(3, 1, N, 1);
        fftComputeShader.SetTexture(4, "_FourierTarget", spectrumTextures);
        fftComputeShader.Dispatch(4, 1, N, 1);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////       Execution       //////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////
    
    void OnEnable()
    {   
        mesh = GetComponent<MeshFilter>().mesh;
        material = GetComponent<MeshRenderer>().material;

        N = 1024;
        RTResolution = N;
        logN = (int)Mathf.Log(N, 2.0f);
        threadGroupsX = Mathf.CeilToInt(N / 8.0f);
        threadGroupsY = Mathf.CeilToInt(N / 8.0f);

        // Create render textures
        initialSpectrumTextures = CreateRenderTex(N, N, 4, RenderTextureFormat.ARGBHalf, true);
        displacementTextures = CreateRenderTex(N, N, 4, RenderTextureFormat.ARGBHalf, true);
        slopeTextures = CreateRenderTex(N, N, 4, RenderTextureFormat.RGHalf, true);
        spectrumTextures = CreateRenderTex(N, N, 8, RenderTextureFormat.ARGBHalf, true);

        // Instance ComputeBuffer
        spectrumBuffer = new ComputeBuffer(8, 8 * sizeof(float));

        // Transfer params from CPU to GPGPU
        SetFFTUniforms();
        SetSpectrumBuffers();

        // Compute initial JONSWAP spectrum
        fftComputeShader.SetTexture(0, "_InitialSpectrumTextures", initialSpectrumTextures);
        fftComputeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        fftComputeShader.SetTexture(1, "_InitialSpectrumTextures", initialSpectrumTextures);
        fftComputeShader.Dispatch(1, threadGroupsX, threadGroupsY, 1);

        //GC
        spectrumBuffer.Release();
    }

    // Update is called once per frame
    void Update()
    {
        // shader'
        material.SetFloat("_TileGlobalScale", tileGlobalScale);
        material.SetFloat("_GlobalScale", globalScale);
        material.SetFloat("_TessellationFactor", tessellationFactor);

        material.SetFloat("_Tile0", tile1);
        material.SetFloat("_Tile1", tile2);
        material.SetFloat("_Tile2", tile3);
        material.SetFloat("_Tile3", tile4);

        material.SetInt("_DebugTile0", visualizeTile1 ? 1 : 0);
        material.SetInt("_DebugTile1", visualizeTile2 ? 1 : 0);
        material.SetInt("_DebugTile2", visualizeTile3 ? 1 : 0);
        material.SetInt("_DebugTile3", visualizeTile4 ? 1 : 0);

        material.SetInt("_DebugLayer0", visualizeLayer1 ? 1 : 0);
        material.SetInt("_DebugLayer1", visualizeLayer2 ? 1 : 0);
        material.SetInt("_DebugLayer2", visualizeLayer3 ? 1 : 0);
        material.SetInt("_DebugLayer3", visualizeLayer4 ? 1 : 0);

        material.SetInt("_ContributeDisplacement0", contributeDisplacement1 ? 1 : 0);
        material.SetInt("_ContributeDisplacement1", contributeDisplacement2 ? 1 : 0);
        material.SetInt("_ContributeDisplacement2", contributeDisplacement3 ? 1 : 0);
        material.SetInt("_ContributeDisplacement3", contributeDisplacement4 ? 1 : 0);

        SetFFTUniforms();
        if (updateSpectrum) 
        {
            SetSpectrumBuffers();
            fftComputeShader.SetTexture(0, "_InitialSpectrumTextures", initialSpectrumTextures);
            fftComputeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
            fftComputeShader.SetTexture(1, "_InitialSpectrumTextures", initialSpectrumTextures);
            fftComputeShader.Dispatch(1, threadGroupsX, threadGroupsY, 1);
        }
        
        // Progress Spectrum For FFT
        fftComputeShader.SetTexture(2, "_InitialSpectrumTextures", initialSpectrumTextures);
        fftComputeShader.SetTexture(2, "_SpectrumTextures", spectrumTextures);
        fftComputeShader.Dispatch(2, threadGroupsX, threadGroupsY, 1);

        // Compute FFT For Height
        InverseFFT(spectrumTextures);

        // Assemble maps
        fftComputeShader.SetTexture(5, "_DisplacementTextures", displacementTextures);
        fftComputeShader.SetTexture(5, "_SpectrumTextures", spectrumTextures);
        fftComputeShader.SetTexture(5, "_SlopeTextures", slopeTextures);

        fftComputeShader.Dispatch(5, threadGroupsX, threadGroupsY, 1);

        displacementTextures.GenerateMips();
        slopeTextures.GenerateMips();

        material.SetTexture("_DisplacementTextures", displacementTextures);
        material.SetTexture("_SlopeTextures", slopeTextures);

        
    }
}
