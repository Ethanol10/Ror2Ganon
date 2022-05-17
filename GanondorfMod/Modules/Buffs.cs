using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GanondorfMod.Modules
{
    public static class Buffs
    {
        // armor buff gained during roll
        internal static BuffDef armorBuff;
        internal static BuffDef triforceBuff;
        internal static BuffDef damageAbsorberBuff;

        internal static List<BuffDef> buffDefs = new List<BuffDef>();

        internal static void RegisterBuffs()
        {
            Sprite shieldSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdHiddenInvincibility.asset").WaitForCompletion().iconSprite;
            armorBuff = AddNewBuff("Super Armor Buff", shieldSprite, Color.white, false, false);
            triforceBuff = AddNewBuff("Triforce Buff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("TriforcePower"), Color.yellow, true, false);
            damageAbsorberBuff = AddNewBuff("Damage Absorber Buff", shieldSprite, Color.yellow, true, false);
        }

        // simple helper method
        internal static BuffDef AddNewBuff(string buffName, Sprite buffIcon, Color buffColor, bool canStack, bool isDebuff)
        {
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = buffName;
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.eliteDef = null;
            buffDef.iconSprite = buffIcon;

            buffDefs.Add(buffDef);

            return buffDef;
        }
    }
}