using System;
using System.Collections.Generic;
using UnityEngine;

namespace ADVance.Data
{
    [CreateAssetMenu(menuName = "ADVance/ScenarioData")]
    public class ScenarioData : ScriptableObject
    {
        public List<ScenarioLine> Lines;
    }

    [Serializable]
    public class ScenarioLine
    {
        public int ID;
        public List<int> NextIDs;
        public string CommandName;
        public List<string> Args;
    }
}