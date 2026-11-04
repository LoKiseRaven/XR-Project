using System.Collections.Generic;
using UnityEngine;

//track which stations were interacted with
public static class StationTracker
{
    // Stores unique "StationName_RoomName" string keys
    private static readonly HashSet<string> RecordedInteractions = new();

    public static void RecordInteraction(string stationName, string roomName)
    {
        string key = $"{stationName}_{roomName}";
        if (RecordedInteractions.Add(key))
        {
            Debug.Log($"[Analytics] First interaction with {stationName} in {roomName}");
        }
    }
    public static bool DidInteractInRoom(string stationName, string roomName)
    {
        return RecordedInteractions.Contains($"{stationName}_{roomName}");
    }

 
    public static bool DidReinteractInRoom2(string stationName)
    {
        return DidInteractInRoom(stationName, "Room1") && DidInteractInRoom(stationName, "Room2");
    }

    // Returns how many unique stations from a list were interacted with in AT LEAST ONE room.
    public static int GetUniqueStationsInteractedCount(IEnumerable<string> stationList)
    {
        int count = 0;
        foreach (var station in stationList)
        {
            // If interacted in Room1 or Room2
            if (DidInteractInAnyRoom(station))
            {
                count++;
            }
        }
        return count;
    }

    private static bool DidInteractInAnyRoom(string stationName)
    {
        foreach (var key in RecordedInteractions)
        {
            if (key.StartsWith($"{stationName}_")) return true;
        }
        return false;
    }

    public static void ResetTracker()
    {
        RecordedInteractions.Clear();
    }
}