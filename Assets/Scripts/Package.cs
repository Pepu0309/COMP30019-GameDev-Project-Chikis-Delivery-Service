using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour
{
    private LevelsController lc;

    // Start is called before the first frame update
    void Start()
    {
        lc = GameObject.Find("LevelController").GetComponent<LevelsController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other) {
        GoalZone goalZone = other.gameObject.GetComponent<GoalZone>();
        if (goalZone != null && goalZone.levelComplete) {
            lc.CompleteLevel();
        }
    }
}
