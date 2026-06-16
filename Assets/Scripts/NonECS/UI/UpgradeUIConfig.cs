using UnityEngine;

namespace NonECS.UI
{
    [CreateAssetMenu(fileName = "UpgradeUIConfig", menuName = "UI/Upgrade UI Config")]
    public class UpgradeUIConfig : ScriptableObject
    {
        [Header("Upgrade Selection")]
        [Range(1, 5)]
        public int NumberOfUpgradeChoices = 3;
        
        [Header("UI Style Classes")]
        public string UpgradeButtonClass = "upgrade-button";
        public string UpgradeLayoutClass = "upgrade-layout";
        public string UpgradeTitleClass = "upgrade-title";
        public string UpgradeLevelClass = "upgrade-level";
        public string UpgradeDescriptionClass = "upgrade-description";
        
        [Header("Animation Settings")]
        public float FadeInDuration = 0.3f;
        public float FadeOutDuration = 0.2f;
        
        [Header("Luck System")]
        public bool EnableLuckRolls = true;
        [Range(0f, 1f)]
        public float LuckChancePerPoint = 0.1f;
        public int MaxBonusRolls = 2;
    }
}