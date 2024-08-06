#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VNEngine.DS.Data.Save
{
    public class DSGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<DSGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<DSNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialize(string fileName)
        {
            this.FileName = fileName;

            this.Groups = new List<DSGroupSaveData>();
            this.Nodes = new List<DSNodeSaveData>();
        }
    }
}
#endif