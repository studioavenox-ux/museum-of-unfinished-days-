using UnityEngine;

/// <summary>Creates the first playable, deliberately unpolished gallery milestone at runtime.</summary>
public sealed class GalleryPrototype : MonoBehaviour
{
    private static readonly Color Midnight = new Color(0.055f, 0.105f, 0.12f);
    private static readonly Color Brass = new Color(0.57f, 0.37f, 0.16f);

    private void Start()
    {
        BuildGallery();
        BuildPlayer();
        RenderSettings.ambientLight = new Color(0.23f, 0.25f, 0.25f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = Midnight;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.012f;
        var sun = new GameObject("Gallery key light").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 0.8f;
        sun.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
    }

    private void BuildGallery()
    {
        Material floor = Mat(new Color(0.19f, 0.20f, 0.19f));
        Material wall = Mat(new Color(0.12f, 0.17f, 0.17f));
        Material trim = Mat(Brass);
        Material dark = Mat(new Color(0.075f, 0.08f, 0.075f));
        Block("Gallery floor", new Vector3(0, -0.2f, 0), new Vector3(14, 0.4f, 24), floor);
        Block("Ceiling", new Vector3(0, 5.2f, 0), new Vector3(14, 0.35f, 24), dark);
        Block("North wall", new Vector3(0, 2.5f, 12), new Vector3(14, 5, 0.35f), wall);
        Block("South wall", new Vector3(0, 2.5f, -12), new Vector3(14, 5, 0.35f), wall);
        Block("West wall", new Vector3(-7, 2.5f, 0), new Vector3(0.35f, 5, 24), wall);
        Block("East wall", new Vector3(7, 2.5f, 0), new Vector3(0.35f, 5, 24), wall);
        for (int z = -8; z <= 8; z += 4)
        {
            Block("Wall pilaster", new Vector3(-6.55f, 2.35f, z), new Vector3(0.55f, 4.7f, 0.6f), trim);
            Block("Wall pilaster", new Vector3(6.55f, 2.35f, z), new Vector3(0.55f, 4.7f, 0.6f), trim);
            Block("Gallery rib", new Vector3(0, 4.85f, z), new Vector3(13, 0.22f, 0.5f), trim);
        }
        Exhibit("Unsent letter", new Vector3(-3.2f, 0.62f, 2.5f), new Vector3(1.25f, 1.25f, 1.25f),
            "A letter addressed to no one. Its final line reads: 'I remember the day we never had.'");
        Exhibit("Stopped pocket watch", new Vector3(3.1f, 0.72f, 0f), new Vector3(1.2f, 1.45f, 1.2f),
            "The hands are fixed at 8:17. A tiny engraving says: 'For the hour that went missing.'");
        Exhibit("Astronomical model", new Vector3(-2.9f, 0.8f, -5f), new Vector3(1.45f, 1.6f, 1.45f),
            "The chart's margin preserves the missing pattern: 'Outside in: North Star, Crescent, Morning Star.' Align those three sigils with the brass pointer, in that order.");
        BuildClockwork();
        Block("Far doorway surround", new Vector3(0, 2.2f, 11.55f), new Vector3(3.4f, 4.4f, 0.45f), dark);
        Block("Doorway left", new Vector3(-2.2f, 2.1f, 11.3f), new Vector3(0.25f, 4.2f, 0.6f), trim);
        Block("Doorway right", new Vector3(2.2f, 2.1f, 11.3f), new Vector3(0.25f, 4.2f, 0.6f), trim);
    }

    private void BuildClockwork()
    {
        Material stand = Mat(new Color(0.12f, 0.095f, 0.065f));
        Material plaque = Mat(new Color(0.22f, 0.15f, 0.075f));
        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "Clockwork museum pedestal";
        pedestal.transform.position = new Vector3(0, 0.52f, 7.2f);
        pedestal.transform.localScale = new Vector3(2.8f, 0.52f, 2.8f);
        pedestal.GetComponent<Renderer>().sharedMaterial = stand;
        var plinth = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plinth.name = "Clockwork display plinth";
        plinth.transform.position = new Vector3(0, 1.02f, 7.2f);
        plinth.transform.localScale = new Vector3(2.2f, 0.18f, 2.2f);
        plinth.GetComponent<Renderer>().sharedMaterial = plaque;
        var instrument = new GameObject("Astronomical Clockwork");
        instrument.transform.position = new Vector3(0, 1.48f, 7.2f);
        var hitArea = instrument.AddComponent<BoxCollider>();
        hitArea.size = new Vector3(2.7f, 0.8f, 2.7f);
        instrument.AddComponent<ClockworkPuzzle>();
    }

    private void BuildPlayer()
    {
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0, 0.15f, -9f);
        var controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.32f;
        controller.center = new Vector3(0, 0.9f, 0);
        var cameraObject = new GameObject("Player camera");
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0, 1.58f, 0);
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<Camera>().fieldOfView = 70f;
        cameraObject.AddComponent<AudioListener>();
        player.AddComponent<GalleryFirstPerson>();
    }

    private void Exhibit(string title, Vector3 position, Vector3 size, string description)
    {
        Material plinth = Mat(new Color(0.16f, 0.12f, 0.085f));
        Block(title + " plinth", position + Vector3.down * 0.28f, new Vector3(1.5f, 0.56f, 1.5f), plinth);
        var item = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        item.name = title;
        item.transform.position = position + Vector3.up * 0.72f;
        item.transform.localScale = size * 0.48f;
        item.GetComponent<Renderer>().material = Mat(Brass);
        var clue = item.AddComponent<GalleryClue>();
        clue.title = title;
        clue.description = description;
    }

    private static void Block(string name, Vector3 position, Vector3 size, Material material)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = size;
        cube.GetComponent<Renderer>().material = material;
    }

    private static Material Mat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var material = new Material(shader);
        material.color = color;
        return material;
    }
}

public sealed class GalleryClue : MonoBehaviour
{
    public string title;
    [TextArea] public string description;
}

/// <summary>Single owner of movement, mouse look, artifact detection, and modal keyboard input.</summary>
public sealed class GalleryFirstPerson : MonoBehaviour
{
    private CharacterController motor;
    private Camera view;
    private GalleryUI ui;
    private EvidenceJournal journal;
    private float pitch;
    private float verticalSpeed;
    private GalleryClue target;
    private ClockworkPuzzle targetPuzzle;
    private ClockworkPuzzle activePuzzle;
    private MuseumStoryProgression story;

    private void Start()
    {
        motor = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>();
        journal = new GameObject("Evidence Journal").AddComponent<EvidenceJournal>();
        story = new GameObject("Museum Story State").AddComponent<MuseumStoryProgression>();
        ui = new GameObject("Gallery UI Controller").AddComponent<GalleryUI>();
        ui.Initialize(journal);
    }

    private void Update()
    {
        if (IsStoryScreen())
        {
            UpdateStoryInput();
            return;
        }
        if (ui.ActiveScreen == GalleryUI.Screen.Clockwork)
        {
            UpdateClockworkInput();
            return;
        }
        if (ui.ActiveScreen == GalleryUI.Screen.Inspection)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) ui.Show(GalleryUI.Screen.Gameplay);
            return;
        }
        if (ui.ActiveScreen == GalleryUI.Screen.Journal)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.J)) ui.Show(GalleryUI.Screen.Gameplay);
            return;
        }
        if (ui.ActiveScreen == GalleryUI.Screen.Pause)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) ui.Show(GalleryUI.Screen.Gameplay);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ui.Show(GalleryUI.Screen.Pause);
            return;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            ui.ToggleJournal();
            return;
        }

        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 2.1f);
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 2.1f, -85f, 85f);
        view.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        DetectTarget();
        if (Input.GetKeyDown(KeyCode.E) && targetPuzzle != null)
        {
            activePuzzle = targetPuzzle;
            ui.OpenClockwork(activePuzzle.CurrentStatus());
            return;
        }
        if (Input.GetKeyDown(KeyCode.E) && target != null)
        {
            journal.Record(target);
            ui.OpenInspection(target);
            return;
        }
        Vector3 direction = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        if (direction.sqrMagnitude > 1) direction.Normalize();
        if (motor.isGrounded && verticalSpeed < 0) verticalSpeed = -1f;
        verticalSpeed += Physics.gravity.y * Time.deltaTime;
        motor.Move((direction * 3.3f + Vector3.up * verticalSpeed) * Time.deltaTime);
    }

    private bool IsStoryScreen()
    {
        return ui.ActiveScreen == GalleryUI.Screen.StoryDiscovery || ui.ActiveScreen == GalleryUI.Screen.StoryDialogue ||
               ui.ActiveScreen == GalleryUI.Screen.StoryChoice || ui.ActiveScreen == GalleryUI.Screen.StoryOutcome;
    }

    private void UpdateStoryInput()
    {
        bool advance = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        if (ui.ActiveScreen == GalleryUI.Screen.StoryDiscovery || ui.ActiveScreen == GalleryUI.Screen.StoryDialogue)
        {
            if (advance && story.Advance()) PresentStoryPhase();
            return; // Escape deliberately cannot dismiss an unanswered narrative sequence.
        }
        if (ui.ActiveScreen == GalleryUI.Screen.StoryChoice)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ChooseStory(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) ChooseStory(1);
            return;
        }
        if (ui.ActiveScreen == GalleryUI.Screen.StoryOutcome &&
            (advance || Input.GetKeyDown(KeyCode.Escape))) FinishStorySequence();
    }

    private void PresentStoryPhase()
    {
        switch (story.CurrentPhase)
        {
            case MuseumStoryProgression.Phase.Dialogue:
                ui.ShowStoryDialogue(story.CurrentSpeaker, story.CurrentLine, story.DialogueIndex, 4);
                break;
            case MuseumStoryProgression.Phase.Choice:
                ui.ShowStoryChoice(ChooseStory);
                break;
            case MuseumStoryProgression.Phase.Outcome:
                ui.ShowStoryOutcome(story.ChoiceTitle, story.OutcomeText, FinishStorySequence);
                break;
        }
    }

    private void ChooseStory(int option)
    {
        if (option < 0 || option > 1) return;
        MuseumStoryProgression.PlayerChoice choice = option == 0
            ? MuseumStoryProgression.PlayerChoice.SharedRecord
            : MuseumStoryProgression.PlayerChoice.KeptPrivate;
        if (!story.Choose(choice)) return;
        journal.Record(story.CreateDecisionEvidence());
        PresentStoryPhase();
    }

    private void FinishStorySequence()
    {
        if (story.CurrentPhase == MuseumStoryProgression.Phase.Outcome) story.Advance();
        activePuzzle = null;
        ui.Show(GalleryUI.Screen.Gameplay);
    }

    private void UpdateClockworkInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ui.Show(GalleryUI.Screen.Gameplay);
            activePuzzle = null;
            return;
        }
        if (activePuzzle == null) { ui.Show(GalleryUI.Screen.Gameplay); return; }
        bool solvedNow = false;
        if (Input.GetKeyDown(KeyCode.Alpha1)) activePuzzle.SelectRing(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) activePuzzle.SelectRing(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) activePuzzle.SelectRing(2);
        else if (Input.GetKeyDown(KeyCode.R)) activePuzzle.ResetPuzzle();
        else if (Input.GetKeyDown(KeyCode.A)) solvedNow = activePuzzle.RotateSelected(-1);
        else if (Input.GetKeyDown(KeyCode.D)) solvedNow = activePuzzle.RotateSelected(1);
        if (solvedNow)
        {
            if (activePuzzle.DiscoveryClue != null) journal.Record(activePuzzle.DiscoveryClue);
            if (story.TryBeginDiscovery())
            {
                ui.ShowStoryDiscovery(MuseumStoryProgression.DiscoveryTitle, MuseumStoryProgression.DiscoveryText);
                return;
            }
        }
        ui.UpdateClockwork(activePuzzle.CurrentStatus(), activePuzzle.IsSolved);
    }

    private void DetectTarget()
    {
        target = null;
        targetPuzzle = null;
        if (Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, 3.2f))
        {
            targetPuzzle = hit.collider.GetComponentInParent<ClockworkPuzzle>();
            target = hit.collider.GetComponentInParent<GalleryClue>();
            if (targetPuzzle != null) target = null;
        }
        ui.SetTarget(target, targetPuzzle);
    }
}
