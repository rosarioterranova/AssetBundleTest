using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AssetBundleManager : MonoBehaviour
{

    enum AssetType { Scene, Prefab };

    [SerializeField] string url;
    [SerializeField] string assetToDownloadName = "scene1";
    [SerializeField] string bundleToSaveName = "AssetBundle";
    [SerializeField] AssetType assetType = AssetType.Scene;

    string assetPath = string.Empty;

    void Start()
    {
        //Construct path to save it
        assetPath = Path.Combine(Application.persistentDataPath, "AssetData"); 
        assetPath = Path.Combine(assetPath, bundleToSaveName);
    }

    public void StartDownloadAsset()
    {
        if(File.Exists(assetPath))
        {
            Debug.Log("[ASSET BUNDLE MANAGER] Asset is already in persistent directory");
        }
        else
        {
            StartCoroutine(DownloadAsset());
        }
    }

    public void StartLoadAsset()
    {
        if (File.Exists(assetPath))
        {
            StartCoroutine(LoadAsset());
        }
        else
        {
            Debug.Log("[ASSET BUNDLE MANAGER] Error: no data");
        }
    }

    public void DeleteAsset()
    {
        if (File.Exists(assetPath))
        {
            File.Delete(assetPath);
            Debug.Log("[ASSET BUNDLE MANAGER] Asset Deleted");
        }
        else
        {
            Debug.Log("[ASSET BUNDLE MANAGER] No file to delete!");
        }
    }

    IEnumerator DownloadAsset()
    {
        //Create request
        UnityWebRequest www = UnityWebRequest.Get(url);     
        DownloadHandler handle = www.downloadHandler;
        yield return www.SendWebRequest();
        Debug.Log("[ASSET BUNDLE MANAGER] Download started");

        //Download asset
        if (www.isNetworkError)
        {
            Debug.Log("[ASSET BUNDLE MANAGER] Error while Downloading Data: " + www.error);
        }
        else
        {
            Debug.Log("[ASSET BUNDLE MANAGER] Download success, asset bundle is in cache");
        }

        //Saving asset
        Debug.Log("[ASSET BUNDLE MANAGER] Start saving asset");
        if (!Directory.Exists(Path.GetDirectoryName(assetPath)))  //create the Directory if it does not exist
        {
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            Debug.Log("[ASSET BUNDLE MANAGER] Created directory " + Path.GetDirectoryName(assetPath));
        }
        try
        {
             File.WriteAllBytes(assetPath, handle.data);
             Debug.Log("[ASSET BUNDLE MANAGER] Saved Data to: " + assetPath.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ASSET BUNDLE MANAGER] Failed To Save Data to: " + assetPath.Replace("/", "\\"));
            Debug.LogWarning("[ASSET BUNDLE MANAGER] Error: " + e.Message);
        }
    }

    IEnumerator LoadAsset()
    {
        Debug.Log("[ASSET BUNDLE MANAGER] Start loading asset");

        AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(assetPath); //Load bundle from persistent path
        yield return bundle;

        AssetBundle myLoadedAssetBundle = bundle.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("[ASSET BUNDLE MANAGER] Failed to load AssetBundle");
            yield break;
        }

        Debug.Log("[ASSET BUNDLE MANAGER] Getting prefab");
        if (assetType == AssetType.Scene) //TEST SCENE
        { 
            Debug.Log("[ASSET BUNDLE MANAGER] Loading Scene");

            string[] scenePath = myLoadedAssetBundle.GetAllScenePaths();
            AsyncOperation async = SceneManager.LoadSceneAsync(scenePath[0], LoadSceneMode.Additive);
            yield return async;

        }
        else if (assetType == AssetType.Prefab)
        {
            AssetBundleRequest request = myLoadedAssetBundle.LoadAssetAsync<GameObject>(assetToDownloadName); //Load prefab
            yield return request;

            Debug.Log("[ASSET BUNDLE MANAGER] Instating prefab");

            GameObject obj = request.asset as GameObject; //instatiate prefab
            obj.transform.position = new Vector3(0f, 0f, 0f);
            obj.transform.Rotate(0f, 0f, 0f);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            Instantiate(obj);
        }
        myLoadedAssetBundle.Unload(false);
    }
}
