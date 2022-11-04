using UnityEngine;
using Landfall.TABS;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using DM;
using Landfall.TABS.Workshop;
using Landfall.TABS.UnitEditor;

namespace SummonerCreator
{
    public class SCMain
    {
        public SCMain()
        {
            var db = ContentDatabase.Instance().LandfallContentDatabase;
            List<UnitBlueprint> realList = (from unit in db.GetUnitBlueprints()
                                            where !db.GetUnitBlueprint(unit.Entity.GUID).IsCustomUnit
                                            orderby db.GetUnitBlueprint(unit.Entity.GUID).Name
                                            select db.GetUnitBlueprint(unit.Entity.GUID)).ToList();
            foreach (var fac in summon.LoadAllAssets<Faction>())
            {
                newFactions.Add(fac);
            }
            foreach (var unit in summon.LoadAllAssets<UnitBlueprint>())
            {
                newUnits.Add(unit);
                foreach (var b in db.GetUnitBases().ToList()) { if (unit.UnitBase != null) { if (b.name == unit.UnitBase.name) { unit.UnitBase = b; } } }
                foreach (var b in db.GetWeapons().ToList()) { if (unit.RightWeapon != null && b.name == unit.RightWeapon.name) unit.RightWeapon = b; if (unit.LeftWeapon != null && b.name == unit.LeftWeapon.name) unit.LeftWeapon = b; }
            }
            foreach (var objecting in summon.LoadAllAssets<GameObject>()) 
            {
                if (objecting != null) {

                    if (objecting.GetComponent<Unit>()) newBases.Add(objecting);
                    else if (objecting.GetComponent<WeaponItem>()) {
                        newWeapons.Add(objecting);
                        int totalSubmeshes = 0;
                        foreach (var rend in objecting.GetComponentsInChildren<MeshFilter>()) {
                            if (rend.gameObject.activeSelf && rend.gameObject.activeInHierarchy && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled == true) {

                                totalSubmeshes += rend.mesh.subMeshCount;
                            }
                        }
                        foreach (var rend in objecting.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                            if (rend.gameObject.activeSelf && rend.sharedMesh.subMeshCount > 0 && rend.enabled) {

                                totalSubmeshes += rend.sharedMesh.subMeshCount;
                            }
                        }
                        if (totalSubmeshes != 0) {
                            float average = 1f / totalSubmeshes;
                            var averageList = new List<float>();
                            for (int i = 0; i < totalSubmeshes; i++) { averageList.Add(average); }
                            objecting.GetComponent<WeaponItem>().SubmeshArea = null;
                            objecting.GetComponent<WeaponItem>().SubmeshArea = averageList.ToArray();
                        }
                    }
                    else if (objecting.GetComponent<ProjectileEntity>()) newProjectiles.Add(objecting);
                    else if (objecting.GetComponent<SpecialAbility>()) newAbilities.Add(objecting);
                    else if (objecting.GetComponent<PropItem>()) {
                        newProps.Add(objecting);
                        int totalSubmeshes = 0;
                        foreach (var rend in objecting.GetComponentsInChildren<MeshFilter>()) {
                            if (rend.gameObject.activeSelf && rend.gameObject.activeInHierarchy && rend.mesh.subMeshCount > 0 && rend.GetComponent<MeshRenderer>() && rend.GetComponent<MeshRenderer>().enabled == true) {

                                totalSubmeshes += rend.mesh.subMeshCount;
                            }
                        }
                        foreach (var rend in objecting.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                            if (rend.gameObject.activeSelf && rend.sharedMesh.subMeshCount > 0 && rend.enabled) {

                                totalSubmeshes += rend.sharedMesh.subMeshCount;
                            }
                        }
                        if (totalSubmeshes != 0) {
                            float average = 1f / totalSubmeshes;
                            var averageList = new List<float>();
                            for (int i = 0; i < totalSubmeshes; i++) { averageList.Add(average); }
                            objecting.GetComponent<PropItem>().SubmeshArea = null;
                            objecting.GetComponent<PropItem>().SubmeshArea = averageList.ToArray();
                        }
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

            AddContentToDatabase();
        }
        
        public void AddContentToDatabase()
        {
            var db = ContentDatabase.Instance().LandfallContentDatabase;
            
            Dictionary<DatabaseID, UnitBlueprint> units = (Dictionary<DatabaseID, UnitBlueprint>)typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var unit in newUnits)
            {
                units.Add(unit.Entity.GUID, unit);
            }
            typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, units);
            
            Dictionary<DatabaseID, Faction> factions = (Dictionary<DatabaseID, Faction>)typeof(LandfallContentDatabase).GetField("m_factions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            List<DatabaseID> defaultHotbarFactions = (List<DatabaseID>)typeof(LandfallContentDatabase).GetField("m_defaultHotbarFactionIds", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var faction in newFactions)
            {
                factions.Add(faction.Entity.GUID, faction);
                defaultHotbarFactions.Add(faction.Entity.GUID);
            }
            typeof(LandfallContentDatabase).GetField("m_factions", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, factions);
            //typeof(LandfallContentDatabase).GetField("m_defaultHotbarFactionIds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, (from DatabaseID fac in defaultHotbarFactions orderby factions[fac].index select fac).ToList());
            
            Dictionary<DatabaseID, TABSCampaignAsset> campaigns = (Dictionary<DatabaseID, TABSCampaignAsset>)typeof(LandfallContentDatabase).GetField("m_campaigns", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var campaign in newCampaigns)
            {
                if (!campaigns.ContainsKey(campaign.Entity.GUID)) campaigns.Add(campaign.Entity.GUID, campaign);
            }
            typeof(LandfallContentDatabase).GetField("m_campaigns", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, campaigns);
            
            Dictionary<DatabaseID, TABSCampaignLevelAsset> campaignLevels = (Dictionary<DatabaseID, TABSCampaignLevelAsset>)typeof(LandfallContentDatabase).GetField("m_campaignLevels", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var campaignLevel in newCampaignLevels)
            {
                if (!campaignLevels.ContainsKey(campaignLevel.Entity.GUID)) campaignLevels.Add(campaignLevel.Entity.GUID, campaignLevel);
            }
            typeof(LandfallContentDatabase).GetField("m_campaignLevels", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, campaignLevels);
            
            Dictionary<DatabaseID, VoiceBundle> voiceBundles = (Dictionary<DatabaseID, VoiceBundle>)typeof(LandfallContentDatabase).GetField("m_voiceBundles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var voiceBundle in newVoiceBundles)
            {
                if (!voiceBundles.ContainsKey(voiceBundle.Entity.GUID)) voiceBundles.Add(voiceBundle.Entity.GUID, voiceBundle);
            }
            typeof(LandfallContentDatabase).GetField("m_voiceBundles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, voiceBundles);
            
            List<DatabaseID> factionIcons = (List<DatabaseID>)typeof(LandfallContentDatabase).GetField("m_factionIconIds", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var factionIcon in newFactionIcons)
            {
                if (!factionIcons.Contains(factionIcon.Entity.GUID)) factionIcons.Add(factionIcon.Entity.GUID);
            }
            typeof(LandfallContentDatabase).GetField("m_factionIconIds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, factionIcons);
            
            Dictionary<DatabaseID, GameObject> unitBases = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_unitBases", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var unitBase in newBases)
            {
                if (!unitBases.ContainsKey(unitBase.GetComponent<Unit>().Entity.GUID)) unitBases.Add(unitBase.GetComponent<Unit>().Entity.GUID, unitBase);
            }
            typeof(LandfallContentDatabase).GetField("m_unitBases", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, unitBases);
            
            Dictionary<DatabaseID, GameObject> props = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_characterProps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var prop in newProps)
            {
                if (!props.ContainsKey(prop.GetComponent<PropItem>().Entity.GUID)) props.Add(prop.GetComponent<PropItem>().Entity.GUID, prop);
            }
            typeof(LandfallContentDatabase).GetField("m_characterProps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, props);
            
            Dictionary<DatabaseID, GameObject> abilities = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_combatMoves", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var ability in newAbilities)
            {
                if (!abilities.ContainsKey(ability.GetComponent<SpecialAbility>().Entity.GUID)) abilities.Add(ability.GetComponent<SpecialAbility>().Entity.GUID, ability);
            }
            typeof(LandfallContentDatabase).GetField("m_combatMoves", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, abilities);
            
            Dictionary<DatabaseID, GameObject> weapons = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_weapons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var weapon in newWeapons)
            {
                if (!weapons.ContainsKey(weapon.GetComponent<WeaponItem>().Entity.GUID)) weapons.Add(weapon.GetComponent<WeaponItem>().Entity.GUID, weapon);
            }
            typeof(LandfallContentDatabase).GetField("m_weapons", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, weapons);
            
            Dictionary<DatabaseID, GameObject> projectiles = (Dictionary<DatabaseID, GameObject>)typeof(LandfallContentDatabase).GetField("m_projectiles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
            foreach (var proj in newProjectiles)
            {
                if (!projectiles.ContainsKey(proj.GetComponent<ProjectileEntity>().Entity.GUID)) projectiles.Add(proj.GetComponent<ProjectileEntity>().Entity.GUID, proj);
            }
            typeof(LandfallContentDatabase).GetField("m_projectiles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(db, projectiles);


            
            ServiceLocator.GetService<CustomContentLoaderModIO>().QuickRefresh(WorkshopContentType.Unit, null);
        }
        
        public List<UnitBlueprint> newUnits = new List<UnitBlueprint>();

        public List<Faction> newFactions = new List<Faction>();
        
        public List<TABSCampaignAsset> newCampaigns = new List<TABSCampaignAsset>();
        
        public List<TABSCampaignLevelAsset> newCampaignLevels = new List<TABSCampaignLevelAsset>();
        
        public List<VoiceBundle> newVoiceBundles = new List<VoiceBundle>();
        
        public List<FactionIcon> newFactionIcons = new List<FactionIcon>();
        
        public List<GameObject> newBases = new List<GameObject>();

        public List<GameObject> newProps = new List<GameObject>();
        
        public List<GameObject> newAbilities = new List<GameObject>();

        public List<GameObject> newWeapons = new List<GameObject>();
        
        public List<GameObject> newProjectiles = new List<GameObject>();

        public AssetBundle summon = AssetBundle.LoadFromMemory(Properties.Resources.summon);
    }
}
