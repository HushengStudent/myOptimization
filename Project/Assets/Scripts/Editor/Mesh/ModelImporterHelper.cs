using System.IO;
using UnityEditor;
using UnityEngine;

public class ModelImporterHelper
{
    private static readonly string _meshPath = "Assets/Bundles/Mesh/Models";

    [MenuItem("Assets/myOptimization/ModelImporterHelper/Create Model", false, 0)]
    public static void CopyLoadPath()
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
        importer.optimizeMeshPolygons = true;
        importer.optimizeMeshVertices = true;
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var model = Object.Instantiate(asset);
        var skinnedMeshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length < 1)
        {
            return;
        }

        var name = asset.name;
        var path = $"{_meshPath}/{name}";

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);

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
        }

        PrefabUtility.SaveAsPrefabAsset(model, $"{path}/{name}.prefab");
        Object.DestroyImmediate(model);
    }
}
