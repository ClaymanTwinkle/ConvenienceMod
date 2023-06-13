using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.QuicklyCreateCharacter.Data
{
    internal class RollFilterConstant
    {
        public static string[] TypeList = new string[]
        {
            "主要属性",
            "技艺资质",
            "功法资质"
        };

        public static string[][] KeyList = new string[][]
        {
            CharacterDataTool.MainAttributeNameArray,
            CharacterDataTool.LifeSkillNameArray,
            CharacterDataTool.CombatSkillNameArray
        };
    }
}
