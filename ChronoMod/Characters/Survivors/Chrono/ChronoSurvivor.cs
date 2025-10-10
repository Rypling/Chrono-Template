using System.Collections.Generic;
using ChronoMod.Modules;
using ChronoMod.Modules.Characters;
using ChronoMod.Survivors.Chrono.Components;
using ChronoMod.Survivors.Chrono.SkillStates;
using RoR2;
using RoR2.Skills;
using RoR2BepInExPack.GameAssetPathsBetter;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Survivors.Chrono {
    public class ChronoSurvivor : SurvivorBase<ChronoSurvivor> {
        //used to load the assetbundle for this character. must be unique
        public override string assetBundleName => "placeholderassetbundle";

        //the name of the prefab we will create. conventionally ending in "Body". must be unique
        public override string bodyName => "ChronoBody"; //if you do not change this, you get the point by now

        //name of the ai master for vengeance and goobo. must be unique
        public override string masterName => "ChronoMonsterMaster"; //if you do not

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlChronoFS";
        public override string displayPrefabName => "ChronoFSDisplay";

        public const string CHRONO_PREFIX = ChronoPlugin.DEVELOPER_PREFIX + "_CHRONO_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => CHRONO_PREFIX;

        public override BodyInfo bodyInfo => new BodyInfo {
            bodyName = bodyName,
            bodyNameToken = CHRONO_PREFIX + "NAME",
            subtitleNameToken = CHRONO_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texHenryIcon"),
            bodyColor = Color.white,
            sortPosition = 100,

            crosshair = Asset.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 110f,
            healthRegen = 1.5f,
            armor = 0f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "BodyMesh",
                    material = assetBundle.LoadMaterial("matBody"),
                },
                new CustomRendererInfo
                {
                    childName = "SpearMesh",
                    material = assetBundle.LoadMaterial("matSpear"),
                },
                new CustomRendererInfo
                {
                     childName = "OutlineMesh",
                    material = assetBundle.LoadMaterial("matOutline")
                }
        };

        public override UnlockableDef characterUnlockableDef => ChronoUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new ChronoItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public override void Initialize() {
            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            base.Initialize();
        }

        public override void InitializeCharacter() {
            //need the character unlockable before you initialize the survivordef
            ChronoUnlockables.Init();

            base.InitializeCharacter();

            ChronoConfig.Init();
            ChronoStates.Init();
            ChronoTokens.Init();

            ChronoBuffs.Init(assetBundle);
            ChronoAssets.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();
        }

        private void AdditionalBodySetup() {
            AddHitboxes();
            bodyPrefab.AddComponent<ChronoController>();

            // Replace animator controllers for false son's
            displayPrefab.GetComponent<Animator>().runtimeAnimatorController = Addressables.LoadAssetAsync<RuntimeAnimatorController>(RoR2_DLC2_FalseSon.animFalseSonDisplay_controller).WaitForCompletion();
            prefabCharacterModel.GetComponent<Animator>().runtimeAnimatorController = Addressables.LoadAssetAsync<RuntimeAnimatorController>(RoR2_DLC2_FalseSon.animFalseSon_controller).WaitForCompletion();
        }

        public void AddHitboxes() {
            Prefabs.SetupHitBoxGroup(characterModelObject, "SwingGroup", "SwingHitbox");
        }

        public override void InitializeEntityStateMachines() {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
        }

        #region skills
        public override void InitializeSkills() {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            AddPassiveSkill();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtilitySkills();
            AddSpecialSkills();
        }

        //skip if you don't have a passive
        //also skip if this is your first look at skills
        private void AddPassiveSkill() {
            //option 1. fake passive icon just to describe functionality we will implement elsewhere
            bodyPrefab.GetComponent<SkillLocator>().passiveSkill = new SkillLocator.PassiveSkill {
                enabled = true,
                skillNameToken = CHRONO_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "PASSIVE_DESCRIPTION",
                // keywordToken = "KEYWORD_STUNNING",
                icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
            };

            /*
            //option 2. a new SkillFamily for a passive, used if you want multiple selectable passives
            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            SkillDef passiveSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoPassive",
                skillNameToken = CHRONO_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "PASSIVE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),

                //unless you're somehow activating your passive like a skill, none of the following is needed.
                //but that's just me saying things. the tools are here at your disposal to do whatever you like with

                //activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                //activationStateMachineName = "Weapon1",
                //interruptPriority = EntityStates.InterruptPriority.Skill,

                //baseRechargeInterval = 1f,
                //baseMaxStock = 1,

                //rechargeStock = 1,
                //requiredStock = 1,
                //stockToConsume = 1,

                //resetCooldownTimerOnUse = false,
                //fullRestockOnAssign = true,
                //dontAllowPastMaxStocks = false,
                //mustKeyPress = false,
                //beginSkillCooldownOnSkillEnd = false,

                //isCombatSkill = true,
                //canceledFromSprinting = false,
                //cancelSprintingOnActivation = false,
                //forceSprintDuringState = false,

            });
            
            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef1);
            */
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills() {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            SteppedSkillDef primarySkillDef1 = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "ChronoEdge",
                    CHRONO_PREFIX + "PRIMARY_EDGE_NAME",
                    CHRONO_PREFIX + "PRIMARY_EDGE_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.SlashCombo)),
                    "Weapon",
                    true
                ));
            //custom Skilldefs can have additional fields that you can set manually
            primarySkillDef1.stepCount = 2;
            primarySkillDef1.stepGraceDuration = 0.5f;

            SkillDef primarySkillDef2 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoThrow",
                skillNameToken = CHRONO_PREFIX + "PRIMARY_THROW_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "PRIMARY_THROW_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SplitSecondThrow)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                dontAllowPastMaxStocks = true,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            Skills.AddPrimarySkills(bodyPrefab, primarySkillDef1, primarySkillDef2);
        }

        private void AddSecondarySkills() {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            SkillDef secondarySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoPiercer",
                skillNameToken = CHRONO_PREFIX + "SECONDARY_PIERCER_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "SECONDARY_PIERCER_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(TimePiercer)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 2f,
                baseMaxStock = 8,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,
                attackSpeedBuffsRestockSpeed = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            SkillDef secondarySkillDef2 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoHorizon",
                skillNameToken = CHRONO_PREFIX + "SECONDARY_HORIZON_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "SECONDARY_HORIZON_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(EventHorizon)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 12f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef1, secondarySkillDef2);
        }

        private void AddUtilitySkills() {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);

            SkillDef utilitySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoEcho",
                skillNameToken = CHRONO_PREFIX + "UTILITY_ECHO_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "UTILITY_ECHO_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(EchoOfTomorrow)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 8f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            SkillDef utilitySkillDef2 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoContinuum",
                skillNameToken = CHRONO_PREFIX + "UTILITY_CONTINUUM_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "UTILITY_CONTINUUM_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ContinuumFreeze)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 16f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef1, utilitySkillDef2);
        }

        private void AddSpecialSkills() {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            //a basic skill. some fields are omitted and will just have default values
            SkillDef specialSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoCollapse",
                skillNameToken = CHRONO_PREFIX + "SPECIAL_COLLAPSE_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "SPECIAL_COLLAPSE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(TimeCollapse)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 30f,

                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                // dontAllowPastMaxStocks = true,
                mustKeyPress = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
            });

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef1);
        }
        #endregion skills

        #region skins
        public override void InitializeSkins() {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySword",
            //    "meshHenryGun",
            //    "meshHenry");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin

            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(HENRY_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    HenryUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshHenryAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);

            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster() {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            ChronoAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }
    }
}