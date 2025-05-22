using UnityEngine;
using System.Reflection;


namespace Animi.Core
{
    public static class ScriptableObjectCloner
    {
        public static T DeepClone<T>(T original) where T : ScriptableObject
        {
            if (original == null) return null;

            // 新しいインスタンスを生成
            T clone = Object.Instantiate(original);
            // すでにクローンしたオブジェクトを追跡（循環参照対策）
            var visited = new System.Collections.Generic.Dictionary<ScriptableObject, ScriptableObject>();
            visited[original] = clone;

            DeepCloneFields(original, clone, visited);
            return clone;
        }

        private static void DeepCloneFields(ScriptableObject original, ScriptableObject clone, System.Collections.Generic.Dictionary<ScriptableObject, ScriptableObject> visited)
        {
            var fields = original.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                {
                    var child = field.GetValue(original) as ScriptableObject;
                    if (child != null)
                    {
                        if (visited.TryGetValue(child, out var existingClone))
                        {
                            field.SetValue(clone, existingClone);
                        }
                        else
                        {
                            var childClone = Object.Instantiate(child);
                            visited[child] = childClone;
                            field.SetValue(clone, childClone);
                            DeepCloneFields(child, childClone, visited);
                        }
                    }
                }
                else if (typeof(System.Collections.IList).IsAssignableFrom(field.FieldType) && field.FieldType.IsGenericType)
                {
                    // List<T> で TがScriptableObjectの場合も対応
                    var list = field.GetValue(original) as System.Collections.IList;
                    if (list != null)
                    {
                        var listClone = field.GetValue(clone) as System.Collections.IList;
                        for (int i = 0; i < list.Count; i++)
                        {
                            var item = list[i] as ScriptableObject;
                            if (item != null)
                            {
                                if (visited.TryGetValue(item, out var existingClone))
                                {
                                    listClone[i] = existingClone;
                                }
                                else
                                {
                                    var itemClone = Object.Instantiate(item);
                                    visited[item] = itemClone;
                                    listClone[i] = itemClone;
                                    DeepCloneFields(item, itemClone, visited);
                                }
                            }
                        }
                    }
                }
                // それ以外の型はInstantiateでコピー済みなので何もしない
            }
        }
    }
}