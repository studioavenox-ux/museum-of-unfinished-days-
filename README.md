# The Museum of Unfinished Days

A first-person, single-player narrative mystery prototype targeting **Unity 6.3 LTS, editor version `6000.3.0f1`**, and URP. The greybox gallery, artifacts, clockwork, and UI are built at runtime by the existing Milestones 1–4 scripts; this repair does not replace those systems or start a new milestone.

## Project status and configuration

- `ProjectSettings/ProjectVersion.txt` declares `6000.3.0f1`. The supplied revision value was all zeroes (a placeholder), so it was removed rather than inventing a Unity revision hash. This is the project's declared editor target; Unity Editor is not installed in the repair environment, so the editor version was not independently launched or verified here.
- `Packages/manifest.json` pins **Universal RP `17.3.0`** and **Unity UI `2.0.0`**. URP 17.3.0 is the package line for Unity 6.3. Package resolution has not been run in this environment.
- A URP package declaration is **not** a configured render pipeline. This checkout contains no URP Pipeline Asset, `ProjectSettings/GraphicsSettings.asset`, or `ProjectSettings/QualitySettings.asset`; no pipeline asset is assigned. Create a real asset in Unity and assign it as described below. Do not use a hand-authored placeholder asset.
- The scene is listed as enabled in `ProjectSettings/EditorBuildSettings.asset`; its GUID matches `Assets/Scenes/Gallery.unity.meta`.
- The gameplay reads the **legacy Input Manager** (`Input.GetAxis`, `GetAxisRaw`, and `GetKeyDown`) and its UI uses `StandaloneInputModule`. No Input System package or Input Actions are used. The checkout does not include `ProjectSettings/ProjectSettings.asset` or `ProjectSettings/InputManager.asset`, so Active Input Handling is not explicitly serialized in this repository. Set it to **Input Manager (Old)** in Unity. Do not add a second input module or duplicate the controls.
- Other usual generated project settings are absent from this supplied project snapshot. Let Unity 6.3 generate defaults on first open; avoid copying settings from another project.

## Open in Unity Hub and finish setup

1. In Unity Hub, install **Unity `6000.3.0f1`** (Unity 6.3 LTS) with a desktop build support module for your operating system. In Hub, choose **Add > Add project from disk** and select this repository's root folder (the folder containing `Assets`, `Packages`, and `ProjectSettings`).
2. Open the project with that editor version. Allow Package Manager to resolve the manifest dependencies. In **Window > Package Manager**, confirm Universal RP 17.3.x and Unity UI 2.0.0 are available. If the manifest's URP package is missing, resolve that before continuing.
3. Set **Edit > Project Settings > Player > Other Settings > Configuration > Active Input Handling** to **Input Manager (Old)**, then accept Unity's restart prompt. This matches the code and `StandaloneInputModule`.
4. Create a genuine URP asset with Unity's editor tooling: in the Project window, use **Assets > Create > Rendering > URP Asset (with Universal Renderer)** (menu wording can vary slightly by 6.3 patch), and save it under `Assets/Settings/` (create that folder if needed). Alternatively use Unity's URP setup/conversion wizard to create the asset. Do not create the file by hand.
5. Assign that created asset in **Edit > Project Settings > Graphics > Default Render Pipeline**. In **Project Settings > Quality**, assign the same asset to the active quality level (and any other quality levels you intend to use). Confirm the active level has a non-null render pipeline asset. The procedural materials look up `Universal Render Pipeline/Lit` and intentionally fall back to `Standard` if URP is unavailable.
6. Open **`Assets/Scenes/Gallery.unity`** from the Project window. It is already enabled in **File > Build Profiles** (or **Build Settings**, depending on the 6.3 patch) as the game's scene. If Unity prompts to upgrade assets, make a backup/commit first and review the changes.
7. For the target Intel Iris Xe laptop, begin with a **16:9 Game view around 1280×720 or 1600×900**, the default/medium URP quality, one realtime directional light, and no added post-processing or renderer features. The scene is lightweight runtime greybox geometry with modest lighting; prefer a comfortable native display resolution if performance is good. No resolution or quality profile is currently serialized in this snapshot.
8. Press **Play** in the Editor with the Game view focused. Click once in the Game view if needed to lock the mouse. Use the controls below and the manual checklist to verify the session.

## Current playable features (Milestones 1–4)

`GalleryPrototype.cs` creates the dark teal gallery, first-person controller, three inspectable artifacts, contextual prompts, and the astronomical regulator. `ClockworkPuzzle.cs` supplies three independently rotatable rings and the deterministic solution. `GalleryUI.cs` provides inspection, journal, puzzle, dialogue, choice, and pause screens. `EvidenceJournal.cs` records unique discoveries for the current Play session. `MuseumStoryProgression.cs` stores the discovery/dialogue phase and selected choice for the current scene session.

## Controls

- **WASD** — move; **mouse** — look.
- **E** — inspect a targeted artifact, or enter the clockwork puzzle when its prompt is visible.
- **J** — open/close the evidence journal. Evidence is unique and session-only; scroll the journal to review a longer list.
- **Escape** — close ordinary inspection, journal, or puzzle panels first; from normal gameplay, open pause; from pause, resume. Escape does not dismiss an active discovery, dialogue, or unanswered choice. After making a choice, Escape can leave the response screen.
- **Puzzle:** **1 / 2 / 3** select outer / middle / inner ring; **A / D** rotate one 45-degree step counterclockwise / clockwise; **R** resets an unsolved puzzle; **Escape** exits it. The solution is 2 / 5 / 1 clockwise steps from the initial marks.
- **Story sequence:** **Enter** or **Space** advances the discovery and each intercom line. At the choice, press **1** to share the record or **2** to keep it private; the visible choice cards can also be clicked. Press **Enter**, **Space**, or **Escape** on the outcome screen to return to the gallery.
- Click **Resume** to continue from pause. **Return to Main Menu** remains disabled because there is no main-menu scene.

## Manual verification checklist

### Scene and controller

- [ ] Confirm the Gallery scene opens without missing script references or import errors; the Console has no errors.
- [ ] Enter Play Mode. Verify the greybox gallery appears, WASD moves, mouse look works, and the cursor is released/relocked appropriately when opening/closing a panel.
- [ ] Inspect each artifact with **E**, close with **Escape**, and verify its evidence appears only once in the journal (**J**).
- [ ] Verify **Escape** opens pause and resumes, and movement/mouse look work after resuming.

### Clockwork and discovery

- [ ] Inspect the astronomical model; confirm its journal clue gives North Star, Crescent, Morning Star outside-in.
- [ ] Approach the display; verify the **E** prompt, puzzle objective, current ring state, selection feedback, and controls.
- [ ] Select rings with **1 / 2 / 3** and rotate with **A / D**. Confirm discrete steps and wrap; an incorrect configuration remains unsolved and reports dark-indicator feedback.
- [ ] Change positions and press **R**; verify all three return to their start marks.
- [ ] Enter outer 2, middle 5, inner 1 clockwise steps. Verify the indicator lights, archive plaque appears, and **The Unregistered Day** discovery panel triggers once.
- [ ] Verify the full fragment appears once in the journal. Reopen the solved puzzle; confirm it does not replay the discovery or duplicate evidence.

### Dialogue, choice, and control restoration

- [ ] From the discovery panel, advance through each exchange with **Enter / Space**. Verify speaker labels and the full exchange with Dr. Elara Voss.
- [ ] Press **Escape** during discovery/dialogue; confirm the scene does not silently discard the sequence or unanswered choice.
- [ ] At the choice, use **1** and verify the shared-record response; restart Play mode, solve the puzzle again, and use **2** to verify the private response. Also verify each visible card works with the mouse.
- [ ] Open the journal after each branch. Confirm it records exactly one `The Unregistered Day` clue and one `Noah's answer to Dr. Voss` entry describing the selected branch and response.
- [ ] Verify the response screen returns to gameplay using its button, **Enter / Space**, or **Escape**. Confirm mouse look and movement resume, and reopening the solved puzzle does not restart the sequence.
- [ ] Recheck artifact inspection, journal toggle, pause/resume, and puzzle close controls.

## Scope and known limitations

Solving the clockwork reveals **ARCHIVE FRAGMENT 07 — THE UNREGISTERED DAY**: the record predates the event it describes, its original author was removed, and a later note connects Noah to a room before its doors opened. A small amber-lit plaque appears beside the regulator. Dr. Elara Voss calls over a text-only intercom. Noah can either share the inscription (Elara warns that archive dates were corrected) or withhold it (she ends the call; Noah resolves to investigate alone). The choice is held by `MuseumStoryProgression` for the current scene session and logged in the journal. No consequences in future chapters are implemented or claimed.

The rings and plaque use lightweight Unity primitives/procedural geometry. The project has no external art/audio, main menu, persistent save system, additional chapter, or ending. URP must be assigned in the editor as above; the asset has not been generated or validated here. Unity Editor and a C# compiler were not available in the repair environment, so package resolution, Unity compilation, scene import, and Play Mode remain **unverified**. Complete the checklist locally before treating the project as validated.
