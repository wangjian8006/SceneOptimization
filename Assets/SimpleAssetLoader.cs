using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAssetLoader
{
    private AssetBundleManifest abmf;

    private string streammingPath = "";

    public SimpleAssetLoader(string manifestName)
    {
        streammingPath = Application.streamingAssetsPath + "/";
        AssetBundle ab = AssetBundle.LoadFromFile(streammingPath + manifestName);
        abmf = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        this.loadedDic["shaders/shaders"] = new LoaderData();
    }

    class LoaderData
    {
        public string assetName;
        public string assetPath;
        public Action<GameObject, object> callback;
        public object param;
        public GameObject go;
        public AssetBundleCreateRequest request;
    }

    private Dictionary<string, LoaderData> loadedDic = new Dictionary<string, LoaderData>();

    private List<LoaderData> waitings = new List<LoaderData>();

    public void LoadAsset(string path, string name, Action<GameObject, object> callback, object param)
    {
        LoaderData wd = new LoaderData();
        wd.assetName = name;
        wd.assetPath = path;
        wd.callback = callback;
        wd.param = param;
        waitings.Add(wd);
    }

    private AssetBundleCreateRequest request;

    private AssetBundleRequest abrequest;

    private LoaderData nowData;

    public void Update()
    {
        if (nowData == null)
        {
            if (waitings.Count == 0) return;
            nowData = waitings[0];

            //查找是否已经加载
            if (loadedDic.ContainsKey(nowData.assetPath))
            {
                
                if (nowData.callback != null)
                {
                    //Debug.LogError("wtf.");
                    //nowData.callback(loadedDic[nowData.assetPath].go, nowData.param);
                    request = loadedDic[nowData.assetPath].request;
                    nowData.request = request;
                }
                else
                {
                    waitings.RemoveAt(0);
                    nowData = null;
                }
                return;
            }

            //find depend
            string[] str = abmf.GetAllDependencies(nowData.assetPath);
            if (str == null || str.Length == 0)
            {
                //没有依赖，直接加载本资源
                string path = streammingPath + nowData.assetPath;
                request = AssetBundle.LoadFromFileAsync(path.ToLower());
                nowData.request = request;
            }
            else
            {
                //有依赖，把依赖加入队首
                bool flag = false;
                for (int i = 0; i < str.Length; ++i)
                {
                    if (loadedDic.ContainsKey(str[i]) == true) continue;
                    flag = true;
                    LoaderData data = new LoaderData();
                    data.assetPath = str[i];
                    int t = data.assetPath.LastIndexOf("/");
                    data.assetName = data.assetPath.Substring(t + 1, data.assetPath.Length - t - 1);
                    waitings.Insert(0, data);
                }
                if (flag == false)
                {
                    string path = streammingPath + nowData.assetPath;
                    request = AssetBundle.LoadFromFileAsync(path.ToLower());
                    nowData.request = request;
                }
                else
                {
                    nowData = null;
                    return;
                }
            }
        }

        if (request != null && request.isDone == true && abrequest == null)
        {
            if (nowData.callback == null)
            {
                loadedDic[nowData.assetPath] = nowData;
                abrequest = null;
                request = null;
                nowData = null;
                this.waitings.RemoveAt(0);
            }
            else
            {
                abrequest = request.assetBundle.LoadAssetAsync(nowData.assetName, typeof(GameObject));
            }
        }
 
        if (request != null && request.isDone == true && abrequest != null && abrequest.isDone == true)
        {
            if (nowData.callback != null)
            {
                nowData.callback(abrequest.asset as GameObject, nowData.param);
            }

            loadedDic[nowData.assetPath] = nowData;

            abrequest = null;
            request = null;
            nowData = null;
            this.waitings.RemoveAt(0);
        }
    }
}