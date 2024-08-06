using UnityEngine;

namespace VNEngine.DS.ScriptableObjects
{
    public class DSDialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName {  get; set; }

        public void Initialize(string groupName)
        {
            this.GroupName = groupName;
        }
    }
}
