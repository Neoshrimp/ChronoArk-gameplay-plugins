using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwiftnessRework
{
    public class QuickManager : FieldManager<string, bool>
    {

        public HashSet<string> defaultQuickness;

        public QuickManager(HashSet<string> defaultQuickness, int cap) : base(cap)
        {
            this.defaultQuickness = defaultQuickness;
        }

        public override string ComputeKey(object inst)
        {
            var key = inst.GetHashCode().ToString();
/*            if (inst is Skill skill)
                key = key + skill.MySkill.KeyID;*/

            if (inst is Skill_Extended se)
            {
                key = key + se.GetType().Name;
            }
            return key;
        }

        public bool SkillGetQuick(Skill inst)
        {
            if (GetVal(inst))
            {
                return true;
            }
            foreach (Skill_Extended se in inst.AllExtendeds)
            {
                if (ExtendedGetQuick(se))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ExtendedGetQuick(Skill_Extended se)
        {
            return GetVal(se);
        }

    }
}
