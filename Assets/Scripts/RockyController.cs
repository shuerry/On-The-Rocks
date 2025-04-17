using UnityEngine;

public class RockyController : MonoBehaviour
{
    public Transform targetLocation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PutRockyBack()
    {
        transform.position = targetLocation.position;
    }
}
