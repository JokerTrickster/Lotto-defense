using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using LottoDefense.Gameplay;
using LottoDefense.Grid;
using LottoDefense.Utils;

namespace LottoDefense.Units
{
    /// <summary>
    /// Singleton manager responsible for the gacha system and unit inventory management.
    /// Handles weighted random unit acquisition and integrates with GameplayManager for gold costs.
    /// </summary>
    public class UnitManager : MonoBehaviour
    {
        #region Singleton
        private static UnitManager _instance;

        /// <summary>
        /// Global access point for the UnitManager singleton.
        /// </summary>
        public static UnitManager Instance
        {
            get
            {
                if (GameplayManager.IsCleaningUp) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UnitManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UnitManager");
                        _instance = go.AddComponent<UnitManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Central game balance configuration loaded at initialization.
        /// Contains all spawn rates, costs, and game rules.
        /// </summary>
        private GameBalanceConfig balanceConfig;

        /// <summary>
        /// Gacha cost - loaded from balance config, defaults to 5 if not set.
        /// </summary>
        private int gachaCost = 5;
        #endregion

        #region Inspector Fields
        [Header("Unit Pool Configuration")]
        [Tooltip("All available units organized by rarity. Load from Resources or assign manually.")]
        [SerializeField] private List<UnitData> normalUnits = new List<UnitData>();
        [SerializeField] private List<UnitData> rareUnits = new List<UnitData>();
        [SerializeField] private List<UnitData> epicUnits = new List<UnitData>();
        [SerializeField] private List<UnitData> legendaryUnits = new List<UnitData>();

        [Header("Inventory Settings")]
        [Tooltip("Maximum number of units that can be stored in inventory")]
        [SerializeField] private int maxInventorySize = 50;
        #endregion

        #region Cached Lists
        #endregion

        #region Properties
        /// <summary>
        /// Current units in player's inventory.
        /// </summary>
        public List<UnitData> Inventory { get; private set; } = new List<UnitData>();

        /// <summary>
        /// Number of available inventory slots remaining.
        /// </summary>
        public int AvailableSlots => maxInventorySize - Inventory.Count;

        /// <summary>
        /// Check if inventory is full.
        /// </summary>
        public bool IsInventoryFull => Inventory.Count >= maxInventorySize;
        #endregion

        #region Events
        /// <summary>
        /// Fired when a unit is successfully drawn from gacha.
        /// Parameters: drawnUnit, remainingGold
        /// </summary>
        public event Action<UnitData, int> OnUnitDrawn;

        /// <summary>
        /// Fired when inventory contents change (add/remove).
        /// Parameters: inventory, operation ("add"/"remove"), affectedUnit
        /// </summary>
        public event Action<List<UnitData>, string, UnitData> OnInventoryChanged;

        /// <summary>
        /// Fired when gacha draw fails (insufficient gold or full inventory).
        /// Parameters: failureReason
        /// </summary>
        public event Action<string> OnDrawFailed;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the unit manager and validate unit pools.
        /// </summary>
        private void Initialize()
        {
            // Load central game balance config
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                // Create runtime instance silently
                balanceConfig = ScriptableObject.CreateInstance<GameBalanceConfig>();
            }

            // Verify config has skill presets

            // Set gacha cost from config
            gachaCost = balanceConfig.gameRules.summonCost;

            // Auto-load units from Resources if pools are empty
            if (normalUnits.Count == 0 && rareUnits.Count == 0 &&
                epicUnits.Count == 0 && legendaryUnits.Count == 0)
            {
                LoadUnitsFromResources();
            }

            // Verify skills were assigned
            var allPools = new[] { normalUnits, rareUnits, epicUnits, legendaryUnits };
            foreach (var pool in allPools)
            {
                foreach (var unit in pool)
                {
                }
            }

            ValidateUnitPools();
        }

        /// <summary>
        /// Validates that all rarity tiers have at least one unit.
        /// Logs warnings if any pool is empty.
        /// </summary>
        private void ValidateUnitPools()
        {
            if (normalUnits.Count == 0)
                Debug.LogWarning("[UnitManager] Normal unit pool is empty!");
            if (rareUnits.Count == 0)
                Debug.LogWarning("[UnitManager] Rare unit pool is empty!");
            if (epicUnits.Count == 0)
                Debug.LogWarning("[UnitManager] Epic unit pool is empty!");
            if (legendaryUnits.Count == 0)
                Debug.LogWarning("[UnitManager] Legendary unit pool is empty!");
        }
        #endregion

        #region Gacha System
        /// <summary>
        /// Attempt to draw a random unit from the gacha system.
        /// Costs 5 gold and adds the unit to inventory.
        /// </summary>
        /// <returns>The drawn UnitData, or null if the draw failed</returns>
        public UnitData DrawUnit()
        {
            // Validate gold availability
            if (GameplayManager.Instance.CurrentGold < gachaCost)
            {
                string reason = $"Insufficient gold. Need {gachaCost}, have {GameplayManager.Instance.CurrentGold}";
                Debug.LogWarning($"[UnitManager] Draw failed: {reason}");
                OnDrawFailed?.Invoke(reason);
                return null;
            }

            // Validate inventory space
            if (IsInventoryFull)
            {
                string reason = $"Inventory full ({maxInventorySize}/{maxInventorySize})";
                Debug.LogWarning($"[UnitManager] Draw failed: {reason}");
                OnDrawFailed?.Invoke(reason);
                return null;
            }

            // Deduct gold cost
            GameplayManager.Instance.ModifyGold(-gachaCost);

            // Perform weighted random selection
            UnitData drawnUnit = PerformWeightedDraw();

            if (drawnUnit != null)
            {
                // Add to inventory
                AddUnit(drawnUnit);

                // Notify listeners
                int remainingGold = GameplayManager.Instance.CurrentGold;
                OnUnitDrawn?.Invoke(drawnUnit, remainingGold);

            }
            else
            {
                Debug.LogError("[UnitManager] Weighted draw returned null! Check unit pools.");
                OnDrawFailed?.Invoke("No units available in selected rarity pool");
            }

            return drawnUnit;
        }

        /// <summary>
        /// Performs weighted random selection based on rarity drop rates from GameBalanceConfig.
        /// Returns a random unit from the selected rarity pool.
        /// All units in Resources are available regardless of shop unlock status.
        /// </summary>
        private UnitData PerformWeightedDraw()
        {
            // Get spawn rates from balance config (no unlock filter - all units available in gacha)
            float legendaryRate = legendaryUnits.Count > 0 ? balanceConfig.spawnRates.legendaryRate : 0f;
            float epicRate = epicUnits.Count > 0 ? balanceConfig.spawnRates.epicRate : 0f;
            float rareRate = rareUnits.Count > 0 ? balanceConfig.spawnRates.rareRate : 0f;
            float normalRate = normalUnits.Count > 0 ? balanceConfig.spawnRates.normalRate : 0f;

            // Redistribute empty pool weights to remaining pools
            float totalRate = normalRate + rareRate + epicRate + legendaryRate;
            if (totalRate <= 0f)
            {
                Debug.LogError("[UnitManager] No unlocked units available for gacha!");
                return null;
            }

            // Normalize rates to 100%
            float scale = 100f / totalRate;
            normalRate *= scale;
            rareRate *= scale;
            epicRate *= scale;
            legendaryRate *= scale;

            // Generate random value between 0 and 100
            float roll = UnityEngine.Random.Range(0f, 100f);

            Rarity selectedRarity;
            List<UnitData> selectedPool;

            // Determine rarity based on cumulative probabilities
            if (roll < legendaryRate)
            {
                selectedRarity = Rarity.Legendary;
                selectedPool = legendaryUnits;
            }
            else if (roll < legendaryRate + epicRate)
            {
                selectedRarity = Rarity.Epic;
                selectedPool = epicUnits;
            }
            else if (roll < legendaryRate + epicRate + rareRate)
            {
                selectedRarity = Rarity.Rare;
                selectedPool = rareUnits;
            }
            else
            {
                selectedRarity = Rarity.Normal;
                selectedPool = normalUnits;
            }

            // Select random unit from the chosen pool
            if (selectedPool.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, selectedPool.Count);
                return selectedPool[randomIndex];
            }

            Debug.LogError($"[UnitManager] Selected {selectedRarity} pool is empty!");
            return null;
        }

        /// <summary>
        /// Check if player can afford a gacha draw.
        /// </summary>
        public bool CanDraw()
        {
            // 카운트다운 중에는 유닛 생산 불가
            if (GameplayManager.Instance.CurrentState == GameState.Countdown)
            {
                return false;
            }
            
            return GameplayManager.Instance.CurrentGold >= gachaCost && !IsInventoryFull;
        }

        /// <summary>
        /// Get the current gacha cost.
        /// </summary>
        public int GetGachaCost()
        {
            return gachaCost;
        }
        #endregion

        #region Inventory Management
        /// <summary>
        /// Add a unit to the inventory.
        /// </summary>
        /// <param name="unit">Unit to add</param>
        /// <returns>True if successfully added, false if inventory is full</returns>
        public bool AddUnit(UnitData unit)
        {
            if (IsInventoryFull)
            {
                Debug.LogWarning($"[UnitManager] Cannot add {unit.unitName} - inventory full");
                return false;
            }

            Inventory.Add(unit);
            OnInventoryChanged?.Invoke(Inventory, "add", unit);
            return true;
        }

        /// <summary>
        /// Remove a unit from the inventory.
        /// </summary>
        /// <param name="unit">Unit to remove</param>
        /// <returns>True if successfully removed, false if unit not found</returns>
        public bool RemoveUnit(UnitData unit)
        {
            bool removed = Inventory.Remove(unit);

            if (removed)
            {
                OnInventoryChanged?.Invoke(Inventory, "remove", unit);
            }
            else
            {
                Debug.LogWarning($"[UnitManager] Failed to remove {unit.unitName} - not found in inventory");
            }

            return removed;
        }

        /// <summary>
        /// Get a copy of the current inventory.
        /// </summary>
        public List<UnitData> GetInventory()
        {
            return new List<UnitData>(Inventory);
        }

        /// <summary>
        /// Get units of a specific type from inventory.
        /// </summary>
        public List<UnitData> GetUnitsByType(UnitType type)
        {
            return Inventory.Where(u => u.type == type).ToList();
        }

        /// <summary>
        /// Get units of a specific rarity from inventory.
        /// </summary>
        public List<UnitData> GetUnitsByRarity(Rarity rarity)
        {
            return Inventory.Where(u => u.rarity == rarity).ToList();
        }

        /// <summary>
        /// Clear all units from inventory.
        /// </summary>
        public void ClearInventory()
        {
            Inventory.Clear();
            OnInventoryChanged?.Invoke(Inventory, "clear", null);
        }
        #endregion

        #region Unit Pool Management
        /// <summary>
        /// Load all unit data from Resources folder.
        /// Expected path: Resources/Units/
        /// </summary>
        public void LoadUnitsFromResources()
        {
            UnitData[] allUnits = Resources.LoadAll<UnitData>("Units");

            normalUnits.Clear();
            rareUnits.Clear();
            epicUnits.Clear();
            legendaryUnits.Clear();

            foreach (var unit in allUnits)
            {
                // Assign skills from GameBalanceConfig
                AssignSkillsFromConfig(unit);

                switch (unit.rarity)
                {
                    case Rarity.Normal:
                        normalUnits.Add(unit);
                        break;
                    case Rarity.Rare:
                        rareUnits.Add(unit);
                        break;
                    case Rarity.Epic:
                        epicUnits.Add(unit);
                        break;
                    case Rarity.Legendary:
                        legendaryUnits.Add(unit);
                        break;
                }
            }

            ValidateUnitPools();
        }

        /// <summary>
        /// Assign skills to a UnitData from GameBalanceConfig based on unit name matching.
        /// Bridges the gap between UnitData assets (no skills) and balance config (has skillIds).
        /// </summary>
        private void AssignSkillsFromConfig(UnitData unitData)
        {
            if (balanceConfig == null || unitData == null)
            {
                Debug.LogWarning($"[UnitManager] AssignSkillsFromConfig: balanceConfig={balanceConfig != null}, unitData={unitData != null}");
                return;
            }

            // Find matching UnitBalance by name
            var unitBalance = balanceConfig.units.Find(u => u.unitName == unitData.unitName);
            if (unitBalance == null)
            {
                Debug.LogWarning($"[UnitManager] No balance config found for unit '{unitData.unitName}' (config has {balanceConfig.units?.Count ?? 0} units)");
                return;
            }

            // Always apply attack pattern from balance config (independent of skills)
            unitData.attackPattern = unitBalance.attackPattern;
            unitData.splashRadius = unitBalance.splashRadius;
            unitData.maxTargets = unitBalance.maxTargets;

            // Get skills from balance config
            var skillBalances = balanceConfig.GetUnitSkills(unitBalance);
            if (skillBalances == null || skillBalances.Count == 0)
            {
                Debug.LogWarning($"[UnitManager] No skills found for '{unitData.unitName}' (skillIds={unitBalance.skillIds?.Count ?? 0}, presets={balanceConfig.skillPresets?.Count ?? 0})");
                return;
            }

            // Convert to UnitSkill array
            var skills = new UnitSkill[skillBalances.Count];
            for (int i = 0; i < skillBalances.Count; i++)
            {
                skills[i] = UnitSkill.FromBalance(skillBalances[i]);
            }

            unitData.skills = skills;

            // Log skill details
            string skillInfo = "";
            foreach (var s in skills)
            {
                skillInfo += $"{s.skillName}({s.skillType},cd={s.cooldownDuration}s) ";
            }
        }

        /// <summary>
        /// Get all units from a specific rarity pool.
        /// </summary>
        public List<UnitData> GetUnitPool(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Normal => new List<UnitData>(normalUnits),
                Rarity.Rare => new List<UnitData>(rareUnits),
                Rarity.Epic => new List<UnitData>(epicUnits),
                Rarity.Legendary => new List<UnitData>(legendaryUnits),
                _ => new List<UnitData>()
            };
        }
        #endregion

        #region Placed Units Query
        /// <summary>
        /// Get all units currently placed on the grid.
        /// Returns a new list each call to prevent collection modification during iteration.
        /// </summary>
        /// <returns>New list of all placed Unit components</returns>
        public List<Unit> GetPlacedUnits()
        {
            var result = new List<Unit>();

            if (GridManager.Instance == null)
            {
                Debug.LogWarning("[UnitManager] GetPlacedUnits: GridManager is null!");
                return result;
            }

            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    Unit unit = GridManager.Instance.GetUnitAt(x, y);
                    if (unit != null)
                    {
                        result.Add(unit);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Number of units currently placed on the grid. Counts directly without list allocation.
        /// </summary>
        public int PlacedUnitCount
        {
            get
            {
                if (GridManager.Instance == null) return 0;
                int count = 0;
                for (int x = 0; x < GridManager.GRID_WIDTH; x++)
                    for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                        if (GridManager.Instance.GetUnitAt(x, y) != null) count++;
                return count;
            }
        }

        /// <summary>
        /// Place a unit on the grid at the specified position.
        /// Creates a new Unit GameObject from UnitData.
        /// </summary>
        /// <param name="unitData">Data for the unit to place</param>
        /// <param name="position">Grid position to place the unit</param>
        /// <returns>The placed Unit component, or null if placement failed</returns>
        public Unit PlaceUnit(UnitData unitData, Vector2Int position)
        {
            if (unitData == null)
            {
                Debug.LogError("[UnitManager] Cannot place unit - null unit data");
                return null;
            }

            if (GridManager.Instance == null)
            {
                Debug.LogError("[UnitManager] Cannot place unit - GridManager not found");
                return null;
            }

            if (!GridManager.Instance.IsPlacementCell(position.x, position.y))
            {
                Debug.LogWarning($"[UnitManager] Cannot place unit at {position} - not a valid placement cell");
                return null;
            }

            // Create unit GameObject
            GameObject unitObj = new GameObject($"Unit_{unitData.unitName}");
            Unit unit = unitObj.AddComponent<Unit>();
            unit.Initialize(unitData, position);

            // Apply existing rarity-wide upgrades to new unit
            if (UnitUpgradeManager.Instance != null)
            {
                UnitUpgradeManager.Instance.ApplyRarityUpgrades(unit);
            }

            // Add visual representation (SpriteRenderer is added by RequireComponent on Unit)
            SpriteRenderer renderer = unitObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                if (unitData.icon != null)
                {
                    renderer.sprite = unitData.icon;
                }
                else if (renderer.sprite == null)
                {
                    // Try loading sprite from Resources/Sprites/Units/
                    Sprite loaded = GameSpriteLoader.LoadUnitSprite(unitData.unitName);
                    if (loaded != null)
                    {
                        renderer.sprite = loaded;
                        renderer.color = UnitData.GetRarityColor(unitData.rarity);
                    }
                    else
                    {
                        renderer.sprite = UnitData.CreateCircleSprite(32);
                        renderer.color = UnitData.GetRarityColor(unitData.rarity);
                    }
                }
                renderer.sortingOrder = 10;
            }

            // Add collider for click detection (needed for OnMouseDown)
            if (unitObj.GetComponent<BoxCollider2D>() == null)
            {
                BoxCollider2D collider = unitObj.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(0.8f, 0.8f);
            }

            // Place on grid
            if (!GridManager.Instance.SetUnit(position, unitObj))
            {
                Debug.LogError($"[UnitManager] Failed to place unit at {position}");
                UnityEngine.Object.Destroy(unitObj);
                return null;
            }

            return unit;
        }

        /// <summary>
        /// Clear all units from the grid.
        /// Destroys all placed unit GameObjects.
        /// </summary>
        public void ClearAllUnits()
        {
            if (GridManager.Instance == null) return;

            for (int x = 0; x < GridManager.GRID_WIDTH; x++)
            {
                for (int y = 0; y < GridManager.GRID_HEIGHT; y++)
                {
                    GameObject unitObj = GridManager.Instance.RemoveUnit(x, y);
                    if (unitObj != null)
                    {
                        UnityEngine.Object.Destroy(unitObj);
                    }
                }
            }

        }

        /// <summary>
        /// Get statistics about placed units.
        /// </summary>
        public string GetStats()
        {
            int placedCount = PlacedUnitCount;
            int inventoryCount = Inventory.Count;
            return $"Placed: {placedCount}, Inventory: {inventoryCount}/{maxInventorySize}";
        }
        #endregion

        #region Debug Utilities
        /// <summary>
        /// Get statistics about current drop rate probabilities.
        /// </summary>
        public string GetDropRateInfo()
        {
            if (balanceConfig == null) return "Drop Rates: Config not loaded";
            return $"Drop Rates: Normal={balanceConfig.spawnRates.normalRate}%, Rare={balanceConfig.spawnRates.rareRate}%, Epic={balanceConfig.spawnRates.epicRate}%, Legendary={balanceConfig.spawnRates.legendaryRate}%";
        }

        /// <summary>
        /// Get detailed inventory summary for debugging.
        /// </summary>
        public string GetInventorySummary()
        {
            if (Inventory.Count == 0)
                return "Inventory: Empty";

            var summary = $"Inventory ({Inventory.Count}/{maxInventorySize}):\n";
            var grouped = Inventory.GroupBy(u => u.rarity)
                                  .OrderByDescending(g => g.Key);

            foreach (var group in grouped)
            {
                summary += $"  {group.Key}: {group.Count()} units\n";
            }

            return summary;
        }
        #endregion
    }
}
