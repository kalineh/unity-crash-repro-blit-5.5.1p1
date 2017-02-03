using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MapCamera))]
public class MapCameraEditor
    : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Refresh"))
        {
            var self = target as MapCamera;
            self.EditorRefreshMap();
        }
    }
}

#endif

//[ExecuteInEditMode]
public class MapCamera : MonoBehaviour {
    
    public Camera MapCam;
    [SerializeField] private RenderTexture LinearDepth;
    [SerializeField] private RenderTexture Texture;
    [SerializeField] private RenderTexture Depth;
    [SerializeField] private RenderTexture TempB;
    [SerializeField] private RenderTexture DepthNormals;

    public static MapCamera Instance;
    [SerializeField] private Material BlurMat;
    [SerializeField] private Material LinearMat;
    [SerializeField] private Material BuildNormalsMat;
    public int Count = 4; 

    public void EditorRefreshMap()
    {
        OnScriptReload();
    }

	void Start()
    {
        OnScriptReload();
    }

    void OnScriptReload()
    {
        Instance = this;

#if UNITY_EDITOR
        if (UnityEngine.Application.isPlaying)
        {
        }
#else
        if (!GameSettings.Instance.EnableCommandCenter)
        {
            MapCam.enabled = false;
            return; 
        }
#endif

        MapCam.enabled = true;
        MapCam.depthTextureMode = DepthTextureMode.DepthNormals;
        MapCam.SetTargetBuffers(Texture.colorBuffer, Depth.depthBuffer);
        MapCam.Render();
        MapCam.enabled = false;

        // normalize depth 
        LinearMat.SetFloat("_ZNear", MapCam.nearClipPlane);
        LinearMat.SetFloat("ZFar", MapCam.farClipPlane);
        Graphics.Blit(Depth, LinearDepth, LinearMat); 

        // passthru 
        BlurMat.SetFloat("_deltaX", 0);
        BlurMat.SetFloat("_deltaY", 0); 
        Graphics.Blit(LinearDepth, Texture, BlurMat);

        // build normals 
        Graphics.Blit(Texture, DepthNormals, BuildNormalsMat);

        // blur 
        BlurMat.SetFloat("_deltaX", 1);
        BlurMat.SetFloat("_deltaY", 0);
        for (var i = 0; i < Count; ++i)
        {
            Graphics.Blit(Texture, TempB, BlurMat);
            Graphics.Blit(TempB, Texture, BlurMat);

            Graphics.Blit(DepthNormals, TempB, BlurMat);
            Graphics.Blit(TempB, DepthNormals, BlurMat);
        }

        BlurMat.SetFloat("_deltaX", 0);
        BlurMat.SetFloat("_deltaY", 1);
        for (var i = 0; i < Count; ++i)
        {
            Graphics.Blit(Texture, TempB, BlurMat);
            Graphics.Blit(TempB, Texture, BlurMat);

            Graphics.Blit(DepthNormals, TempB, BlurMat);
            Graphics.Blit(TempB, DepthNormals, BlurMat);
        }

        // free up unused textures 
        TempB.Release();
        Depth.Release();
        LinearDepth.Release(); 
    }
}
