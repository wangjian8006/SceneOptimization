using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SceneInfoCollege : Editor
{
    [MenuItem("Tools/SceneCollege")]
    public static void Init()
    {
        //先加载场景初始资源
        //场景名字

        BuildSceneInfoData data = new BuildSceneInfoData();
        data.build();
    }

    [MenuItem("Tools/Text")]
    public static void W()
    {
        UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string path = s.path.Substring(0, s.path.Length - 6) + "_floor.unity";
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(s, path);
        UnityEngine.SceneManagement.Scene s2 = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
        GameObject[] gos = s2.GetRootGameObjects();
        for (int i = 0; i < gos.Length; ++i)
        {
            if (gos[i].name == "Build" ||
                gos[i].name == "Others" ||
                gos[i].name == "Tree" ||
                gos[i].name == "ExtraNode" ||
                gos[i].name == "SceneEffect")
            {
                GameObject.DestroyImmediate(gos[i]);
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(s2);
    }

    class BuildSceneInfoData
    {
        private UnityEngine.SceneManagement.Scene scene;

        private string sceneName;

        private string sceneBundleName;

        class ScenePrefabData
        {
            public Vector3 pos = Vector3.zero;
            public Vector3 scaler = Vector3.zero;
            public Vector3 rotate = Vector3.zero;
            public Vector4 lightMap = Vector4.zero;
            public int lightIndex;
        }

        private Dictionary<string, List<ScenePrefabData>> prefabPos = new Dictionary<string, List<ScenePrefabData>>();

        protected void InitSceneInfo()
        {
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            sceneName = scene.name;
            sceneBundleName = "level/" + scene.name;

            //SetBundleName(scene.path, sceneBundleName);
        }

        public void build()
        {
            InitSceneInfo();

            FindSceneExtra("Build");
            FindSceneExtra("Others");
            FindSceneExtra("Tree");
            FindSceneExtra("ExtraNode");
            FindSceneExtra("SceneEffect");

            //CreateInitScene();
            RecordPrefabTxt();

            CreateFloorScene();
        }

        private void CreateFloorScene()
        {
            UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string path = s.path.Substring(0, s.path.Length - 6) + "_floor.unity";
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(s, path);
            UnityEngine.SceneManagement.Scene s2 = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
            GameObject[] gos = s2.GetRootGameObjects();
            for (int i = 0; i < gos.Length; ++i)
            {
                if (gos[i].name == "Build" ||
                    gos[i].name == "Others" ||
                    gos[i].name == "Tree" ||
                    gos[i].name == "ExtraNode" ||
                    gos[i].name == "SceneEffect")
                {
                    GameObject.DestroyImmediate(gos[i]);
                }
            }
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(s2);
            SetBundleName(s2.path, "level/" + scene.name);
        }

        private string removeExtends(string path)
        {
            int index = path.LastIndexOf(".");
            if (index < 0) return path;
            return path.Substring(0, index);
        }

        /// <summary>
        /// 得到场景的大小
        /// </summary>
        private void CheckSceneInfo(ref Vector3 minPos, ref Vector3 maxPos)
        {
            List<Collider> list = new List<Collider>();
            GameObject[] ArrRoot = scene.GetRootGameObjects();
            for (int i = 0; i < ArrRoot.Length; ++i)
            {
                if (ArrRoot[i].layer == LayerMask.NameToLayer("Floor"))
                {
                    Collider[] arrCollider = ArrRoot[i].GetComponentsInChildren<Collider>();
                    for (int j = 0; j < arrCollider.Length; ++j)
                    {
                        list.Add(arrCollider[j]);
                    }
                }
            }
            if (list.Count > 0)
            {
                Bounds groundBound = new Bounds(Vector3.zero, Vector3.zero);
                for (int i = 0; i < list.Count; i++)
                {
                    groundBound.Encapsulate(list[i].bounds);
                }
                minPos = groundBound.min;
                maxPos = groundBound.max;
            }
        }

        private void RecordPrefabTxt()
        {
            Vector3 minPos = Vector3.zero;
            Vector3 maxPos = Vector3.zero;
            CheckSceneInfo(ref minPos, ref maxPos);
            Vector3 sizePos = maxPos - minPos;
            string txt = "";

            txt += sizePos.x + "\r\n";
            txt += sizePos.z + "\r\n";
            txt += "8\r\n";
            txt += Vector32Str(minPos) + "\r\n";

            foreach (KeyValuePair<string, List<ScenePrefabData>> kv in prefabPos)
            {
                string key = removeExtends(kv.Key);
                for (int i = 0; i < kv.Value.Count; ++i)
                {
                    txt += key + "\t" + 
                        Vector32Str(kv.Value[i].pos) + "\t" + 
                        Vector32Str(kv.Value[i].rotate) + "\t" +
                        Vector32Str(kv.Value[i].scaler) + "\t" +
                        kv.Value[i].lightIndex.ToString() + "\t" +
                        Vector42Str(kv.Value[i].lightMap) + "\r\n";
                }
            }
            //Debug.LogError(txt);

            File.WriteAllText(Application.streamingAssetsPath + "/level/" + this.sceneName + ".info", txt);
        }

        private string Vector32Str(Vector3 vec)
        {
            return vec.x.ToString() + "," + vec.y.ToString() + "," + vec.z.ToString();
        }

        private string Vector42Str(Vector4 vec)
        {
            return vec.x.ToString() + "," + vec.y.ToString() + "," + vec.z.ToString() + "," + vec.w.ToString();
        }

        private void RecordPrefabPosition(string path, GameObject go, Object prefab)
        {
            List<ScenePrefabData> list;
            if (prefabPos.ContainsKey(path) == false)
            {
                list = new List<ScenePrefabData>();
                prefabPos.Add(path, list);
            }else
            {
                list = prefabPos[path];
            }
            ScenePrefabData data = new ScenePrefabData();
            MeshRenderer mr = go.GetComponentInChildren<MeshRenderer>();
            if (mr == null)
            {
                data.lightIndex = -1;
            }
            else
            {
                data.lightIndex = mr.lightmapIndex;
                data.lightMap = mr.lightmapScaleOffset;
            }
            data.scaler = go.transform.localScale;
            data.rotate = go.transform.eulerAngles;
            data.pos = go.transform.position;

            list.Add(data);
        }

        private void FindSceneExtra(string parentNodeName)
        {
            GameObject go = GameObject.Find(parentNodeName);
            if (go == null)
            {
                Debug.LogError("Can't found Node " + parentNodeName);
                return;
            }
            int childLen = go.transform.childCount;

            for (int i = 0; i < childLen; ++i)
            {
                GameObject child = go.transform.GetChild(i).gameObject;
                if (child.activeInHierarchy == false) continue;
                if (PrefabUtility.GetPrefabType(child) != PrefabType.PrefabInstance)
                {
                    continue;
                }
                Object prefab = PrefabUtility.GetPrefabParent(child);

                string path = AssetDatabase.GetAssetPath(prefab);
                if (prefab == null) continue;
                RecordPrefabPosition(path, child, prefab);
            }
        }

        private void SetBundleName(string path, string bundle)
        {
            bundle = removeExtends(bundle);

            AssetImporter import = AssetImporter.GetAtPath(path);
            if (import == null)
            {
                Debug.LogError("find bundle object failed." + path);
                return;
            }

            if (string.IsNullOrEmpty(bundle) == false) import.SetAssetBundleNameAndVariant(bundle.ToLower(), string.Empty);
            else import.SetAssetBundleNameAndVariant(string.Empty, string.Empty);

            import.SaveAndReimport();
        }
    }
}

/*
private int GetBatchType(MeshRenderer mr)
{
    for (int i = 0; i < this.batchType.Count; ++i)
    {
        if (batchLightMapIndex[i] != mr.lightmapIndex) continue;

        Material[] rawmts = this.batchType[i];
        Material[] destmts = mr.sharedMaterials;

        if (rawmts.Length != destmts.Length) continue;
        bool flag = true;
        for (int j = 0; j < rawmts.Length; ++j)
        {
            if (MaterialIsSame(rawmts[j], destmts[j]) == false)
            {
                Debug.Assert(rawmts[j].name != destmts[j].name, "fkkk");
                flag = false;
                break;
            }
        }
        if (flag == true)
        {
            return i;
        }
    }
    this.batchLightMapIndex.Add(mr.lightmapIndex);
    this.batchType.Add(mr.sharedMaterials);
    return this.batchType.Count - 1;
}

private bool MaterialIsSame(Material src, Material dest)
{
    if (src == dest) return true;
    if (src.shader != dest.shader) return false;
    int count = ShaderUtil.GetPropertyCount(src.shader);
    for (int i = 0; i < count; ++i)
    {
        string properyName = ShaderUtil.GetPropertyName(src.shader, i);
        ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(src.shader, i);

        if (type == ShaderUtil.ShaderPropertyType.Color)
        {
            if ( src.GetColor(properyName) != dest.GetColor(properyName))
            {
                return false;
            }
        }else if (type == ShaderUtil.ShaderPropertyType.Float ||
            type == ShaderUtil.ShaderPropertyType.Range)
        {
            if (src.GetFloat(properyName) != dest.GetFloat(properyName))
            {
                return false;
            }
        }
        else if (type == ShaderUtil.ShaderPropertyType.TexEnv)
        {
            if (src.GetTexture(properyName) != dest.GetTexture(properyName) ||
                src.GetTextureOffset(properyName) != dest.GetTextureOffset(properyName) ||
                src.GetTextureScale(properyName) != dest.GetTextureScale(properyName))
            {
                return false;
            }
        }
        else if (type == ShaderUtil.ShaderPropertyType.Vector)
        {
            if (src.GetVector(properyName) != dest.GetVector(properyName))
            {
                return false;
            }
        }
    }
    if (src.name != dest.name)
        Debug.LogError(src.name + "---------------" + dest.name);
    return true;
}

private void CreateInitScene()
{
    GameObject go = GameObject.Find("Floor");
    for (int i = 0; i < go.transform.childCount; ++i)
    {
        GameObject child = go.transform.GetChild(i).gameObject;
        if (PrefabUtility.GetPrefabType(child) != PrefabType.PrefabInstance)
        {
            continue;
        }
        Object prefab = PrefabUtility.GetPrefabParent(child);
        string path = AssetDatabase.GetAssetPath(prefab);

        SetBundleName(path, sceneBundleName);
        if (prefab == null) continue;
        RecordPrefabPosition(path, child, prefab);

        string[] paths = AssetDatabase.GetDependencies(path);

        for (int j = 0; j < paths.Length; ++j)
        {
            SetBundleName(paths[j], sceneBundleName);
        }
    }
}
*/