using UnityEngine;
using Landfall.TABS;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Landfall.TABS.AI.Components.Modifiers;
using Landfall.TABS.AI.Components.Tags;

namespace SummonerCreator
{
    public class SCModTools
    {
        public static GameObject GetUnitBase(string name)
        {
            // Use this when you don't want to create a new one, and just grab the existing item.
            List<GameObject> unitBase = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("UnitBases", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LandfallUnitDatabase.GetDatabase());
            return unitBase.Find(x => x.name == name);
        }
        public static GameObject GetProp(string name)
        {
            // Use this when you don't want to create a new one, and just grab the existing item.
            List<GameObject> prop = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("CharacterProps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LandfallUnitDatabase.GetDatabase());
            return prop.Find(x => x.name == name);
        }
        public static GameObject GetWeapon(string name)
        {
            List<GameObject> weapon = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("Weapons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LandfallUnitDatabase.GetDatabase());
            return weapon.Find(x => x.name == name);
        }
        public static GameObject GetAbility(string name)
        {
            List<GameObject> ability = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("CombatMoves", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LandfallUnitDatabase.GetDatabase());
            return ability.Find(x => x.name == name);
        }
        public static GameObject GetProjectile(string name)
        {
            List<GameObject> projectile = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("Projectiles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LandfallUnitDatabase.GetDatabase());
            return projectile.Find(x => x.name == name);
        }
        public static GameObject GetExplosion(string name)
        {
            List<GameObject> explosion = Resources.FindObjectsOfTypeAll<GameObject>().ToList();
            return explosion.Find(x => x.name == name && x.GetComponent<Explosion>());
        }
        public static GameObject GetEffect(string name)
        {
            List<GameObject> effect = Resources.FindObjectsOfTypeAll<GameObject>().ToList();
            return effect.Find(x => x.name == name && x.GetComponent<UnitEffectBase>());
        }
        public static Sprite GetIcon(string name)
        {
            List<Sprite> sprite = Resources.FindObjectsOfTypeAll<Sprite>().ToList();
            return sprite.Find(x => x.name == name);
        }
        public static UnitBlueprint GetUnit(string name)
        {
            List<UnitBlueprint> unit = (List<UnitBlueprint>)typeof(LandfallUnitDatabase).GetField("Units", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LandfallUnitDatabase.GetDatabase());
            return unit.Find(x => x.name == name);
        }
        public static Faction GetFaction(string name)
        {
            List<Faction> faction = (List<Faction>)typeof(LandfallUnitDatabase).GetField("Factions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LandfallUnitDatabase.GetDatabase());
            return faction.Find(x => x.name == name);
        }
        public static GameObject CreateUnitBase(string name)
        {
            var obj = Object.Instantiate(GetUnitBase(name), AssetPool.transform);
            SetFlagsChildren(obj.transform, HideFlags.HideAndDontSave);
            return obj;
        }
        public static GameObject CreateProp(string name)
        {
            var obj = Object.Instantiate(GetProp(name), AssetPool.transform);
            SetFlagsChildren(obj.transform, HideFlags.HideAndDontSave);
            return obj;
        }
        public static GameObject CreateWeapon(string name)
        {
            var obj = Object.Instantiate(GetWeapon(name), AssetPool.transform);
            SetFlagsChildren(obj.transform, HideFlags.HideAndDontSave);
            return obj;
        }
        public static GameObject CreateAbility(string name)
        {
            var obj = Object.Instantiate(GetAbility(name), AssetPool.transform);
            SetFlagsChildren(obj.transform, HideFlags.HideAndDontSave);
            return obj;
        }
        public static GameObject CreateProjectile(string name)
        {
            var obj = Object.Instantiate(GetProjectile(name), AssetPool.transform);
            SetFlagsChildren(obj.transform, HideFlags.HideAndDontSave);
            return obj;
        }
        public static GameObject CreateExplosion(string name)
        {
            var obj = Object.Instantiate(GetExplosion(name), AssetPool.transform);
            SetFlagsChildren(obj.transform, HideFlags.HideAndDontSave);
            return obj;
        }
        public static GameObject CreateEffect(string name)
        {
            var obj = Object.Instantiate(GetEffect(name), AssetPool.transform);
            SetFlagsChildren(obj.transform, HideFlags.HideAndDontSave);
            return obj;
        }

        public static UnitBlueprint CloneUnit(string name, UnitBlueprint blueprintBase)
        {
            // This code sets up units. I wouldn't recommend touching this unless you absolutely know what you are doing. Worse case, you can redownload the mod to fix it.
            var unit = Object.Instantiate(blueprintBase);
            unit.name = name;
            unit.Entity.Name = name;
            unit.Entity.GenerateNewID();
            LandfallUnitDatabase.GetDatabase().AddUnitWithID(unit);
            return unit;
        }

        public static UnitBlueprint CreateUnit(string name, float health, uint cost, GameObject unitBase, Sprite icon, bool ranged = false, bool targetFriends = false)
        {
            // This code sets up units. I wouldn't recommend touching this unless you absolutely know what you are doing. Worse case, you can redownload the mod to fix it.
            var unit = Object.Instantiate(LandfallUnitDatabase.GetDatabase().m_unitEditorBlueprint);
            unit.name = name;
            unit.Entity.Name = name;
            unit.Entity.GenerateNewID();
            unit.health = health;
            unit.UnitBase = unitBase;
            unit.forceCost = cost;
            unit.Entity.SpriteIcon = icon;
            unit.movementSpeedMuiltiplier = 1f;
            unit.animationMultiplier = 1f;
            unit.stepMultiplier = 1f;
            unit.balanceMultiplier = 1000f;
            unit.balanceForceMultiplier = 2.5f;
            unit.holdinigWithTwoHands = false;
            if (ranged)
            {
                unit.MovementComponents = new List<IMovementComponent>()
                {
                    default(KeepRangedDistance)
                };
            }
            else
            {
                unit.MovementComponents = new List<IMovementComponent>()
                {
                    default(KeepPreferredDistance)
                };
            }
            if (targetFriends)
            {
                unit.TargetingComponent = default(FindNearestFriendTargeting);
            }
            else
            {
                unit.TargetingComponent = default(EnemyLeastWeightTargeting);
            }
            unit.ArmLimitMultiplier = 1f;
            unit.weaponSeparation = 0.5f;
            unit.massMultiplier = 1f;
            unit.minSizeRandom = 0.8f;
            unit.maxSizeRandom = 1.2f;
            unit.sizeMultiplier = 1f;
            unit.turnSpeed = 15f;
            // You can set all of these values yourself manually, I wouldn't recommend changing it here, as it applies to all custom units you create by default.
            LandfallUnitDatabase.GetDatabase().AddUnitWithID(unit);
            return unit;
        }

        public static Faction CreateFaction(string name, Sprite icon, int index, Color color)
        {
            var fac = Object.Instantiate(GetFaction("Tribal"));
            fac.Units = new UnitBlueprint[] { };
            fac.name = name;
            fac.Entity.Name = name;
            fac.Entity.GenerateNewID();
            fac.Entity.SpriteIcon = icon;
            fac.m_FactionColor = color;
            fac.index = index;
            fac.m_FactionCatagory = Faction.FactionCatagory.Landfall;
            LandfallUnitDatabase.GetDatabase().AddFactionWithID(fac);
            return fac;
        }

        public static void AddUnitToFaction(UnitBlueprint u, Faction fac)
        {
            var facUnits = new List<UnitBlueprint>(fac.Units);
            facUnits.Add(u);
            var newUnits = (
                from UnitBlueprint unit
                in facUnits
                orderby unit.GetUnitCost()
                select unit).ToList();
            fac.Units = newUnits.ToArray();
        }

        public static GameObject AssetPool;
        public static void SetFlagsChildren(Transform child, HideFlags flag)
        {
            child.gameObject.hideFlags = flag;
            for (int i = 0; i < child.childCount; i++)
            {
                SetFlagsChildren(child.GetChild(i), flag);
            }
        }
    }
}
