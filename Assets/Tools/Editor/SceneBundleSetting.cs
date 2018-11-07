using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SceneBundleSetting
{
    [MenuItem("Tools/SceneBundleSetting")]
    public static void Init()
    {
        SceneBundle scenebundle = new SceneBundle();
        scenebundle.Start();
    }

    public class BundleInfoData
    {
        public int count = 0;
        public string firstName;
    }

    public class SceneBundle
    {
        private Dictionary<string, BundleInfoData> pathBundle = new Dictionary<string, BundleInfoData>();

        private List<string> findedPrefabs = new List<string>();

        public void Start()
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            FindSceneExtra("Build");
            FindSceneExtra("Others");
            FindSceneExtra("Tree");
            FindSceneExtra("ExtraNode");
            FindSceneExtra("SceneEffect");

            SetSceneBundleNames();
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
                if (findedPrefabs.Contains(path) == true) continue;
                findedPrefabs.Add(path);
                RecordPath(path, path);
                if (prefab == null) continue;

                string[] paths = AssetDatabase.GetDependencies(path);
                int pathLen = paths.Length;

                for (int j = 0; j < pathLen; ++j)
                {
                    RecordPath(paths[j], path);
                }
            }
        }

        private void RecordPath(string assetPath, string bundleName)
        {
            if (string.IsNullOrEmpty(assetPath) == true ||
                string.IsNullOrEmpty(bundleName) == true)
            {
                UnityEngine.Debug.Break();
            }

            if (pathBundle.ContainsKey(assetPath) == true)
            {
                pathBundle[assetPath].count++;
            }
            else
            {
                BundleInfoData data = new BundleInfoData();
                data.count = 1;
                data.firstName = bundleName;
                pathBundle.Add(assetPath, data);
            }
        }

        private void SetSceneBundleNames()
        {
            foreach (KeyValuePair<string, BundleInfoData> kv in pathBundle)
            {
                Debug.Assert(kv.Value.count > 0, "Count must greater than zero");
                if (kv.Key.ToLower().Contains("assets/shaders") == true) continue;
                if (kv.Value.count == 1)
                {
                    SetBundleName(kv.Key, kv.Value.firstName);
                }
                else
                {
                    SetBundleName(kv.Key, kv.Key);
                }
            }
        }

        private void SetBundleName(string path, string bundle)
        {
            bundle = removeExtends(bundle);
            //Debug.LogError("{" + path + "}{" + bundle + "}");
            AssetImporter import = AssetImporter.GetAtPath(path);
            if (import == null)
            {
                Debug.LogError("find bundle object failed." + path);
                return;
            }
            /*
            if (path.CompareTo(bundle) != 0)
                Debug.LogError(path + "  ==> " + bundle);
            //return;
            
            if (import.assetBundleName == bundle) return;
            if (string.IsNullOrEmpty(bundle) == true &&
                string.IsNullOrEmpty(import.assetBundleName) == true) return;
            */
            if (string.IsNullOrEmpty(bundle) == false) import.SetAssetBundleNameAndVariant(bundle.ToLower(), string.Empty);
            else import.SetAssetBundleNameAndVariant(string.Empty, string.Empty);

            import.SaveAndReimport();
        }

        private string removeExtends(string path)
        {
            int index = path.LastIndexOf(".");
            if (index < 0) return path;
            return path.Substring(0, index);
        }
    }
} 