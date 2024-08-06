using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNEngine.DS.Data
{
    using ScriptableObjects;

    public class DSDialogueChioceData
    {
        [field: SerializeField] public string Text {  get; set; }
        [field: SerializeField] public DSDialogueSO NextDialogue { get; set; }
    }
}
