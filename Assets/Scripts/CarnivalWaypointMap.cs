using UnityEngine;

public class CarnivalWaypointMap : MonoBehaviour
{
    public Transform WP_Start;
    public Transform WP_Trinket;
    public Transform WP_Food;
    public Transform WP_MeetBack;

    public Transform Get(string id)
    {
        return id switch
        {
            "start" => WP_Start,
            "trinkets" => WP_Trinket,
            "food" => WP_Food,
            "meetback" => WP_MeetBack,
            _ => null
        };
    }
}

