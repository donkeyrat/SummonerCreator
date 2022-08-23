using System;
using System.Collections;
using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;

namespace SummonerCreator
{
	public class SummonerCast : MonoBehaviour
	{
		private void Start()
		{
			data = transform.root.GetComponentInChildren<DataHandler>();
			weapon = data.weaponHandler.rightWeapon;
			Dictionary<string, Tuple<UnitBlueprint, SummonerStats>> summonerDic = SummonerSingleton.GetInstance().summonerDic;
			if (summonerDic != null && summonerDic.Count > 0)
			{
				foreach (Tuple<UnitBlueprint, SummonerStats> tuple in summonerDic.Values)
				{
					if (tuple.Item1 == data.unit.unitBlueprint)
					{
						stats = tuple.Item2;
						break;
					}
				}
			}
			if (stats != null)
			{
				minions = stats.minionsPerSpawn;
				weapon.internalCooldown = stats.cooldown;
				spawnableUnits = stats.spawnables;
			}
			else
			{
				Destroy(gameObject);
			}
			StartCoroutine(ExecuteMove());
		}

		private IEnumerator ExecuteMove()
		{
			int num;
			for (int i = 0; i < minions; i = num + 1)
			{
				UnitBlueprint unitBlueprint = spawnableUnits[UnityEngine.Random.Range(0, spawnableUnits.Length)];
				bool flag = false;
				Quaternion quaternion = Quaternion.LookRotation(data.targetMainRig.transform.position - data.mainRig.transform.position);
				quaternion.x = 0f;
				quaternion.z = 0f;
				Vector3 randomSpawnPos = GetRandomSpawnPos();
				Vector3 occupation = unitBlueprint.UnitBase.GetComponent<Unit>().PlacementSquare.Size;
				while (!flag)
				{
					randomSpawnPos = GetRandomSpawnPos();
					CheckForTerrain(randomSpawnPos, occupation, quaternion, unitBlueprint, out flag, out randomSpawnPos);
					yield return new WaitForEndOfFrame();
					unitBlueprint = spawnableUnits[UnityEngine.Random.Range(0, spawnableUnits.Length)];
					occupation = unitBlueprint.UnitBase.GetComponent<Unit>().PlacementSquare.Size;
					quaternion = Quaternion.LookRotation(data.targetMainRig.transform.position - data.mainRig.transform.position);
					quaternion.x = 0f;
					quaternion.z = 0f;
				}
				Unit unit;
				unitBlueprint.Spawn(randomSpawnPos, quaternion, data.team, out unit);
				if (weapon && weapon.GetComponent<RangeWeapon>() != null)
				{
					GameObject gameObject = Instantiate<GameObject>(Resources.FindObjectsOfTypeAll<LineEffect>()[0].gameObject, weapon.GetComponentInChildren<ShootPosition>().transform);
					gameObject.GetComponent<LineEffect>().DoEffect(weapon.GetComponentInChildren<ShootPosition>().transform, unit.data.mainRig.transform);
					Destroy(gameObject, 3f);
				}
				randomSpawnPos = default(Vector3);
				yield return new WaitForSeconds(0.05f);
				quaternion = default(Quaternion);
				randomSpawnPos = default(Vector3);
				occupation = default(Vector3);
				unit = null;
				num = i;
				randomSpawnPos = default(Vector3);
			}
			yield break;
		}

		private Vector3 GetRandomSpawnPos()
		{
			Vector3 position = data.mainRig.transform.position;
			position.x += UnityEngine.Random.Range(-maxRange, maxRange);
			position.z += UnityEngine.Random.Range(-maxRange, maxRange);
			position.y += 1000f;
			return position;
		}

		private void CheckForTerrain(Vector3 pos, Vector3 occupation, Quaternion rot, UnitBlueprint unit, out bool boolean, out Vector3 newVector)
		{
			Vector3 vector = pos;
			RaycastHit raycastHit;
			Physics.Raycast(new Ray(vector, Vector3.down), out raycastHit, float.PositiveInfinity, 512);
			if (!raycastHit.transform)
			{
				newVector = Vector3.zero;
				boolean = false;
				return;
			}
			vector.y = raycastHit.point.y + occupation.y + 0.1f;
			if (!Physics.CheckBox(vector, occupation, rot) && Vector3.Distance(data.mainRig.transform.position, vector) >= minRange && !Physics.Linecast(weapon.GetComponentInChildren<ShootPosition>().transform.position, vector))
			{
				vector.y = raycastHit.point.y + unit.UnitBase.GetComponent<Unit>().data.baseHeight;
				newVector = vector;
				boolean = true;
				return;
			}
			newVector = Vector3.zero;
			boolean = false;
		}

		private DataHandler data;

		private Weapon weapon;

		public int minions;

		public UnitBlueprint[] spawnableUnits;

		public int cooldown;

		private SummonerStats stats;

		private float maxRange = 15f;

		private float minRange = 3f;
	}
}
