using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scene00
{
    [ExecuteInEditMode]
    public class DrawSceneDepth : MonoBehaviour
    {
        private Material _SceneMaterial;
        private Camera _MainCamera;
        // Start is called before the first frame update
        void Start()
        {
            //Camera.main.depthTextureMode = DepthTextureMode.Depth;

            _MainCamera = GetComponent<Camera>();
            _MainCamera.depthTextureMode |= DepthTextureMode.Depth;

            _SceneMaterial = new Material(Shader.Find("TiledShadingInUnity/0_SceneDepthShader"));
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_SceneMaterial != null)
                Graphics.Blit(source, destination, _SceneMaterial);
            else
                Graphics.Blit(source, destination);
        }
    }
}