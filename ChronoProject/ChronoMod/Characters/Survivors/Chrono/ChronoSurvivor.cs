using ChronoMod.Modules;
using ChronoMod.Modules.Characters;
using ChronoMod.Survivors.Chrono.Components;
using ChronoMod.Survivors.Chrono.SkillStates;
using RoR2;
using RoR2.Skills;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Survivors.Chrono {
    public class ChronoSurvivor : SurvivorBase<ChronoSurvivor> {
        public override string assetBundleName => "mwmwchronomodbundle";

        public override string bodyName => "ChronoBody";

        public override string masterName => "ChronoMonsterMaster";

        public override string modelPrefabName => "mdlChronoFS";

        public override string displayPrefabName => "ChronoFSDisplay";

        public const string CHRONO_PREFIX = ChronoPlugin.DEVELOPER_PREFIX + "_CHRONO_";

        public override string survivorTokenPrefix => CHRONO_PREFIX;

        public override BodyInfo bodyInfo => new BodyInfo {
            bodyName = bodyName,
            bodyNameToken = CHRONO_PREFIX + "NAME",
            subtitleNameToken = CHRONO_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texChronoIcon"),
            bodyColor = new Color(0f, 0.78f, 0.99f),
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
                    ignoreOverlays = true,
                },
                new CustomRendererInfo
                {
                    childName = "OutlineMesh",
                    material = assetBundle.LoadMaterial("matOutline")
                },
                new CustomRendererInfo
                {
                    childName = "GunMesh",
                    material = assetBundle.LoadMaterial("matGun"),
                    ignoreOverlays = true,
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

            // Load soundbanks required for sfx reuse
            // Too many soundbanks!!!!
            SetupAkBanks();
        }

        private void SetupAkBanks() {
            AkBank[] banksToLoad = {
                Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Merc.MercBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
                Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Brother.BrotherBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
                Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Captain.CaptainBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
                Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Huntress.HuntressBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
                Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_Railgunner.RailgunnerBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
                Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidSurvivor.VoidSurvivorBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
                Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_FalseSon.FalseSonBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
                Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_Seeker.SeekerBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
                Addressables.LoadAssetAsync<GameObject>(RoR2_DLC3_Drifter.DrifterBody_prefab).WaitForCompletion()?.GetComponent<AkBank>(),
            };
            foreach (AkBank bank in banksToLoad) {
                if (bank != null) {
                    AkBank akBank = bodyPrefab.AddComponent<AkBank>();
                    akBank.triggerList = bank.triggerList;
                    akBank.data.WwiseObjectReference = bank.data.WwiseObjectReference;
                    akBank.unloadTriggerList = bank.unloadTriggerList;
                }
            }
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
            Prefabs.AddEntityStateMachine(bodyPrefab, "Utility");
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
                keywordToken = CHRONO_PREFIX + "KEYWORD_HYPERCRIT",
                icon = Addressables.LoadAssetAsync<Sprite>(RoR2_DLC2_FalseSon.texFalseSonSkillIcons_png_texFalseSonSkillIcons_0_).WaitForCompletion(),
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
            SteppedSkillDef primarySkillDef1 = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo {
                skillName = "ChronoEdge",
                skillNameToken = CHRONO_PREFIX + "PRIMARY_EDGE_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "PRIMARY_EDGE_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = Addressables.LoadAssetAsync<Sprite>(RoR2_Base_Merc.texMercSkillIcons_png_texMercSkillIcons_1_).WaitForCompletion(), // assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(EonsEdge)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Any,

                dontAllowPastMaxStocks = true,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            });

            //custom Skilldefs can have additional fields that you can set manually
            primarySkillDef1.stepCount = 2;
            primarySkillDef1.stepGraceDuration = 0.5f;

            SteppedSkillDef primarySkillDef2 = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo {
                skillName = "ChronoThrow",
                skillNameToken = CHRONO_PREFIX + "PRIMARY_THROW_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "PRIMARY_THROW_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = Addressables.LoadAssetAsync<Sprite>(RoR2_DLC2_FalseSon.texFalseSonSkillIcons_png_texFalseSonSkillIcons_2_).WaitForCompletion(),

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
            primarySkillDef2.stepCount = 2;
            primarySkillDef2.stepGraceDuration = 0.5f;

            Skills.AddPrimarySkills(bodyPrefab, primarySkillDef1, primarySkillDef2);
        }

        private void AddSecondarySkills() {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            SkillDef secondarySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoPiercer",
                skillNameToken = CHRONO_PREFIX + "SECONDARY_PIERCER_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "SECONDARY_PIERCER_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE", "KEYWORD_STUNNING", "KEYWORD_FREEZING" },
                skillIcon = Addressables.LoadAssetAsync<Sprite>(RoR2_Base_Commando.texCommandoSkillIcons_png_texCommandoSkillIcons_5_).WaitForCompletion(),

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
                keywordTokens = new string[] { "KEYWORD_AGILE", "KEYWORD_FREEZING" },
                skillIcon = Addressables.LoadAssetAsync<Sprite>(RoR2_Base_Mage.texMageSkillIcons_png_texMageSkillIcons_1_).WaitForCompletion(),

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
                mustKeyPress = true,
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
                keywordTokens = new string[] { "KEYWORD_FREEZING", CHRONO_PREFIX + "KEYWORD_TIME_WARP" },
                skillIcon = Addressables.LoadAssetAsync<Sprite>(RoR2_Base_Commando.texCommandoSkillIcons_png_texCommandoSkillIcons_6_).WaitForCompletion(),

                activationState = new EntityStates.SerializableEntityStateType(typeof(EchoOfTomorrowAim)),
                activationStateMachineName = "Utility",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 10f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            SkillDef utilitySkillDef2 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoContinuum",
                skillNameToken = CHRONO_PREFIX + "UTILITY_CONTINUUM_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "UTILITY_CONTINUUM_DESCRIPTION",
                keywordTokens = new string[] { CHRONO_PREFIX + "KEYWORD_RIFTED_TOUCH", "KEYWORD_FREEZING" },
                skillIcon = Addressables.LoadAssetAsync<Sprite>(RoR2_Base_Engi.texEngiSkillIcons_png_texEngiSkillIcons_2_).WaitForCompletion(),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ContinuumFreeze)),
                activationStateMachineName = "Utility",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 16f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef1, utilitySkillDef2);
        }

        private void AddSpecialSkills() {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            SkillDef specialSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "ChronoCollapse",
                skillNameToken = CHRONO_PREFIX + "SPECIAL_COLLAPSE_NAME",
                skillDescriptionToken = CHRONO_PREFIX + "SPECIAL_COLLAPSE_DESCRIPTION",
                keywordTokens = new string[] { CHRONO_PREFIX + "KEYWORD_FINALE" },
                skillIcon = Addressables.LoadAssetAsync<Sprite>(RoR2_DLC2_FalseSon.texFalseSonSkillIcons_png_texFalseSonSkillIcons_8_).WaitForCompletion(),

                activationState = new EntityStates.SerializableEntityStateType(typeof(TimeCollapseAim)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 60f,
                stockToConsume = 0,

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
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texChronoDefaultIcon"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            skins.Add(defaultSkin);
            #endregion

            #region MasterySkin

            SkinDef masterySkin = Modules.Skins.CreateSkinDef(CHRONO_PREFIX + "MASTERY_SKIN_NAME",
                assetBundle.LoadAsset<Sprite>("texChronoMasteryIcon"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject,
                ChronoUnlockables.masterySkinUnlockableDef);

            masterySkin.skinDefParams.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matBodyMastery");
            masterySkin.skinDefParams.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matSpearMastery");
            masterySkin.skinDefParams.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matOutlineMastery");
            masterySkin.skinDefParams.rendererInfos[3].defaultMaterial = assetBundle.LoadMaterial("matGunMastery");

            skins.Add(masterySkin);

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