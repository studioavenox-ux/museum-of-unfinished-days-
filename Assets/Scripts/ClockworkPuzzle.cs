using UnityEngine;

/// <summary>Deterministic three-ring alignment puzzle with a lightweight procedural brass mechanism.</summary>
public sealed class ClockworkPuzzle : MonoBehaviour
{
    private const int RingCount = 3;
    private const int Positions = 8;
    private static readonly int[] Solution = { 2, 5, 1 };
    private static readonly string[] RingNames = { "Outer · North Star", "Middle · Crescent", "Inner · Morning Star" };
    private readonly int[] positions = new int[RingCount];
    private readonly Transform[] rings = new Transform[RingCount];
    private int selectedRing;
    private bool solved;
    private GalleryClue discoveryClue;
    private string feedback = "The indicator is dark. The astronomical chart holds the answer.";
    private Renderer centralLens;
    private Light indicatorLight;
    private GameObject archiveReveal;

    public int SelectedRing => selectedRing;
    public bool IsSolved => solved;
    public int Position(int ring) => positions[Mathf.Clamp(ring, 0, RingCount - 1)];
    public string SelectedName => RingNames[selectedRing];
    public GalleryClue DiscoveryClue => discoveryClue;

    private void Start() => BuildMechanism();

    public void SelectRing(int ring)
    {
        if (!solved) selectedRing = Mathf.Clamp(ring, 0, RingCount - 1);
        UpdateRingVisuals();
    }

    /// <returns>True only on the transition from unsolved to solved.</returns>
    public bool RotateSelected(int direction)
    {
        if (solved || direction == 0) return false;
        positions[selectedRing] = (positions[selectedRing] + (direction > 0 ? 1 : Positions - 1)) % Positions;
        rings[selectedRing].localRotation = Quaternion.Euler(0, positions[selectedRing] * 45f, 0);
        if (MatchesSolution())
        {
            solved = true;
            feedback = "The alignment locks into place.";
            if (archiveReveal != null) archiveReveal.SetActive(true);
            UpdateRingVisuals();
            return true;
        }
        feedback = "The central indicator stays dark. The arrangement is not yet aligned.";
        UpdateRingVisuals();
        return false;
    }

    public void ResetPuzzle()
    {
        if (solved) return;
        for (int i = 0; i < RingCount; i++)
        {
            positions[i] = 0;
            rings[i].localRotation = Quaternion.identity;
        }
        feedback = "Rings returned to their first marks.";
        UpdateRingVisuals();
    }

    public bool MatchesSolution()
    {
        for (int i = 0; i < RingCount; i++) if (positions[i] != Solution[i]) return false;
        return true;
    }

    public string CurrentStatus()
    {
        if (solved) return "The archive plate is open. The recovered record is in your journal.";
        return "OBJECTIVE  ·  Align North Star, Crescent, Morning Star with the amber pointer.\n" +
               "RINGS  ·  O " + (positions[0] + 1) + "/8   M " + (positions[1] + 1) + "/8   I " + (positions[2] + 1) + "/8\n" +
               "SELECTED  ·  " + RingNames[selectedRing] + "   |   Position " + (positions[selectedRing] + 1) + "/" + Positions + "\n\n" +
               "1 / 2 / 3 select ring     A / D rotate     R reset     Esc close\n" +
               feedback + "\nThe astronomical model's journal entry gives the order, outside in.";
    }

    private void BuildMechanism()
    {
        Material brass = MaterialFor(new Color(0.54f, 0.34f, 0.15f), 0.65f);
        Material brightBrass = MaterialFor(new Color(0.78f, 0.56f, 0.24f), 0.45f);
        Material iron = MaterialFor(new Color(0.075f, 0.105f, 0.11f), 0.25f);
        Material amber = MaterialFor(new Color(1f, 0.55f, 0.17f), 0.15f);
        amber.EnableKeyword("_EMISSION");
        amber.SetColor("_EmissionColor", new Color(0.28f, 0.09f, 0.015f));
        for (int i = 0; i < RingCount; i++)
        {
            float radius = 1.12f - i * 0.28f;
            var ringObject = new GameObject(RingNames[i] + " rotating ring", typeof(MeshFilter), typeof(MeshRenderer));
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = new Vector3(0, i * 0.065f, 0);
            rings[i] = ringObject.transform;
            ringObject.GetComponent<MeshFilter>().mesh = MakeTorus(radius, 0.035f, 64, 8);
            ringObject.GetComponent<MeshRenderer>().sharedMaterial = i == 1 ? brightBrass : brass;
            for (int marker = 0; marker < Positions; marker++)
            {
                float angle = marker * 45f;
                float radians = angle * Mathf.Deg2Rad;
                bool keyMark = marker == (Positions / 2 + Positions - Solution[i]) % Positions;
                var glyph = GameObject.CreatePrimitive(keyMark ? PrimitiveType.Sphere : PrimitiveType.Cube);
                glyph.name = keyMark ? RingNames[i] + " alignment sigil" : "Engraved star tick";
                glyph.transform.SetParent(rings[i], false);
                glyph.transform.localPosition = new Vector3(Mathf.Sin(radians) * radius, 0, Mathf.Cos(radians) * radius);
                glyph.transform.localRotation = Quaternion.Euler(0, angle, 0);
                glyph.transform.localScale = keyMark ? Vector3.one * 0.11f : new Vector3(0.055f, 0.035f, 0.055f);
                glyph.GetComponent<Renderer>().sharedMaterial = keyMark ? amber : iron;
                Collider collider = glyph.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
                if (keyMark && i == 0) AddMark(glyph.transform, PrimitiveType.Cube, new Vector3(1.2f, 0.32f, 0.32f), Vector3.zero, amber);
                if (keyMark && i == 1) AddMark(glyph.transform, PrimitiveType.Sphere, Vector3.one * 0.62f, new Vector3(0.48f, 0.12f, -0.18f), iron);
                if (keyMark && i == 2) AddMark(glyph.transform, PrimitiveType.Cube, new Vector3(0.32f, 0.32f, 1.2f), Vector3.zero, amber);
            }
        }
        var pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.name = "Fixed amber alignment pointer";
        pointer.transform.SetParent(transform, false);
        pointer.transform.localPosition = new Vector3(0, 0.13f, -1.35f);
        pointer.transform.localScale = new Vector3(0.075f, 0.08f, 0.35f);
        pointer.GetComponent<Renderer>().sharedMaterial = amber;
        Destroy(pointer.GetComponent<Collider>());

        var lens = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lens.name = "Clockwork central amber lens";
        lens.transform.SetParent(transform, false);
        lens.transform.localPosition = new Vector3(0, 0.23f, 0);
        lens.transform.localScale = Vector3.one * 0.2f;
        lens.GetComponent<Renderer>().sharedMaterial = amber;
        centralLens = lens.GetComponent<Renderer>();
        Destroy(lens.GetComponent<Collider>());
        var lightObject = new GameObject("Clockwork restrained amber indicator light");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = new Vector3(0, 0.32f, 0);
        indicatorLight = lightObject.AddComponent<Light>();
        indicatorLight.type = LightType.Point;
        indicatorLight.color = new Color(1f, 0.58f, 0.25f);
        indicatorLight.range = 1.5f;
        indicatorLight.intensity = 0.15f;

        var label = new GameObject("Clockwork display label");
        label.transform.SetParent(transform, false);
        label.transform.localPosition = new Vector3(0, 0.62f, -1.42f);
        label.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        TextMesh text = label.AddComponent<TextMesh>();
        text.text = "THE CELESTIAL REGULATOR\nE  ·  ALIGN THE RINGS";
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 36;
        text.characterSize = 0.07f;
        text.color = new Color(0.88f, 0.68f, 0.35f);

        archiveReveal = new GameObject("Revealed archive plaque");
        archiveReveal.transform.SetParent(transform, false);
        archiveReveal.transform.localPosition = new Vector3(0, 0.92f, 1.72f);
        var plaque = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plaque.name = "Archive plaque backing";
        plaque.transform.SetParent(archiveReveal.transform, false);
        plaque.transform.localScale = new Vector3(0.98f, 0.54f, 0.07f);
        plaque.GetComponent<Renderer>().sharedMaterial = iron;
        Destroy(plaque.GetComponent<Collider>());
        var archiveLabel = new GameObject("Archive plaque inscription");
        archiveLabel.transform.SetParent(archiveReveal.transform, false);
        archiveLabel.transform.localPosition = new Vector3(0, 0, -0.041f);
        archiveLabel.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        TextMesh archiveText = archiveLabel.AddComponent<TextMesh>();
        archiveText.text = "ARCHIVE FRAGMENT 07\nTHE UNREGISTERED DAY";
        archiveText.anchor = TextAnchor.MiddleCenter;
        archiveText.alignment = TextAlignment.Center;
        archiveText.fontSize = 36;
        archiveText.characterSize = 0.055f;
        archiveText.color = new Color(0.93f, 0.65f, 0.28f);
        var revealLamp = new GameObject("Archive plaque amber light");
        revealLamp.transform.SetParent(archiveReveal.transform, false);
        revealLamp.transform.localPosition = new Vector3(0, 0.12f, -0.18f);
        Light plaqueLight = revealLamp.AddComponent<Light>();
        plaqueLight.type = LightType.Point;
        plaqueLight.color = new Color(1f, 0.55f, 0.22f);
        plaqueLight.range = 1.1f;
        plaqueLight.intensity = 0.35f;
        archiveReveal.SetActive(false);

        discoveryClue = gameObject.AddComponent<GalleryClue>();
        discoveryClue.title = MuseumStoryProgression.DiscoveryTitle;
        discoveryClue.description = MuseumStoryProgression.DiscoveryText;
        UpdateRingVisuals();
    }

    private void UpdateRingVisuals()
    {
        for (int i = 0; i < RingCount; i++)
        {
            if (rings[i] == null) continue;
            var renderer = rings[i].GetComponent<MeshRenderer>();
            renderer.material.color = solved ? new Color(0.9f, 0.65f, 0.28f) :
                (i == selectedRing ? new Color(0.92f, 0.68f, 0.31f) : new Color(0.57f, 0.37f, 0.18f));
        }
        if (centralLens != null)
        {
            centralLens.material.SetColor("_EmissionColor", solved ? new Color(1f, 0.32f, 0.06f) : new Color(0.12f, 0.035f, 0.005f));
            if (indicatorLight != null) indicatorLight.intensity = solved ? 0.7f : 0.12f;
        }
    }

    private static void AddMark(Transform parent, PrimitiveType shape, Vector3 scale, Vector3 offset, Material material)
    {
        var mark = GameObject.CreatePrimitive(shape);
        mark.name = "Celestial sigil detail";
        mark.transform.SetParent(parent, false);
        mark.transform.localPosition = offset;
        mark.transform.localScale = scale;
        mark.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = mark.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
    }

    private static Mesh MakeTorus(float radius, float tube, int segments, int sides)
    {
        var vertices = new Vector3[(segments + 1) * (sides + 1)];
        var triangles = new int[segments * sides * 6];
        for (int i = 0; i <= segments; i++)
        {
            float a = i * Mathf.PI * 2 / segments;
            for (int j = 0; j <= sides; j++)
            {
                float b = j * Mathf.PI * 2 / sides;
                float r = radius + tube * Mathf.Cos(b);
                vertices[i * (sides + 1) + j] = new Vector3(r * Mathf.Sin(a), tube * Mathf.Sin(b), r * Mathf.Cos(a));
            }
        }
        int index = 0;
        for (int i = 0; i < segments; i++) for (int j = 0; j < sides; j++)
        {
            int current = i * (sides + 1) + j, next = current + sides + 1;
            triangles[index++] = current; triangles[index++] = next; triangles[index++] = current + 1;
            triangles[index++] = current + 1; triangles[index++] = next; triangles[index++] = next + 1;
        }
        var mesh = new Mesh { name = "Procedural brass torus" };
        mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals();
        return mesh;
    }

    private static Material MaterialFor(Color color, float metallic)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var material = new Material(shader) { color = color };
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        return material;
    }
}
