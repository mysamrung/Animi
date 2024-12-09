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
            toolbar.Add(loadButton);

            graphView = new AnimiGraphView();
            rootVisualElement.Add(graphView);
        }

        private void Save() {
            AnimiData data = graphView.Save();

            AssetDatabase.CreateAsset(data, "Assets/Test.asset");
            foreach (var nodeData in data.nodeDataObjects) {
                nodeData.name = "node_data";
                AssetDatabase.AddObjectToAsset(nodeData, data);
            }
            AssetDatabase.SaveAssets();
        }
    }

}