using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SplitSubMesh : EditorWindow 
{
    [MenuItem("GameObject/SpliteSubMesh")]
    public static void Init()
    {
        Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

        for (int i = 0; i < transforms.Length; ++i)
        {
            GameObject go = transforms[i].gameObject;
            MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>();
            FindSubMesh(mrs[i], go);
        }

        /*

        string[] ids = Selection.assetGUIDs;
        for (int i = 0; i < ids.Length; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(ids[i]);

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path.ToLower());

            MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>();
            FindSubMesh(mrs[i], go);
        }
         * */
    }

    private static void FindSubMesh(MeshRenderer render, GameObject root)
    {
        if (render.sharedMaterials == null || render.sharedMaterials.Length <= 1) return;
        MeshFilter filter = render.gameObject.GetComponent<MeshFilter>();
        Matrix4x4 matrix = render.transform.worldToLocalMatrix;

        GameObject parent = new GameObject(root.name);

        for (int i = 0; i < render.sharedMaterials.Length; ++i)
        {
            GameObject child = new GameObject("submesh" + i.ToString());

            child.transform.parent = parent.transform;

            CombineInstance[] combines = new CombineInstance[1];
            combines[0].lightmapScaleOffset = render.lightmapScaleOffset;
            combines[0].mesh = filter.sharedMesh;
            combines[0].subMeshIndex = i;
            combines[0].transform = matrix * child.transform.localToWorldMatrix;

            MeshRenderer mr = child.AddComponent<MeshRenderer>();
            MeshFilter mf = child.AddComponent<MeshFilter>();
            mr.lightmapIndex = render.lightmapIndex;
            mr.sharedMaterial = render.sharedMaterials[i];
            mf.mesh = new Mesh();
            mf.mesh.CombineMeshes(combines, false, true, true);
        }
    }
}
