using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > Build Toilet Level
// Makes the ToiletLevel scene from a copy of Main. Hides the old level and adds
// the toilet map, walls, poop islands, the sewer cleaner, the flush ending, and a new menu look.
// Running it again rebuilds the scene from scratch
public static class ToiletLevelBuilder {
    const string MainScene   = "Assets/Scenes/Main.unity";
    const string ToiletScene = "Assets/Scenes/ToiletLevel.unity";

    // these match the toilet art
    const float MapCenterY = 30f;
    const float BowlBottom = 8f;
    const float BowlTop    = 40f;
    const float Step       = 1.75f;  // height between islands, needs a small swing
    static Vector3 startPos = new Vector3(-24, -29, 0);
    static Vector3 drainPos = new Vector3(0, 11, 0);

    // stuff from the old level we don't need
    static List<string> oldStuff = new List<string> {
        "Section 1 - Launch Pad", "Section 2 - Sky Gaps", "Section 3 - Crumbling Isles",
        "Section 4 - Moving Isles", "Section 5 - Mushroom Peak",
        "Ground", "Ground (1)", "Ramp", "Hill", "Background", "World Edges", "Tutorial Hints",
    };

    // menu words, change these to whatever you want
    const string GameTitle = "BIG SACK";
    const string Subtitle  = "Somebody has to clean the pipes.";
    const string WinText   = "FLUSHED!";
    const string HowToText =
        "A / D  -  Walk. Your tool bag comes with you.\n\n" +
        "Hold SHIFT  -  Plant your feet so the bag can't drag you.\n\n" +
        "SHIFT + A / D  -  Swing the bag. Push with the swing to make it bigger.\n\n" +
        "SPACE  -  Jump. Jump as the bag swings UP to launch way higher.\n\n\n" +
        "Worried poop crumbles if you stand on it too long.\n\n" +
        "Fall down a pit and you go back to the last flag.\n\n" +
        "Make it to the top of the toilet and get flushed!\n\n" +
        "ESC  -  Pause        R  -  Stuck? Back to the last flag";

    // menu colors
    static Color navy      = new Color32(67, 74, 95, 255);
    static Color lightBlue = new Color32(159, 220, 242, 255);

    static Transform toilet;
    static GameObject islandCopy;
    static GameObject crumblingCopy;

    [MenuItem("Tools/Build Toilet Level")]
    public static void Build () {
        // unity can't open scenes while the game is playing
        if (EditorApplication.isPlaying) {
            EditorUtility.DisplayDialog("Build Toilet Level", "Stop the game first (press the Play button again), then try this again.", "OK");
            return;
        }

        // copy Main into a new scene
        AssetDatabase.DeleteAsset(ToiletScene);
        AssetDatabase.CopyAsset(MainScene, ToiletScene);
        var scene = EditorSceneManager.OpenScene(ToiletScene);

        FindIslandsToCopy();
        foreach (GameObject obj in scene.GetRootGameObjects()) {
            if (oldStuff.Contains(obj.name)) obj.SetActive(false);
        }

        toilet = new GameObject("Toilet").transform;
        AddMap();
        AddWalls();
        AddPoopIslands();
        AddFlush();
        SetUpPlayer();
        RedoMenu();

        EditorSceneManager.SaveScene(scene);
        AddToBuildSettings();
        Debug.Log("Toilet level built");
    }

    // use an island and a crumbling island from the old level so they keep their scripts
    static void FindIslandsToCopy () {
        Island[] islands = Object.FindObjectsByType<Island>(FindObjectsInactive.Include);
        foreach (Island island in islands) {
            // skip mushrooms and moving islands, we just want normal ones
            if (island.GetComponent<BouncyMushroom>() != null) continue;
            if (island.GetComponent<MovingIsland>() != null) continue;

            if (island.GetComponent<CrumblingIsland>() != null) crumblingCopy = island.gameObject;
            else islandCopy = island.gameObject;
        }
    }

    static void AddMap () {
        MakeSprite("Toilet Back", "toilet_back", new Vector3(0, MapCenterY, 0), -20, toilet);
        MakeSprite("Toilet Walls", "toilet_walls", new Vector3(0, MapCenterY, 0), 0, toilet);

        // camera background matches the wall in case you see past the map
        Camera.main.backgroundColor = new Color32(127, 211, 242, 255);
    }

    // invisible lines along the inside of the pipes and bowl that you can't go through
    static void AddWalls () {
        var walls = new GameObject("Walls");
        walls.transform.SetParent(toilet);
        walls.layer = islandCopy.layer;  // same layer as islands so you can stand on it

        // left side: the pit, the dip, start, the tunnels, then up the pipe
        var left = new List<Vector2> {
            new Vector2(-9.5f, -36), new Vector2(-9.5f, -30),
            new Vector2(-16, -30), new Vector2(-16, -31),
            new Vector2(-18, -31), new Vector2(-18, -30),
            new Vector2(-26, -30), new Vector2(-26, -24),
            new Vector2(6, -24),   new Vector2(6, -18),
            new Vector2(-26, -18), new Vector2(-26, -4),
            new Vector2(-4, -4),   new Vector2(-4, BowlBottom),
        };
        AddBowlSide(left, -1);

        // right side: the pit, the tunnels, then up the pipe
        var right = new List<Vector2> {
            new Vector2(-8, -36),  new Vector2(-8, -30),
            new Vector2(16, -30),  new Vector2(16, -12),
            new Vector2(-16, -12), new Vector2(-16, -9),
            new Vector2(4, -9),    new Vector2(4, BowlBottom),
        };
        AddBowlSide(right, 1);

        AddEdge(walls, left);
        AddEdge(walls, right);
    }

    // follows the curve of the bowl up to the top
    static void AddBowlSide (List<Vector2> points, float side) {
        for (float y = BowlBottom; y <= BowlTop; y += 0.5f) {
            points.Add(new Vector2(side * BowlWidth(y), y));
        }
    }

    static void AddEdge (GameObject obj, List<Vector2> points) {
        var edge = obj.AddComponent<EdgeCollider2D>();
        edge.SetPoints(points);
        edge.edgeRadius = 0.05f;
    }

    static void AddPoopIslands () {
        var islands = new GameObject("Poop Islands").transform;
        islands.SetParent(toilet);

        // right shaft
        Zigzag(islands, -29, -18.5f, 8.5f, 13.5f);
        // left shaft
        Zigzag(islands, -17, -10, -18.5f, -23.5f);
        // pipe going up into the toilet
        Zigzag(islands, -8, BowlBottom, -2.3f, 2.3f);

        // the bowl, moving left and right so you use the whole space
        int count = 0;
        for (float y = BowlBottom + 1.5f; y <= BowlTop - 1.5f; y += Step) {
            float x = 5 * Mathf.Sin((y - BowlBottom) / 7);
            if (count % 2 == 0) x -= 3;
            else x += 3;
            bool crumbly = count > 3 && count % 4 == 0;
            AddPoop(islands, x, y, crumbly);
            count++;
        }
    }

    // islands going up, switching sides each time
    static void Zigzag (Transform parent, float fromY, float toY, float firstX, float secondX) {
        bool first = true;
        for (float y = fromY; y <= toY; y += Step) {
            if (first) AddPoop(parent, firstX, y, false);
            else AddPoop(parent, secondX, y, false);
            first = !first;
        }
    }

    // y is the top of the island
    static void AddPoop (Transform parent, float x, float y, bool crumbly) {
        GameObject copy = crumbly ? crumblingCopy : islandCopy;
        GameObject poop = Object.Instantiate(copy, parent);
        poop.name = crumbly ? "Crumbly Poop" : "Poop";
        poop.SetActive(true);
        poop.transform.position = new Vector3(x, y - 0.25f, 0);

        // the old islands have a stem under them, get rid of it
        for (int i = poop.transform.childCount - 1; i >= 0; i--) {
            Object.DestroyImmediate(poop.transform.GetChild(i).gameObject);
        }

        SpriteRenderer sr = poop.GetComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(crumbly ? "island_poop_crumbly" : "island_poop");
        sr.size   = new Vector2(2.5f, 0.5f);
        BoxCollider2D box = poop.GetComponent<BoxCollider2D>();
        if (box != null) {
            box.size   = new Vector2(2.5f, box.size.y);
            box.offset = new Vector2(0, box.offset.y);
        }

        // face is its own sprite so it doesn't stretch with the island
        string face = crumbly ? "poop_face_worried" : "poop_face";
        MakeSprite("Face", face, poop.transform.position, sr.sortingOrder + 1, poop.transform);
    }

    static void AddFlush () {
        var drain = new GameObject("Drain").transform;
        drain.SetParent(toilet);
        drain.position = drainPos;

        var start = new GameObject("Start Point").transform;
        start.SetParent(toilet);
        start.position = startPos;

        GameObject swirl = MakeSprite("Swirl", "toilet_swirl", drainPos, -1, toilet);

        // trigger across the top of the bowl
        var goal = new GameObject("Flush Goal");
        goal.transform.SetParent(toilet);
        goal.transform.position = new Vector3(0, BowlTop + 1.6f, 0);
        var trigger = goal.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(48, 2);

        var flush = goal.AddComponent<FlushGoal>();
        flush.stats      = Object.FindAnyObjectByType<GameManager>();
        flush.player     = Object.FindAnyObjectByType<PlayerController>();
        flush.drain      = drain;
        flush.startPoint = start;
        flush.swirl      = swirl.GetComponent<SpriteRenderer>();
        flush.flushSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/lowDown.ogg");
        flush.landSound  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/impactSoft_heavy_000.ogg");
    }

    static void SetUpPlayer () {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        Rigidbody2D sack = player.GetComponent<DistanceJoint2D>().connectedBody;

        // sewer cleaner and his tool bag
        PlayerVisuals visuals = player.GetComponent<PlayerVisuals>();
        visuals.idle = LoadSprite("player_cleaner_idle");
        visuals.step = LoadSprite("player_cleaner_step");
        player.GetComponent<SpriteRenderer>().sprite = visuals.idle;
        sack.GetComponent<SpriteRenderer>().sprite   = LoadSprite("sack_toolbag");

        // checks for walls every frame so fast swings don't go through them
        player.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        sack.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // start at the bottom of the sewer, falling in a pit sends you back
        player.respawnBelowY = -33;
        player.transform.position = startPos;
        sack.transform.position   = startPos + new Vector3(-0.8f, 0, 0);
        Transform cam = Camera.main.transform;
        cam.position = new Vector3(startPos.x, startPos.y + 1.5f, cam.position.z);

        // checkpoint before the climb up the toilet
        Checkpoint oldCheckpoint = Object.FindAnyObjectByType<Checkpoint>(FindObjectsInactive.Include);
        GameObject checkpoint = Object.Instantiate(oldCheckpoint.gameObject, toilet);
        checkpoint.name = "Checkpoint - Toilet";
        checkpoint.SetActive(true);
        checkpoint.GetComponent<Checkpoint>().sectionName = "The Toilet";  // banner when you reach it
        checkpoint.transform.position = new Vector3(-8, -8.5f, 0);
    }

    static void RedoMenu () {
        UIManager ui = Object.FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);
        Transform canvas = ui.titlePanel.transform.parent;

        SetText(ui.titlePanel, "Title Text", GameTitle);
        SetText(ui.titlePanel, "Subtitle", Subtitle);
        SetText(ui.winPanel, "Heading", WinText);
        SetText(ui.howToPanel, "Steps", HowToText);

        // navy see-through backgrounds instead of black
        SetBackground(ui.titlePanel, 0.3f);
        SetBackground(ui.howToPanel, 0.92f);
        SetBackground(ui.pausePanel, 0.7f);
        SetBackground(ui.winPanel, 0.7f);
        SetBackground(ui.hintPanel, 0.7f);

        // pixel fonts get blurry at in-between sizes. Kenney Pixel is made on a 16 pixel
        // grid and Kenney Blocks on an 8 pixel grid, so round every size to fit
        canvas.GetComponent<Canvas>().pixelPerfect = true;
        foreach (Text text in canvas.GetComponentsInChildren<Text>(true)) {
            int grid = text.font.name.Contains("Blocks") ? 8 : 16;
            text.fontSize = Mathf.Max(grid, Mathf.RoundToInt(text.fontSize / (float)grid) * grid);
        }

        // porcelain buttons that turn blue when selected
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true)) {
            Image image = button.GetComponent<Image>();
            image.sprite = LoadSprite("button_porcelain");
            image.type   = Image.Type.Sliced;

            ColorBlock colors = button.colors;
            colors.highlightedColor = lightBlue;
            colors.selectedColor    = lightBlue;
            button.colors = colors;

            // dark words on the buttons, no shadow so they stay sharp
            Text label = button.GetComponentInChildren<Text>(true);
            label.color = navy;
            foreach (Shadow shadow in label.GetComponents<Shadow>()) {
                Object.DestroyImmediate(shadow);
            }
        }
    }

    static void SetText (GameObject panel, string child, string words) {
        panel.transform.Find(child).GetComponent<Text>().text = words;
    }

    static void SetBackground (GameObject panel, float seeThrough) {
        Image image = panel.GetComponent<Image>();
        image.color = new Color(navy.r, navy.g, navy.b, seeThrough);
    }

    // how wide the inside of the bowl is at height y (same curve as the art)
    static float BowlWidth (float y) {
        float t = Mathf.Clamp01((y - BowlBottom) / (BowlTop - BowlBottom));
        return 4.4f + 17 * Mathf.Sqrt(1 - Mathf.Pow(1 - t, 2.4f));
    }

    static GameObject MakeSprite (string name, string sprite, Vector3 pos, int order, Transform parent) {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(sprite);
        sr.sortingOrder = order;
        return obj;
    }

    static Sprite LoadSprite (string name) {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/" + name + ".png");
    }

    // adds the scene to File > Build Settings so restarting the level works
    static void AddToBuildSettings () {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene s in scenes) {
            if (s.path == ToiletScene) return;
        }
        scenes.Add(new EditorBuildSettingsScene(ToiletScene, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
