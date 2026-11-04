using UnityEngine;

/*
    Put on a distraction that is a child of a station (an object with a StationInteractable, like BinStation
    or CasinoStation). When the player uses it, the station records it in StationTracker.
    It listens to the components already on the object, so nothing else needs to call it:
    - Pickable: picking the object up counts
    - Interactable: pressing it counts (only the player's click, not the slot machines' auto play)
*/

public class TrackedObject : MonoBehaviour
{
    private StationInteractable station;

    void Awake()
    {
        // Outside a station (e.g. the piano in Room2, or a sandbox scene), the object just isn't tracked
        station = GetComponentInParent<StationInteractable>();
        if (station == null)
        {
            return;
        }

        if (TryGetComponent(out Pickable pickable))
        {
            pickable.OnPickedUp += _ => station.OnPlayerInteracted();
        }

        if (TryGetComponent(out Interactable interactable))
        {
            interactable.onPressed.AddListener(station.OnPlayerInteracted);
        }
    }
}
