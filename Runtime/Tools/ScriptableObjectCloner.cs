using UnityEngine;
using System.Reflection;


namespace Animi.Core
{
    public static class ScriptableObjectCloner
    {
        public static T DeepClone<T>(T original) where T : ScriptableObject
        {
            if (original == null) return null;

            // �V�����C���X�^���X�𐶐�
            T clone = Object.Instantiate(original);
            // ���łɃN���[�������I�u�W�F�N�g��ǐՁi�z�Q�Ƒ΍�j
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
                    // List<T> �� T��ScriptableObject�̏ꍇ���Ή�
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
                // ����ȊO�̌^��Instantiate�ŃR�s�[�ς݂Ȃ̂ŉ������Ȃ�
            }
        }
    }
}