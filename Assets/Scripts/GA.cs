using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GA
{

    public int currentGenome;
    public int totalPopulation;
    private int genomeID;
    public int generation;

    private int nInputs, nHiddenLayers, nHiddenNeurons, nOutput;

    public float MUTATION_RATE;
    public float MAX_PERBETUATION;

    public List<Genome> population = new List<Genome>();

    public GA(int nInput, int nHiddenLayers, int nHiddenNeurons, int nOutput)
    {
        this.nInputs = nInput;
        this.nHiddenLayers = nHiddenLayers;
        this.nHiddenNeurons = nHiddenNeurons;
        this.nOutput = nOutput;

        this.currentGenome = -1;
        this.totalPopulation = 0;
        genomeID = 0;
        generation = 1;
        MUTATION_RATE = 0.15f;
        MAX_PERBETUATION = 0.3f;
    }

    public Genome GetNextGenome()
    {
        currentGenome++;
        if (currentGenome >= population.Count)
            return null;

        return population[this.currentGenome];
    }

    public List<Genome> GetBestGenomes(int count = 1)
    {
        SortPopulation();
        return population.GetRange(0, count);
    }

    public List<Genome> GetWorstGenomes(int count = 1)
    {
        SortPopulation();
        population.Reverse();
        return population.GetRange(0, count);
    }

    public Genome GetGenome(int index)
    {
        if (index >= totalPopulation)
            return null;
        return population[index];
    }

    public int GetCurrentGenomeIndex()
    {
        return currentGenome;
    }

    public int GetCurrentGenomeID()
    {
        return population[currentGenome].ID;
    }

    public int GetCurrentGeneration()
    {
        return generation;
    }

    public int GetTotalPopulation()
    {
        return totalPopulation;
    }
    private int GetNextGenomeID()
    {
        return genomeID++;
    }

    private void SortPopulation()
    {
        population = population.OrderByDescending(x => x.fitness).ToList();
    }

    private bool RandBool()
    {
        return Random.value > 0.5f ? true : false;
    }

    public void CrossBreed(Genome g1, Genome g2, ref Genome baby1, ref Genome baby2)
    {
        baby1 = new Genome(g1, GetNextGenomeID());
        baby2 = new Genome(g2, GetNextGenomeID());

        Breeder.Breed(g1.net, g2.net, baby1.net, baby2.net);
    }

    public Genome CreateNewGenome()
    {
        return new Genome(nInputs, nHiddenLayers, nHiddenNeurons, nOutput, GetNextGenomeID());
    }

    public void GenerateNewPopulation(int totalPop)
    {
        generation = 1;
        ClearPopulation();
        currentGenome = -1;
        totalPopulation = totalPop;
        //resize
        if (population.Count < totalPop)
        {
            for (int i = population.Count; i < totalPop; i++)
            {
                Genome genome = CreateNewGenome();
                Mutate(genome);
                population.Add(genome);
            }
        }
    }

    public void BreedPopulation()
    {
        //find the 4 best genomes
        List<Genome> bestGenomes = GetBestGenomes(4);

        //Breed them with each other twice to form 3*2 + 2*2 + 1*2 = 12 children
        List<Genome> children = new List<Genome>();

        //Carry on the best
        Genome best = new Genome(bestGenomes[0], bestGenomes[0].ID);
        best.fitness = 0.0f;
        //best.ID = bestGenomes[0].ID;
        //best.weights = bestGenomes[0].weights;
        //Mutate(best);
        children.Add(best);

        //Child genomes
        Genome baby1 = CreateNewGenome();//new Genome();
        Genome baby2 = CreateNewGenome(); //new Genome();

        // Breed with genome 0.
        CrossBreed(bestGenomes[0], bestGenomes[1], ref baby1, ref baby2);
        Mutate(baby1);
        Mutate(baby2);
        children.Add(baby1);
        children.Add(baby2);
        CrossBreed(bestGenomes[0], bestGenomes[2], ref baby1, ref baby2);
        Mutate(baby1);
        Mutate(baby2);
        children.Add(baby1);
        children.Add(baby2);
        CrossBreed(bestGenomes[0], bestGenomes[3], ref baby1, ref baby2);
        Mutate(baby1);
        Mutate(baby2);
        children.Add(baby1);
        children.Add(baby2);

        // Breed with genome 1.
        CrossBreed(bestGenomes[1], bestGenomes[2], ref baby1, ref baby2);
        Mutate(baby1);
        Mutate(baby2);
        children.Add(baby1);
        children.Add(baby2);
        CrossBreed(bestGenomes[1], bestGenomes[3], ref baby1, ref baby2);
        Mutate(baby1);
        Mutate(baby2);
        children.Add(baby1);
        children.Add(baby2);

        //For the remainding n population, add some random
        int remainingChildren = (totalPopulation - children.Count);
        for (int i = 0; i < remainingChildren; i++)
        {
            children.Add(CreateNewGenome());
        }

        ClearPopulation();
        population = children;
        currentGenome = 0;
        generation++;
    }

    public void ClearPopulation()
    {
        for (int i = 0; i < population.Count; i++)
        {
            if (population[i] != null)
            {
                population[i] = null;
            }
        }
        population.Clear();
    }

    public void Mutate(Genome genome)
    {
        genome.Mutate(MUTATION_RATE, MAX_PERBETUATION);
    }

    public void SetGenomeFitness(float fitness, int index)
    {
        if (index >= population.Count)
        {
            return;
        }
        else
        {
            population[index].fitness = fitness;
        }
    }
}
