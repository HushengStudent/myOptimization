/********************************************************************************
** auth:  https://github.com/HushengStudent
** date:  2022/02/05 01:51:30
** desc:  Mesh/Animator/AnimationClip
*********************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ModelImporterHelper
{
    private static readonly string _meshPath = "Assets/Bundles/Mesh/Models";

    [MenuItem("Assets/myOptimization/ModelImporterHelper/Create Model", false, 0)]
    public static void CreateModel()
    {
        var targets = Selection.objects;
        if (targets.Length > 1 || targets.Length < 1)
        {
            return;
        }

        var asset = targets[0] as GameObject;
        if (!asset)
        {
            return;
        }

        var assetPath = AssetDatabase.GetAssetPath(asset);
        var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        importer.isReadable = false;
        importer.meshOptimizationFlags = MeshOptimizationFlags.Everything;
        importer.optimizeGameObjects = false;
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var model = Object.Instantiate(asset);
        var skinnedMeshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length < 1)
        {
            return;
        }

        var name = asset.name.Split('@')[0];
        var path = $"{_meshPath}/{name}";

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            AssetDatabase.Refresh();
        }
        Directory.CreateDirectory(path);

        //Mesh
        foreach (var smr in skinnedMeshRenderers)
        {
            var mesh = Object.Instantiate(smr.sharedMesh);
            mesh.colors = null;
            //mesh.tangents = null;
            //mesh.normals = null;
            mesh.uv3 = null;
            mesh.uv4 = null;

            var meshName = smr.name;
            var meshPath = $"{path}/{meshName}.asset";
            var index = 0;
            while (File.Exists(meshPath))
            {
                index++;
                meshPath = $"{path}/{meshName}_{index}.asset";
            }
            AssetDatabase.CreateAsset(mesh, meshPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            smr.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            /*
            smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            smr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            smr.skinnedMotionVectors = false;
            smr.receiveShadows = false;
            smr.sharedMaterial = null;
            */
        }

        //Avatar
        var animator = model.GetComponent<Animator>();
        if (animator)
        {
            var avatar = animator.avatar;
            if (avatar)
            {
                var avatarName = avatar.name;
                var avatarPath = $"{path}/{avatarName}.asset";
                var newAvatar = Object.Instantiate(avatar);
                AssetDatabase.CreateAsset(newAvatar, avatarPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                animator.avatar = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
            }

            //OptimizeTransformHierarchy
            var transforms = model.GetComponentsInChildren<Transform>(true);
            var list = new List<string>();
            foreach (var trans in transforms)
            {
                if (!trans.name.StartsWith("Bip"))
                {
                    list.Add(trans.name);
                }
            }
            AnimatorUtility.OptimizeTransformHierarchy(model, list.ToArray());
        }

        PrefabUtility.SaveAsPrefabAsset(model, $"{path}/{name}.prefab");
        Object.DestroyImmediate(model);
    }

    [MenuItem("Assets/myOptimization/ModelImporterHelper/Create Animation", false, 1)]
    public static void CreateAnimation()
    {
        var targets = Selection.objects;
        if (targets.Length > 1 || targets.Length < 1)
        {
            return;
        }

        var asset = targets[0] as GameObject;
        if (!asset)
        {
            return;
        }

        var assetPath = AssetDatabase.GetAssetPath(asset);
        var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        importer.animationCompression = ModelImporterAnimationCompression.Optimal;
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
        if (!animationClip)
        {
            return;
        }

        var name = asset.name.Split('@')[0];
        var path = $"{_meshPath}/{name}/Animation";
        /*
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);
        */
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var animationClipName = animationClip.name;
        var animationClipPath = $"{path}/{animationClipName}.anim";
        var newAnimationClip = Object.Instantiate(animationClip);

        if (File.Exists(animationClipPath))
        {
            AssetDatabase.DeleteAsset(animationClipPath);
            AssetDatabase.Refresh();
        }

        try
        {
            //delete scale info
            foreach (var curveBinding in AnimationUtility.GetCurveBindings(newAnimationClip))
            {
                var propertyName = curveBinding.propertyName.ToLower();
                if (propertyName.Contains("m_localscale"))
                {
                    AnimationUtility.SetEditorCurve(newAnimationClip, curveBinding, null);
                    continue;
                }

                if (propertyName.Contains("m_localrotation") || propertyName.Contains("m_localposition"))
                {
                    //transform float decimal point
                    var animationCurve = AnimationUtility.GetEditorCurve(newAnimationClip, curveBinding);
                    if (animationCurve == null || animationCurve.keys == null)
                    {
                        continue;
                    }
                    var keyFrames = animationCurve.keys;
                    for (var i = 0; i < keyFrames.Length; i++)
                    {
                        var key = keyFrames[i];
                        key.value = float.Parse(key.value.ToString("f3"));
                        key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                        key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                        keyFrames[i] = key;
                    }
                    animationCurve.keys = keyFrames;
                    AnimationUtility.SetEditorCurve(newAnimationClip, curveBinding, animationCurve);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Compress animationClip failed,animationPath:{assetPath} error:{e}.");
        }

        var serializedObject = new SerializedObject(newAnimationClip);
        serializedObject.FindProperty("m_EditorCurves").arraySize = 0;
        serializedObject.FindProperty("m_EulerEditorCurves").arraySize = 0;
        serializedObject.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(newAnimationClip, animationClipPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}