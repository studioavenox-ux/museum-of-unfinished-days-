using System.Collections.Generic;
using UnityEngine;

/// <summary>Session evidence ledger. Ordered, unique discoveries survive UI close/reopen.</summary>
public sealed class EvidenceJournal : MonoBehaviour
{
    private readonly List<GalleryClue> entries = new List<GalleryClue>();
    private readonly HashSet<string> ids = new HashSet<string>();

    public IReadOnlyList<GalleryClue> Entries => entries;

    public bool Record(GalleryClue clue)
    {
        if (clue == null || !ids.Add(clue.title)) return false;
        entries.Add(clue);
        return true;
    }
}
