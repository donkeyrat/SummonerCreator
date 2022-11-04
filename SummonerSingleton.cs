using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DM;
using Landfall.TABS;
using Landfall.TABS.GameMode;
using Landfall.TABS.GameState;
using Landfall.TABS.UnitPlacement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SummonerCreator
{
	public class SummonerSingleton : GameStateListener
	{
		private static int h
		{
			get
			{
				return Screen.height;
			}
		}

		private static int w
		{
			get
			{
				return Screen.width;
			}
		}

		private static float hc(float val)
		{
			return val * Screen.height / 1080f;
		}

		private static float wc(float val)
		{
			return val * Screen.width / 1920f;
		}

		public static SummonerSingleton GetInstance()
		{
			if (!instance)
			{
				instance = new GameObject
				{
					hideFlags = HideFlags.DontSave
				}.AddComponent<SummonerSingleton>();
			}
			return instance;
		}

		public void ConfirmValues()
		{
			summonerDic.Add("Summoner", Tuple.Create<UnitBlueprint, SummonerStats>(summonerOriginal, summonerStats));
		}

		private void CreateStyles()
		{
			styles = new Dictionary<string, GUIStyle>();
			GUIStyle guistyle = new GUIStyle(GUI.skin.button);
			guistyle.fontSize = Screen.width / 148;
			guistyle.fontStyle = FontStyle.Bold;
			styles.Add("button", guistyle);
			GUIStyle guistyle2 = new GUIStyle(GUI.skin.textField);
			guistyle2.fontSize = Screen.width / 148;
			styles.Add("textfield", guistyle2);
			GUIStyle guistyle3 = new GUIStyle(GUI.skin.label);
			guistyle3.normal.textColor = new Color(5f, 5f, 5f);
			guistyle3.fontSize = Screen.width / 137;
			guistyle3.fontStyle = FontStyle.Bold;
			styles.Add("label", guistyle3);
			GUIStyle value = new GUIStyle(guistyle3);
			styles.Add("log", value);
		}

		private void OnGUI()
		{
			if (canvasChild)
			{
				canvasChild.SetActive(false);
			}
			ServiceLocator.GetService<GameStateManager>();
			MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
			if (!mainMenuUIHandler || (mainMenuUIHandler && mainMenuUIHandler.currentMenuState == MenuState.Main))
			{
				PlacementGUI();
				return;
			}
			if (canvasChild)
			{
				DestroyImmediate(canvasChild);
				return;
			}
		}

		private void PlacementGUI()
		{
			var db = ContentDatabase.Instance().LandfallContentDatabase;
			GameModeService service = ServiceLocator.GetService<GameModeService>();
			UnitPlacementBrush unitPlacementBrush = null;
			if (service != null && service.CurrentGameMode != null)
			{
				unitPlacementBrush = service.CurrentGameMode.Brush;
			}
			if (unitPlacementBrush != null && !(unitPlacementBrush.UnitToSpawn == null))
			{
				bool flag = false;
				if (summonerDic != null && summonerDic.Count > 0)
				{
					using (Dictionary<string, Tuple<UnitBlueprint, SummonerStats>>.ValueCollection.Enumerator enumerator = summonerDic.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.Item1 == unitPlacementBrush.UnitToSpawn)
							{
								flag = true;
								break;
							}
						}
					}
				}
				if (unitDictionary == null)
				{
					unitDictionary = (Dictionary<DatabaseID, UnitBlueprint>)typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db);
				}
				if (unitPlacementBrush.UnitToSpawn != summonerOriginal && flag)
				{
					Rect position = new Rect(0.79f * w, 0.06f * h, 0.2f * w, 0.05f * h);
					if (!canvasChild)
					{
						canvasChild = new GameObject("summoner_canvas_fern");
					}
					Canvas canvas = FindObjectOfType<Canvas>();
					if (canvasChild && canvas)
					{
						canvasChild.SetActive(true);
						canvasChild.transform.SetParent(canvas.transform);
						canvasChild.FetchComponent<EventSystem>();
						canvasChild.FetchComponent<Image>().color = new Color32(125, 150, 115, byte.MaxValue);
						RectTransform rectTransform2;
						RectTransform rectTransform4 = rectTransform2 = canvasChild.FetchComponent<RectTransform>();
						Vector2 vector = new Vector2(position.x / w, 1f - position.y / h);
						rectTransform4.anchorMax = vector;
						rectTransform2.anchorMin = vector;
						rectTransform4.anchoredPosition = Vector2.zero;
						rectTransform4.pivot = new Vector2(0f, 1f);
						rectTransform4.sizeDelta = new Vector2(position.width, position.height);
					}
					CreateStyles();
					GUI.backgroundColor = Color.black;
					GUI.Box(position, "");
					if (GUI.Button(new Rect(position.x + wc(7f), position.y + hc(7f), position.width - wc(14f), position.height - hc(14f)), "Delete Summoner", styles["button"]))
					{
						UnitBlueprint unitToSpawn = unitPlacementBrush.UnitToSpawn;
						if (db.GetUnitBlueprint(unitToSpawn.Entity.GUID))
						{
							((Dictionary<DatabaseID, UnitBlueprint>)typeof(LandfallContentDatabase).GetField("m_unitBlueprints", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(db)).Remove(unitToSpawn.Entity.GUID);
							if (unitDictionary.ContainsKey(unitToSpawn.Entity.GUID))
							{
								unitDictionary.Remove(unitToSpawn.Entity.GUID);
							}
						}
						if (summonerFaction.Units.Contains(unitToSpawn))
						{
							List<UnitBlueprint> list = summonerFaction.Units.ToList<UnitBlueprint>();
							list.Remove(unitToSpawn);
							summonerFaction.Units = list.ToArray();
						}
						summonerDic.Remove(unitToSpawn.Entity.Name);
						RedrawFaction();
					}
				}
				if (unitPlacementBrush.UnitToSpawn == summonerOriginal)
				{
					Rect position2 = new Rect(0.79f * w, 0.06f * h, 0.2f * w, 0.415f * h);
					if (!canvasChild)
					{
						canvasChild = new GameObject("summoner_canvas_fern");
					}
					Canvas canvas2 = FindObjectOfType<Canvas>();
					if (canvasChild && canvas2)
					{
						canvasChild.SetActive(true);
						canvasChild.transform.SetParent(canvas2.transform);
						canvasChild.FetchComponent<EventSystem>();
						canvasChild.FetchComponent<Image>().color = new Color32(125, 150, 115, byte.MaxValue);
						RectTransform rectTransform3;
						RectTransform rectTransform5 = rectTransform3 = canvasChild.FetchComponent<RectTransform>();
						Vector2 vector2 = new Vector2(position2.x / w, 1f - position2.y / h);
						rectTransform5.anchorMax = vector2;
						rectTransform3.anchorMin = vector2;
						rectTransform5.anchoredPosition = Vector2.zero;
						rectTransform5.pivot = new Vector2(0f, 1f);
						rectTransform5.sizeDelta = new Vector2(position2.width, position2.height);
					}
					CreateStyles();
					GUI.backgroundColor = Color.black;
					GUI.Box(position2, "");
					GUI.BeginGroup(position2);
					Rect position3 = new Rect(wc(15f), hc(15f), position2.width - wc(30f), 0.1f * h);
					List<UnitBlueprint> list2 = (from unit in db.GetUnitBlueprints()
						where !db.GetUnitBlueprint(unit.Entity.GUID).IsCustomUnit
						orderby db.GetUnitBlueprint(unit.Entity.GUID).Name
						select db.GetUnitBlueprint(unit.Entity.GUID)).ToList();
					list2 = (from unit in list2
							 where ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret(unit.Entity.UnlockKey)
							 select unit).ToList<UnitBlueprint>();
					Rect viewRect = new Rect(0f, 0f, position3.width - position3.x, 0.02f * h * (list2.Count + 1));
					unitListScroll = GUI.BeginScrollView(position3, unitListScroll, viewRect);
					position3 = new Rect(0f, 0f, position3.width, 0.02f * h);
					if (GUI.Button(position3, "(---> All Units <---)", styles["button"]))
					{
						selUnits = new List<UnitBlueprint>(list2);
					}
					for (int i = 0; i < list2.Count; i++)
					{
						if (GUI.Button(new Rect(position3.x, position3.y + position3.height * (i + 1), position3.width, position3.height), list2[i].Name, styles["button"]))
						{
							selUnits.Add(list2[i]);
						}
					}
					GUI.EndScrollView();
					position3 = new Rect(wc(15f), 0.1f * h + hc(40f), position2.width - wc(30f), 0.1f * h);
					GUI.Label(position3, "List Length : " + ((selUnits.Count > 0) ? selUnits.Count.ToString() : "Empty"), styles["label"]);
					position3 = new Rect(position3.x + wc(200f), position3.y, position3.width - wc(200f), 0.015f * h);
					if (GUI.Button(position3, "Clear List", styles["button"]))
					{
						selUnits = new List<UnitBlueprint>();
					}
					position3 = new Rect(position3.x - wc(200f), position3.y + position3.height + wc(45f), position3.width + wc(200f), 0.02f * h);
					GUI.Label(position3, "Units Per Spawn : " + unitsPerSpawn.ToString(), styles["label"]);
					position3 = new Rect(position3.x, position3.y + position3.height + wc(5f), position3.width, 0.02f * h);
					unitsPerSpawn = (int)GUI.HorizontalSlider(position3, unitsPerSpawn, 1f, 50f);
					position3 = new Rect(position3.x, position3.y + position3.height + wc(5f), position3.width, position3.height);
					GUI.Label(position3, "Cooldown : " + cooldown.ToString(), styles["label"]);
					position3 = new Rect(position3.x, position3.y + position3.height + wc(5f), position3.width, position3.height);
					cooldown = (int)GUI.HorizontalSlider(position3, cooldown, 0f, 60f);
					position3 = new Rect(position3.x, position3.y + position3.height + wc(15f), position3.width, position3.height);
					GUI.Label(position3, "Summoner Name", styles["label"]);
					position3 = new Rect(position3.x, position3.y + position3.height + wc(5f), position3.width - wc(140f), position3.height);
					summonerName = GUI.TextField(position3, summonerName, styles["textfield"]);
					position3 = new Rect(position3.x, position3.y + position3.height + wc(12f), position3.width + wc(40f), position3.height);
					if (GUI.Button(position3, "Create Summoner", styles["button"]) && !string.IsNullOrEmpty(summonerName) && !string.IsNullOrWhiteSpace(summonerName) && summonerName != "Summoner")
					{
						if (!summonerDic.ContainsKey(summonerName))
						{
							Log("Summoner created successfully!", Color.green);
                            SummonerStats summonerStats = new SummonerStats
                            {
                                minionsPerSpawn = unitsPerSpawn,
                                cooldown = cooldown,
                                spawnables = new List<UnitBlueprint>(selUnits).ToArray()
                            };
							UnitBlueprint unitBlueprint = SCModTools.CloneUnit(summonerName, summonerOriginal);
							summonerDic.Add(summonerName, Tuple.Create(unitBlueprint, summonerStats));
							SCModTools.AddUnitToFaction(unitBlueprint, summonerFaction);
							RedrawFaction();
						}
						else
						{
							Log("Can't create the summoner, it already exists!", Color.red);
						}
					}
					position3 = new Rect(position3.x, position3.y + position3.height + wc(7f), position3.width, position3.height);
					styles["log"].normal.textColor = logColor;
					GUI.Label(position3, logText, styles["log"]);
					GUI.EndGroup();
				}
			}
		}

		private void RedrawFaction()
		{
			PlacementUI placementUI = FindObjectOfType<PlacementUI>();
			if (placementUI)
			{
				placementUI.RedrawFactionUnits(summonerFaction);
			}
		}

		private void Log(string text, Color color)
		{
			logColor = color;
			logText = text;
		}

		public override void OnEnterPlacementState()
		{
		}

		public override void OnEnterBattleState()
		{
			if (canvasChild)
			{
				canvasChild.SetActive(false);
			}
		}

		public Faction summonerFaction;

		public UnitBlueprint summonerOriginal;

		public SummonerStats summonerStats;

		public Dictionary<string, Tuple<UnitBlueprint, SummonerStats>> summonerDic = new Dictionary<string, Tuple<UnitBlueprint, SummonerStats>>();

		private Dictionary<string, GUIStyle> styles;

		private static GameObject canvasChild;

		private Vector2 unitListScroll;

		private List<UnitBlueprint> selUnits = new List<UnitBlueprint>();

		private int unitsPerSpawn = 5;

		private int cooldown = 15;

		private string summonerName = "";

		private string logText;

		private Color logColor;

		private static SummonerSingleton instance;

		private Dictionary<DatabaseID, UnitBlueprint> unitDictionary;
	}
}
