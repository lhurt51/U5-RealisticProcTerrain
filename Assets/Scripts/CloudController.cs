using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour {

    // Public Properties ---------------------
    ParticleSystem cloudSystem;
    public Color color;
    public Color lining;
    public int numOfParticles;
    public float minSpeed;
    public float maxSpeed;
    public float dst;

    // private Properties ---------------------
    private float speed;
    private Vector3 startPos;
    private bool painted = false;

    void Spawn()
    {
        // Extend the range of the scale on either side of the manager
        float xpos = UnityEngine.Random.Range(-0.5f, 0.5f);
        float ypos = UnityEngine.Random.Range(-0.5f, 0.5f);
        float zpos = UnityEngine.Random.Range(-0.5f, 0.5f);

        this.transform.localPosition = new Vector3(xpos, ypos, zpos);
        startPos = this.transform.position;
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
    }

    // Use this for initialization
    void Start () {
        cloudSystem = this.GetComponent<ParticleSystem>();
        Spawn();
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.Translate(0, 0, speed);

        if (Vector3.Distance(this.transform.position, startPos) > dst) Spawn();
	}
}
