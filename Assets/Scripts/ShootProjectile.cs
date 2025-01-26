using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShootProjectile : MonoBehaviour
{
    public GameObject waterPrefab;

    public float shootSpeed = 50f;

    public Image reticleImage;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            GameObject projectile =
                Instantiate(waterPrefab,
                transform.position + transform.forward,
                transform.rotation) as GameObject;

            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            rb.AddForce(transform.forward * shootSpeed, ForceMode.VelocityChange);

            projectile.transform.SetParent(
                GameObject.FindGameObjectWithTag("WaterParent").transform);
        }
    }

    private void FixedUpdate()
    {
        ReticleEffect();
    }

    void ReticleEffect()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Target"))
            {
                reticleImage.transform.localScale = Vector3.Lerp(reticleImage.transform.localScale,
                    new Vector3(0.7f, 0.7f, 1), Time.deltaTime * 2);
            }
            else
            {
                reticleImage.transform.localScale = Vector3.Lerp(reticleImage.transform.localScale,
                    Vector3.one, Time.deltaTime * 2);
            }
        }
    }
}