using System.Collections.Generic;
using System.Linq;
using NonECS.WeaponGeneration;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Editor
{
    public partial class WeaponGeneratorWindow : EditorWindow
    {
        private List<WeaponDefinition> weaponDefinitions = new List<WeaponDefinition>();
        private Vector2 scrollPosition;
        private Vector2 creatorScrollPosition;
        private WeaponDefinition selectedWeapon;
        private bool showWeaponCreator = false;
        private int selectedTab = 0;
        private string searchFilter = "";
        
        // New weapon creation fields
        private string newWeaponName = "";
        private string newWeaponDisplayName = "";
        private string newWeaponDescription = "";
        private WeaponCategory newWeaponCategory = WeaponCategory.Projectile;
        private WeaponArchetype newWeaponArchetype = WeaponArchetype.SingleTarget;
        private WeaponStats newWeaponStats = WeaponStats.Default;
        private WeaponBehaviorFlags newWeaponFlags = WeaponBehaviorFlags.AutoTarget;
        
        // Styles
        private GUIStyle headerStyle;
        private GUIStyle subHeaderStyle;
        private GUIStyle weaponItemStyle;
        private GUIStyle selectedWeaponItemStyle;
        private GUIStyle buttonStyle;
        private GUIStyle primaryButtonStyle;
        private GUIStyle warningBoxStyle;
        private GUIStyle infoBoxStyle;
        private GUIStyle separatorStyle;
        
        // Icons and textures
        private Texture2D weaponIcon;
        private Texture2D settingsIcon;
        private Texture2D addIcon;
        private Texture2D refreshIcon;
        private Texture2D exportIcon;
        private Texture2D importIcon;
        private Texture2D generateIcon;
        
        // Constants
        private const float SIDEBAR_WIDTH = 300f;
        private const float MIN_WINDOW_WIDTH = 800f;
        private const float MIN_WINDOW_HEIGHT = 600f;
        
        [MenuItem("Tools/Project Starship/Weapon Generator ⚔️")]
        public static void ShowWindow()
        {
            var window = GetWindow<WeaponGeneratorWindow>("Weapon Generator");
            window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
            window.titleContent = new GUIContent("Weapon Generator ⚔️", "Create and manage weapon systems");
        }
        
        private void OnEnable()
        {
            LoadWeaponDefinitions();
            LoadIcons();
        }
        
        private void InitializeStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };
            
            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.3f, 0.3f, 0.3f) }
            };
            
            weaponItemStyle = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 2, 2),
                normal = { background = MakeTex(1, 1, EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f, 0.3f) : new Color(0.8f, 0.8f, 0.8f, 0.3f)) }
            };
            
            selectedWeaponItemStyle = new GUIStyle(weaponItemStyle)
            {
                normal = { background = MakeTex(1, 1, EditorGUIUtility.isProSkin ? new Color(0.2f, 0.4f, 0.8f, 0.6f) : new Color(0.3f, 0.6f, 1f, 0.6f)) }
            };
            
            primaryButtonStyle = new GUIStyle("button")
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 30,
                normal = { background = MakeTex(1, 1, EditorGUIUtility.isProSkin ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.4f, 0.8f, 0.4f)) }
            };
            
            warningBoxStyle = new GUIStyle("box")
            {
                normal = { background = MakeTex(1, 1, new Color(0.8f, 0.6f, 0.2f, 0.3f)) },
                padding = new RectOffset(10, 10, 8, 8)
            };
            
            infoBoxStyle = new GUIStyle("box")
            {
                normal = { background = MakeTex(1, 1, new Color(0.2f, 0.6f, 0.8f, 0.3f)) },
                padding = new RectOffset(10, 10, 8, 8)
            };
            
            buttonStyle = new GUIStyle("button")
            {
                fontSize = 11,
                padding = new RectOffset(8, 8, 6, 6)
            };
            
            separatorStyle = new GUIStyle()
            {
                normal = { background = MakeTex(1, 1, EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.6f, 0.6f, 0.6f)) },
                fixedHeight = 1,
                margin = new RectOffset(0, 0, 5, 5)
            };
        }
        
        private void LoadIcons()
        {
            // Try to load Unity's built-in icons
            weaponIcon = EditorGUIUtility.IconContent("d_ScriptableObject Icon").image as Texture2D;
            settingsIcon = EditorGUIUtility.IconContent("d_Settings").image as Texture2D;
            addIcon = EditorGUIUtility.IconContent("d_Toolbar Plus").image as Texture2D;
            refreshIcon = EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;
            exportIcon = EditorGUIUtility.IconContent("d_SaveAs").image as Texture2D;
            importIcon = EditorGUIUtility.IconContent("d_Import").image as Texture2D;
            generateIcon = EditorGUIUtility.IconContent("d_BuildSettings.Editor.Small").image as Texture2D;
        }
        
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        
        private void OnGUI()
        {
            if (headerStyle == null)
            {
                InitializeStyles();
            }
            
            DrawHeader();
            DrawTabBar();
            
            EditorGUILayout.BeginHorizontal();
            {
                DrawSidebar();
                DrawVerticalSeparator();
                DrawMainContent();
            }
            EditorGUILayout.EndHorizontal();
            
            DrawFooter();
        }
        
        private void DrawHeader()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.BeginHorizontal();
                {
                    if (weaponIcon != null)
                        GUILayout.Label(weaponIcon, GUILayout.Width(32), GUILayout.Height(32));
                    
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("Project Starship", headerStyle);
                        GUILayout.Label("Weapon Generator & Code Automation", subHeaderStyle);
                    }
                    GUILayout.EndVertical();
                    
                    GUILayout.FlexibleSpace();
                    
                    if (GUILayout.Button(new GUIContent(settingsIcon, "Settings"), EditorStyles.toolbarButton, GUILayout.Width(30)))
                    {
                        ShowSettingsMenu();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        
        private void DrawTabBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                string[] tabNames = { "🏠 Overview", "⚔️ Weapons", "🛠️ Create", "📁 Import/Export" };
                selectedTab = GUILayout.Toolbar(selectedTab, tabNames, EditorStyles.toolbarButton);
                
                GUILayout.FlexibleSpace();
                
                GUILayout.Label($"{weaponDefinitions.Count} weapons", EditorStyles.miniLabel);
            }
            GUILayout.EndHorizontal();
        }
        
        private void DrawSidebar()
        {
            GUILayout.BeginVertical(GUILayout.Width(SIDEBAR_WIDTH));
            {
                switch (selectedTab)
                {
                    case 0: DrawOverviewSidebar(); break;
                    case 1: DrawWeaponsSidebar(); break;
                    case 2: DrawCreateSidebar(); break;
                    case 3: DrawImportExportSidebar(); break;
                }
            }
            GUILayout.EndVertical();
        }
        
        private void DrawVerticalSeparator()
        {
            GUILayout.Box("", separatorStyle, GUILayout.ExpandHeight(true), GUILayout.Width(1));
        }
        
        private void DrawMainContent()
        {
            GUILayout.BeginVertical();
            {
                switch (selectedTab)
                {
                    case 0: DrawOverviewContent(); break;
                    case 1: DrawWeaponsContent(); break;
                    case 2: DrawCreateContent(); break;
                    case 3: DrawImportExportContent(); break;
                }
            }
            GUILayout.EndVertical();
        }
        
        private void DrawFooter()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Ready", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                
                if (selectedWeapon != null)
                {
                    if (GUILayout.Button(new GUIContent(generateIcon, "Generate Selected"), EditorStyles.toolbarButton))
                    {
                        GenerateSelectedWeapon();
                    }
                }
                
                if (weaponDefinitions.Count > 0)
                {
                    if (GUILayout.Button(new GUIContent("🚀 Generate All", generateIcon, "Generate all weapons"), EditorStyles.toolbarButton))
                    {
                        GenerateAllWeapons();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        
        private void DrawWeaponCreator()
        {
            EditorGUILayout.LabelField("Create New Weapon", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            newWeaponName = EditorGUILayout.TextField("Weapon Name (Code)", newWeaponName);
            newWeaponDisplayName = EditorGUILayout.TextField("Display Name", newWeaponDisplayName);
            newWeaponDescription = EditorGUILayout.TextArea(newWeaponDescription, GUILayout.Height(60));
            newWeaponCategory = (WeaponCategory)EditorGUILayout.EnumPopup("Category", newWeaponCategory);
            newWeaponArchetype = (WeaponArchetype)EditorGUILayout.EnumPopup("Archetype", newWeaponArchetype);
            
            EditorGUILayout.Space();
            
            bool canCreate = !string.IsNullOrEmpty(newWeaponName) && !string.IsNullOrEmpty(newWeaponDisplayName);
            GUI.enabled = canCreate;
            
            if (GUILayout.Button("Create Weapon Definition"))
            {
                CreateNewWeapon();
                showWeaponCreator = false;
            }
            
            GUI.enabled = true;
            
            if (!canCreate)
            {
                EditorGUILayout.HelpBox("Please fill in weapon name and display name", MessageType.Warning);
            }
        }
        
        private void DrawWeaponList()
        {
            EditorGUILayout.LabelField($"Found {weaponDefinitions.Count} Weapon Definitions", EditorStyles.boldLabel);
            
            if (weaponDefinitions.Count == 0)
            {
                EditorGUILayout.HelpBox("No weapon definitions found. Create some WeaponDefinition ScriptableObjects first!", MessageType.Info);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            foreach (var weapon in weaponDefinitions)
            {
                DrawWeaponItem(weapon);
            }
            
            EditorGUILayout.EndScrollView();
            
            if (selectedWeapon != null)
            {
                DrawWeaponDetails();
            }
        }
        
        private void DrawWeaponItem(WeaponDefinition weapon)
        {
            EditorGUILayout.BeginHorizontal("box");
            
            bool isSelected = selectedWeapon == weapon;
            var style = isSelected ? EditorStyles.boldLabel : EditorStyles.label;
            
            if (GUILayout.Button($"{weapon.displayName} ({weapon.weaponName})", style, GUILayout.ExpandWidth(true)))
            {
                selectedWeapon = isSelected ? null : weapon;
            }
            
            EditorGUILayout.LabelField($"[{weapon.category}]", GUILayout.Width(80));
            
            if (GUILayout.Button("Edit", GUILayout.Width(40)))
            {
                Selection.activeObject = weapon;
                EditorGUIUtility.PingObject(weapon);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawWeaponDetails()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Details", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField($"Name: {selectedWeapon.weaponName}");
            EditorGUILayout.LabelField($"Display Name: {selectedWeapon.displayName}");
            EditorGUILayout.LabelField($"Category: {selectedWeapon.category}");
            EditorGUILayout.LabelField($"Archetype: {selectedWeapon.archetype}");
            EditorGUILayout.LabelField($"Upgrade Levels: {selectedWeapon.upgradeLevels.Count}");
            
            if (!string.IsNullOrEmpty(selectedWeapon.description))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(selectedWeapon.description, EditorStyles.wordWrappedLabel);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generated Files:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"• {selectedWeapon.GetComponentName()}.cs");
            EditorGUILayout.LabelField($"• {selectedWeapon.GetWeaponBaseName()}.cs");
            EditorGUILayout.LabelField($"• {selectedWeapon.weaponName}Data.cs");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGenerationButtons()
        {
            EditorGUILayout.LabelField("Code Generation", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = selectedWeapon != null;
            if (GUILayout.Button("Generate Selected Weapon"))
            {
                var weapons = new List<WeaponDefinition> { selectedWeapon };
                WeaponCodeGenerator.GenerateAllWeaponCode(weapons);
            }
            GUI.enabled = true;
            
            GUI.enabled = weaponDefinitions.Count > 0;
            if (GUILayout.Button("Generate All Weapons"))
            {
                WeaponCodeGenerator.GenerateAllWeaponCode(weaponDefinitions);
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Generated files will be created in:\n" +
                "• Components: Assets/Scripts/ShipECS/Components/Generated/\n" +
                "• Weapon Bases: Assets/Scripts/NonECS/BaseWeapons/Generated/\n" +
                "• Data Classes: Assets/Scripts/NonECS/ScriptableObjects/Generated/",
                MessageType.Info);
        }
        
        private void LoadWeaponDefinitions()
        {
            weaponDefinitions.Clear();
            var guids = AssetDatabase.FindAssets("t:WeaponDefinition");
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
                if (weapon != null)
                {
                    weaponDefinitions.Add(weapon);
                }
            }
            
            weaponDefinitions = weaponDefinitions.OrderBy(w => w.weaponName).ToList();
        }
        
        private void CreateNewWeapon()
        {
            var weapon = CreateInstance<WeaponDefinition>();
            weapon.weaponName = newWeaponName;
            weapon.displayName = newWeaponDisplayName;
            weapon.description = newWeaponDescription;
            weapon.category = newWeaponCategory;
            weapon.archetype = newWeaponArchetype;
            weapon.baseStats = WeaponStats.Default;
            
            // Create a few default upgrade levels
            weapon.upgradeLevels.Add(new WeaponUpgradeLevel
            {
                level = 1,
                description = "Basic upgrade",
                statMultipliers = new WeaponStats { baseDamage = 1.2f },
                statAdditions = new WeaponStats()
            });
            
            var path = $"Assets/ScriptableObjects/Weapons/{newWeaponName}Definition.asset";
            AssetDatabase.CreateAsset(weapon, path);
            AssetDatabase.SaveAssets();
            
            LoadWeaponDefinitions();
            selectedWeapon = weapon;
            
            Debug.Log($"Created new weapon definition: {newWeaponName}");
        }
        
        // Create Tab Methods
        private void DrawCreateWeaponForm()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("🛠️ Create New Weapon", headerStyle);
                GUILayout.Space(10);
                
                // Basic Info Section
                GUILayout.Label("📝 Basic Information", subHeaderStyle);
                
                newWeaponName = EditorGUILayout.TextField("Weapon Name (Code)", newWeaponName);
                if (!string.IsNullOrEmpty(newWeaponName))
                {
                    EditorGUILayout.HelpBox($"Component will be named: {newWeaponName}Attack", MessageType.Info);
                }
                
                newWeaponDisplayName = EditorGUILayout.TextField("Display Name", newWeaponDisplayName);
                newWeaponDescription = EditorGUILayout.TextArea(newWeaponDescription, GUILayout.Height(60));
                
                GUILayout.Space(10);
                
                // Category and Type
                GUILayout.Label("🎯 Weapon Type", subHeaderStyle);
                newWeaponCategory = (WeaponCategory)EditorGUILayout.EnumPopup("Category", newWeaponCategory);
                newWeaponArchetype = (WeaponArchetype)EditorGUILayout.EnumPopup("Archetype", newWeaponArchetype);
                
                GUILayout.Space(10);
                
                // Stats Section
                GUILayout.Label("📊 Base Stats", subHeaderStyle);
                newWeaponStats.baseDamage = EditorGUILayout.FloatField("Base Damage", newWeaponStats.baseDamage);
                newWeaponStats.baseFireRate = EditorGUILayout.FloatField("Fire Rate", newWeaponStats.baseFireRate);
                newWeaponStats.baseRange = EditorGUILayout.FloatField("Range", newWeaponStats.baseRange);
                newWeaponStats.baseSpeed = EditorGUILayout.FloatField("Projectile Speed", newWeaponStats.baseSpeed);
                newWeaponStats.baseNumProjectiles = EditorGUILayout.IntField("Projectile Count", newWeaponStats.baseNumProjectiles);
                
                GUILayout.Space(10);
                
                // Behavior Flags
                GUILayout.Label("⚙️ Behavior Flags", subHeaderStyle);
                newWeaponFlags = (WeaponBehaviorFlags)EditorGUILayout.EnumFlagsField("Behavior Flags", newWeaponFlags);
                
                GUILayout.Space(20);
                
                // Create Button
                bool canCreate = !string.IsNullOrEmpty(newWeaponName) && !string.IsNullOrEmpty(newWeaponDisplayName);
                GUI.enabled = canCreate;
                
                if (GUILayout.Button("🚀 Create Weapon Definition", primaryButtonStyle, GUILayout.Height(40)))
                {
                    CreateNewWeapon();
                    ClearCreateForm();
                    selectedTab = 1; // Switch to weapons tab
                }
                
                GUI.enabled = true;
                
                if (!canCreate)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("Please fill in weapon name and display name to continue.", MessageType.Warning);
                }
            }
            GUILayout.EndVertical();
        }
        
        private void ClearCreateForm()
        {
            newWeaponName = "";
            newWeaponDisplayName = "";
            newWeaponDescription = "";
            newWeaponCategory = WeaponCategory.Projectile;
            newWeaponArchetype = WeaponArchetype.SingleTarget;
            newWeaponStats = WeaponStats.Default;
            newWeaponFlags = WeaponBehaviorFlags.AutoTarget;
        }
        
        // Template Methods
        private void LoadProjectileTemplate()
        {
            newWeaponName = "BasicProjectile";
            newWeaponDisplayName = "Basic Projectile";
            newWeaponDescription = "A simple projectile weapon that fires straight bullets";
            newWeaponCategory = WeaponCategory.Projectile;
            newWeaponArchetype = WeaponArchetype.SingleTarget;
            newWeaponStats = new WeaponStats
            {
                baseDamage = 20f,
                baseFireRate = 1.5f,
                baseRange = 15f,
                baseSpeed = 12f,
                baseNumProjectiles = 1
            };
            newWeaponFlags = WeaponBehaviorFlags.AutoTarget | WeaponBehaviorFlags.DestroyOnContact;
        }
        
        private void LoadArtilleryTemplate()
        {
            newWeaponName = "BasicArtillery";
            newWeaponDisplayName = "Artillery Cannon";
            newWeaponDescription = "Heavy artillery that fires explosive shells with area damage";
            newWeaponCategory = WeaponCategory.Artillery;
            newWeaponArchetype = WeaponArchetype.AreaOfEffect;
            newWeaponStats = new WeaponStats
            {
                baseDamage = 60f,
                baseFireRate = 0.8f,
                baseRange = 20f,
                baseSpeed = 8f,
                baseSize = 4f,
                baseNumProjectiles = 1
            };
            newWeaponFlags = WeaponBehaviorFlags.ExplodeOnImpact | WeaponBehaviorFlags.AimAtCursor;
        }
        
        private void LoadBeamTemplate()
        {
            newWeaponName = "PlasmaBeam";
            newWeaponDisplayName = "Plasma Beam";
            newWeaponDescription = "Continuous beam weapon that pierces through enemies";
            newWeaponCategory = WeaponCategory.Beam;
            newWeaponArchetype = WeaponArchetype.Continuous;
            newWeaponStats = new WeaponStats
            {
                baseDamage = 50f,
                baseFireRate = 0.3f,
                baseRange = 25f,
                baseSize = 2f,
                basePenetration = 99
            };
            newWeaponFlags = WeaponBehaviorFlags.RequiresChargeUp | WeaponBehaviorFlags.ContinuousFire | WeaponBehaviorFlags.PierceEnemies;
        }
        
        private void LoadChainLightningTemplate()
        {
            newWeaponName = "ChainLightning";
            newWeaponDisplayName = "Chain Lightning";
            newWeaponDescription = "Lightning that chains between nearby enemies";
            newWeaponCategory = WeaponCategory.Laser;
            newWeaponArchetype = WeaponArchetype.Piercing;
            newWeaponStats = new WeaponStats
            {
                baseDamage = 30f,
                baseFireRate = 1.5f,
                baseRange = 15f,
                basePenetration = 5
            };
            newWeaponFlags = WeaponBehaviorFlags.ChainToNearby | WeaponBehaviorFlags.AutoTarget;
        }
        
        // Import/Export Methods
        private void ImportWeaponFromJson()
        {
            var path = EditorUtility.OpenFilePanel("Import Weapon JSON", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var jsonContent = File.ReadAllText(path);
                    var importedWeapon = WeaponJsonImporter.ImportFromJson(jsonContent);
                    
                    var assetPath = $"Assets/ScriptableObjects/Weapons/{importedWeapon.weaponName}Definition.asset";
                    AssetDatabase.CreateAsset(importedWeapon, assetPath);
                    AssetDatabase.SaveAssets();
                    
                    LoadWeaponDefinitions();
                    selectedWeapon = importedWeapon;
                    
                    ShowNotification(new GUIContent($"Imported {importedWeapon.displayName}!"));
                    Debug.Log($"Successfully imported weapon: {importedWeapon.displayName}");
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("Import Error", $"Failed to import weapon:\n{e.Message}", "OK");
                }
            }
        }
        
        private void ImportBatchWeaponsFromFolder()
        {
            var folderPath = EditorUtility.OpenFolderPanel("Import Weapons Folder", "", "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                var jsonFiles = Directory.GetFiles(folderPath, "*.json");
                var importedCount = 0;
                
                foreach (var file in jsonFiles)
                {
                    try
                    {
                        var jsonContent = File.ReadAllText(file);
                        var importedWeapon = WeaponJsonImporter.ImportFromJson(jsonContent);
                        
                        var assetPath = $"Assets/ScriptableObjects/Weapons/{importedWeapon.weaponName}Definition.asset";
                        AssetDatabase.CreateAsset(importedWeapon, assetPath);
                        importedCount++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to import {file}: {e.Message}");
                    }
                }
                
                AssetDatabase.SaveAssets();
                LoadWeaponDefinitions();
                
                ShowNotification(new GUIContent($"Imported {importedCount} weapons!"));
                Debug.Log($"Batch import completed: {importedCount} weapons imported");
            }
        }
        
        private void ExportWeaponToJson(WeaponDefinition weapon)
        {
            if (weapon == null) return;
            
            var folderPath = Path.Combine(Application.dataPath, "WeaponDefinitions");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            var filePath = Path.Combine(folderPath, $"{weapon.weaponName}.json");
            
            try
            {
                WeaponJsonImporter.ExportToJson(weapon, filePath);
                ShowNotification(new GUIContent($"Exported {weapon.displayName}!"));
                Debug.Log($"Exported weapon to: {filePath}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Export Error", $"Failed to export weapon:\n{e.Message}", "OK");
            }
        }
        
        private void ExportAllWeaponsToJson()
        {
            var folderPath = Path.Combine(Application.dataPath, "WeaponDefinitions");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            var exportedCount = 0;
            
            foreach (var weapon in weaponDefinitions)
            {
                try
                {
                    var filePath = Path.Combine(folderPath, $"{weapon.weaponName}.json");
                    WeaponJsonImporter.ExportToJson(weapon, filePath);
                    exportedCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to export {weapon.weaponName}: {e.Message}");
                }
            }
            
            ShowNotification(new GUIContent($"Exported {exportedCount} weapons!"));
            Debug.Log($"Batch export completed: {exportedCount} weapons exported");
        }
        
        private void CreateExampleWeapons()
        {
            var exampleWeapons = new List<WeaponDefinition>
            {
                CreateBasicProjectileExample(),
                CreateBasicArtilleryExample(),
                CreateBasicLaserExample()
            };
            
            foreach (var weapon in exampleWeapons)
            {
                var path = $"Assets/ScriptableObjects/Weapons/{weapon.weaponName}Definition.asset";
                AssetDatabase.CreateAsset(weapon, path);
            }
            
            AssetDatabase.SaveAssets();
            LoadWeaponDefinitions();
            
            ShowNotification(new GUIContent($"Created {exampleWeapons.Count} example weapons!"));
        }
        
        private void CreateAdvancedExampleWeapons()
        {
            var advancedWeapons = new List<WeaponDefinition>
            {
                ExampleComplexWeapons.CreateBeamWeapon(),
                ExampleComplexWeapons.CreateChainLightningWeapon(),
                ExampleComplexWeapons.CreateHomingMissileWeapon(),
                ExampleComplexWeapons.CreateDroneSwarmWeapon()
            };
            
            foreach (var weapon in advancedWeapons)
            {
                var path = $"Assets/ScriptableObjects/Weapons/{weapon.weaponName}Definition.asset";
                AssetDatabase.CreateAsset(weapon, path);
            }
            
            AssetDatabase.SaveAssets();
            LoadWeaponDefinitions();
            
            ShowNotification(new GUIContent($"Created {advancedWeapons.Count} advanced weapons!"));
        }
        
        private WeaponDefinition CreateBasicProjectileExample()
        {
            var weapon = CreateInstance<WeaponDefinition>();
            weapon.weaponName = "AutoCannon";
            weapon.displayName = "Auto Cannon";
            weapon.description = "Rapid-fire projectile weapon";
            weapon.category = WeaponCategory.Projectile;
            weapon.archetype = WeaponArchetype.SingleTarget;
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 15f,
                baseFireRate = 3f,
                baseRange = 18f,
                baseSpeed = 14f,
                baseNumProjectiles = 1
            };
            weapon.behaviorFlags = WeaponBehaviorFlags.AutoTarget | WeaponBehaviorFlags.DestroyOnContact;
            return weapon;
        }
        
        private WeaponDefinition CreateBasicArtilleryExample()
        {
            var weapon = CreateInstance<WeaponDefinition>();
            weapon.weaponName = "HeavyCannon";
            weapon.displayName = "Heavy Cannon";
            weapon.description = "Powerful explosive shells";
            weapon.category = WeaponCategory.Artillery;
            weapon.archetype = WeaponArchetype.AreaOfEffect;
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 80f,
                baseFireRate = 0.6f,
                baseRange = 22f,
                baseSpeed = 6f,
                baseSize = 5f,
                baseNumProjectiles = 1
            };
            weapon.behaviorFlags = WeaponBehaviorFlags.ExplodeOnImpact | WeaponBehaviorFlags.AimAtCursor;
            return weapon;
        }
        
        private WeaponDefinition CreateBasicLaserExample()
        {
            var weapon = CreateInstance<WeaponDefinition>();
            weapon.weaponName = "LaserRifle";
            weapon.displayName = "Laser Rifle";
            weapon.description = "Precise energy weapon";
            weapon.category = WeaponCategory.Laser;
            weapon.archetype = WeaponArchetype.SingleTarget;
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 25f,
                baseFireRate = 2f,
                baseRange = 20f,
                baseSpeed = 25f,
                basePenetration = 2,
                baseNumProjectiles = 1
            };
            weapon.behaviorFlags = WeaponBehaviorFlags.PierceEnemies | WeaponBehaviorFlags.AutoTarget;
            return weapon;
        }
        
        private void ClearAllGeneratedCode()
        {
            var componentsPath = "Assets/Scripts/ShipECS/Components/Generated/";
            var weaponBasePath = "Assets/Scripts/NonECS/BaseWeapons/Generated/";
            var upgradeTypesPath = "Assets/Scripts/NonECS/ScriptableObjects/Generated/";
            
            DeleteDirectoryContents(componentsPath);
            DeleteDirectoryContents(weaponBasePath);
            DeleteDirectoryContents(upgradeTypesPath);
            
            AssetDatabase.Refresh();
            ShowNotification(new GUIContent("Cleared all generated code!"));
        }
        
        private void DeleteDirectoryContents(string path)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.cs");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
        }
    }
}