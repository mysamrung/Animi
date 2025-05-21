using Animi.Core;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Animi.Editor {
    public class AnimiEditor : EditorWindow {

        public AnimiGraphView graphView { get; private set; }

        [MenuItem("Animi/Animi Graph")]
        public static void Open() {
            AnimiEditor animiEditor = (AnimiEditor)EditorWindow.GetWindow(typeof(AnimiEditor));
            animiEditor.Show();
        }

        public void OnEnable() {
            CreateGraphView();
        }

        private void CreateGraphView() {
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

        private void Save() {
            var filePath = EditorUtility.SaveFilePanel(
                       "Save scenarioScript as asset",
                       "",
                       "scenarioScript",
                       "asset"
                   );

            if (filePath.Length != 0) {

                filePath = filePath.Remove(0, Application.dataPath.Length);
                filePath = "Assets" + filePath;

                AnimiData data = graphView.Save();
                AssetDatabase.CreateAsset(data, filePath);
                foreach (var nodeData in data.nodeDataObjects) {
                    nodeData.name = "node_data";
                    AssetDatabase.AddObjectToAsset(nodeData, data);
                }

                AssetDatabase.SaveAssets();
            }
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

                AnimiData data = AssetDatabase.LoadAssetAtPath<AnimiData>(filePath);
                graphView.Load(data);
                //opendFileLabel.text = filePath;
            }
        }
    }

}