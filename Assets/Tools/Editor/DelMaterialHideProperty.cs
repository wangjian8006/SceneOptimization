using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DelMaterialHideProperty
{
    [MenuItem("ResourceCleanup/DelMaterialHideProperty")]
    public static void CheckMaterial()
    {
        if (EditorUtility.DisplayDialog("DelMaterialHideProperty", "Do you want to clear material unuserd properity?", "Yes", "No") == false) return;

        string[] ids = AssetDatabase.FindAssets("t:material");
        for (int i = 0; i < ids.Length; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(ids[i]);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            MaterialCleaner(mat);

            EditorUtility.DisplayProgressBar("DelMaterialHideProperty", "Check " + mat.name, (float)i / (float)ids.Length);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void MaterialCleaner(Material mat)
    {
        if (mat)
        {
            SerializedObject matInfo = new SerializedObject(mat);
            SerializedProperty propArr = matInfo.FindProperty("m_SavedProperties");

            propArr.Next(true);
            do
            {
                if (!propArr.isArray) continue;
                for (int i = propArr.arraySize - 1; i >= 0; --i)
                {
                    var p1 = propArr.GetArrayElementAtIndex(i);
                    if (p1.isArray)
                    {
                        for (int ii = p1.arraySize - 1; ii >= 0; --ii)
                        {
                            var p2 = p1.GetArrayElementAtIndex(ii);
                            var val = p2.FindPropertyRelative("first");
                            if (!mat.HasProperty(val.stringValue))
                            {
                                Debug.Log("remove " + mat.name + "," + val.stringValue);
                                p1.DeleteArrayElementAtIndex(ii);
                            }
                        }
                    }
                    else
                    {
                        var val = p1.FindPropertyRelative("first");
                        if (!mat.HasProperty(val.stringValue))
                        {
                            Debug.Log("remove " + mat.name + "," + val.stringValue);
                            propArr.DeleteArrayElementAtIndex(i);
                        }
                    }
                }
            } while (propArr.Next(false));

            matInfo.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}