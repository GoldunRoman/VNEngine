#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNEngine.DS.Data.Error
{
    using Elements;

    public class DSNodeErrorData
    {
        public DSErrorData ErrorData { get; set; }
        public List<DSNode> Nodes { get; set; }

        public DSNodeErrorData() 
        { 
            ErrorData = new DSErrorData();
            Nodes = new List<DSNode>();
        }    
    }
}
#endif