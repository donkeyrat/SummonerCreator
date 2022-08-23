using UnityEngine;
using Landfall.TABS;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SummonerCreator
{
    public class SCMain
    {
        public SCMain()
        {
            var db = LandfallUnitDatabase.GetDatabase();
            List<UnitBlueprint> realList = (from unit in LandfallUnitDatabase.GetDatabase().UnitList
                                            where !LandfallUnitDatabase.GetDatabase().GetUnitByGUID(unit.Entity.GUID).IsCustomUnit
                                            orderby LandfallUnitDatabase.GetDatabase().GetUnitByGUID(unit.Entity.GUID).Name
                                            select LandfallUnitDatabase.GetDatabase().GetUnitByGUID(unit.Entity.GUID)).ToList();
            foreach (var fac in summon.LoadAllAssets<Faction>())
            {
                db.FactionList.AddItem(fac);
                db.AddFactionWithID(fac);
                foreach (var unit in fac.Units)
                {
                    if (!db.UnitList.Contains(unit))
                    {
                        db.UnitList.AddItem(unit);
                        db.AddUnitWithID(unit);
                    }
                }
            }
            foreach (var unit in summon.LoadAllAssets<UnitBlueprint>())
            {
                if (!db.UnitList.Contains(unit))
                {
                    db.UnitList.AddItem(unit);
                    db.AddUnitWithID(unit);
                }
                foreach (var b in db.UnitBaseList)
                {
                    if (unit.UnitBase != null)
                    {
                        if (b.name == unit.UnitBase.name)
                        {
                            unit.UnitBase = b;
                        }
                    }
                }
                foreach (var b in db.WeaponList)
                {
                    if (unit.RightWeapon != null && b.name == unit.RightWeapon.name) unit.RightWeapon = b;
                    if (unit.LeftWeapon != null && b.name == unit.LeftWeapon.name) unit.LeftWeapon = b;
                }
            }
            foreach (var objecting in summon.LoadAllAssets<GameObject>())
            {
                if (objecting != null)
                {
                    if (objecting.GetComponent<Unit>())
                    {
                        List<GameObject> stuff = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("UnitBases", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
                        stuff.Add(objecting);
                        typeof(LandfallUnitDatabase).GetField("UnitBases", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff);
                    }
                    else if (objecting.GetComponent<WeaponItem>())
                    {
                        List<GameObject> stuff = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("Weapons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
                        stuff.Add(objecting);
                        typeof(LandfallUnitDatabase).GetField("Weapons", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff);
                    }
                    else if (objecting.GetComponent<ProjectileEntity>())
                    {
                        List<GameObject> stuff = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("Projectiles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
                        stuff.Add(objecting);
                        typeof(LandfallUnitDatabase).GetField("Projectiles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff);
                    }
                    else if (objecting.GetComponent<Landfall.TABS.UnitEditor.SpecialAbility>())
                    {
                        List<GameObject> stuff = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("CombatMoves", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
                        stuff.Add(objecting);
                        typeof(LandfallUnitDatabase).GetField("CombatMoves", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff);
                    }
                    else if (objecting.GetComponent<PropItem>())
                    {
                        List<GameObject> stuff = (List<GameObject>)typeof(LandfallUnitDatabase).GetField("CharacterProps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
                        stuff.Add(objecting);
                        typeof(LandfallUnitDatabase).GetField("CharacterProps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, stuff);
                    }
                }
            }
            SummonerStats summonerStats = new SummonerStats
            {
                cooldown = 15f,
                minionsPerSpawn = 5,
                spawnables = new List<UnitBlueprint>(realList).ToArray()
            };
            SummonerSingleton shitstance = SummonerSingleton.GetInstance();
            shitstance.summonerFaction = summon.LoadAsset<Faction>("Summoners");
            shitstance.summonerOriginal = summon.LoadAsset<UnitBlueprint>("Summoner");
            shitstance.summonerStats = summonerStats;
            shitstance.ConfirmValues();
        }

        public AssetBundle summon = AssetBundle.LoadFromMemory(Properties.Resources.summon);
    }
}
