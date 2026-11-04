using System.Text;
using TMPro;
using UnityEngine;

/*
    Shows the metrics of the run on a text. Meant for the end room: it reads them when the scene starts.
    - StationTracker: which stations the player used in each room, and if they used them again in Room 2
    - Pickable and HoopScorer: how many objects were picked up, and how many baskets were scored
*/

// A station shown on the board, with the name written on it
[System.Serializable]
public class BoardStation
{
    public string label;
    // Name recorded by its StationInteractable (the station's GameObject name by default)
    public string stationName;
}

public class ResultsBoard : MonoBehaviour
{
    [SerializeField] private TMP_Text resultsText;

    [Header("Rooms")]
    // Scene names as in the Build Settings, and how they are written on the board
    [SerializeField] private string[] roomScenes = { "Room1", "Room2" };
    [SerializeField] private string[] roomLabels = { "Room 1", "Room 2" };

    [Header("Stations")]
    // Stations detailed room by room
    [SerializeField] private BoardStation[] detailedStations =
    {
        new BoardStation { label = "Trash", stationName = "BinStation" },
        new BoardStation { label = "Slot machines", stationName = "CasinoStation" }
    };
    // Every station, to count how many different ones were used
    [SerializeField] private string[] allStations = { "BinStation", "PianoStation", "CasinoStation", "BasketBallStation", "MusicStation", "DartsScene" };

    void Start()
    {
        if (resultsText == null)
        {
            resultsText = GetComponent<TMP_Text>();
        }

        StringBuilder text = new StringBuilder();
        text.AppendLine("Results");

        foreach (BoardStation station in detailedStations)
        {
            AppendRoomLine(text, station);
        }
        AppendUsedAgainLine(text);

        int stationsUsed = StationTracker.GetUniqueStationsInteractedCount(allStations);
        text.AppendLine($"Stations used: {stationsUsed} / {allStations.Length}");
        text.AppendLine($"Objects picked up: {Pickable.TotalPickups} ({Pickable.UniqueItemsPickedCount} different)");
        text.AppendLine($"Baskets scored: {HoopScorer.TotalHoopsScored}");

        resultsText.text = text.ToString();
    }

    // For example "Trash: Room 1 yes / Room 2 no"
    void AppendRoomLine(StringBuilder text, BoardStation station)
    {
        text.Append(station.label).Append(": ");
        for (int i = 0; i < roomScenes.Length; i++)
        {
            if (i > 0)
            {
                text.Append(" / ");
            }
            text.Append(roomLabels[i]).Append(' ').Append(YesNo(StationTracker.DidInteractInRoom(station.stationName, roomScenes[i])));
        }
        text.AppendLine();
    }

    // Did the player go back to the same distractions in Room 2, after using them in Room 1?
    void AppendUsedAgainLine(StringBuilder text)
    {
        text.Append("Used again in Room 2: ");
        for (int i = 0; i < detailedStations.Length; i++)
        {
            if (i > 0)
            {
                text.Append(" / ");
            }
            text.Append(detailedStations[i].label).Append(' ').Append(YesNo(StationTracker.DidReinteractInRoom2(detailedStations[i].stationName)));
        }
        text.AppendLine();
    }

    static string YesNo(bool value)
    {
        return value ? "yes" : "no";
    }
}
