﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Entity : MonoBehaviour {

	Agent testAgent;
	public float currentAgentFitness;
	public float bestFitness;
    public float overallBestFitness;

    public int numberOfHiddenLayers = 1;
    public int neuronsPerHiddenLayer = 8;

	public GA genAlg;
	public GameObject[] CPs;
	public Material normal;

	private Vector3 defaultpos;
	private Quaternion defaultrot;

    private bool fullyTrained = false;
    private Genome bestGenome;

	public void OnGUI(){
		int x = 0;
		int y = 0;
		GUI.Label (new Rect (x, y, 200, 20), "CurrentFitness: " + currentAgentFitness);
		GUI.Label (new Rect (x, y+20, 200, 20), "Current gen best: " + bestFitness);
        GUI.Label(new Rect(x, y + 40, 200, 20), "Overall best fitness: " + overallBestFitness);
        GUI.Label(new Rect(x + 200, y, 200, 20), "Genome: " + genAlg.currentGenome + " of " + genAlg.totalPopulation);
		GUI.Label (new Rect (x+200, y + 20, 200, 20), "Generation: " + genAlg.generation);
        if(fullyTrained)
        {
            GUI.Label(new Rect(x + 200, y + 40, 200, 20), "Fully trained");
        }

	}

	// Use this for initialization
	void Start () {
        var raycast = GetComponent<RayCast>();

		genAlg = new GA (raycast.rayCount, numberOfHiddenLayers, neuronsPerHiddenLayer, 2);

		genAlg.GenerateNewPopulation (15);
		currentAgentFitness = 0.0f;
		bestFitness = 0.0f;
        overallBestFitness = 0.0f;

		Genome genome = genAlg.GetNextGenome ();
		testAgent = gameObject.GetComponent<Agent>();
		testAgent.Attach (genome);

		defaultpos = transform.position;
		defaultrot = transform.rotation;
	}

	// Update is called once per frame
	void Update () {
        if (fullyTrained)
            return;
		if (testAgent.hasFailed) {
			if(genAlg.GetCurrentGenomeIndex() == genAlg.GetTotalPopulation())
            {
                if (bestFitness < overallBestFitness)
                {
                    fullyTrained = true;
                    SetTestSubject(bestGenome, true);
                    return;
                }
                overallBestFitness = bestFitness;
                bestGenome = new Genome(genAlg.GetBestGenomes(1)[0], -1);    // Take a copy...
                EvolveGenomes();
				return;
			}
			NextTestSubject();
		}
		currentAgentFitness = testAgent.GetFitness();
		if (currentAgentFitness > bestFitness) {
			bestFitness = currentAgentFitness;
		}
	}

	public void NextTestSubject(){
		genAlg.SetGenomeFitness (currentAgentFitness, genAlg.GetCurrentGenomeIndex ());
		Genome genome = genAlg.GetNextGenome ();
        SetTestSubject(genome);
	}

    public void EvolveGenomes()
    {
        genAlg.BreedPopulation();
        bestFitness = 0.0f;
        Genome genome = genAlg.GetNextGenome();
        SetTestSubject(genome);
    }

    private void SetTestSubject(Genome genome, bool selfDrive = false)
    {
        currentAgentFitness = 0.0f;

        transform.position = defaultpos;
        transform.rotation = defaultrot;

        Transform root = this.transform;
        while (root.parent != null) {
            root = root.parent;
        }
        root.BroadcastMessage("Restart");

        testAgent.Attach(genome, selfDrive);

        //reset the checkpoints
        CPs = GameObject.FindGameObjectsWithTag("Checkpoint");

        foreach (GameObject c in CPs)
        {
            Renderer tmp = c.gameObject.GetComponent<Renderer>();
            tmp.material = normal;
            Checkpoint p = c.gameObject.GetComponent<Checkpoint>();
            if (p)
            {
                p.passed = false;
            }
        }
    }
}
