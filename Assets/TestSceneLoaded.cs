using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class TestSceneLoaded : MonoBehaviour
{
    public class ScenePrefabData
    {
        public Vector3 pos = Vector3.zero;
        public Vector3 scaler = Vector3.zero;
        public Vector3 rotate = Vector3.zero;
        public Vector4 lightMap = Vector4.zero;
        public int lightIndex;
    }

    public class MeshCombianData
    {
        public GameObject go;
        public MeshFilter mf;
        public MeshRenderer mr;
        public List<CombineInstance> combineInstances = new List<CombineInstance>();
    }

    private SimpleAssetLoader simpleLoader;

    protected Dictionary<string, MeshCombianData> batchObj = new Dictionary<string, MeshCombianData>();

    void Start()
    {
        simpleLoader = new SimpleAssetLoader("Android");
        string text = File.ReadAllText(Application.streamingAssetsPath + "/level/yaosai.info");

        string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
        if (lines.Length < 4)
        {
            Debug.LogError("Parse scene info file failed.");
            return;
        }
        int mapWidth = (int)float.Parse(lines[0]);
        int mapHeight = (int)float.Parse(lines[1]);
        int blockSize = (int)float.Parse(lines[2]);
        Vector3 vStartPos = Str2Vec3(lines[3]);

        for (int i = 4; i < lines.Length; ++i)
        {
            string[] col = lines[i].Split(new char[] { '\t' });
            if (col.Length == 6)
            {
                string assetName = col[0];

                ScenePrefabData data = new ScenePrefabData();
                data.pos = Str2Vec3(col[1]);
                data.rotate = Str2Vec3(col[2]);
                data.scaler = Str2Vec3(col[3]);
                data.lightIndex = int.Parse(col[4]);
                data.lightMap = Str2Vec4(col[5]);

                assets.Add(assetName);
                assetsPos.Add(data);
                int t = assetName.LastIndexOf("/");
                names.Add(assetName.Substring(t + 1, assetName.Length - t - 1));
            }
        }

        for (int i = 0; i < assets.Count; ++i)
        {
            simpleLoader.LoadAsset(assets[i], names[i], OnSuccess, i);
        }
    }

    protected void OnSuccess(GameObject res, object param)
    {
        int index = (int) param;

        res.transform.position = assetsPos[index].pos;
        res.transform.eulerAngles = assetsPos[index].rotate;
        res.transform.localScale = assetsPos[index].scaler;

        MeshRenderer[] mrs = res.GetComponentsInChildren<MeshRenderer>();
        if (mrs == null || mrs.Length == 0)
        {
            Debug.Assert(assetsPos[index].lightIndex < 0, "wtfffk");
            GameObject go = GameObject.Instantiate(res);
            go.SetActive(true);
            return;
        }

        for (int i = 0; i < mrs.Length; ++i)
        {
            MeshRenderer meshRenderer = mrs[i];

            Material[] mts = meshRenderer.sharedMaterials;
            for (int j = 0; j < mts.Length; ++j)
            {
                Material t = mts[j];

                MeshCombianData data = null;

                batchObj.TryGetValue(t.name + assetsPos[index].lightIndex.ToString(), out data);

                if (data == null)
                {
                    data = new MeshCombianData();
                    data.go = new GameObject(res.name + i.ToString());

                    data.mr = data.go.AddComponent<MeshRenderer>();
                    data.mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    data.mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    data.mr.receiveShadows = false;
                    data.mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    data.mr.sharedMaterial = t;
                    data.mr.lightmapIndex = assetsPos[index].lightIndex;

                    data.mf = data.go.AddComponent<MeshFilter>();
                    data.mf.mesh = new Mesh();
                    batchObj.Add(t.name + assetsPos[index].lightIndex.ToString(), data);
                }

                Combine(data, data.mr, meshRenderer, assetsPos[index].lightMap, j);
            }
        }
    }

    public void Combine(MeshCombianData data,
        MeshRenderer newMeshRender,
        MeshRenderer needCombines,
        Vector4 lightmapScaleOffset,
        int subMeshIndex)
    {
        Matrix4x4 matrix = newMeshRender.transform.worldToLocalMatrix;

        MeshFilter mf = needCombines.gameObject.GetComponent<MeshFilter>();

        CombineInstance ci = new CombineInstance();
        ci.mesh = mf.sharedMesh;
        ci.subMeshIndex = subMeshIndex;
        ci.lightmapScaleOffset = lightmapScaleOffset;
        ci.transform = matrix * mf.gameObject.transform.localToWorldMatrix;
        data.combineInstances.Add(ci);
        
        //data.mf.mesh.CombineMeshes(data.combineInstances.ToArray(), true, true, true);
        if (waitCombianData.Contains(data) == false) waitCombianData.Add(data);
    }

    List<MeshCombianData> waitCombianData = new List<MeshCombianData>();

    protected List<string> names = new List<string>();
    protected List<string> assets = new List<string>();
    protected List<ScenePrefabData> assetsPos = new List<ScenePrefabData>();

    protected Vector3 Str2Vec3(string str)
    {
        //str = str.Substring(1, str.Length - 2);
        string[] s = str.Split(',');
        return new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
    }

    protected Vector4 Str2Vec4(string str)
    {
        string[] s = str.Split(',');
        return new Vector4(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
    }

    void Update()
    {
        simpleLoader.Update();
        //if (Input.GetKey(KeyCode.A) == false) return;
        if (waitCombianData.Count > 0)
        {
            waitCombianData[0].mf.mesh.Clear();
            waitCombianData[0].mf.mesh.CombineMeshes(waitCombianData[0].combineInstances.ToArray(), true, true, true);
            waitCombianData.RemoveAt(0);
        }
    }
}