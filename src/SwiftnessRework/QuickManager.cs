using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;
using System;

namespace SwiftnessRework
{
    class QuickManager
    {


        public struct QuickBool
        {
            public WeakReference weakRef;
            public bool Bool;
        }

        static public Dictionary<int, QuickBool> quickDic = new Dictionary<int, QuickBool>();

        public HashSet<string> defaultQuickness;


        public void AddField(object inst, bool val)
        {
            if(!quickDic.ContainsKey(inst.GetHashCode()))
            { 
                quickDic.Add(inst.GetHashCode(), new QuickBool() { weakRef = new WeakReference(inst, false), Bool = val });
            }

        }

        public bool GetVal(object inst)
        {
            if (quickDic.TryGetValue(inst.GetHashCode(), out QuickBool outValue))
            {
                return outValue.Bool;
            }
            else
            {
                UnityEngine.Debug.Log("no value");
                throw new Exception();
            }
        }

        public void SetVal(object inst, bool val)
        {
            if (quickDic.TryGetValue(inst.GetHashCode(), out QuickBool outValue))
            {
                outValue.Bool = val;
                quickDic[inst.GetHashCode()] = outValue;
            }
            else
            {
                UnityEngine.Debug.Log("no value to set");
                throw new Exception();
            }
        }

        public bool SkillGetQuick(Skill inst)
        {
            if (GetVal(inst))
            {
                return true;
            }
            foreach (Skill_Extended se in inst.AllExtendeds)
            {
                if (GetVal(se))
                {
                    return true;
                }
            }
            return false;
        }


        public QuickManager(HashSet<string> defaultQuickness)
        {
            this.defaultQuickness = defaultQuickness;
        }


        public void CullDestroyed()
        {


            //defaultQuickness.Add(GDEItemKeys.Skill_S_Mement_P);

            Debug.Log("quickDic size: " + quickDic.Count);
            foreach (var kv in quickDic.ToArray())
            {
                if (!kv.Value.weakRef.IsAlive)
                {
                    quickDic.Remove(kv.Key);
                }
            }
        }



    }
}
