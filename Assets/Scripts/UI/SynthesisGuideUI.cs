using UnityEngine;
using UnityEngine.UI;
using LottoDefense.Gameplay;
using LottoDefense.Units;
using System.Collections.Generic;

namespace LottoDefense.UI
{
    /// <summary>
    /// 합성 가이드 UI - 책처럼 페이지를 넘기며 합성 레시피를 확인할 수 있습니다.
    /// 각 페이지는 하나의 합성 레시피를 보여줍니다.
    /// </summary>
    public class SynthesisGuideUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private GameObject guidePanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;

        [Header("Content Display")]
        [SerializeField] private Text pageNumberText;
        [SerializeField] private Text recipeTitleText;
        [SerializeField] private Image sourceUnitIcon;
        [SerializeField] private Text sourceUnitNameText;
        [SerializeField] private Text sourceUnitStatsText;
        [SerializeField] private Image resultUnitIcon;
        [SerializeField] private Text resultUnitNameText;
        [SerializeField] private Text resultUnitStatsText;
        [SerializeField] private Text requiredCountText;
        [SerializeField] private Text synthesisInfoText;
        #endregion

        #region Private Fields
        private GameBalanceConfig balanceConfig;
        private List<GameBalanceConfig.SynthesisRecipe> recipes;
        private int currentPage = 0;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Use Start instead of Awake because GameSceneBootstrapper sets
        /// serialized fields via reflection AFTER AddComponent triggers Awake.
        /// By Start(), all fields are assigned.
        /// </summary>
        private void Start()
        {
            // Load config
            balanceConfig = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (balanceConfig == null)
            {
                Debug.LogError("[SynthesisGuideUI] GameBalanceConfig not found!");
                return;
            }

            // Get all recipes
            recipes = balanceConfig.synthesisRecipes;

            // Setup buttons (fields are now assigned via reflection)
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
            else
            {
                Debug.LogWarning("[SynthesisGuideUI] closeButton is null in Start!");
            }

            if (prevPageButton != null)
            {
                prevPageButton.onClick.AddListener(OnPrevPage);
            }
            else
            {
                Debug.LogWarning("[SynthesisGuideUI] prevPageButton is null in Start!");
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.AddListener(OnNextPage);
            }
            else
            {
                Debug.LogWarning("[SynthesisGuideUI] nextPageButton is null in Start!");
            }

            // Start hidden
            if (guidePanel != null)
            {
                guidePanel.SetActive(false);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show synthesis guide UI.
        /// </summary>
        public void Show()
        {
            if (guidePanel != null)
            {
                guidePanel.SetActive(true);
            }

            currentPage = 0;
            UpdatePage();
        }

        /// <summary>
        /// Hide synthesis guide UI.
        /// </summary>
        public void Hide()
        {
            if (guidePanel != null)
            {
                guidePanel.SetActive(false);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Go to previous page.
        /// </summary>
        private void OnPrevPage()
        {
            if (recipes == null || recipes.Count == 0) return;

            currentPage--;
            if (currentPage < 0)
            {
                currentPage = recipes.Count - 1; // Wrap to last page
            }

            UpdatePage();
        }

        /// <summary>
        /// Go to next page.
        /// </summary>
        private void OnNextPage()
        {
            if (recipes == null || recipes.Count == 0) return;

            currentPage++;
            if (currentPage >= recipes.Count)
            {
                currentPage = 0; // Wrap to first page
            }

            UpdatePage();
        }

        /// <summary>
        /// Update current page content.
        /// </summary>
        private void UpdatePage()
        {
            if (recipes == null || recipes.Count == 0)
            {
                Debug.LogWarning("[SynthesisGuideUI] No synthesis recipes available!");
                return;
            }

            if (currentPage < 0 || currentPage >= recipes.Count)
            {
                Debug.LogError($"[SynthesisGuideUI] Invalid page index: {currentPage}");
                return;
            }

            var recipe = recipes[currentPage];

            // Update page number
            if (pageNumberText != null)
            {
                pageNumberText.text = $"{currentPage + 1} / {recipes.Count}";
            }

            // Update recipe title
            if (recipeTitleText != null)
            {
                recipeTitleText.text = "합성 레시피";
            }

            // Find source and result unit data
            UnitData sourceUnit = FindUnitData(recipe.sourceUnitName);
            UnitData resultUnit = FindUnitData(recipe.resultUnitName);

            // Update source unit info
            if (sourceUnit != null)
            {
                if (sourceUnitIcon != null)
                {
                    sourceUnitIcon.sprite = sourceUnit.icon != null ? sourceUnit.icon : CreatePlaceholderSprite(sourceUnit.rarity);
                    sourceUnitIcon.color = sourceUnit.icon != null ? Color.white : UnitData.GetRarityColor(sourceUnit.rarity);
                }

                if (sourceUnitNameText != null)
                {
                    sourceUnitNameText.text = sourceUnit.GetDisplayName();
                    sourceUnitNameText.color = UnitData.GetRarityColor(sourceUnit.rarity);
                }

                if (sourceUnitStatsText != null)
                {
                    sourceUnitStatsText.text = GetUnitStatsText(sourceUnit);
                }
            }

            // Update result unit info
            if (resultUnit != null)
            {
                if (resultUnitIcon != null)
                {
                    resultUnitIcon.sprite = resultUnit.icon != null ? resultUnit.icon : CreatePlaceholderSprite(resultUnit.rarity);
                    resultUnitIcon.color = resultUnit.icon != null ? Color.white : UnitData.GetRarityColor(resultUnit.rarity);
                }

                if (resultUnitNameText != null)
                {
                    resultUnitNameText.text = resultUnit.GetDisplayName();
                    resultUnitNameText.color = UnitData.GetRarityColor(resultUnit.rarity);
                }

                if (resultUnitStatsText != null)
                {
                    resultUnitStatsText.text = GetUnitStatsText(resultUnit);
                }
            }

            // Update required count
            if (requiredCountText != null)
            {
                requiredCountText.text = "2개 필요";
            }

            // Update synthesis info
            if (synthesisInfoText != null)
            {
                string costText = recipe.synthesisGoldCost > 0 ? $"{recipe.synthesisGoldCost} 골드 소모" : "무료";
                synthesisInfoText.text = $"같은 유닛 2개를 모아서 합성하세요!\n{costText}";
            }

            // Update button states
            if (prevPageButton != null)
            {
                prevPageButton.interactable = recipes.Count > 1;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable = recipes.Count > 1;
            }
        }

        /// <summary>
        /// Find UnitData by unit name.
        /// </summary>
        private UnitData FindUnitData(string unitName)
        {
            // Try matching by unitName field first
            UnitData[] allUnits = Resources.LoadAll<UnitData>("Units");
            foreach (var unit in allUnits)
            {
                if (unit.unitName == unitName)
                {
                    return unit;
                }
            }

            // Fallback: try loading by Resources path (asset filename)
            UnitData byPath = Resources.Load<UnitData>($"Units/{unitName}");
            if (byPath != null)
            {
                return byPath;
            }

            Debug.LogWarning($"[SynthesisGuideUI] Unit data not found: {unitName}");
            return null;
        }

        /// <summary>
        /// Get formatted unit stats text.
        /// </summary>
        private string GetUnitStatsText(UnitData unit)
        {
            return $"공격력: {unit.attack}\n" +
                   $"공격속도: {unit.attackSpeed:F1}/s\n" +
                   $"사거리: {unit.attackRange:F1}\n" +
                   $"DPS: {unit.GetDPS():F1}";
        }

        /// <summary>
        /// Create placeholder sprite for units without icons.
        /// </summary>
        private Sprite CreatePlaceholderSprite(Rarity rarity)
        {
            return UnitData.CreateCircleSprite(64);
        }
        #endregion
    }
}
