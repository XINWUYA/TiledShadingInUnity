using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scene02
{
    public struct SPointLightInfo
    {
        public Vector3 Position;
        public Vector3 Color;
    }

    //[ExecuteInEditMode]
    public class DeferredShading : MonoBehaviour
    {
        private static RenderTexture[] _GBufferRTs;
        private RenderTexture _ResultTex;
        private RenderBuffer[] _ColorBuffers;
        private RenderBuffer _DepthBuffer;
        private Camera _MainCamera;
        private Camera _RenderCamera;
        private Shader _GBufferShader;
        private Material _SceneMaterial;
        private ComputeShader _TiledDeferredShadingShader;
        private int _TiledDeferredShadingKernel;
        private SPointLightInfo[] _PointLightInfoArray;
        private const int _TotalPointLightNum = 10000;
        private ComputeBuffer _PointLightInfoArrayBuffer;

        // Start is called before the first frame update
        void Start()
        {
            ReformCameras();
            CreateBuffers();
            InitPointLightInfo();

            _GBufferShader = Shader.Find("TiledShadingInUnity/2_GBufferShader");
            _SceneMaterial = new Material(Shader.Find("TiledShadingInUnity/2_DrawSceneShader"));

            _ResultTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
            _ResultTex.enableRandomWrite = true;
            _ResultTex.Create();

            _TiledDeferredShadingShader = (ComputeShader)Resources.Load("2_TiledDeferredShadingShader");
            _TiledDeferredShadingKernel = _TiledDeferredShadingShader.FindKernel("CalcuateResultColor");
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _TiledDeferredShadingShader.SetTexture(_TiledDeferredShadingKernel, Shader.PropertyToID("_ResultTex"), _ResultTex);
            _TiledDeferredShadingShader.SetTexture(_TiledDeferredShadingKernel, Shader.PropertyToID("_MainTex"), _GBufferRTs[0]);
            _TiledDeferredShadingShader.SetTexture(_TiledDeferredShadingKernel, Shader.PropertyToID("_NormalTex"), _GBufferRTs[1]);
            _TiledDeferredShadingShader.SetTexture(_TiledDeferredShadingKernel, Shader.PropertyToID("_PositionTex"), _GBufferRTs[2]);
            _TiledDeferredShadingShader.SetBuffer(_TiledDeferredShadingKernel, Shader.PropertyToID("_PointLightInfoArrayBuffer"), _PointLightInfoArrayBuffer);
            _TiledDeferredShadingShader.SetMatrix(Shader.PropertyToID("_InverseProjectionMatrix"), _MainCamera.projectionMatrix.inverse);
            _TiledDeferredShadingShader.SetInt(Shader.PropertyToID("_TotalPointLightNum"), _TotalPointLightNum);
            _TiledDeferredShadingShader.SetInt(Shader.PropertyToID("_ScreenWidth"), Screen.width);
            _TiledDeferredShadingShader.SetInt(Shader.PropertyToID("_ScreenHeight"), Screen.height);
            _TiledDeferredShadingShader.SetFloat(Shader.PropertyToID("_LightRadius"), 5.0f);
            _TiledDeferredShadingShader.SetFloat(Shader.PropertyToID("_Near"), _MainCamera.nearClipPlane);
            _TiledDeferredShadingShader.SetFloat(Shader.PropertyToID("_Far"), _MainCamera.farClipPlane);
            _TiledDeferredShadingShader.Dispatch(_TiledDeferredShadingKernel, (Screen.width + 15) / 16, (Screen.height + 15) / 16, 1);

            //Deferred Shading
            Graphics.ClearRandomWriteTargets();
            _SceneMaterial.SetTexture("_ResultTex", _ResultTex);

            source = _GBufferRTs[0];
            Graphics.Blit(source, destination, _SceneMaterial);
        }

        void OnPostRender()
        {
            _RenderCamera.SetTargetBuffers(_ColorBuffers, _DepthBuffer);
            _RenderCamera.RenderWithShader(_GBufferShader, "");
        }

        void OnGUI()
        {
            Vector2 size = new Vector2(240, 120);
            float margin = 20;
            GUI.DrawTexture(new Rect(margin, Screen.height - (size.y + margin), size.x, size.y), _GBufferRTs[0], ScaleMode.StretchToFill, false, 1);
            GUI.DrawTexture(new Rect(margin + margin + size.x, Screen.height - (size.y + margin), size.x, size.y), _GBufferRTs[1], ScaleMode.StretchToFill, false, 1);
            GUI.DrawTexture(new Rect(margin + margin + margin + size.x + size.x, Screen.height - (size.y + margin), size.x, size.y), _GBufferRTs[2], ScaleMode.StretchToFill, false, 1);
            //GUI.DrawTexture(new Rect(margin + margin + margin + margin + size.x + size.x, Screen.height - (size.y + margin), size.x, size.y), _ResultTex, ScaleMode.StretchToFill, false, 1);
        }

        void OnDestroy()
        {
            _PointLightInfoArrayBuffer.Release();
            Destroy(_RenderCamera.gameObject);
        }

        void ReformCameras()
        {
            _MainCamera = GetComponent<Camera>();
            _MainCamera.renderingPath = RenderingPath.VertexLit;
            //_MainCamera.cullingMask = 0;
            _MainCamera.clearFlags = CameraClearFlags.Depth;
            _MainCamera.backgroundColor = Color.black;
            _MainCamera.depthTextureMode |= DepthTextureMode.Depth;

            _RenderCamera = new GameObject("RenderCamera").AddComponent<Camera>();
            _RenderCamera.depthTextureMode |= DepthTextureMode.Depth;
            _RenderCamera.enabled = false;
            _RenderCamera.transform.parent = gameObject.transform;
            _RenderCamera.transform.localPosition = Vector3.zero;
            _RenderCamera.transform.localRotation = Quaternion.identity;
            _RenderCamera.renderingPath = RenderingPath.VertexLit;
            _RenderCamera.clearFlags = CameraClearFlags.SolidColor;
            _RenderCamera.farClipPlane = _MainCamera.farClipPlane;
            _RenderCamera.fieldOfView = _MainCamera.fieldOfView;
        }

        void CreateBuffers()
        {
            _GBufferRTs = new RenderTexture[]
            {
            RenderTexture.GetTemporary(Screen.width, Screen.height, 32, RenderTextureFormat.DefaultHDR),
            RenderTexture.GetTemporary(Screen.width, Screen.height, 32, RenderTextureFormat.DefaultHDR),
            RenderTexture.GetTemporary(Screen.width, Screen.height, 32, RenderTextureFormat.DefaultHDR)
            };

            _ColorBuffers = new RenderBuffer[]
            {
            _GBufferRTs[0].colorBuffer,
            _GBufferRTs[1].colorBuffer,
            _GBufferRTs[2].colorBuffer
            };

            _DepthBuffer = _GBufferRTs[1].depthBuffer;
        }

        void InitPointLightInfo()
        {
            Random.InitState(0);//为了保证每次重新运行生成相同的随机数序列
            _PointLightInfoArray = new SPointLightInfo[_TotalPointLightNum];
            for(int i = 0; i < _TotalPointLightNum; ++i)
            {
                _PointLightInfoArray[i].Position = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(-30.0f, 30.0f), Random.Range(-50.0f, 50.0f));
                _PointLightInfoArray[i].Color = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            }

            _PointLightInfoArrayBuffer = new ComputeBuffer(_TotalPointLightNum, sizeof(float) * 6, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            _PointLightInfoArrayBuffer.SetData(_PointLightInfoArray);
        }
    }
}