#if UNITY_EDITOR
using System;
using UnityEngine;

namespace VNEngine.DS.Data.Save
{
    [Serializable]
    public class DSChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}
#endif