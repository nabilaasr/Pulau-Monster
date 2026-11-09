using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[InitializeOnLoad]
public static class PulauMonsterEditor
{
    private const string BaseProjectDir = "Assets/_Project";
    private const string StarterPackDir = "Assets/_Project/ThirdParty/StarterAssetPack";

    static PulauMonsterEditor()
    {
        EditorApplication.delayCall += CheckAutoRun;
    }

    private static void CheckAutoRun()
    {
        if (!SessionState.GetBool("PM_RunAll_Executed", false))
        {
            SessionState.SetBool("PM_RunAll_Executed", true);
            RunAll();
        }
    }

    [MenuItem("Pulau Monster/00 Run All", priority = 0)]
    public static void RunAll()
    {
        Debug.Log("[Pulau Monster] Starting 00 Run All pipeline...");
        ApplyProjectSettings();
        SetupPixelArtPipeline();
        BuildPlayerAssetsAndRuleTiles();
        BuildMonsterDataAndOverrides();
        BuildSandboxScene();
        BuildWorldScene();
        BuildBattleScene();
        BuildTitleAndEndScenes();
        BuildBootstrapSceneAndLighting();
        ApplyBuildSettings();
        Debug.Log("[Pulau Monster] 00 Run All completed successfully!");
    }

    [MenuItem("Pulau Monster/01 Apply Project Settings", priority = 1)]
    public static void ApplyProjectSettings()
    {
        PlayerSettings.companyName = "Politeknik Negeri Batam";
        PlayerSettings.productName = "Pulau Monster";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.runInBackground = true;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;

        EnsureTag("Player");
        EnsureLayer("Player");
        EnsureLayer("Interactable");
        EnsureLayer("Wild");
        EnsureLayer("CameraBounds");

        EnsureSortingLayer("Ground");
        EnsureSortingLayer("Decor");
        EnsureSortingLayer("Characters");

        int cbLayer = LayerMask.NameToLayer("CameraBounds");
        if (cbLayer >= 0)
        {
            for (int i = 0; i < 32; i++)
            {
                Physics2D.IgnoreLayerCollision(cbLayer, i, true);
            }
        }

        string[] rendererGuids = AssetDatabase.FindAssets("t:Renderer2DData");
        foreach (string guid in rendererGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var rendererData = AssetDatabase.LoadAssetAtPath<Renderer2DData>(path);
            if (rendererData != null)
            {
                var so = new SerializedObject(rendererData);
                SerializedProperty sortModeProp = so.FindProperty("m_TransparencySortMode");
                SerializedProperty sortAxisProp = so.FindProperty("m_TransparencySortAxis");
                if (sortModeProp != null) sortModeProp.enumValueIndex = (int)TransparencySortMode.CustomAxis;
                if (sortAxisProp != null) sortAxisProp.vector3Value = new Vector3(0, 1, 0);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(rendererData);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Pulau Monster] Project Settings applied.");
    }

    [MenuItem("Pulau Monster/02 Setup Pixel Art Pipeline", priority = 2)]
    public static void SetupPixelArtPipeline()
    {
        EnsureFolder("Assets/_Project/Art");
        EnsureFolder("Assets/_Project/Art/Characters");
        EnsureFolder("Assets/_Project/Art/Monsters");
        EnsureFolder("Assets/_Project/Art/Tiles");
        EnsureFolder("Assets/_Project/Art/UI");
        EnsureFolder("Assets/_Project/Art/Backgrounds");
        EnsureFolder("Assets/_Project/Art/Vfx");
        EnsureFolder("Assets/_Project/Art/Fonts");
        EnsureFolder("Assets/_Project/Audio");
        EnsureFolder("Assets/_Project/Data");
        EnsureFolder("Assets/_Project/Settings/Presets");

        // Move/Copy files from StarterAssetPack
        MovePackFolder("Characters", "Assets/_Project/Art/Characters");
        MovePackFolder("Monsters", "Assets/_Project/Art/Monsters");
        MovePackFolder("Tiles", "Assets/_Project/Art/Tiles");
        MovePackFolder("UI", "Assets/_Project/Art/UI");
        MovePackFolder("Backgrounds", "Assets/_Project/Art/Backgrounds");
        MovePackFolder("Vfx", "Assets/_Project/Art/Vfx");
        MovePackFolder("Fonts", "Assets/_Project/Art/Fonts");
        MovePackFolder("Audio", "Assets/_Project/Audio");
        MovePackFolder("Data", "Assets/_Project/Data");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // Configure single textures
        ConfigureSingleSprite("Assets/_Project/Art/UI/UI_Panel_Navy.png", 5);
        ConfigureSingleSprite("Assets/_Project/Art/UI/UI_Panel_Gold.png", 5);
        ConfigureSingleSprite("Assets/_Project/Art/UI/UI_Panel_Narr.png", 5);
        ConfigureSingleSprite("Assets/_Project/Art/UI/UI_Pixel.png", 0);
        ConfigureSingleSprite("Assets/_Project/Art/UI/UI_Ring.png", 0);
        ConfigureSingleSprite("Assets/_Project/Art/UI/UI_Alert.png", 0);

        ConfigureSingleSprite("Assets/_Project/Art/Backgrounds/BG_Battle_Forest.png", 0);
        ConfigureSingleSprite("Assets/_Project/Art/Backgrounds/BG_Battle_Grassland.png", 0);
        ConfigureSingleSprite("Assets/_Project/Art/Backgrounds/BG_Battle_Cave.png", 0);
        ConfigureSingleSprite("Assets/_Project/Art/Backgrounds/BG_Title_World.png", 0);

        // Slice characters (48x64, 3 cols x 4 rows, pivot Bottom)
        string[] chars = { "Player_Walk_16x16.png", "NPC_Elder_16x16.png", "NPC_Girl_16x16.png", "NPC_Guard_16x16.png", "NPC_Merchant_16x16.png" };
        foreach (string c in chars)
        {
            SliceGridSprite($"Assets/_Project/Art/Characters/{c}", 16, 16, 3, 4, new Vector2(0.5f, 0f));
        }

        // Slice monsters (4 cols x 1 row, pivot Bottom)
        string[] mons = {
            "Pyro_1_Pyrit_16x16.png", "Aqua_1_Blubi_16x16.png", "Sprout_1_Sprig_16x16.png", "Volt_1_Zapi_16x16.png",
            "Pyro_2_Pyrago_20x20.png", "Aqua_2_Finnox_20x20.png", "Sprout_2_Bloomlet_20x20.png", "Volt_2_Voltcat_20x20.png",
            "Pyro_3_Infernox_24x24.png", "Aqua_3_Tidalord_24x24.png", "Sprout_3_Florazor_24x24.png", "Volt_3_Raijugar_24x24.png"
        };
        foreach (string m in mons)
        {
            int sz = m.Contains("16x16") ? 16 : (m.Contains("20x20") ? 20 : 24);
            SliceGridSprite($"Assets/_Project/Art/Monsters/{m}", sz, sz, 4, 1, new Vector2(0.5f, 0f));
        }

        // Slice tileset (192x96, 12 cols x 6 rows of 16x16, pivot Center)
        SliceGridSprite("Assets/_Project/Art/Tiles/Tileset_Overworld.png", 16, 16, 12, 6, new Vector2(0.5f, 0.5f));

        // Slice VFX Slash (128x32, 4 cols x 1 row of 32x32, pivot Center)
        SliceGridSprite("Assets/_Project/Art/Vfx/VFX_Slash.png", 32, 32, 4, 1, new Vector2(0.5f, 0.5f));

        AssetDatabase.SaveAssets();
        Debug.Log("[Pulau Monster] Pixel art pipeline and sprite slicing configured.");
    }

    [MenuItem("Pulau Monster/03 Build Sandbox Scene", priority = 3)]
    public static void BuildSandboxScene()
    {
        EnsureFolder("Assets/_Project/Scenes/Sandbox");
        string scenePath = "Assets/_Project/Scenes/Sandbox/Scn_Sandbox.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.625f;
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0, 0, -10);

        var playerGo = new GameObject("Player");
        playerGo.tag = "Player";
        playerGo.layer = LayerMask.NameToLayer("Player");
        playerGo.transform.position = Vector3.zero;

        var sr = playerGo.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        Sprite playerSprite = LoadSpriteSubAsset("Assets/_Project/Art/Characters/Player_Walk_16x16.png", "Player_Walk_16x16_0");
        sr.sprite = playerSprite;

        var rb = playerGo.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = playerGo.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.4f);
        col.offset = new Vector2(0f, 0.2f);

        var pm = playerGo.AddComponent<PlayerMovement>();
        SetPrivateField(pm, "rb", rb);

        var playerCfg = AssetDatabase.LoadAssetAtPath<PlayerConfig>("Assets/_Project/Data/Cfg_Player.asset");
        SetPrivateField(pm, "config", playerCfg);

        playerGo.AddComponent<SandboxInstaller>();

        // Test Wall at (2.5, 0)
        CreateWall("Wall_Test", new Vector2(2.5f, 0f), new Vector2(1f, 4f));

        // 4 Boundary Walls
        CreateWall("Wall_North", new Vector2(0f, 6f), new Vector2(16f, 1f));
        CreateWall("Wall_South", new Vector2(0f, -6f), new Vector2(16f, 1f));
        CreateWall("Wall_West", new Vector2(-8f, 0f), new Vector2(1f, 12f));
        CreateWall("Wall_East", new Vector2(8f, 0f), new Vector2(1f, 12f));

        // 3 Obstacles
        CreateWall("Obstacle_1", new Vector2(-3f, 2f), new Vector2(1.5f, 1.5f));
        CreateWall("Obstacle_2", new Vector2(-2f, -3f), new Vector2(2f, 1f));
        CreateWall("Obstacle_3", new Vector2(4f, 3f), new Vector2(1.5f, 2f));

        // 1 Stationary NPC
        var npcGo = new GameObject("NPC_Static");
        npcGo.transform.position = new Vector2(-1.5f, 0.5f);
        var npcSr = npcGo.AddComponent<SpriteRenderer>();
        npcSr.sortingLayerName = "Characters";
        npcSr.sprite = LoadSpriteSubAsset("Assets/_Project/Art/Characters/NPC_Elder_16x16.png", "NPC_Elder_16x16_0");
        var npcCol = npcGo.AddComponent<BoxCollider2D>();
        npcCol.size = new Vector2(0.8f, 0.5f);
        npcCol.offset = new Vector2(0f, 0.25f);

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Pulau Monster] Scn_Sandbox created.");
    }

    [MenuItem("Pulau Monster/04 Build Player Assets and Rule Tiles", priority = 4)]
    public static void BuildPlayerAssetsAndRuleTiles()
    {
        EnsureFolder("Assets/_Project/Data");
        EnsureFolder("Assets/_Project/Art/Characters/Animations");
        EnsureFolder("Assets/_Project/Art/Tiles/RuleTiles");

        // PlayerConfig
        string cfgPath = "Assets/_Project/Data/Cfg_Player.asset";
        var cfg = AssetDatabase.LoadAssetAtPath<PlayerConfig>(cfgPath);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<PlayerConfig>();
            AssetDatabase.CreateAsset(cfg, cfgPath);
        }

        // Animation Clips for Player
        string sheetPath = "Assets/_Project/Art/Characters/Player_Walk_16x16.png";
        AnimationClip idleDown = CreateOrGetClip("Assets/_Project/Art/Characters/Animations/Anim_Player_Idle_Down.anim", sheetPath, new int[] { 0 }, 1, false);
        AnimationClip idleLeft = CreateOrGetClip("Assets/_Project/Art/Characters/Animations/Anim_Player_Idle_Left.anim", sheetPath, new int[] { 3 }, 1, false);
        AnimationClip idleRight = CreateOrGetClip("Assets/_Project/Art/Characters/Animations/Anim_Player_Idle_Right.anim", sheetPath, new int[] { 6 }, 1, false);
        AnimationClip idleUp = CreateOrGetClip("Assets/_Project/Art/Characters/Animations/Anim_Player_Idle_Up.anim", sheetPath, new int[] { 9 }, 1, false);

        AnimationClip walkDown = CreateOrGetClip("Assets/_Project/Art/Characters/Animations/Anim_Player_Walk_Down.anim", sheetPath, new int[] { 0, 1, 0, 2 }, 8, true);
        AnimationClip walkLeft = CreateOrGetClip("Assets/_Project/Art/Characters/Animations/Anim_Player_Walk_Left.anim", sheetPath, new int[] { 3, 4, 3, 5 }, 8, true);
        AnimationClip walkRight = CreateOrGetClip("Assets/_Project/Art/Characters/Animations/Anim_Player_Walk_Right.anim", sheetPath, new int[] { 6, 7, 6, 8 }, 8, true);
        AnimationClip walkUp = CreateOrGetClip("Assets/_Project/Art/Characters/Animations/Anim_Player_Walk_Up.anim", sheetPath, new int[] { 9, 10, 9, 11 }, 8, true);

        // AC_Player AnimatorController
        string controllerPath = "Assets/_Project/Art/Characters/Animations/AC_Player.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

        var rootSm = controller.layers[0].stateMachine;

        var idleState = rootSm.AddState("Idle");
        var idleTree = new BlendTree { name = "IdleTree", blendType = BlendTreeType.SimpleDirectional2D, blendParameter = "MoveX", blendParameterY = "MoveY" };
        idleTree.AddChild(idleDown, new Vector2(0, -1));
        idleTree.AddChild(idleLeft, new Vector2(-1, 0));
        idleTree.AddChild(idleRight, new Vector2(1, 0));
        idleTree.AddChild(idleUp, new Vector2(0, 1));
        idleState.motion = idleTree;
        rootSm.defaultState = idleState;

        var walkState = rootSm.AddState("Walk");
        var walkTree = new BlendTree { name = "WalkTree", blendType = BlendTreeType.SimpleDirectional2D, blendParameter = "MoveX", blendParameterY = "MoveY" };
        walkTree.AddChild(walkDown, new Vector2(0, -1));
        walkTree.AddChild(walkLeft, new Vector2(-1, 0));
        walkTree.AddChild(walkRight, new Vector2(1, 0));
        walkTree.AddChild(walkUp, new Vector2(0, 1));
        walkState.motion = walkTree;

        var toWalk = idleState.AddTransition(walkState);
        toWalk.hasExitTime = false;
        toWalk.duration = 0f;
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");

        var toIdle = walkState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.duration = 0f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");

        // Rule Tiles: Path & Water
        BuildRuleTile("Assets/_Project/Art/Tiles/RuleTiles/RT_Path.asset", "Tileset_Overworld",
            new int[] { 0, 1, 2, 12, 13, 14, 24, 25, 26 });

        BuildRuleTile("Assets/_Project/Art/Tiles/RuleTiles/RT_Water.asset", "Tileset_Overworld",
            new int[] { 5, 6, 7, 17, 18, 19, 29, 30, 31 });

        AssetDatabase.SaveAssets();
        Debug.Log("[Pulau Monster] Player animations, controller, and RuleTiles built.");
    }

    [MenuItem("Pulau Monster/05 Build World Scene", priority = 5)]
    public static void BuildWorldScene()
    {
        EnsureFolder("Assets/_Project/Scenes");
        string scenePath = "Assets/_Project/Scenes/Scn_World.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var gridGo = new GameObject("Grid");
        var grid = gridGo.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        var tmGround = CreateTilemapLayer(gridGo, "TM_Ground", "Ground", 0, false);
        var tmDecor = CreateTilemapLayer(gridGo, "TM_Decor", "Decor", 0, false);
        var tmBlocking = CreateTilemapLayer(gridGo, "TM_Blocking", "Characters", 0, true);
        var tmTallGrass = CreateTilemapLayer(gridGo, "TM_TallGrass", "Decor", 1, false, true);

        PaintWorldTilemaps(tmGround, tmDecor, tmBlocking, tmTallGrass);

        // Camera Bounds
        var boundsGo = new GameObject("CameraBounds");
        boundsGo.layer = LayerMask.NameToLayer("CameraBounds");
        var poly = boundsGo.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.points = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(40, 0),
            new Vector2(40, 36),
            new Vector2(0, 36)
        };

        // Player
        var playerGo = new GameObject("Player");
        playerGo.tag = "Player";
        playerGo.layer = LayerMask.NameToLayer("Player");
        playerGo.transform.position = new Vector3(20, 6, 0);

        var sr = playerGo.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        sr.sprite = LoadSpriteSubAsset("Assets/_Project/Art/Characters/Player_Walk_16x16.png", "Player_Walk_16x16_0");

        var rb = playerGo.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = playerGo.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.4f);
        col.offset = new Vector2(0f, 0.2f);

        var pm = playerGo.AddComponent<PlayerMovement>();
        var cfg = AssetDatabase.LoadAssetAtPath<PlayerConfig>("Assets/_Project/Data/Cfg_Player.asset");
        SetPrivateField(pm, "config", cfg);
        SetPrivateField(pm, "rb", rb);

        var anim = playerGo.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Art/Characters/Animations/AC_Player.controller");
        playerGo.AddComponent<PlayerAnimator>();

        var interactor = playerGo.AddComponent<PlayerInteractor>();
        SetPrivateField(interactor, "movement", pm);
        SetPrivateField(interactor, "interactableMask", (LayerMask)(1 << LayerMask.NameToLayer("Interactable")));

        var rustle = playerGo.AddComponent<GrassRustle>();
        SetPrivateField(rustle, "tallGrass", tmTallGrass);
        SetPrivateField(rustle, "movement", pm);

        // Partner
        var partnerGo = new GameObject("Partner");
        var partnerSr = partnerGo.AddComponent<SpriteRenderer>();
        partnerSr.sortingLayerName = "Characters";
        var follower = partnerGo.AddComponent<PartnerFollower>();
        SetPrivateField(follower, "body", partnerSr);

        // PixelBurst
        var burstGo = new GameObject("PixelBurst");
        var burst = burstGo.AddComponent<PixelBurst>();
        var pixelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Pixel.png");
        SetPrivateField(burst, "pixel", pixelSprite);
        SetPrivateField(rustle, "burst", burst);

        // Cameras
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(20, 6, -10);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.625f;

        camGo.AddComponent<Unity.Cinemachine.CinemachineBrain>();

        var cmFollowGo = new GameObject("CM_Follow");
        var vcam = cmFollowGo.AddComponent<Unity.Cinemachine.CinemachineCamera>();
        vcam.Target.TrackingTarget = playerGo.transform;
        vcam.Lens.OrthographicSize = 5.625f;

        var composer = cmFollowGo.AddComponent<Unity.Cinemachine.CinemachinePositionComposer>();
        composer.CameraDistance = 10f;
        composer.Damping = new Vector3(0.5f, 0.5f, 0f);

        var confiner = cmFollowGo.AddComponent<Unity.Cinemachine.CinemachineConfiner2D>();
        confiner.BoundingShape2D = poly;

        // NPCs
        CreateNpcs(burst);

        // Gate Palang
        var gateGo = new GameObject("Gate_Hutan");
        gateGo.transform.position = new Vector3(20f, 11.5f, 0f);
        var gateCol = gateGo.AddComponent<BoxCollider2D>();
        gateCol.size = new Vector2(2f, 1f);
        gateGo.AddComponent<FlagToggle>();

        // Area Zones
        CreateAreaZone("Desa Awal", new Vector2(20f, 5.5f), new Vector2(40f, 11f), "BGM_Village.wav");
        CreateAreaZone("Hutan Rimbun", new Vector2(20f, 17.5f), new Vector2(40f, 13f), "BGM_Forest.wav");
        CreateAreaZone("Danau Sunyi", new Vector2(10f, 30f), new Vector2(20f, 12f), "BGM_Lake.wav");
        CreateAreaZone("Gua Petir", new Vector2(30f, 30f), new Vector2(20f, 12f), "BGM_Cave.wav");

        // Wild Monsters
        CreateWildMonsters();

        // World HUD
        var hud = BuildWorldHud();

        // WorldInstaller
        var installerGo = new GameObject("WorldInstaller");
        var wi = installerGo.AddComponent<WorldInstaller>();
        SetPrivateField(wi, "player", pm);
        SetPrivateField(wi, "interactor", interactor);
        SetPrivateField(wi, "partnerFollower", follower);
        SetPrivateField(wi, "grassRustle", rustle);
        SetPrivateField(wi, "partnerHud", hud);
        SetPrivateField(wi, "burst", burst);
        SetPrivateField(wi, "respawnPoint", new Vector2(21.5f, 6.0f));

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Pulau Monster] Scn_World built.");
    }

    [MenuItem("Pulau Monster/06 Build Monster Data and Overrides", priority = 6)]
    public static void BuildMonsterDataAndOverrides()
    {
        EnsureFolder("Assets/_Project/Data/Monsters");
        string csvPath = "Assets/_Project/Data/MonsterDatabase.csv";
        if (!File.Exists(csvPath))
        {
            csvPath = "Assets/_Project/ThirdParty/StarterAssetPack/Data/MonsterDatabase.csv";
        }

        string[] lines = File.ReadAllLines(csvPath);
        var monDataDict = new Dictionary<string, MonsterData>();
        var evolutionMap = new Dictionary<string, string>();
        var evolveLevelMap = new Dictionary<string, int>();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] cols = line.Split(',');
            string id = cols[0];
            string name = cols[1];
            Element elem = (Element)Enum.Parse(typeof(Element), cols[2]);
            int stage = int.Parse(cols[3]);
            int hp = int.Parse(cols[4]);
            int atk = int.Parse(cols[5]);
            int def = int.Parse(cols[6]);
            int spd = int.Parse(cols[7]);
            string skill = cols[8];
            string evolvesTo = cols[9];
            int evLvl = int.Parse(cols[10]);

            int sz = stage == 1 ? 16 : (stage == 2 ? 20 : 24);
            string sheetPath = $"Assets/_Project/Art/Monsters/{cols[2]}_{stage}_{name}_{sz}x{sz}.png";
            Sprite[] frames = LoadAllSpriteSubAssets(sheetPath);

            string assetPath = $"Assets/_Project/Data/Monsters/Mon_{name}.asset";
            var md = AssetDatabase.LoadAssetAtPath<MonsterData>(assetPath);
            if (md == null)
            {
                md = ScriptableObject.CreateInstance<MonsterData>();
                AssetDatabase.CreateAsset(md, assetPath);
            }
            md.Setup(id, name, elem, hp, atk, def, spd, skill, null, evLvl, frames);
            EditorUtility.SetDirty(md);

            monDataDict[name] = md;
            if (!string.IsNullOrEmpty(evolvesTo))
            {
                evolutionMap[name] = evolvesTo;
                evolveLevelMap[name] = evLvl;
            }
        }

        foreach (var kvp in evolutionMap)
        {
            string fromName = kvp.Key;
            string toName = kvp.Value;
            if (monDataDict.ContainsKey(fromName) && monDataDict.ContainsKey(toName))
            {
                var md = monDataDict[fromName];
                var toMd = monDataDict[toName];
                SetPrivateField(md, "evolvesTo", toMd);
                EditorUtility.SetDirty(md);
            }
        }

        string dbPath = "Assets/_Project/Data/MonsterDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<MonsterDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }
        db.Setup(new List<MonsterData>(monDataDict.Values));
        EditorUtility.SetDirty(db);

        EnsureFolder("Assets/_Project/Art/Monsters/Animations");
        string monCtrlPath = "Assets/_Project/Art/Monsters/Animations/AC_Monster.controller";
        var monCtrl = AnimatorController.CreateAnimatorControllerAtPath(monCtrlPath);
        var baseSm = monCtrl.layers[0].stateMachine;
        var idleState = baseSm.AddState("Idle");

        foreach (var md in monDataDict.Values)
        {
            if (md.Frames != null && md.Frames.Length >= 2)
            {
                string clipPath = $"Assets/_Project/Art/Monsters/Animations/Anim_Mon_{md.DisplayName}_Idle.anim";
                AnimationClip clip = CreateOrGetClipFromSprites(clipPath, new Sprite[] { md.Frames[0], md.Frames[1] }, 3, true);

                string aocPath = $"Assets/_Project/Art/Monsters/Animations/AOC_Mon_{md.DisplayName}.overrideController";
                var aoc = new AnimatorOverrideController(monCtrl);
                aoc["Idle"] = clip;
                AssetDatabase.CreateAsset(aoc, aocPath);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Pulau Monster] Monster data and overrides built.");
    }

    [MenuItem("Pulau Monster/07 Build Battle Scene", priority = 7)]
    public static void BuildBattleScene()
    {
        EnsureFolder("Assets/_Project/Scenes");
        string scenePath = "Assets/_Project/Scenes/Scn_Battle.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Battle Camera");
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0, 0, -10);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.625f;

        var bgGo = new GameObject("Background");
        bgGo.transform.position = new Vector3(0, 0, 5);
        var bgSr = bgGo.AddComponent<SpriteRenderer>();
        var forestBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/BG_Battle_Forest.png");
        var caveBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/BG_Battle_Cave.png");
        bgSr.sprite = forestBg;

        var playerMonGo = new GameObject("PlayerMon");
        playerMonGo.transform.position = new Vector3(-4.5f, -2.375f, 0f);
        var playerMonSr = playerMonGo.AddComponent<SpriteRenderer>();
        playerMonSr.sortingLayerName = "Characters";
        playerMonSr.flipX = true;
        playerMonGo.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

        var enemyMonGo = new GameObject("EnemyMon");
        enemyMonGo.transform.position = new Vector3(4.5f, -0.125f, 0f);
        var enemyMonSr = enemyMonGo.AddComponent<SpriteRenderer>();
        enemyMonSr.sortingLayerName = "Characters";
        enemyMonGo.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

        var canvasGo = new GameObject("Canvas_Battle");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/Fonts/PixelifySans-Regular.ttf");
        var panelNavy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Panel_Navy.png");
        var pixelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Pixel.png");
        var ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Ring.png");

        var epGo = CreatePanel(canvasGo.transform, "EnemyPanel", 32, -32, 520, 140, panelNavy);
        var ep = epGo.AddComponent<MonsterPanel>();
        var epName = CreateText(epGo.transform, "Name", 18, -14, 300, 32, font, 24, UiTheme.Gold, "MONSTER LAWAN", TextAnchor.MiddleLeft);
        var epLvl = CreateText(epGo.transform, "Level", 320, -14, 182, 32, font, 22, UiTheme.Gold, "Lv. 03", TextAnchor.MiddleRight);
        var epElem = CreateText(epGo.transform, "Element", 18, -48, 160, 26, font, 20, UiTheme.Muted, "VOLT", TextAnchor.MiddleLeft);
        var epHpText = CreateText(epGo.transform, "HpText", 200, -48, 302, 26, font, 20, UiTheme.Text, "HP 60 / 80", TextAnchor.MiddleRight);
        var epBar = CreateBar(epGo.transform, "HpBar", 18, -80, 484, 22, pixelSprite, UiTheme.Ink, HpColors.High);
        SetPrivateField(ep, "nameText", epName);
        SetPrivateField(ep, "levelText", epLvl);
        SetPrivateField(ep, "elementText", epElem);
        SetPrivateField(ep, "hpText", epHpText);
        SetPrivateField(ep, "hpFill", epBar);
        SetPrivateField(ep, "monsterWorldSprite", enemyMonSr);

        var ppGo = CreatePanel(canvasGo.transform, "PlayerPanel", 728, -355, 520, 160, panelNavy);
        var pp = ppGo.AddComponent<MonsterPanel>();
        var ppName = CreateText(ppGo.transform, "Name", 18, -14, 300, 32, font, 24, UiTheme.Gold, "MONSTER PARTNER", TextAnchor.MiddleLeft);
        var ppLvl = CreateText(ppGo.transform, "Level", 320, -14, 182, 32, font, 22, UiTheme.Gold, "Lv. 05", TextAnchor.MiddleRight);
        var ppElem = CreateText(ppGo.transform, "Element", 18, -48, 160, 26, font, 20, UiTheme.Muted, "PYRO", TextAnchor.MiddleLeft);
        var ppHpText = CreateText(ppGo.transform, "HpText", 200, -48, 302, 26, font, 20, UiTheme.Text, "HP 80 / 100", TextAnchor.MiddleRight);
        var ppBar = CreateBar(ppGo.transform, "HpBar", 18, -80, 484, 22, pixelSprite, UiTheme.Ink, HpColors.High);
        var ppExpBar = CreateBar(ppGo.transform, "ExpBar", 18, -112, 484, 14, pixelSprite, UiTheme.Ink, UiTheme.Exp);
        SetPrivateField(pp, "nameText", ppName);
        SetPrivateField(pp, "levelText", ppLvl);
        SetPrivateField(pp, "elementText", ppElem);
        SetPrivateField(pp, "hpText", ppHpText);
        SetPrivateField(pp, "hpFill", ppBar);
        SetPrivateField(pp, "expFill", ppExpBar);
        SetPrivateField(pp, "monsterWorldSprite", playerMonSr);

        var mpGo = CreatePanel(canvasGo.transform, "MessagePanel", 0, -528, 784, 192, panelNavy);
        var msgText = CreateText(mpGo.transform, "Message", 24, -22, 736, 100, font, 26, UiTheme.Text, "");
        var hintText = CreateText(mpGo.transform, "SkillHint", 24, -132, 736, 38, font, 22, UiTheme.Muted, "");

        var menuGo = CreatePanel(canvasGo.transform, "MenuPanel", 784, -528, 496, 192, panelNavy);
        var textMenu = menuGo.AddComponent<TextMenu>();
        var menuTexts = new List<Text>();
        var menuBtns = new List<Button>();

        int[,] coords = { { 24, -28 }, { 256, -28 }, { 24, -100 }, { 256, -100 } };
        for (int i = 0; i < 4; i++)
        {
            var btnGo = new GameObject($"Btn_{i}");
            btnGo.transform.SetParent(menuGo.transform, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(coords[i, 0], coords[i, 1]);
            rect.sizeDelta = new Vector2(216, 60);

            var t = CreateText(btnGo.transform, "Text", 0, 0, 216, 60, font, 26, UiTheme.Text, $"Option {i}", TextAnchor.MiddleCenter);
            var btn = btnGo.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            menuTexts.Add(t);
            menuBtns.Add(btn);
        }
        textMenu.SetupItems(menuTexts, menuBtns, 2);

        var flashGo = new GameObject("Flash");
        flashGo.transform.SetParent(canvasGo.transform, false);
        var fRect = flashGo.AddComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero;
        fRect.anchorMax = Vector2.one;
        fRect.sizeDelta = Vector2.zero;
        var fImg = flashGo.AddComponent<Image>();
        fImg.sprite = pixelSprite;
        fImg.color = new Color(1, 1, 1, 0);

        var wipeTopGo = new GameObject("WipeTop");
        wipeTopGo.transform.SetParent(canvasGo.transform, false);
        var wtRect = wipeTopGo.AddComponent<RectTransform>();
        wtRect.anchorMin = new Vector2(0, 0.5f);
        wtRect.anchorMax = new Vector2(1, 1f);
        wtRect.sizeDelta = Vector2.zero;
        var wtImg = wipeTopGo.AddComponent<Image>();
        wtImg.sprite = pixelSprite;
        wtImg.color = UiTheme.Ink;
        wipeTopGo.SetActive(false);

        var wipeBotGo = new GameObject("WipeBottom");
        wipeBotGo.transform.SetParent(canvasGo.transform, false);
        var wbRect = wipeBotGo.AddComponent<RectTransform>();
        wbRect.anchorMin = new Vector2(0, 0f);
        wbRect.anchorMax = new Vector2(1, 0.5f);
        wbRect.sizeDelta = Vector2.zero;
        var wbImg = wipeBotGo.AddComponent<Image>();
        wbImg.sprite = pixelSprite;
        wbImg.color = UiTheme.Ink;
        wipeBotGo.SetActive(false);

        var rings = new RectTransform[3];
        for (int i = 0; i < 3; i++)
        {
            var rGo = new GameObject($"Ring_{i}");
            rGo.transform.SetParent(canvasGo.transform, false);
            var rRect = rGo.AddComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0.5f);
            rRect.anchorMax = new Vector2(0.5f, 0.5f);
            rRect.pivot = new Vector2(0.5f, 0.5f);
            rRect.sizeDelta = new Vector2(256, 256);
            var rImg = rGo.AddComponent<Image>();
            rImg.sprite = ringSprite;
            rGo.SetActive(false);
            rings[i] = rRect;
        }

        var view = canvasGo.AddComponent<BattleView>();
        SetPrivateField(view, "enemyPanel", ep);
        SetPrivateField(view, "playerPanel", pp);
        SetPrivateField(view, "messageText", msgText);
        SetPrivateField(view, "skillHintText", hintText);
        SetPrivateField(view, "battleMenu", textMenu);
        SetPrivateField(view, "flashOverlay", fImg);
        SetPrivateField(view, "wipeTop", wtRect);
        SetPrivateField(view, "wipeBottom", wbRect);
        SetPrivateField(view, "rings", rings);

        var bsGo = new GameObject("BattleSystem");
        var battleSys = bsGo.AddComponent<BattleSystem>();

        var baGo = new GameObject("BattleAudio");
        var battleAud = baGo.AddComponent<BattleAudio>();
        SetPrivateField(battleAud, "sfxHit", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Hit.wav"));
        SetPrivateField(battleAud, "sfxHitCritical", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Hit_Critical.wav"));
        SetPrivateField(battleAud, "sfxMiss", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Miss.wav"));
        SetPrivateField(battleAud, "sfxLevelUp", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_LevelUp.wav"));
        SetPrivateField(battleAud, "sfxEvolve", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Evolve.wav"));
        SetPrivateField(battleAud, "sfxVictory", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Victory_Jingle.wav"));
        SetPrivateField(battleAud, "sfxFaint", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Faint.wav"));
        SetPrivateField(battleAud, "sfxHeal", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Heal.wav"));

        var bfGo = new GameObject("BattleFeel");
        var feel = bfGo.AddComponent<BattleFeel>();

        var burstGo = new GameObject("PixelBurst");
        var burst = burstGo.AddComponent<PixelBurst>();
        SetPrivateField(burst, "pixel", pixelSprite);

        var vfxGo = new GameObject("VfxPool");
        var vfxPool = vfxGo.AddComponent<VfxPool>();
        Sprite[] slashFrames = LoadAllSpriteSubAssets("Assets/_Project/Art/Vfx/VFX_Slash.png");
        vfxPool.Construct(slashFrames);

        SetPrivateField(feel, "burst", burst);
        SetPrivateField(feel, "hitVfx", vfxPool);
        SetPrivateField(feel, "playerBody", playerMonSr);
        SetPrivateField(feel, "enemyBody", enemyMonSr);
        SetPrivateField(feel, "cameraRig", camGo.transform);

        var biGo = new GameObject("BattleInstaller");
        var bi = biGo.AddComponent<BattleInstaller>();
        SetPrivateField(bi, "backgroundRenderer", bgSr);
        SetPrivateField(bi, "caveBackground", caveBg);
        SetPrivateField(bi, "forestBackground", forestBg);
        SetPrivateField(bi, "battleSystem", battleSys);
        SetPrivateField(bi, "battleView", view);
        SetPrivateField(bi, "battleAudio", battleAud);
        SetPrivateField(bi, "battleFeel", feel);
        SetPrivateField(bi, "normalBattleMusic", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/BGM_Battle.wav"));
        SetPrivateField(bi, "bossBattleMusic", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/BGM_Boss.wav"));

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Pulau Monster] Scn_Battle built.");
    }

    [MenuItem("Pulau Monster/08 Build Title and End Scenes", priority = 8)]
    public static void BuildTitleAndEndScenes()
    {
        EnsureFolder("Assets/_Project/Scenes");
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/Fonts/PixelifySans-Regular.ttf");
        var pixelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Pixel.png");
        var panelNavy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Panel_Navy.png");

        // 1. Scn_Title
        string titlePath = "Assets/_Project/Scenes/Scn_Title.unity";
        Scene titleScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var tCamGo = new GameObject("Camera");
        var tCam = tCamGo.AddComponent<Camera>();
        tCam.orthographic = true;
        tCam.orthographicSize = 5.625f;

        var tCanvasGo = new GameObject("Canvas_Title");
        var tCanvas = tCanvasGo.AddComponent<Canvas>();
        tCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var tScaler = tCanvasGo.AddComponent<CanvasScaler>();
        tScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        tScaler.referenceResolution = new Vector2(1280, 720);
        tScaler.matchWidthOrHeight = 0.5f;

        var bgGo = new GameObject("BG_Title");
        bgGo.transform.SetParent(tCanvasGo.transform, false);
        var bgRect = bgGo.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(1920, 720);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/BG_Title_World.png");

        var overGo = new GameObject("Overlay");
        overGo.transform.SetParent(tCanvasGo.transform, false);
        var oRect = overGo.AddComponent<RectTransform>();
        oRect.anchorMin = Vector2.zero; oRect.anchorMax = Vector2.one; oRect.sizeDelta = Vector2.zero;
        var oImg = overGo.AddComponent<Image>();
        oImg.sprite = pixelSprite;
        oImg.color = new Color32(18, 16, 39, 140);

        var fontBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/Fonts/PixelifySans-Bold.ttf");
        var titleFont = (fontBold != null) ? fontBold : font;

        // Title Container (Centered horizontally at top)
        var titleGroupGo = new GameObject("TitleGroup");
        titleGroupGo.transform.SetParent(tCanvasGo.transform, false);
        var tgRect = titleGroupGo.AddComponent<RectTransform>();
        tgRect.anchorMin = new Vector2(0.5f, 1f);
        tgRect.anchorMax = new Vector2(0.5f, 1f);
        tgRect.pivot = new Vector2(0.5f, 1f);
        tgRect.anchoredPosition = new Vector2(0, -45);
        tgRect.sizeDelta = new Vector2(960, 160);

        // Subtitle badge
        CreateText(titleGroupGo.transform, "Title_Badge", 0, -10, 960, 32, font, 20, UiTheme.Gold, "★ PETUALANGAN 2D RPG MONSTER NUSANTARA ★", TextAnchor.MiddleCenter);

        // Grand Main Title: PULAU MONSTER (Full title, never truncated!)
        var tMain = CreateText(titleGroupGo.transform, "Title_Main", 0, -42, 960, 78, titleFont, 56, UiTheme.Gold, "PULAU MONSTER", TextAnchor.MiddleCenter);
        var tOutline = tMain.gameObject.AddComponent<Outline>();
        tOutline.effectColor = UiTheme.Ink;
        tOutline.effectDistance = new Vector2(3, -3);

        // Tagline subtitle
        CreateText(titleGroupGo.transform, "Title_Tagline", 0, -120, 960, 32, font, 22, UiTheme.Text, "— LEGENDA PENJELAJAH ELEMEN —", TextAnchor.MiddleCenter);

        var starterImages = new Image[4];
        var starterData = new MonsterData[4];
        string[] startNames = { "Pyrit", "Blubi", "Sprig", "Zapi" };
        for (int i = 0; i < 4; i++)
        {
            var sGo = new GameObject($"Starter_{startNames[i]}");
            sGo.transform.SetParent(tCanvasGo.transform, false);
            var sRect = sGo.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 0.5f);
            sRect.anchorMax = new Vector2(0.5f, 0.5f);
            sRect.anchoredPosition = new Vector2(-225 + i * 150, 40);
            sRect.sizeDelta = new Vector2(96, 96);
            var simg = sGo.AddComponent<Image>();
            var sData = AssetDatabase.LoadAssetAtPath<MonsterData>($"Assets/_Project/Data/Monsters/Mon_{startNames[i]}.asset");
            if (sData != null && sData.Frames != null && sData.Frames.Length > 0)
            {
                simg.sprite = sData.Frames[0];
            }
            starterImages[i] = simg;
            starterData[i] = sData;
        }

        var pnlMenu = CreatePanel(tCanvasGo.transform, "MenuPanel", 440, -440, 400, 200, panelNavy);
        var tMenu = pnlMenu.AddComponent<TextMenu>();
        var tTexts = new List<Text>();
        var tBtns = new List<Button>();
        for (int i = 0; i < 3; i++)
        {
            var btnGo = new GameObject($"Btn_{i}");
            btnGo.transform.SetParent(pnlMenu.transform, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -20 - i * 55);
            rect.sizeDelta = new Vector2(-40, 45);

            var t = CreateText(btnGo.transform, "Text", 0, 0, 360, 45, font, 28, UiTheme.Text, $"Option {i}", TextAnchor.MiddleCenter);
            var btn = btnGo.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            var nav = btn.navigation; nav.mode = Navigation.Mode.None; btn.navigation = nav;

            tTexts.Add(t);
            tBtns.Add(btn);
        }
        tMenu.SetupItems(tTexts, tBtns, 1);

        var hintGo = new GameObject("HintGroup");
        hintGo.transform.SetParent(tCanvasGo.transform, false);
        var hgRect = hintGo.AddComponent<RectTransform>();
        hgRect.anchorMin = new Vector2(0.5f, 0f);
        hgRect.anchorMax = new Vector2(0.5f, 0f);
        hgRect.pivot = new Vector2(0.5f, 0f);
        hgRect.anchoredPosition = new Vector2(0, 20);
        hgRect.sizeDelta = new Vector2(800, 40);
        CreateText(hintGo.transform, "Hint", 0, 0, 800, 40, font, 24, UiTheme.Muted, "Enter / Spasi untuk mulai · Klik juga bisa", TextAnchor.MiddleCenter);

        var tiGo = new GameObject("TitleInstaller");
        var ti = tiGo.AddComponent<TitleInstaller>();
        SetPrivateField(ti, "backgroundRect", bgRect);
        SetPrivateField(ti, "starterImages", starterImages);
        SetPrivateField(ti, "starterData", starterData);
        SetPrivateField(ti, "menu", tMenu);
        SetPrivateField(ti, "titleMusic", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/BGM_Title.wav"));

        EditorSceneManager.SaveScene(titleScene, titlePath);

        // 2. Scn_End
        string endPath = "Assets/_Project/Scenes/Scn_End.unity";
        Scene endScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var eCamGo = new GameObject("Camera");
        var eCam = eCamGo.AddComponent<Camera>();
        eCam.orthographic = true;
        eCam.orthographicSize = 5.625f;

        var eCanvasGo = new GameObject("Canvas_End");
        var eCanvas = eCanvasGo.AddComponent<Canvas>();
        eCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var eScaler = eCanvasGo.AddComponent<CanvasScaler>();
        eScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        eScaler.referenceResolution = new Vector2(1280, 720);
        eScaler.matchWidthOrHeight = 0.5f;

        var eBgGo = new GameObject("Background");
        eBgGo.transform.SetParent(eCanvasGo.transform, false);
        var ebRect = eBgGo.AddComponent<RectTransform>();
        ebRect.anchorMin = Vector2.zero; ebRect.anchorMax = Vector2.one; ebRect.sizeDelta = Vector2.zero;
        var ebImg = eBgGo.AddComponent<Image>();
        ebImg.sprite = pixelSprite;
        ebImg.color = UiTheme.Ink;

        var endTitleGroupGo = new GameObject("EndTitleGroup");
        endTitleGroupGo.transform.SetParent(eCanvasGo.transform, false);
        var etgRect = endTitleGroupGo.AddComponent<RectTransform>();
        etgRect.anchorMin = new Vector2(0.5f, 1f);
        etgRect.anchorMax = new Vector2(0.5f, 1f);
        etgRect.pivot = new Vector2(0.5f, 1f);
        etgRect.anchoredPosition = new Vector2(0, -60);
        etgRect.sizeDelta = new Vector2(800, 90);
        var endTitle = CreateText(endTitleGroupGo.transform, "Title", 0, 0, 800, 90, titleFont, 60, UiTheme.Gold, "DEMO SELESAI", TextAnchor.MiddleCenter);
        var etOutline = endTitle.gameObject.AddComponent<Outline>();
        etOutline.effectColor = UiTheme.Ink;
        etOutline.effectDistance = new Vector2(3, -3);

        var pImgGo = new GameObject("PartnerImage");
        pImgGo.transform.SetParent(eCanvasGo.transform, false);
        var pImgRect = pImgGo.AddComponent<RectTransform>();
        pImgRect.anchorMin = new Vector2(0.5f, 0.5f);
        pImgRect.anchorMax = new Vector2(0.5f, 0.5f);
        pImgRect.anchoredPosition = new Vector2(0, 40);
        pImgRect.sizeDelta = new Vector2(192, 192);
        var pImg = pImgGo.AddComponent<Image>();

        var endStatsGroupGo = new GameObject("EndStatsGroup");
        endStatsGroupGo.transform.SetParent(eCanvasGo.transform, false);
        var esgRect = endStatsGroupGo.AddComponent<RectTransform>();
        esgRect.anchorMin = new Vector2(0.5f, 0.5f);
        esgRect.anchorMax = new Vector2(0.5f, 0.5f);
        esgRect.pivot = new Vector2(0.5f, 0.5f);
        esgRect.anchoredPosition = new Vector2(0, -180);
        esgRect.sizeDelta = new Vector2(800, 150);

        var partnerTxt = CreateText(endStatsGroupGo.transform, "PartnerText", 0, 40, 800, 45, font, 30, UiTheme.Text, "Partner · Lv 10", TextAnchor.MiddleCenter);
        var timeTxt = CreateText(endStatsGroupGo.transform, "PlaytimeText", 0, -10, 800, 40, font, 26, UiTheme.Gold, "Waktu bermain 5 menit 12 detik", TextAnchor.MiddleCenter);
        CreateText(endStatsGroupGo.transform, "LeaveHint", 0, -65, 800, 36, font, 22, UiTheme.Muted, "Tekan Enter atau Klik untuk kembali ke judul", TextAnchor.MiddleCenter);

        var ccGo = new GameObject("ClickCatcher");
        ccGo.transform.SetParent(eCanvasGo.transform, false);
        var ccRect = ccGo.AddComponent<RectTransform>();
        ccRect.anchorMin = Vector2.zero; ccRect.anchorMax = Vector2.one; ccRect.sizeDelta = Vector2.zero;
        var ccImg = ccGo.AddComponent<Image>();
        ccImg.color = new Color(0, 0, 0, 0);
        var ccBtn = ccGo.AddComponent<Button>();

        var eiGo = new GameObject("EndInstaller");
        var ei = eiGo.AddComponent<EndInstaller>();
        SetPrivateField(ei, "partnerImage", pImg);
        SetPrivateField(ei, "partnerText", partnerTxt);
        SetPrivateField(ei, "timeText", timeTxt);
        SetPrivateField(ei, "clickCatcher", ccBtn);

        EditorSceneManager.SaveScene(endScene, endPath);
        Debug.Log("[Pulau Monster] Scn_Title and Scn_End built.");
    }

    [MenuItem("Pulau Monster/09 Build Bootstrap Scene and Lighting", priority = 9)]
    public static void BuildBootstrapSceneAndLighting()
    {
        EnsureFolder("Assets/_Project/Scenes");
        string scenePath = "Assets/_Project/Scenes/Scn_Bootstrap.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/Fonts/PixelifySans-Regular.ttf");
        var pixelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Pixel.png");
        var panelNavy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Panel_Navy.png");
        var panelGold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Panel_Gold.png");
        var panelNarr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Panel_Narr.png");

        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var audGo = new GameObject("AudioService");
        var audioService = audGo.AddComponent<AudioService>();

        var faderCanvasGo = new GameObject("Canvas_Fader");
        var fCanvas = faderCanvasGo.AddComponent<Canvas>();
        fCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fCanvas.sortingOrder = 100;
        var fScaler = faderCanvasGo.AddComponent<CanvasScaler>();
        fScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        fScaler.referenceResolution = new Vector2(1280, 720);

        var fGroup = faderCanvasGo.AddComponent<CanvasGroup>();
        var fImgGo = new GameObject("BlackImage");
        fImgGo.transform.SetParent(faderCanvasGo.transform, false);
        var fiRect = fImgGo.AddComponent<RectTransform>();
        fiRect.anchorMin = Vector2.zero; fiRect.anchorMax = Vector2.one; fiRect.sizeDelta = Vector2.zero;
        var fiImg = fImgGo.AddComponent<Image>();
        fiImg.sprite = pixelSprite;
        fiImg.color = Color.black;
        var fader = faderCanvasGo.AddComponent<ScreenFader>();
        SetPrivateField(fader, "canvasGroup", fGroup);
        SetPrivateField(fader, "blackImage", fiImg);

        var dlgCanvasGo = new GameObject("Canvas_Dialogue");
        var dCanvas = dlgCanvasGo.AddComponent<Canvas>();
        dCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dCanvas.sortingOrder = 20;
        var dScaler = dlgCanvasGo.AddComponent<CanvasScaler>();
        dScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        dScaler.referenceResolution = new Vector2(1280, 720);
        dlgCanvasGo.AddComponent<GraphicRaycaster>();

        var dlgRoot = new GameObject("DialogueRoot");
        dlgRoot.transform.SetParent(dlgCanvasGo.transform, false);
        var drRect = dlgRoot.AddComponent<RectTransform>();
        drRect.anchorMin = Vector2.zero; drRect.anchorMax = Vector2.one; drRect.sizeDelta = Vector2.zero;

        var dCatcherGo = new GameObject("ClickCatcher");
        dCatcherGo.transform.SetParent(dlgRoot.transform, false);
        var dcRect = dCatcherGo.AddComponent<RectTransform>();
        dcRect.anchorMin = Vector2.zero; dcRect.anchorMax = Vector2.one; dcRect.sizeDelta = Vector2.zero;
        var dcImg = dCatcherGo.AddComponent<Image>();
        dcImg.color = new Color(0, 0, 0, 0);
        var dcBtn = dCatcherGo.AddComponent<Button>();

        var boxGo = CreatePanel(dlgRoot.transform, "Box", 24, -512, 1232, 184, panelNavy);
        var boxImg = boxGo.GetComponent<Image>();
        var bodyTxt = CreateText(boxGo.transform, "BodyText", 24, -20, 1184, 140, font, 28, UiTheme.Text, "");

        var tagGo = CreatePanel(dlgRoot.transform, "SpeakerTag", 48, -468, 260, 48, panelGold);
        var spkTxt = CreateText(tagGo.transform, "SpeakerText", 16, -6, 228, 36, font, 24, UiTheme.Ink, "Tetua Rahmat");

        var hintTxt = CreateText(dlgRoot.transform, "ContinueHint", 1200, -640, 40, 40, font, 28, UiTheme.Gold, "▼", TextAnchor.MiddleCenter);

        var choiceGo = CreatePanel(dlgRoot.transform, "ChoiceMenu", 800, -320, 400, 160, panelNavy);
        var cMenu = choiceGo.AddComponent<TextMenu>();
        var cTexts = new List<Text>();
        var cBtns = new List<Button>();
        for (int i = 0; i < 4; i++)
        {
            var btnGo = new GameObject($"Btn_{i}");
            btnGo.transform.SetParent(choiceGo.transform, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -12 - i * 36);
            rect.sizeDelta = new Vector2(-24, 32);

            var t = CreateText(btnGo.transform, "Text", 0, 0, 350, 32, font, 24, UiTheme.Text, $"Choice {i}", TextAnchor.MiddleLeft);
            var btn = btnGo.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            var nav = btn.navigation; nav.mode = Navigation.Mode.None; btn.navigation = nav;

            cTexts.Add(t);
            cBtns.Add(btn);
        }
        cMenu.SetupItems(cTexts, cBtns, 1);
        choiceGo.SetActive(false);

        var dlgView = dlgCanvasGo.AddComponent<DialogueView>();
        SetPrivateField(dlgView, "canvas", dCanvas);
        SetPrivateField(dlgView, "root", dlgRoot);
        SetPrivateField(dlgView, "boxImage", boxImg);
        SetPrivateField(dlgView, "speakerTagRoot", tagGo);
        SetPrivateField(dlgView, "speakerText", spkTxt);
        SetPrivateField(dlgView, "bodyText", bodyTxt);
        SetPrivateField(dlgView, "continueHint", hintTxt);
        SetPrivateField(dlgView, "choiceMenu", cMenu);
        SetPrivateField(dlgView, "clickCatcher", dcBtn);
        SetPrivateField(dlgView, "navyPanel", panelNavy);
        SetPrivateField(dlgView, "narrPanel", panelNarr);

        dlgRoot.SetActive(false);

        var runnerGo = new GameObject("DialogueRunner");
        var dlgRunner = runnerGo.AddComponent<DialogueRunner>();
        SetPrivateField(dlgRunner, "blipClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Dialog_Blip.wav"));

        var toastCanvasGo = new GameObject("Canvas_Toast");
        var tstCanvas = toastCanvasGo.AddComponent<Canvas>();
        tstCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tstCanvas.sortingOrder = 30;
        var tstScaler = toastCanvasGo.AddComponent<CanvasScaler>();
        tstScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        tstScaler.referenceResolution = new Vector2(1280, 720);

        var tstPanel = CreatePanel(toastCanvasGo.transform, "ToastPanel", 390, -30, 500, 60, panelGold);
        var tstTxt = CreateText(tstPanel.transform, "Text", 0, 0, 500, 60, font, 24, UiTheme.Ink, "Notifikasi", TextAnchor.MiddleCenter);
        var tstGroup = tstPanel.AddComponent<CanvasGroup>();
        var toastView = tstPanel.AddComponent<ToastView>();
        SetPrivateField(toastView, "group", tstGroup);
        SetPrivateField(toastView, "messageText", tstTxt);
        tstPanel.SetActive(false);

        var pauseCanvasGo = new GameObject("Canvas_Pause");
        var pCanvas = pauseCanvasGo.AddComponent<Canvas>();
        pCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pCanvas.sortingOrder = 40;
        var pScaler = pauseCanvasGo.AddComponent<CanvasScaler>();
        pScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        pScaler.referenceResolution = new Vector2(1280, 720);
        pauseCanvasGo.AddComponent<GraphicRaycaster>();

        var pRoot = new GameObject("PauseRoot");
        pRoot.transform.SetParent(pauseCanvasGo.transform, false);
        var prRect = pRoot.AddComponent<RectTransform>();
        prRect.anchorMin = Vector2.zero; prRect.anchorMax = Vector2.one; prRect.sizeDelta = Vector2.zero;

        var pDim = new GameObject("Dim");
        pDim.transform.SetParent(pRoot.transform, false);
        var pdRect = pDim.AddComponent<RectTransform>();
        pdRect.anchorMin = Vector2.zero; pdRect.anchorMax = Vector2.one; pdRect.sizeDelta = Vector2.zero;
        var pdImg = pDim.AddComponent<Image>();
        pdImg.sprite = pixelSprite;
        pdImg.color = new Color32(18, 16, 39, 150);

        var pausePnl = CreatePanel(pRoot.transform, "Panel", 416, -112, 448, 248, panelNavy);
        var pMenu = pausePnl.AddComponent<TextMenu>();
        var pTexts = new List<Text>();
        var pBtns = new List<Button>();
        for (int i = 0; i < 3; i++)
        {
            var btnGo = new GameObject($"Btn_{i}");
            btnGo.transform.SetParent(pausePnl.transform, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -25 - i * 65);
            rect.sizeDelta = new Vector2(-40, 50);

            var t = CreateText(btnGo.transform, "Text", 0, 0, 408, 50, font, 26, UiTheme.Text, $"Option {i}", TextAnchor.MiddleCenter);
            var btn = btnGo.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            var nav = btn.navigation; nav.mode = Navigation.Mode.None; btn.navigation = nav;

            pTexts.Add(t);
            pBtns.Add(btn);
        }
        pMenu.SetupItems(pTexts, pBtns, 1);
        pRoot.SetActive(false);

        var pauseMenuComp = pauseCanvasGo.AddComponent<PauseMenu>();
        SetPrivateField(pauseMenuComp, "root", pRoot);
        SetPrivateField(pauseMenuComp, "menu", pMenu);

        var bootstrapGo = new GameObject("GameBootstrap");
        var gb = bootstrapGo.AddComponent<GameBootstrap>();
        SetPrivateField(gb, "firstScene", SceneFlow.TitleScene);
        SetPrivateField(gb, "monsterDatabase", AssetDatabase.LoadAssetAtPath<MonsterDatabase>("Assets/_Project/Data/MonsterDatabase.asset"));
        SetPrivateField(gb, "fader", fader);
        SetPrivateField(gb, "dialogueView", dlgView);
        SetPrivateField(gb, "dialogueRunner", dlgRunner);
        SetPrivateField(gb, "toastView", toastView);
        SetPrivateField(gb, "pauseMenu", pauseMenuComp);
        SetPrivateField(gb, "audioService", audioService);

        EditorSceneManager.SaveScene(scene, scenePath);

        SetupWorldLighting();

        Debug.Log("[Pulau Monster] Scn_Bootstrap built and World lighting configured.");
    }

    [MenuItem("Pulau Monster/10 Apply Build Settings", priority = 10)]
    public static void ApplyBuildSettings()
    {
        var buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Scn_Bootstrap.unity", true),
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Scn_Title.unity", true),
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Scn_World.unity", true),
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Scn_Battle.unity", true),
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Scn_End.unity", true),
        };
        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("[Pulau Monster] Build Settings configured with 5 scenes.");
    }

    [MenuItem("Pulau Monster/Z Build Windows Executable", priority = 99)]
    public static void BuildWindowsExecutable()
    {
        ApplyBuildSettings();

        string buildDir = "Builds/PulauMonster_Win";
        Directory.CreateDirectory(buildDir);
        string exePath = Path.Combine(buildDir, "PulauMonster.exe");

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new string[]
            {
                "Assets/_Project/Scenes/Scn_Bootstrap.unity",
                "Assets/_Project/Scenes/Scn_Title.unity",
                "Assets/_Project/Scenes/Scn_World.unity",
                "Assets/_Project/Scenes/Scn_Battle.unity",
                "Assets/_Project/Scenes/Scn_End.unity"
            },
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[Pulau Monster] Build succeeded! Standalone executable at: {exePath}");
        }
        else
        {
            Debug.LogError($"[Pulau Monster] Build failed with result: {report.summary.result}");
        }
    }

    private static void SetupWorldLighting()
    {
        string worldPath = "Assets/_Project/Scenes/Scn_World.unity";
        Scene worldScene = EditorSceneManager.OpenScene(worldPath, OpenSceneMode.Single);

        var globalLightGo = new GameObject("GlobalLight2D");
        var gLight = globalLightGo.AddComponent<Light2D>();
        gLight.lightType = Light2D.LightType.Global;
        gLight.intensity = 1.0f;

        var playerObj = GameObject.FindWithTag("Player");
        Light2D pLight = null;
        if (playerObj != null)
        {
            var pLightGo = new GameObject("PlayerLight");
            pLightGo.transform.SetParent(playerObj.transform, false);
            pLightGo.transform.localPosition = Vector3.zero;
            pLight = pLightGo.AddComponent<Light2D>();
            pLight.lightType = Light2D.LightType.Point;
            pLight.pointLightInnerRadius = 1.2f;
            pLight.pointLightOuterRadius = 3.6f;
            pLight.color = new Color32(255, 245, 220, 255);
            pLight.intensity = 0f;
        }

        Vector2[] crystalPos = {
            new Vector2(24f, 27f), new Vector2(36f, 28f), new Vector2(26f, 32f),
            new Vector2(35f, 32f), new Vector2(33f, 27f), new Vector2(28f, 30f),
            new Vector2(31f, 33f)
        };
        var caveLightsGo = new GameObject("CaveLights");
        var extraLights = new List<Light2D>();

        for (int i = 0; i < crystalPos.Length; i++)
        {
            var cLightGo = new GameObject($"Crystal_{crystalPos[i].x}_{crystalPos[i].y}");
            cLightGo.transform.SetParent(caveLightsGo.transform, false);
            cLightGo.transform.position = new Vector3(crystalPos[i].x + 0.5f, crystalPos[i].y + 0.5f, 0f);
            var cl = cLightGo.AddComponent<Light2D>();
            cl.lightType = Light2D.LightType.Point;
            cl.pointLightInnerRadius = 0.6f;
            cl.pointLightOuterRadius = 3.6f;
            cl.color = new Color32(45, 225, 255, 255); // Vivid Electric Cyan
            cl.intensity = 0f;
            cLightGo.AddComponent<CrystalSparkEffect>();
            extraLights.Add(cl);
        }

        var bossObj = GameObject.Find("Boss_Raijugar");
        if (bossObj != null)
        {
            var blGo = new GameObject("BossLight");
            blGo.transform.SetParent(bossObj.transform, false);
            blGo.transform.localPosition = new Vector3(0, 0.9f, 0);
            var bl = blGo.AddComponent<Light2D>();
            bl.lightType = Light2D.LightType.Point;
            bl.pointLightInnerRadius = 0.5f;
            bl.pointLightOuterRadius = 2.8f;
            bl.color = new Color32(70, 220, 255, 255); // Electric Blue/Cyan
            bl.intensity = 0f;
            extraLights.Add(bl);
        }

        var areaGua = GameObject.Find("Area_Gua Petir");
        if (areaGua != null)
        {
            var lz = areaGua.AddComponent<LightZone>();
            lz.Setup(gLight, pLight, extraLights.ToArray());
        }

        EditorSceneManager.SaveScene(worldScene, worldPath);
    }

    private static void PaintWorldTilemaps(Tilemap tmGround, Tilemap tmDecor, Tilemap tmBlocking, Tilemap tmTallGrass)
    {
        Sprite[] tiles = LoadAllSpriteSubAssets("Assets/_Project/Art/Tiles/Tileset_Overworld.png");
        if (tiles == null || tiles.Length < 72) return;

        var tileMap = new Dictionary<int, Tile>();
        for (int i = 0; i < tiles.Length; i++)
        {
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = tiles[i];
            tileMap[i] = t;
        }

        var rtPath = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/_Project/Art/Tiles/RuleTiles/RT_Path.asset");
        var rtWater = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/_Project/Art/Tiles/RuleTiles/RT_Water.asset");

        TileBase grassTile = tileMap[10];
        TileBase grassVar1 = tileMap[11];
        TileBase grassVar2 = tileMap[66];

        TileBase caveFloorTile = tileMap[44];
        TileBase caveRubbleTile = tileMap[68];
        TileBase caveRockStalagmite = tileMap[45];
        TileBase caveWallTop = tileMap[47];
        TileBase caveWallBot = tileMap[48];
        TileBase caveWallLeft = tileMap[49];
        TileBase caveWallRight = tileMap[50];

        TileBase crystalTile = tileMap[46];
        TileBase crystalSpireTile = tileMap[67];

        TileBase fenceTile = tileMap[38];
        TileBase tallGrassA = tileMap[34];
        TileBase treeTop = tileMap[36];
        TileBase treeBot = tileMap[37];
        TileBase bushTile = tileMap[33];
        TileBase flowerRed = tileMap[40];
        TileBase flowerYellow = tileMap[41];
        TileBase signTile = tileMap[32];

        // 1. Paint Ground Layer
        for (int y = 0; y < 36; y++)
        {
            for (int x = 0; x < 40; x++)
            {
                if (x >= 21 && x <= 38 && y >= 25 && y <= 34)
                {
                    // Gua Petir: 100% subterranean bedrock, zero grass!
                    tmGround.SetTile(new Vector3Int(x, y, 0), caveFloorTile);
                }
                else
                {
                    // Overworld / Forest: Clean, seamless green meadow with subtle natural variations
                    if ((x * 3 + y * 7) % 23 == 0)
                    {
                        tmGround.SetTile(new Vector3Int(x, y, 0), grassVar1);
                    }
                    else if ((x * 5 + y * 11) % 29 == 0)
                    {
                        tmGround.SetTile(new Vector3Int(x, y, 0), grassVar2);
                    }
                    else
                    {
                        tmGround.SetTile(new Vector3Int(x, y, 0), grassTile);
                    }
                }
            }
        }

        // 2. Water Bodies (Blocking)
        for (int y = 25; y <= 34; y++)
        {
            for (int x = 2; x <= 17; x++)
            {
                tmBlocking.SetTile(new Vector3Int(x, y, 0), rtWater != null ? rtWater : tileMap[18]);
            }
        }

        for (int y = 2; y <= 5; y++)
        {
            for (int x = 28; x <= 35; x++)
            {
                tmBlocking.SetTile(new Vector3Int(x, y, 0), rtWater != null ? rtWater : tileMap[18]);
            }
        }

        // 3. Roads & Paths (Ground)
        for (int y = 6; y <= 23; y++)
        {
            for (int x = 19; x <= 20; x++)
            {
                tmGround.SetTile(new Vector3Int(x, y, 0), rtPath != null ? rtPath : tileMap[13]);
            }
        }

        for (int y = 4; y <= 5; y++)
        {
            for (int x = 3; x <= 36; x++)
            {
                tmGround.SetTile(new Vector3Int(x, y, 0), rtPath != null ? rtPath : tileMap[13]);
            }
        }

        // 4. Wooden Fences (Blocking)
        for (int x = 1; x <= 38; x++)
        {
            if (x != 19 && x != 20)
            {
                tmBlocking.SetTile(new Vector3Int(x, 11, 0), fenceTile);
            }
        }

        // 5. Tall Encounter Grass
        for (int y = 14; y <= 17; y++)
        {
            for (int x = 5; x <= 9; x++)
            {
                tmTallGrass.SetTile(new Vector3Int(x, y, 0), tallGrassA);
            }
            for (int x = 26; x <= 31; x++)
            {
                tmTallGrass.SetTile(new Vector3Int(x, y, 0), tallGrassA);
            }
        }

        // 6. Forest Trees (tmDecor canopy, tmBlocking trunk)
        Vector2Int[] forestTrees = {
            new Vector2Int(16, 16),
            new Vector2Int(12, 19),
            new Vector2Int(24, 20),
            new Vector2Int(8, 20),
            new Vector2Int(32, 19)
        };
        foreach (var tpos in forestTrees)
        {
            tmDecor.SetTile(new Vector3Int(tpos.x, tpos.y + 1, 0), treeTop);
            tmBlocking.SetTile(new Vector3Int(tpos.x, tpos.y, 0), treeBot);
        }

        // 7. Outer Boundary Trees
        for (int x = 0; x < 40; x++)
        {
            tmBlocking.SetTile(new Vector3Int(x, 0, 0), treeBot);
            if (x < 21 || x > 38)
            {
                tmBlocking.SetTile(new Vector3Int(x, 35, 0), treeTop);
                tmBlocking.SetTile(new Vector3Int(x, 34, 0), treeBot);
            }
        }
        for (int y = 1; y < 34; y += 2)
        {
            tmBlocking.SetTile(new Vector3Int(0, y + 1, 0), treeTop);
            tmBlocking.SetTile(new Vector3Int(0, y, 0), treeBot);
            tmBlocking.SetTile(new Vector3Int(39, y + 1, 0), treeTop);
            tmBlocking.SetTile(new Vector3Int(39, y, 0), treeBot);
        }

        // 8. Village Houses
        int[] houseX = { 5, 11, 24 };
        foreach (int hx in houseX)
        {
            tmBlocking.SetTile(new Vector3Int(hx, 8, 0), tileMap[56]);
            tmBlocking.SetTile(new Vector3Int(hx + 1, 8, 0), tileMap[57]);
            tmBlocking.SetTile(new Vector3Int(hx + 2, 8, 0), tileMap[58]);

            tmBlocking.SetTile(new Vector3Int(hx, 7, 0), tileMap[59]);
            tmBlocking.SetTile(new Vector3Int(hx + 1, 7, 0), tileMap[60]);
            tmBlocking.SetTile(new Vector3Int(hx + 2, 7, 0), tileMap[61]);

            tmBlocking.SetTile(new Vector3Int(hx, 6, 0), tileMap[62]);
            tmBlocking.SetTile(new Vector3Int(hx + 1, 6, 0), tileMap[63]);
            tmBlocking.SetTile(new Vector3Int(hx + 2, 6, 0), tileMap[64]);
        }

        // 9. Gua Petir Walls (Cliff formations)
        for (int x = 21; x <= 38; x++)
        {
            tmBlocking.SetTile(new Vector3Int(x, 34, 0), caveWallTop);
            if (x != 29 && x != 30)
            {
                tmBlocking.SetTile(new Vector3Int(x, 25, 0), caveWallBot);
            }
        }
        for (int y = 25; y <= 34; y++)
        {
            tmBlocking.SetTile(new Vector3Int(21, y, 0), caveWallLeft);
            tmBlocking.SetTile(new Vector3Int(38, y, 0), caveWallRight);
        }

        // 10. Gua Petir Interior Stalagmites / Craggy Formations (Blocking)
        Vector2Int[] stalagmites = {
            new Vector2Int(25, 29), new Vector2Int(34, 29),
            new Vector2Int(27, 33), new Vector2Int(33, 33)
        };
        foreach (var st in stalagmites)
        {
            tmBlocking.SetTile(new Vector3Int(st.x, st.y, 0), caveRockStalagmite);
        }

        // 11. Gua Petir Electric Volt Crystals (Decor)
        Vector2Int[] crystalClusters = {
            new Vector2Int(24, 27), new Vector2Int(36, 28), new Vector2Int(26, 32),
            new Vector2Int(35, 32), new Vector2Int(33, 27)
        };
        foreach (var cs in crystalClusters)
        {
            tmDecor.SetTile(new Vector3Int(cs.x, cs.y, 0), crystalTile);
        }

        Vector2Int[] crystalSpires = {
            new Vector2Int(28, 30), new Vector2Int(31, 33)
        };
        foreach (var sp in crystalSpires)
        {
            tmDecor.SetTile(new Vector3Int(sp.x, sp.y, 0), crystalSpireTile);
        }

        // 12. Gua Petir Rubble & Broken Slate (Decor)
        Vector2Int[] caveRubbles = {
            new Vector2Int(23, 26), new Vector2Int(32, 28), new Vector2Int(37, 31),
            new Vector2Int(27, 27), new Vector2Int(35, 30)
        };
        foreach (var cr in caveRubbles)
        {
            tmDecor.SetTile(new Vector3Int(cr.x, cr.y, 0), caveRubbleTile);
        }

        // 13. Forest & Village Decor (Wildflowers, Bushes, Signs)
        Vector2Int[] redFlowers = {
            new Vector2Int(15, 8), new Vector2Int(17, 10), new Vector2Int(23, 14), new Vector2Int(10, 18)
        };
        foreach (var rf in redFlowers)
        {
            tmDecor.SetTile(new Vector3Int(rf.x, rf.y, 0), flowerRed);
        }

        Vector2Int[] yellowFlowers = {
            new Vector2Int(18, 9), new Vector2Int(22, 12), new Vector2Int(14, 15), new Vector2Int(25, 17)
        };
        foreach (var yf in yellowFlowers)
        {
            tmDecor.SetTile(new Vector3Int(yf.x, yf.y, 0), flowerYellow);
        }

        Vector2Int[] bushes = {
            new Vector2Int(3, 12), new Vector2Int(36, 12), new Vector2Int(23, 7), new Vector2Int(15, 22)
        };
        foreach (var b in bushes)
        {
            tmDecor.SetTile(new Vector3Int(b.x, b.y, 0), bushTile);
        }

        tmDecor.SetTile(new Vector3Int(21, 6, 0), signTile);
    }

    private static void CreateNpcs(PixelBurst burst)
    {
        var elderGo = CreateNpcObject("Pfb_Npc_Elder", "Tetua Rahmat", new Vector2(25.5f, 7f), "NPC_Elder_16x16.png");
        var sg = elderGo.AddComponent<StarterGiver>();
        var pyrit = AssetDatabase.LoadAssetAtPath<MonsterData>("Assets/_Project/Data/Monsters/Mon_Pyrit.asset");
        var blubi = AssetDatabase.LoadAssetAtPath<MonsterData>("Assets/_Project/Data/Monsters/Mon_Blubi.asset");
        var sprig = AssetDatabase.LoadAssetAtPath<MonsterData>("Assets/_Project/Data/Monsters/Mon_Sprig.asset");
        SetPrivateField(sg, "starters", new MonsterData[] { pyrit, blubi, sprig });
        SetPrivateField(sg, "starterLevel", 3);

        var sariGo = CreateNpcObject("Pfb_Npc_Girl", "Sari", new Vector2(7f, 7f), "NPC_Girl_16x16.png");
        sariGo.AddComponent<Npc>();
        var sariDlg = ScriptableObject.CreateInstance<DialogueData>();
        SetPrivateField(sariDlg, "speaker", "Sari");
        SetPrivateField(sariDlg, "lines", new string[] {
            "Halo petualang! Ingat tips elemen ini ya:",
            "Api membakar Tumbuhan, Air memadamkan Api, Tumbuhan menyerap Air, dan Listrik menyengat Air!",
            "Semoga perjalananmu menyenangkan!"
        });
        EnsureFolder("Assets/_Project/Data/Dialogue");
        AssetDatabase.CreateAsset(sariDlg, "Assets/_Project/Data/Dialogue/Dlg_Sari.asset");
        SetPrivateField(sariGo.GetComponent<Npc>(), "dialogue", sariDlg);

        var guardGo = CreateNpcObject("Pfb_Npc_Guard", "Penjaga Batas", new Vector2(20f, 11.4f), "NPC_Guard_16x16.png");
        var gNpc = guardGo.AddComponent<GuardNpc>();
        SetPrivateField(gNpc, "asidePosition", new Vector2(22.5f, 10.4f));

        var merchantGo = CreateNpcObject("Pfb_Npc_Merchant", "Pak Dagang", new Vector2(13f, 4f), "NPC_Merchant_16x16.png");
        var mNpc = merchantGo.AddComponent<MerchantNpc>();
        SetPrivateField(mNpc, "healClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Heal.wav"));

        var stoneGo = new GameObject("SaveStone");
        stoneGo.layer = LayerMask.NameToLayer("Interactable");
        stoneGo.transform.position = new Vector3(23.5f, 7f, 0f);
        var sCol = stoneGo.AddComponent<BoxCollider2D>();
        sCol.size = new Vector2(0.8f, 0.8f);
        var stoneComp = stoneGo.AddComponent<SaveStone>();
        SetPrivateField(stoneComp, "saveClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX_Save.wav"));
        SetPrivateField(stoneComp, "burst", burst);
    }

    private static GameObject CreateNpcObject(string prefabName, string name, Vector2 pos, string sheetFile)
    {
        var go = new GameObject(prefabName);
        go.layer = LayerMask.NameToLayer("Interactable");
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        string sheetPath = $"Assets/_Project/Art/Characters/{sheetFile}";
        Sprite fDown = LoadSpriteSubAsset(sheetPath, $"{Path.GetFileNameWithoutExtension(sheetFile)}_0");
        Sprite fLeft = LoadSpriteSubAsset(sheetPath, $"{Path.GetFileNameWithoutExtension(sheetFile)}_3");
        Sprite fRight = LoadSpriteSubAsset(sheetPath, $"{Path.GetFileNameWithoutExtension(sheetFile)}_6");
        Sprite fUp = LoadSpriteSubAsset(sheetPath, $"{Path.GetFileNameWithoutExtension(sheetFile)}_9");

        sr.sprite = fDown;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.5f);
        col.offset = new Vector2(0f, 0.25f);

        SetPrivateField(go, "npcName", name);
        SetPrivateField(go, "body", sr);
        SetPrivateField(go, "faceDown", fDown);
        SetPrivateField(go, "faceLeft", fLeft);
        SetPrivateField(go, "faceRight", fRight);
        SetPrivateField(go, "faceUp", fUp);

        return go;
    }

    private static void CreateWildMonsters()
    {
        var alertSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Alert.png");

        (string name, Vector2 pos)[] westMons = {
            ("Sprig", new Vector2(6.5f, 15.5f)),
            ("Zapi", new Vector2(8.5f, 16.5f)),
            ("Blubi", new Vector2(7f, 14.5f)),
            ("Pyrit", new Vector2(9f, 15.2f))
        };
        foreach (var m in westMons)
        {
            CreateWildMonsterGo(m.name, m.pos, 2, 4, false, false, alertSprite);
        }

        (string name, Vector2 pos)[] eastMons = {
            ("Pyrago", new Vector2(27.5f, 15.5f)),
            ("Finnox", new Vector2(29.5f, 16.5f)),
            ("Voltcat", new Vector2(28f, 14.5f)),
            ("Bloomlet", new Vector2(30.5f, 15.2f))
        };
        foreach (var m in eastMons)
        {
            CreateWildMonsterGo(m.name, m.pos, 5, 7, false, false, alertSprite);
        }

        CreateWildMonsterGo("Raijugar", new Vector2(30.5f, 31.5f), 9, 9, true, true, alertSprite);
    }

    private static GameObject CreateWildMonsterGo(string speciesName, Vector2 pos, int minLvl, int maxLvl,
                                                  bool stationary, bool isBoss, Sprite alertSprite)
    {
        var go = new GameObject(isBoss ? "Boss_Raijugar" : $"Wild_{speciesName}");
        go.layer = LayerMask.NameToLayer("Wild");
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";

        var species = AssetDatabase.LoadAssetAtPath<MonsterData>($"Assets/_Project/Data/Monsters/Mon_{speciesName}.asset");
        if (species != null && species.Frames != null && species.Frames.Length > 0)
        {
            sr.sprite = species.Frames[0];
        }

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.6f);
        col.offset = new Vector2(0f, 0.3f);

        var anim = go.AddComponent<Animator>();
        string aocPath = $"Assets/_Project/Art/Monsters/Animations/AOC_Mon_{speciesName}.overrideController";
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(aocPath);

        var alertGo = new GameObject("Alert");
        alertGo.transform.SetParent(go.transform, false);
        alertGo.transform.localPosition = new Vector3(0f, isBoss ? 2.2f : 1.3f, 0f);
        var aSr = alertGo.AddComponent<SpriteRenderer>();
        aSr.sortingLayerName = "Characters";
        aSr.sortingOrder = 10;
        aSr.sprite = alertSprite;
        alertGo.SetActive(false);

        var wild = go.AddComponent<WildMonster>();
        SetPrivateField(wild, "species", species);
        SetPrivateField(wild, "minLevel", minLvl);
        SetPrivateField(wild, "maxLevel", maxLvl);
        SetPrivateField(wild, "stationary", stationary);
        SetPrivateField(wild, "isBoss", isBoss);
        SetPrivateField(wild, "body", sr);
        SetPrivateField(wild, "rb", rb);
        SetPrivateField(wild, "alert", alertGo);

        return go;
    }

    private static PartnerHud BuildWorldHud()
    {
        var canvasGo = new GameObject("Canvas_World");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/Fonts/PixelifySans-Regular.ttf");
        var panelNavy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Panel_Navy.png");
        var pixelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Pixel.png");

        var pnlPartner = CreatePanel(canvasGo.transform, "PartnerPanel", 16, -16, 480, 144, panelNavy);

        var portGo = new GameObject("Portrait");
        portGo.transform.SetParent(pnlPartner.transform, false);
        var portRect = portGo.AddComponent<RectTransform>();
        portRect.anchorMin = new Vector2(0, 1); portRect.anchorMax = new Vector2(0, 1);
        portRect.pivot = new Vector2(0, 1);
        portRect.anchoredPosition = new Vector2(18, -20);
        portRect.sizeDelta = new Vector2(104, 104);
        var portImg = portGo.AddComponent<Image>();

        var nameTxt = CreateText(pnlPartner.transform, "Name", 138, -16, 200, 32, font, 24, UiTheme.Gold, "Pyrit", TextAnchor.MiddleLeft);
        var lvlTxt = CreateText(pnlPartner.transform, "Level", 340, -16, 122, 32, font, 22, UiTheme.Gold, "Lv. 05", TextAnchor.MiddleRight);
        var hpTxt = CreateText(pnlPartner.transform, "HpText", 240, -50, 222, 24, font, 20, UiTheme.Text, "HP 80 / 100", TextAnchor.MiddleRight);
        var hpBar = CreateBar(pnlPartner.transform, "HpBar", 138, -76, 324, 20, pixelSprite, UiTheme.Ink, HpColors.High);
        var expBar = CreateBar(pnlPartner.transform, "ExpBar", 138, -104, 324, 12, pixelSprite, UiTheme.Ink, UiTheme.Exp);

        var pnlArea = CreatePanel(canvasGo.transform, "AreaPanel", 904, -16, 360, 96, panelNavy);
        var areaTxt = CreateText(pnlArea.transform, "AreaName", 0, -12, 360, 36, font, 28, UiTheme.Gold, "Desa Awal", TextAnchor.MiddleCenter);
        var potTxt = CreateText(pnlArea.transform, "PotionsText", 0, -52, 360, 32, font, 24, UiTheme.Text, "Ramuan ×3", TextAnchor.MiddleCenter);

        var hud = canvasGo.AddComponent<PartnerHud>();
        SetPrivateField(hud, "partnerPanel", pnlPartner);
        SetPrivateField(hud, "portraitImage", portImg);
        SetPrivateField(hud, "nameText", nameTxt);
        SetPrivateField(hud, "levelText", lvlTxt);
        SetPrivateField(hud, "hpFill", hpBar);
        SetPrivateField(hud, "hpText", hpTxt);
        SetPrivateField(hud, "expFill", expBar);
        SetPrivateField(hud, "areaPanel", pnlArea);
        SetPrivateField(hud, "areaNameText", areaTxt);
        SetPrivateField(hud, "potionsText", potTxt);

        return hud;
    }

    private static void CreateAreaZone(string name, Vector2 center, Vector2 size, string musicFile)
    {
        var go = new GameObject($"Area_{name}");
        go.transform.position = new Vector3(center.x, center.y, 0f);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = size;

        var az = go.AddComponent<AreaZone>();
        SetPrivateField(az, "areaName", name);
        if (!string.IsNullOrEmpty(musicFile))
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/_Project/Audio/{musicFile}");
            SetPrivateField(az, "areaMusic", clip);
        }
    }

    private static Tilemap CreateTilemapLayer(GameObject parent, string name, string sortingLayer, int order,
                                              bool hasBlockingCollider, bool isTrigger = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tm = go.AddComponent<Tilemap>();
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingLayerName = sortingLayer;
        tr.sortingOrder = order;

        if (hasBlockingCollider)
        {
            var tc = go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            var cc = go.AddComponent<CompositeCollider2D>();
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        }
        else if (isTrigger)
        {
            var tc = go.AddComponent<TilemapCollider2D>();
            tc.isTrigger = true;
        }

        return tm;
    }

    private static void CreateWall(string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        var square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/UI_Pixel.png");
        sr.sprite = square;
        go.transform.localScale = new Vector3(size.x * 4, size.y * 4, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
    }

    private static GameObject CreatePanel(Transform parent, string name, float x, float y, float w, float h, Sprite bgSprite)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);

        var img = go.AddComponent<Image>();
        img.sprite = bgSprite;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1.5625f;
        return go;
    }

    private static Text CreateText(Transform parent, string name, float x, float y, float w, float h,
                                  Font font, int fontSize, Color color, string initialText,
                                  TextAnchor alignment = TextAnchor.UpperLeft,
                                  HorizontalWrapMode wrapMode = HorizontalWrapMode.Wrap)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);

        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.text = initialText;
        text.alignment = alignment;
        text.horizontalOverflow = wrapMode;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = UiTheme.Ink;
        shadow.effectDistance = new Vector2(2, -2);

        return text;
    }

    private static Image CreateBar(Transform parent, string name, float x, float y, float w, float h,
                                  Sprite fillSprite, Color bgColor, Color fillColor)
    {
        var bgGo = new GameObject(name);
        bgGo.transform.SetParent(parent, false);
        var bgRect = bgGo.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 1); bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.anchoredPosition = new Vector2(x, y);
        bgRect.sizeDelta = new Vector2(w, h);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = fillSprite;
        bgImg.color = bgColor;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(bgGo.transform, false);
        var fRect = fillGo.AddComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero; fRect.anchorMax = Vector2.one;
        fRect.offsetMin = new Vector2(2, 2); fRect.offsetMax = new Vector2(-2, -2);
        var fImg = fillGo.AddComponent<Image>();
        fImg.sprite = fillSprite;
        fImg.type = Image.Type.Filled;
        fImg.fillMethod = Image.FillMethod.Horizontal;
        fImg.fillOrigin = 0;
        fImg.fillAmount = 1f;
        fImg.color = fillColor;

        return fImg;
    }

    private static AnimationClip CreateOrGetClip(string clipPath, string sheetPath, int[] frameIndices, float fps, bool loop)
    {
        var clip = new AnimationClip { frameRate = fps };
        if (loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        var keyframes = new ObjectReferenceKeyframe[frameIndices.Length];
        string baseName = Path.GetFileNameWithoutExtension(sheetPath);
        for (int i = 0; i < frameIndices.Length; i++)
        {
            Sprite s = LoadSpriteSubAsset(sheetPath, $"{baseName}_{frameIndices[i]}");
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = s
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private static AnimationClip CreateOrGetClipFromSprites(string clipPath, Sprite[] sprites, float fps, bool loop)
    {
        var clip = new AnimationClip { frameRate = fps };
        if (loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private static void BuildRuleTile(string tilePath, string textureName, int[] indices)
    {
        string sheetPath = "Assets/_Project/Art/Tiles/Tileset_Overworld.png";
        var ruleTile = ScriptableObject.CreateInstance<RuleTile>();
        ruleTile.m_TilingRules = new List<RuleTile.TilingRule>();

        ruleTile.m_DefaultSprite = LoadSpriteSubAsset(sheetPath, $"{textureName}_{indices[4]}");

        int[,] neighborMasks = {
            { 0,  2, 0,  2, 0,  1,  0, 1, 0 }, // TL
            { 0,  2, 0,  1, 0,  1,  0, 1, 0 }, // T
            { 0,  2, 0,  1, 0,  2,  0, 1, 0 }, // TR
            { 0,  1, 0,  2, 0,  1,  0, 1, 0 }, // L
            { 0,  1, 0,  1, 0,  2,  0, 1, 0 }, // R
            { 0,  1, 0,  2, 0,  1,  0, 2, 0 }, // BL
            { 0,  1, 0,  1, 0,  1,  0, 2, 0 }, // B
            { 0,  1, 0,  1, 0,  2,  0, 2, 0 }, // BR
        };

        for (int i = 0; i < 8; i++)
        {
            int idx = (i >= 4) ? i + 1 : i;
            var rule = new RuleTile.TilingRule
            {
                m_Sprites = new Sprite[] { LoadSpriteSubAsset(sheetPath, $"{textureName}_{indices[idx]}") }
            };
            ruleTile.m_TilingRules.Add(rule);
        }

        AssetDatabase.CreateAsset(ruleTile, tilePath);
    }

    private static Sprite LoadSpriteSubAsset(string texturePath, string spriteName)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        foreach (var obj in assets)
        {
            if (obj is Sprite s && s.name == spriteName)
                return s;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
    }

    private static Sprite[] LoadAllSpriteSubAssets(string texturePath)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        var list = new List<Sprite>();
        foreach (var obj in assets)
        {
            if (obj is Sprite s) list.Add(s);
        }
        return list.ToArray();
    }

    private static void ConfigureSingleSprite(string assetPath, int border)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        if (border > 0)
        {
            importer.spriteBorder = new Vector4(border, border, border, border);
        }
        importer.SaveAndReimport();
    }

    private static void SliceGridSprite(string assetPath, int cellW, int cellH, int cols, int rows, Vector2 pivot)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        string baseName = Path.GetFileNameWithoutExtension(assetPath);
        var sheets = new List<SpriteMetaData>();

        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int y = (rows - 1 - r) * cellH;
                int x = c * cellW;

                var smd = new SpriteMetaData
                {
                    name = $"{baseName}_{index}",
                    rect = new Rect(x, y, cellW, cellH),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = pivot
                };
                sheets.Add(smd);
                index++;
            }
        }

        importer.spritesheet = sheets.ToArray();
        importer.SaveAndReimport();
    }

    private static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
        }
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        for (int i = 8; i < 32; i++)
        {
            SerializedProperty element = layersProp.GetArrayElementAtIndex(i);
            if (element.stringValue == layerName) return;
            if (string.IsNullOrEmpty(element.stringValue))
            {
                element.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
    }

    private static void EnsureSortingLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("m_SortingLayers");
        for (int i = 0; i < layersProp.arraySize; i++)
        {
            SerializedProperty entry = layersProp.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("name").stringValue == layerName) return;
        }
        layersProp.InsertArrayElementAtIndex(layersProp.arraySize);
        SerializedProperty newEntry = layersProp.GetArrayElementAtIndex(layersProp.arraySize - 1);
        newEntry.FindPropertyRelative("name").stringValue = layerName;
        newEntry.FindPropertyRelative("uniqueID").intValue = (int)UnityEngine.Random.Range(1000, 99999);
        tagManager.ApplyModifiedProperties();
    }

    private static void MovePackFolder(string subfolder, string destDir)
    {
        string srcDir = Path.Combine(StarterPackDir, subfolder);
        if (!Directory.Exists(srcDir)) return;
        EnsureFolder(destDir);
        string[] files = Directory.GetFiles(srcDir);
        foreach (string file in files)
        {
            if (file.EndsWith(".meta")) continue;
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            if (!File.Exists(destFile))
            {
                File.Copy(file, destFile, true);
            }
        }
    }

    private static void EnsureFolder(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null) return;
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null) field.SetValue(target, value);
    }
}
