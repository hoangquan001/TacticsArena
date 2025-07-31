
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TacticsArena.Core;
using TacticsArena.Battle;
using TacticsArena.Champions;
using UnityEditor.Rendering;

namespace TacticsArena.Core 
{
    public class AttributeManager : MonoBehaviour
    {
        [Header("Attribute Settings")]
        public AttributeData baseAttribute;
        public AttributeData bonusAttribute;

        public void InitChampion(Champion champion)
        {
            baseAttribute = champion.GetBaseAttribute();
        }

        public AttributeData GetAttributeData()
        {
            return baseAttribute + bonusAttribute;
        }
    }
}