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
        var skinnedMeshRenderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!skinnedMeshRenderer)
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

        var mesh = Object.Instantiate(skinnedMeshRenderer.sharedMesh);
        mesh.colors = null;
        mesh.tangents = null;
        //mesh.normals = null;
        mesh.uv3 = null;
        mesh.uv4 = null;

        var meshPath = $"{path}/{name}.asset";
        AssetDatabase.CreateAsset(mesh, meshPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        skinnedMeshRenderer.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        PrefabUtility.SaveAsPrefabAsset(model, $"{path}/{name}.prefab");
        Object.DestroyImmediate(model);
    }
}
