using UnityEngine;

/// <summary>Session story-state for the first clockwork discovery and Elara Voss's one-time choice.</summary>
public sealed class MuseumStoryProgression : MonoBehaviour
{
    public enum Phase { Dormant, Discovery, Dialogue, Choice, Outcome, Complete }
    public enum PlayerChoice { Unanswered, SharedRecord, KeptPrivate }

    private static readonly string[] Speakers =
    {
        "DR. ELARA VOSS  ·  INTERCOM",
        "NOAH VALE",
        "DR. ELARA VOSS  ·  INTERCOM",
        "DR. ELARA VOSS  ·  INTERCOM"
    };
    private static readonly string[] Lines =
    {
        "Noah? The mechanism was not meant to be opened.",
        "Then why leave the clue where I could find it?",
        "Because some records become more dangerous when people believe they understand them.",
        "The fragment names an author the catalogue erased. Tell me what it says."
    };

    public Phase CurrentPhase { get; private set; } = Phase.Dormant;
    public PlayerChoice ChoiceMade { get; private set; } = PlayerChoice.Unanswered;
    public int DialogueIndex { get; private set; }

    public const string DiscoveryTitle = "The Unregistered Day";
    public const string DiscoveryText =
        "ARCHIVE FRAGMENT 07\n\n" +
        "The record was entered one day before the event it describes. Its original author has been removed from the catalogue.\n\n" +
        "A later hand has pressed through the paper: ‘The child was present before the doors were opened.’\n" +
        "The date field is crossed out. Beneath it, almost erased: NOAH VALE.";

    public bool TryBeginDiscovery()
    {
        if (CurrentPhase != Phase.Dormant) return false;
        CurrentPhase = Phase.Discovery;
        return true;
    }

    public bool Advance()
    {
        if (CurrentPhase == Phase.Discovery)
        {
            CurrentPhase = Phase.Dialogue;
            DialogueIndex = 0;
            return true;
        }
        if (CurrentPhase == Phase.Dialogue)
        {
            if (DialogueIndex < Lines.Length - 1) DialogueIndex++;
            else CurrentPhase = Phase.Choice;
            return true;
        }
        if (CurrentPhase == Phase.Outcome)
        {
            CurrentPhase = Phase.Complete;
            return true;
        }
        return false;
    }

    public bool Choose(PlayerChoice choice)
    {
        if (CurrentPhase != Phase.Choice || ChoiceMade != PlayerChoice.Unanswered ||
            (choice != PlayerChoice.SharedRecord && choice != PlayerChoice.KeptPrivate)) return false;
        ChoiceMade = choice;
        CurrentPhase = Phase.Outcome;
        return true;
    }

    public string CurrentSpeaker => Speakers[Mathf.Clamp(DialogueIndex, 0, Speakers.Length - 1)];
    public string CurrentLine => Lines[Mathf.Clamp(DialogueIndex, 0, Lines.Length - 1)];

    public string ChoiceTitle => ChoiceMade == PlayerChoice.SharedRecord ? "A RECORD SHARED" : "A RECORD WITHHELD";

    public string OutcomeText
    {
        get
        {
            if (ChoiceMade == PlayerChoice.SharedRecord)
                return "NOAH  ·  ‘It says the author was removed. It names me as the child in the room.’\n\n" +
                       "DR. VOSS  ·  ‘Then do not trust the dates in this archive. Some were corrected after the doors opened.’\n\n" +
                       "The line goes quiet before Noah can ask who changed them.";
            if (ChoiceMade == PlayerChoice.KeptPrivate)
                return "NOAH  ·  ‘Nothing conclusive. The plate was blank.’\n\n" +
                       "DR. VOSS  ·  ‘Then let it remain blank, for tonight.’ The channel clicks off.\n\n" +
                       "Noah keeps the fragment to himself. The catalogue's missing author is now a question he must pursue alone.";
            return string.Empty;
        }
    }

    public GalleryClue CreateDecisionEvidence()
    {
        var evidenceObject = new GameObject("Story decision evidence");
        var clue = evidenceObject.AddComponent<GalleryClue>();
        clue.title = "Noah's answer to Dr. Voss";
        clue.description = ChoiceMade == PlayerChoice.SharedRecord
            ? "Decision: Noah shared Archive Fragment 07 with Dr. Elara Voss. She warned him that dates in the archive were corrected after the doors opened, then ended the call before he could ask who changed them."
            : "Decision: Noah kept Archive Fragment 07 private and told Dr. Elara Voss he found nothing conclusive. She accepted the answer and ended the call. Noah will investigate the catalogue's missing author alone.";
        return clue;
    }
}
