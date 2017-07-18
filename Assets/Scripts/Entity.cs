﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Entity : MonoBehaviour {

	Agent testAgent;
	//private List<Agent> agents;
	public float currentAgentFitness;
	public float bestFitness;
    public float overallBestFitness;
    //private float currentTimer;
	//private int checkPointsHit;

	public NNet neuralNet;

	public GA genAlg;
	public int checkpoints;
	public GameObject[] CPs;
	public Material normal;

	private Vector3 defaultpos;
	private Quaternion defaultrot;

    private bool fullyTrained = false;
    private Genome bestGenome;

	hit hit;

	public void OnGUI(){
		int x = 600;
		int y = 400;
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

		genAlg = new GA ();
		int totalWeights = 5 * 8 + 8 * 2 + 8 + 2;
		genAlg.GenerateNewPopulation (15, totalWeights);
		currentAgentFitness = 0.0f;
		bestFitness = 0.0f;
        overallBestFitness = 0.0f;

		neuralNet = new NNet ();
		neuralNet.CreateNet (1, 5, 8, 2);
		Genome genome = genAlg.GetNextGenome ();
		neuralNet.FromGenome (genome, 5, 8, 2);

		testAgent = gameObject.GetComponent<Agent>();
		testAgent.Attach (neuralNet);

		hit = gameObject.GetComponent<hit> ();
		checkpoints = hit.checkpoints;
		defaultpos = transform.position;
		defaultrot = transform.rotation;
	}

	// Update is called once per frame
	void FixedUpdate () {
        if (fullyTrained)
            return;
		checkpoints = hit.checkpoints;
		if (testAgent.hasFailed) {
			if(genAlg.GetCurrentGenomeIndex() == 15-1)
            {
                if (bestFitness < overallBestFitness)
                {
                    fullyTrained = true;
                    SetTestSubject(bestGenome, true);
                    return;
                }
                overallBestFitness = bestFitness;
                bestGenome = new Genome(genAlg.GetBestGenome());
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
		currentAgentFitness = 0.0f;
		Genome genome = genAlg.GetNextGenome ();

        SetTestSubject(genome);
	}

    private void SetTestSubject(Genome genome, bool selfDrive = false)
    {
        neuralNet.FromGenome(genome, 5, 8, 2);

        transform.position = defaultpos;
        transform.rotation = defaultrot;

        testAgent.Attach(neuralNet, selfDrive);
        testAgent.ClearFailure();

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

	/*public void BreedNewPopulation(){
		genAlg.ClearPopulation ();
		int totalweights = 5 * 8 + 8 * 2 + 8 + 2;
		genAlg.GenerateNewPopulation (15, totalweights);
	}*/

	public void EvolveGenomes(){
        bestFitness = 0.0f;
        genAlg.BreedPopulation();
		NextTestSubject ();
	}

	public int GetCurrentMemberOfPopulation(){
		return genAlg.GetCurrentGenomeIndex ();
	}

	public void PrintStats(){
		//to be implemented
	}

}
