using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public Genome GetBestGenome()
    {
        int bestGenome = -1;
        float fitness = 0;
        for (int i = 0; i < population.Count; i++)
        {
            fitness = population[i].fitness;
            bestGenome = i;
        }

        return population[bestGenome];
    }

    public Genome GetWorstGenome()
    {
        int worstGenome = -1;
        float fitness = 1000000.0f;
        for (int i = 0; i < population.Count; i++)
        {
            if (population[i].fitness < fitness)
            {
                fitness = population[i].fitness;
                worstGenome = i;
            }
        }

        return population[worstGenome];
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

    public void GetBestCases(int totalGenomes, ref List<Genome> output)
    {
        int genomeCount = 0;
        int runCount = 0;

        while (genomeCount < totalGenomes)
        {
            if (runCount > 10)
                return;

            runCount++;

            //Find the best cases for cross breeding based on fitness score
            float bestFitness = 0;
            int bestIndex = -1;
            for (int i = 0; i < this.totalPopulation; i++)
            {
                if (population[i].fitness > bestFitness)
                {
                    bool isUsed = false;

                    for (int j = 0; j < output.Count; j++)
                    {
                        if (output[j].ID == population[i].ID)
                        {
                            isUsed = true;
                        }
                    }

                    if (isUsed == false)
                    {
                        bestIndex = i;
                        bestFitness = population[bestIndex].fitness;
                    }
                }
            }

            if (bestIndex != -1)
            {
                genomeCount++;
                output.Add(population[bestIndex]);
            }
        }
    }

    private bool RandBool()
    {
        return Random.value > 0.5f ? true : false;
    }

    public void CrossBreed(Genome g1, Genome g2, ref Genome baby1, ref Genome baby2)
    {
        baby1 = new Genome(g1, GetNextGenomeID());
        baby2 = new Genome(g2, GetNextGenomeID());



        /*int totalWeights = g1.NumberOfWeights();
        int crossover = (int)Random.Range(0, totalWeights - 1);

        baby1 = new Genome(g1);
        baby1.ID = GetNextGenomeID();

        baby2 = new Genome(g2);
        baby2.ID = GetNextGenomeID();

        //Go from start to crossover point, copying the weights from g1
        for (int i = 0; i < crossover; i++)
        {
            baby1.weights[i] = g1.weights[i];
            baby2.weights[i] = g2.weights[i];
        }

        for (int i = crossover; i < totalWeights; i++)
        {
            baby1.weights[i] = g2.weights[i];
            baby2.weights[i] = g1.weights[i];
        }*/
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

        /*for (int i = 0; i < population.Count; i++)
        {
            Genome genome = new Genome();
            genome.ID = GetNextGenomeID();
            genome.weights = new List<float>();
            //resize
            for (int k = 0; k < totalWeights; k++)
            {
                genome.weights.Add(RandomClamped());
            }

            population[i] = genome;
        }*/
    }

    public void BreedPopulation()
    {
        List<Genome> bestGenomes = new List<Genome>();

        //find the 4 best genomes
        this.GetBestCases(4, ref bestGenomes);

        //Breed them with each other twice to form 3*2 + 2*2 + 1*2 = 12 children
        List<Genome> children = new List<Genome>();

        //Carry on the best
        Genome best = new Genome(bestGenomes[0]);
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
