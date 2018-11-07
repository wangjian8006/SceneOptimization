using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
        string sceneName = "yaosai_floor";

        string shader = Application.streamingAssetsPath + "/shaders/shaders";
        string path = Application.streamingAssetsPath + "/level/" + sceneName;

        AssetBundle shaderAsset = AssetBundle.LoadFromFile(shader);
        AssetBundle level = AssetBundle.LoadFromFile(path);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

        GameObject go = new GameObject("DontDestory");
        GameObject.DontDestroyOnLoad(go);
        go.AddComponent<TestSceneLoaded>();
	}
}
