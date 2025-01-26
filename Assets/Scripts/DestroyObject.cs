using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float destroyDuration = 1;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destroyDuration);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
