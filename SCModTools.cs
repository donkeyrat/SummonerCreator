using UnityEngine;
using Landfall.TABS;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Landfall.TABS.AI.Components.Modifiers;
using Landfall.TABS.AI.Components.Tags;
using DM;

namespace SummonerCreator
{
    public class SCModTools
    {
        public static UnitBlueprint CloneUnit(string name, UnitBlueprint blueprintBase)
        {
            Dictionary<DatabaseID, UnitBlueprint> units = (Dictionary<DatabaseID, UnitBlueprint>)typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ContentDatabase.Instance().LandfallContentDatabase);
            // This code sets up units. I wouldn't recommend touching this unless you absolutely know what you are doing. Worse case, you can redownload the mod to fix it.
            var unit = Object.Instantiate(blueprintBase);
            unit.name = name;
            unit.Entity.Name = name;
            unit.Entity.GenerateNewID();
            units.Add(unit.Entity.GUID, unit);
            typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ContentDatabase.Instance().LandfallContentDatabase, units);
            return unit;
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
    }
}
