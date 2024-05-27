using System;
using System.Collections.Generic;
using TGame.Common;
using UnityEditor;
using UnityEngine;

namespace TGame.Editor.Inspector
{
    /// <summary>
    /// 作者: Teddy
    /// 时间: 2018/03/05
    /// 功能: 
    /// </summary>
    [CustomEditor(typeof(ProcedureModule))]
    public class ProcedureModuleInspector : BaseInspector
    {
        //表示序列化对象中的一个属性或字段的类
        private SerializedProperty proceduresProperty;
        private SerializedProperty defaultProcedureProperty;
        //所有的流程模块的类型
        private List<string> allProcedureTypes;

        protected override void OnInspectorEnable()
        {
            base.OnInspectorEnable();
            //在serializedObject中查找一个名为proceduresNames的属性
            //在序列化对象中查找具有指定名称的属性
            proceduresProperty = serializedObject.FindProperty("proceduresNames");
            //proceduresProperty = serializedObject.FindProperty("isTest");
            defaultProcedureProperty = serializedObject.FindProperty("defaultProcedureName");
            //Debug.Log("::::::::::::::::"+proceduresProperty.boolValue);
            UpdateProcedures();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();
            UpdateProcedures();
        }

        private void UpdateProcedures()
        {
            //来获取BaseProcedure类型的所有子类，并将这些子类的全名（FullName属性）转换为一个字符串列表
            // false 用于指示是否包含接口作为子类
            allProcedureTypes = Utility.Types.GetAllSubclasses(typeof(BaseProcedure), false, Utility.Types.GAME_CSHARP_ASSEMBLY).ConvertAll((Type t) => { return t.FullName; });

            //移除不存在的procedure
            for (int i = proceduresProperty.arraySize - 1; i >= 0; i--)
            {
                //得到流程数组中元素的字符串值
                //GetArrayElementAtIndex 或 FindPropertyRelative 方法来导航到其子属性
                string procedureTypeName = proceduresProperty.GetArrayElementAtIndex(i).stringValue;
                //判断所有的流程类型是否含有这个类型，如果没有就从数组中移除
                if (!allProcedureTypes.Contains(procedureTypeName))
                {
                    proceduresProperty.DeleteArrayElementAtIndex(i);
                }
            }
            //提交对proceduresProperty的修改
            //将所有通过SerializedProperty进行的修改应用到原始的MonoBehaviour或ScriptableObject实例上
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                if (allProcedureTypes.Count > 0)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        for (int i = 0; i < allProcedureTypes.Count; i++)
                        {
                            GUI.changed = false;
                            int? index = FindProcedureTypeIndex(allProcedureTypes[i]);
                            //HasValue 属性来检查该实例是否包含一个值
                            bool selected = EditorGUILayout.ToggleLeft(allProcedureTypes[i], index.HasValue);
                            if (GUI.changed)
                            {
                                if (selected)
                                {
                                    AddProcedure(allProcedureTypes[i]);
                                }
                                else
                                {
                                    RemoveProcedure(index.Value);
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUI.EndDisabledGroup();

            if (proceduresProperty.arraySize == 0)
            {
                if (allProcedureTypes.Count == 0)
                {
                    EditorGUILayout.HelpBox("Can't find any procedure", UnityEditor.MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Please select a procedure at least", UnityEditor.MessageType.Info);
                }
            }
            else
            {
                if (Application.isPlaying)
                {
                    //播放中显示当前状态
                    EditorGUILayout.LabelField("Current Procedure", TGameFramework.Instance.GetModule<ProcedureModule>().CurrentProcedure?.GetType().FullName);
                }
                else
                {
                    //显示默认状态
                    List<string> selectedProcedures = new List<string>();
                    for (int i = 0; i < proceduresProperty.arraySize; i++)
                    {
                        selectedProcedures.Add(proceduresProperty.GetArrayElementAtIndex(i).stringValue);
                    }
                    selectedProcedures.Sort();
                    int defaultProcedureIndex = selectedProcedures.IndexOf(defaultProcedureProperty.stringValue);
                    defaultProcedureIndex = EditorGUILayout.Popup("Default Procedure", defaultProcedureIndex, selectedProcedures.ToArray());
                    if (defaultProcedureIndex >= 0)
                    {
                        defaultProcedureProperty.stringValue = selectedProcedures[defaultProcedureIndex];
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AddProcedure(string procedureType)
        {
            //在数组的开始位置插入一个新元素
            proceduresProperty.InsertArrayElementAtIndex(0);
            proceduresProperty.GetArrayElementAtIndex(0).stringValue = procedureType;
        }

        private void RemoveProcedure(int index)
        {
            string procedureType = proceduresProperty.GetArrayElementAtIndex(index).stringValue;
            if (procedureType == defaultProcedureProperty.stringValue)
            {
                Debug.LogWarning("Can't remove default procedure");
                return;
            }
            proceduresProperty.DeleteArrayElementAtIndex(index);
        }

        private int? FindProcedureTypeIndex(string procedureType)
        {
            for (int i = 0; i < proceduresProperty.arraySize; i++)
            {
                SerializedProperty p = proceduresProperty.GetArrayElementAtIndex(i);
                if (p.stringValue == procedureType)
                {
                    return i;
                }
            }
            return null;
        }
    }
}