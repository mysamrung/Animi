using NUnit.Framework.Internal;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public class AnimationExtractor : MonoBehaviour
{
    [MenuItem("Assets/ExtractAnimation")]
    public static void Init() {
        foreach(string guid in Selection.assetGUIDs) {
            Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GUIDToAssetPath(guid));
            foreach (Object asset in subAssets) {
                if (asset is AnimationClip) {
                    string destPath = "Assets/" + asset.name + ".anim";
                    long replaceFileID = -1;
                    if(AssetDatabase.AssetPathExists(destPath)) {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string _, out replaceFileID);
                    }

                    ExtractFromAsset(asset, destPath);

                    if(replaceFileID != -1) {
                        string assetPath = Application.dataPath.Replace("Assets", "") + destPath;
                        string[] data = System.IO.File.ReadAllLines(assetPath);
                        for(int i = 0; i < data.Length; i++) {
                            if (data[i][0] == '-' && data[i][1] == '-' && data[i][2] == '-') {
                                string[] split = data[i].Split(' ');
                                split[2] = "&" + replaceFileID;

                                data[i] = split[0] + " " + split[1] + " " + split[2];
                                break;
                            }
                        }
                        System.IO.File.WriteAllLines(assetPath, data);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    public static void ExtractFromAsset(Object subAsset, string destinationPath) {
        string assetPath = AssetDatabase.GetAssetPath(subAsset);

        var clone = Object.Instantiate(subAsset);
        AssetDatabase.CreateAsset(clone, destinationPath);

        var assetImporter = AssetImporter.GetAtPath(assetPath);
        assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(subAsset), clone);

        AssetDatabase.WriteImportSettingsIfDirty(assetPath);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }
}
