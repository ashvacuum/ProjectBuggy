using System.Collections.Generic;
using System.Linq;
using NonECS.WeaponGeneration;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Editor
{
    public partial class WeaponGeneratorWindow
    {
        // Overview Tab
        private void DrawOverviewSidebar()
        {
            GUILayout.Label("📊 Statistics", subHeaderStyle);
            
            GUILayout.BeginVertical(infoBoxStyle);
            {
                GUILayout.Label($"Total Weapons: {weaponDefinitions.Count}", EditorStyles.boldLabel);
                
                var categories = weaponDefinitions.GroupBy(w => w.category).ToDictionary(g => g.Key, g => g.Count());
                foreach (var category in categories)
                {
                    GUILayout.Label($"  {GetCategoryIcon(category.Key)} {category.Key}: {category.Value}");
                }
                
                GUILayout.Space(10);
                GUILayout.Label($"Generated Components: {weaponDefinitions.Count * 3}");
                GUILayout.Label($"Lines of Code: ~{weaponDefinitions.Count * 50}");
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            GUILayout.Label("🚀 Quick Actions", subHeaderStyle);
            
            if (GUILayout.Button(new GUIContent("📁 Open Generated Folder", "Open the generated components folder"), buttonStyle))
            {
                EditorUtility.RevealInFinder("Assets/Scripts/ShipECS/Components/Generated");
            }
            
            if (GUILayout.Button(new GUIContent("📖 Open Documentation", "Open the README.md file"), buttonStyle))
            {
                Application.OpenURL("file://" + Path.Combine(Application.dataPath, "../README.md"));
            }
            
            if (GUILayout.Button(new GUIContent("🔄 Refresh All", refreshIcon, "Reload all weapon definitions"), buttonStyle))
            {
                LoadWeaponDefinitions();
            }
        }
        
        private void DrawOverviewContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                DrawWelcomeSection();
                GUILayout.Space(10);
                DrawQuickStartSection();
                GUILayout.Space(10);
                DrawFeaturesSection();
                GUILayout.Space(10);
                DrawRecentActivitySection();
            }
            GUILayout.EndScrollView();
        }
        
        private void DrawWelcomeSection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("Welcome to Project Starship Weapon Generator! 🚀", headerStyle);
                GUILayout.Space(5);
                
                var welcomeText = "This tool automates the creation of weapon systems for your space combat game. " +
                                "Create simple projectiles or complex beam weapons with just a few clicks!";
                GUILayout.Label(welcomeText, EditorStyles.wordWrappedLabel);
                
                GUILayout.Space(10);
                
                if (weaponDefinitions.Count == 0)
                {
                    GUILayout.BeginVertical(warningBoxStyle);
                    {
                        GUILayout.Label("⚠️ No weapons found", EditorStyles.boldLabel);
                        GUILayout.Label("Get started by creating your first weapon in the Create tab!");
                    }
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.BeginVertical(infoBoxStyle);
                    {
                        GUILayout.Label($"✅ {weaponDefinitions.Count} weapons ready", EditorStyles.boldLabel);
                        GUILayout.Label("Click 'Generate All' to create ECS components for all weapons.");
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
        }
        
        private void DrawQuickStartSection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("🚀 Quick Start Guide", subHeaderStyle);
                
                string[] steps = {
                    "1. Go to 'Create' tab to design a new weapon",
                    "2. Configure stats, behaviors, and upgrade progression", 
                    "3. Click 'Generate' to create ECS components automatically",
                    "4. For complex behaviors, implement the suggested system",
                    "5. Test your weapon in-game and iterate!"
                };
                
                foreach (var step in steps)
                {
                    GUILayout.Label(step);
                }
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("📖 Open Full Documentation", buttonStyle))
                {
                    Application.OpenURL("file://" + Path.Combine(Application.dataPath, "../README.md"));
                }
            }
            GUILayout.EndVertical();
        }
        
        private void DrawFeaturesSection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("✨ Features", subHeaderStyle);
                
                string[] features = {
                    "🔧 Automatic ECS component generation",
                    "⚔️ Support for complex weapon behaviors",
                    "📁 JSON import/export for collaboration",
                    "🎨 Visual weapon design interface",
                    "🔄 Live code generation and updates",
                    "📊 Built-in validation and error checking"
                };
                
                foreach (var feature in features)
                {
                    GUILayout.Label(feature);
                }
            }
            GUILayout.EndVertical();
        }
        
        private void DrawRecentActivitySection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("📈 Recent Activity", subHeaderStyle);
                
                if (weaponDefinitions.Count == 0)
                {
                    GUILayout.Label("No recent activity", EditorStyles.miniLabel);
                }
                else
                {
                    GUILayout.Label($"Last updated: {System.DateTime.Now:HH:mm}", EditorStyles.miniLabel);
                    GUILayout.Label($"Recently modified weapons:", EditorStyles.miniLabel);
                    
                    var recentWeapons = weaponDefinitions.Take(3);
                    foreach (var weapon in recentWeapons)
                    {
                        GUILayout.Label($"  • {weapon.displayName}");
                    }
                }
            }
            GUILayout.EndVertical();
        }
        
        // Weapons Tab
        private void DrawWeaponsSidebar()
        {
            GUILayout.Label("🔍 Search & Filter", subHeaderStyle);
            
            var newSearchFilter = EditorGUILayout.TextField("Search:", searchFilter);
            if (newSearchFilter != searchFilter)
            {
                searchFilter = newSearchFilter;
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button(new GUIContent("🔄 Refresh", refreshIcon), buttonStyle))
            {
                LoadWeaponDefinitions();
            }
            
            if (GUILayout.Button(new GUIContent("➕ Create New", addIcon), primaryButtonStyle))
            {
                selectedTab = 2; // Switch to Create tab
            }
            
            GUILayout.Space(10);
            
            if (selectedWeapon != null)
            {
                DrawSelectedWeaponSidebar();
            }
        }
        
        private void DrawSelectedWeaponSidebar()
        {
            GUILayout.Label("🎯 Selected Weapon", subHeaderStyle);
            
            GUILayout.BeginVertical(selectedWeaponItemStyle);
            {
                GUILayout.Label(selectedWeapon.displayName, EditorStyles.boldLabel);
                GUILayout.Label($"{GetCategoryIcon(selectedWeapon.category)} {selectedWeapon.category}", EditorStyles.miniLabel);
                GUILayout.Label($"🏹 {selectedWeapon.archetype}", EditorStyles.miniLabel);
                
                if (!string.IsNullOrEmpty(selectedWeapon.description))
                {
                    GUILayout.Space(5);
                    GUILayout.Label(selectedWeapon.description, EditorStyles.wordWrappedMiniLabel);
                }
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("🎛️ Edit in Inspector", buttonStyle))
            {
                Selection.activeObject = selectedWeapon;
                EditorGUIUtility.PingObject(selectedWeapon);
            }
            
            if (GUILayout.Button("⚡ Generate Code", primaryButtonStyle))
            {
                GenerateSelectedWeapon();
            }
            
            if (GUILayout.Button("📤 Export JSON", buttonStyle))
            {
                ExportWeaponToJson(selectedWeapon);
            }
        }
        
        private void DrawWeaponsContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                var filteredWeapons = string.IsNullOrEmpty(searchFilter) 
                    ? weaponDefinitions 
                    : weaponDefinitions.Where(w => 
                        w.weaponName.ToLower().Contains(searchFilter.ToLower()) ||
                        w.displayName.ToLower().Contains(searchFilter.ToLower())).ToList();
                
                if (filteredWeapons.Count == 0)
                {
                    if (string.IsNullOrEmpty(searchFilter))
                    {
                        DrawEmptyWeaponsState();
                    }
                    else
                    {
                        GUILayout.BeginVertical(warningBoxStyle);
                        {
                            GUILayout.Label($"No weapons found matching '{searchFilter}'", EditorStyles.boldLabel);
                            GUILayout.Label("Try a different search term or create a new weapon.");
                        }
                        GUILayout.EndVertical();
                    }
                }
                else
                {
                    foreach (var weapon in filteredWeapons)
                    {
                        DrawWeaponCard(weapon);
                    }
                }
            }
            GUILayout.EndScrollView();
        }
        
        private void DrawEmptyWeaponsState()
        {
            GUILayout.BeginVertical(infoBoxStyle);
            {
                GUILayout.Label("🎯 No Weapons Found", headerStyle, GUILayout.Height(40));
                GUILayout.Label("Get started by creating your first weapon system!", subHeaderStyle);
                
                GUILayout.Space(20);
                
                if (GUILayout.Button("🛠️ Create Your First Weapon", primaryButtonStyle, GUILayout.Height(40)))
                {
                    selectedTab = 2;
                }
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("📥 Import Example Weapons", buttonStyle))
                {
                    CreateExampleWeapons();
                }
            }
            GUILayout.EndVertical();
        }
        
        private void DrawWeaponCard(WeaponDefinition weapon)
        {
            var isSelected = selectedWeapon == weapon;
            var style = isSelected ? selectedWeaponItemStyle : weaponItemStyle;
            
            GUILayout.BeginVertical(style);
            {
                GUILayout.BeginHorizontal();
                {
                    // Weapon icon and basic info
                    GUILayout.BeginVertical(GUILayout.Width(60));
                    {
                        GUILayout.Label(GetCategoryIcon(weapon.category), headerStyle, GUILayout.Height(30));
                        GUILayout.Label(weapon.category.ToString(), EditorStyles.miniLabel);
                    }
                    GUILayout.EndVertical();
                    
                    // Main weapon info
                    GUILayout.BeginVertical();
                    {
                        if (GUILayout.Button(weapon.displayName, isSelected ? EditorStyles.boldLabel : EditorStyles.label))
                        {
                            selectedWeapon = isSelected ? null : weapon;
                        }
                        
                        GUILayout.Label($"Code Name: {weapon.weaponName}", EditorStyles.miniLabel);
                        GUILayout.Label($"Type: {weapon.archetype}", EditorStyles.miniLabel);
                        
                        if (weapon.upgradeLevels?.Count > 0)
                        {
                            GUILayout.Label($"Upgrade Levels: {weapon.upgradeLevels.Count}", EditorStyles.miniLabel);
                        }
                        
                        if (!string.IsNullOrEmpty(weapon.description))
                        {
                            var shortDesc = weapon.description.Length > 100 
                                ? weapon.description.Substring(0, 100) + "..."
                                : weapon.description;
                            GUILayout.Label(shortDesc, EditorStyles.miniLabel);
                        }
                    }
                    GUILayout.EndVertical();
                    
                    // Action buttons
                    GUILayout.BeginVertical(GUILayout.Width(80));
                    {
                        if (GUILayout.Button("⚡", buttonStyle, GUILayout.Width(25), GUILayout.Height(25)))
                        {
                            var weapons = new List<WeaponDefinition> { weapon };
                            WeaponCodeGenerator.GenerateAllWeaponCode(weapons);
                        }
                        
                        if (GUILayout.Button("🎛️", buttonStyle, GUILayout.Width(25), GUILayout.Height(25)))
                        {
                            Selection.activeObject = weapon;
                            EditorGUIUtility.PingObject(weapon);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        
        // Create Tab
        private void DrawCreateSidebar()
        {
            GUILayout.Label("🛠️ Weapon Creator", subHeaderStyle);
            
            GUILayout.BeginVertical(infoBoxStyle);
            {
                GUILayout.Label("Quick Tips:", EditorStyles.boldLabel);
                GUILayout.Label("• Use PascalCase for weapon names");
                GUILayout.Label("• Choose meaningful categories");
                GUILayout.Label("• Start with simple behaviors");
                GUILayout.Label("• Test frequently during development");
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            GUILayout.Label("📋 Templates", subHeaderStyle);
            
            if (GUILayout.Button("🔫 Basic Projectile", buttonStyle))
            {
                LoadProjectileTemplate();
            }
            
            if (GUILayout.Button("💥 Artillery Weapon", buttonStyle))
            {
                LoadArtilleryTemplate();
            }
            
            if (GUILayout.Button("⚡ Beam Weapon", buttonStyle))
            {
                LoadBeamTemplate();
            }
            
            if (GUILayout.Button("🔗 Chain Lightning", buttonStyle))
            {
                LoadChainLightningTemplate();
            }
        }
        
        private void DrawCreateContent()
        {
            creatorScrollPosition = GUILayout.BeginScrollView(creatorScrollPosition);
            {
                DrawCreateWeaponForm();
            }
            GUILayout.EndScrollView();
        }
        
        // Import/Export Tab
        private void DrawImportExportSidebar()
        {
            GUILayout.Label("📁 File Operations", subHeaderStyle);
            
            GUILayout.BeginVertical(infoBoxStyle);
            {
                GUILayout.Label("Supported Formats:", EditorStyles.boldLabel);
                GUILayout.Label("• JSON weapon definitions");
                GUILayout.Label("• Batch import/export");
                GUILayout.Label("• Template collections");
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button(new GUIContent("📥 Import JSON", importIcon), buttonStyle))
            {
                ImportWeaponFromJson();
            }
            
            if (GUILayout.Button(new GUIContent("📤 Export All", exportIcon), buttonStyle))
            {
                ExportAllWeaponsToJson();
            }
            
            if (GUILayout.Button("📁 Open JSON Folder", buttonStyle))
            {
                var path = Path.Combine(Application.dataPath, "WeaponDefinitions");
                EditorUtility.RevealInFinder(path);
            }
        }
        
        private void DrawImportExportContent()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                DrawImportSection();
                GUILayout.Space(20);
                DrawExportSection();
                GUILayout.Space(20);
                DrawBatchOperationsSection();
            }
            GUILayout.EndScrollView();
        }
        
        private void DrawImportSection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("📥 Import Weapons", headerStyle);
                GUILayout.Space(5);
                
                GUILayout.Label("Import weapon definitions from JSON files to quickly add new weapons to your project.", EditorStyles.wordWrappedLabel);
                
                GUILayout.Space(10);
                
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("📥 Import Single JSON", primaryButtonStyle))
                    {
                        ImportWeaponFromJson();
                    }
                    
                    if (GUILayout.Button("📦 Import Batch", buttonStyle))
                    {
                        ImportBatchWeaponsFromFolder();
                    }
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                GUILayout.BeginVertical(infoBoxStyle);
                {
                    GUILayout.Label("✨ Quick Import Templates", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("🏹 Basic Set", buttonStyle))
                        {
                            CreateExampleWeapons();
                        }
                        
                        if (GUILayout.Button("⚡ Advanced Set", buttonStyle))
                        {
                            CreateAdvancedExampleWeapons();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        
        private void DrawExportSection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("📤 Export Weapons", headerStyle);
                GUILayout.Space(5);
                
                GUILayout.Label("Export your weapon definitions to JSON for backup, sharing, or version control.", EditorStyles.wordWrappedLabel);
                
                GUILayout.Space(10);
                
                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = selectedWeapon != null;
                    if (GUILayout.Button("📤 Export Selected", buttonStyle))
                    {
                        ExportWeaponToJson(selectedWeapon);
                    }
                    GUI.enabled = true;
                    
                    GUI.enabled = weaponDefinitions.Count > 0;
                    if (GUILayout.Button("📦 Export All", primaryButtonStyle))
                    {
                        ExportAllWeaponsToJson();
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("📁 Open Export Folder", buttonStyle))
                {
                    var path = Path.Combine(Application.dataPath, "WeaponDefinitions");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    EditorUtility.RevealInFinder(path);
                }
            }
            GUILayout.EndVertical();
        }
        
        private void DrawBatchOperationsSection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("🔧 Batch Operations", headerStyle);
                GUILayout.Space(5);
                
                GUILayout.Label("Perform operations on multiple weapons at once.", EditorStyles.wordWrappedLabel);
                
                GUILayout.Space(10);
                
                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = weaponDefinitions.Count > 0;
                    if (GUILayout.Button("🚀 Generate All Code", primaryButtonStyle))
                    {
                        GenerateAllWeapons();
                    }
                    
                    if (GUILayout.Button("🔄 Refresh All", buttonStyle))
                    {
                        LoadWeaponDefinitions();
                        ShowNotification(new GUIContent("Refreshed weapon definitions!"));
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                GUILayout.BeginVertical(warningBoxStyle);
                {
                    GUILayout.Label("⚠️ Destructive Operations", EditorStyles.boldLabel);
                    GUILayout.Space(5);
                    
                    if (GUILayout.Button("🗑️ Clear All Generated Code", buttonStyle))
                    {
                        if (EditorUtility.DisplayDialog("Clear Generated Code", 
                            "This will delete all generated weapon components and bases. This action cannot be undone.\n\nAre you sure?", 
                            "Delete", "Cancel"))
                        {
                            ClearAllGeneratedCode();
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        
        // Helper Methods
        private string GetCategoryIcon(WeaponCategory category)
        {
            return category switch
            {
                WeaponCategory.Projectile => "🔫",
                WeaponCategory.Artillery => "💥",
                WeaponCategory.Laser => "⚡",
                WeaponCategory.Beam => "🌟",
                WeaponCategory.Missile => "🚀",
                WeaponCategory.Drone => "🤖",
                WeaponCategory.Special => "✨",
                _ => "⚔️"
            };
        }
        
        private void ShowSettingsMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset Window Layout"), false, () => {
                selectedTab = 0;
                selectedWeapon = null;
                searchFilter = "";
            });
            menu.AddItem(new GUIContent("Refresh All Data"), false, LoadWeaponDefinitions);
            menu.AddItem(new GUIContent("Open Documentation"), false, () => {
                Application.OpenURL("file://" + Path.Combine(Application.dataPath, "../README.md"));
            });
            menu.ShowAsContext();
        }
        
        private void GenerateSelectedWeapon()
        {
            if (selectedWeapon != null)
            {
                var weapons = new List<WeaponDefinition> { selectedWeapon };
                WeaponCodeGenerator.GenerateAllWeaponCode(weapons);
                ShowNotification(new GUIContent($"Generated {selectedWeapon.displayName}!"));
            }
        }
        
        private void GenerateAllWeapons()
        {
            WeaponCodeGenerator.GenerateAllWeaponCode(weaponDefinitions);
            ShowNotification(new GUIContent($"Generated {weaponDefinitions.Count} weapons!"));
        }
    }
}