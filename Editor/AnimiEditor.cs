using Animi.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Animi.Editor
{
    public class AnimiEditor : EditorWindow
    {

        public AnimiGraphView graphView { get; private set; }

        [MenuItem("Animi/Animi Graph")]
        public static void Open()
        {
            AnimiEditor animiEditor = (AnimiEditor)EditorWindow.GetWindow(typeof(AnimiEditor));
            animiEditor.Show();
        }

        public void OnEnable()
        {
            CreateGraphView(); 
        }

        private void CreateGraphView()
        {
            var toolbar = new Toolbar();
            rootVisualElement.Add(toolbar);

            var saveButton = new ToolbarButton();
            saveButton.text = "Save";
            saveButton.clicked += Save;
            toolbar.Add(saveButton);

            var loadButton = new ToolbarButton();
            loadButton.text = "Load";
            loadButton.clicked += Load;
            toolbar.Add(loadButton); 

            graphView = new AnimiGraphView();
            rootVisualElement.Add(graphView);
        }


        private void Load()
        {
            var filePath = EditorUtility.OpenFilePanel(
                         "Open scenarioScript",
                         "scenarioScript",
                         "asset"
                     );

            if (filePath.Length != 0)
            {
                filePath = filePath.Remove(0, Application.dataPath.Length);
                filePath = "Assets" + filePath;

                LoadAtPath(filePath);
            }
        }

        private void Save()
        {
            var filePath = EditorUtility.SaveFilePanel(
                "Save",
                "",
                "AnimiController",
                "asset"
            );

            if (filePath.Length != 0)
            {
                filePath = filePath.Remove(0, Application.dataPath.Length);
                filePath = "Assets" + filePath;

                SaveAtPath(filePath);
            }
        }

        private void LoadAtPath(string path)
        {
            AnimiData animiAssetData = AssetDatabase.LoadMainAssetAtPath(path) as AnimiData;
            graphView.Load(animiAssetData);
        }

        private void SaveAtPath(string path)
        {
            AnimiData animiAssetData = AssetDatabase.LoadMainAssetAtPath(path) as AnimiData;
            AnimiData graphAnimiData = graphView.Save();

            if (animiAssetData != null)
            {
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in allAssets)
                {
                    if (asset != null && asset != animiAssetData)
                    {
                        Object.DestroyImmediate(asset, true);
                    }
                }

                string originalName = animiAssetData.name;
                EditorUtility.CopySerialized(graphAnimiData, animiAssetData);
                animiAssetData.name = originalName;
            }
            else
            {
                AssetDatabase.CreateAsset(animiAssetData, path);
                animiAssetData = graphAnimiData;
            }

            graphAnimiData.name = graphAnimiData.name;

            foreach (var nodeData in graphAnimiData.nodeDataObjects)
            {
                nodeData.name = "node_data";
                AssetDatabase.AddObjectToAsset(nodeData, animiAssetData);
            }
            foreach (var edgeData in graphAnimiData.edgeDataObjects)
            {
                edgeData.name = "edge_data";
                AssetDatabase.AddObjectToAsset(edgeData, animiAssetData);
            }

            AssetDatabase.SaveAssets();
        }
    }

}