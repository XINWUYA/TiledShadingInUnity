using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scene01
{
    public class DeferredShading : MonoBehaviour
    {
        private static RenderTexture[] _GBufferRTs;
        private RenderBuffer[] _ColorBuffers;
        private RenderBuffer _DepthBuffer;
        private Camera _MainCamera;
        private Camera _RenderCamera;
        private Shader _GBufferShader;
        private Material _SceneMaterial;

        // Start is called before the first frame update
        void Start()
        {
            ReformCameras();
            CreateBuffers();

            _GBufferShader = Shader.Find("TiledShadingInUnity/1_GBufferShader");
            _SceneMaterial = new Material(Shader.Find("TiledShadingInUnity/1_DrawSceneShader"));

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            //Graphics.ClearRandomWriteTargets();
            //SceneMaterial.SetInt(Shader.PropertyToID("_UsefulVPLNum"), VPLGenerator.Instance.GetVPLNumInCut());
            _SceneMaterial.SetVector(Shader.PropertyToID("_PointLightPos"), GameObject.Find("Point Light").transform.position);
            ////_material.SetVector(Shader.PropertyToID("_PointLightPosWS"), GameObject.Find("Point Light").transform.position);
            ////_material.SetColor(Shader.PropertyToID("_PointLightColor"), Light.GetLights(LightType.Point, 0)[0].color);

            //SceneMaterial.SetBuffer(_VPLInfoSet4AllObjectsBufferShaderId, _VPLInfoSet4AllObjectsBuffer);
            //Graphics.SetRandomWriteTarget(1, _VPLInfoSet4AllObjectsBuffer, false);

            //SceneMaterial.SetBuffer(_UsefulVPLIndexSetBufferShaderId, _UsefulVPLIndexArrayBuffer);
            //Graphics.SetRandomWriteTarget(2, _UsefulVPLIndexArrayBuffer, false);

            //SceneMaterial.SetBuffer(_Model2WorldMatrixArrayBufferShaderId, _Model2WorldMatrixArrayBuffer);
            //Graphics.SetRandomWriteTarget(3, _Model2WorldMatrixArrayBuffer, false);

            //SceneMaterial.SetBuffer(_VisibilityOfUsefulVPLsBufferShaderId, _VisibilityOfUsefulVPLsBuffer);
            //Graphics.SetRandomWriteTarget(4, _VisibilityOfUsefulVPLsBuffer, false);

            //Matrix4x4[] Object2WorldMatrixArray = VPLGenerator.Instance.GetObject2WorldMatrix4AllObjects();
            //SceneMaterial.SetMatrix(Shader.PropertyToID("_ObjectMatrix00"), Object2WorldMatrixArray[0]);
            //SceneMaterial.SetMatrix(Shader.PropertyToID("_InvObjectMatrix00"), Object2WorldMatrixArray[0].inverse);
            //SceneMaterial.SetMatrix(Shader.PropertyToID("_ObjectMatrix01"), Object2WorldMatrixArray[1]);
            //SceneMaterial.SetMatrix(Shader.PropertyToID("_InvObjectMatrix01"), Object2WorldMatrixArray[1].inverse);
            _SceneMaterial.SetTexture("_MainTex", _GBufferRTs[0]);
            _SceneMaterial.SetTexture("_NormalAndDepthTex", _GBufferRTs[1]);
            _SceneMaterial.SetTexture("_PositionTex", _GBufferRTs[2]);

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
        }

        private void OnDestroy()
        {
            Destroy(_RenderCamera.gameObject);
        }

        void ReformCameras()
        {
            _MainCamera = GetComponent<Camera>();
            _MainCamera.renderingPath = RenderingPath.VertexLit;
            _MainCamera.cullingMask = 0;
            _MainCamera.clearFlags = CameraClearFlags.Depth;
            _MainCamera.backgroundColor = Color.black;

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
            //DepthBuffer = new RenderBuffer();
        }
    }
}