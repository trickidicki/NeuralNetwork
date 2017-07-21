using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NNet
{
    private int inputAmount;
    private int outputAmount;

    List<float> inputs = new List<float>();
    NLayer inputlayer = new NLayer(LayerType.NInput);

    List<NLayer> hiddenLayers = new List<NLayer>();
    NLayer outputLayer = new NLayer(LayerType.NOutput);

    List<float> outputs = new List<float>();


    public void refresh()
    {
        outputs.Clear();

        for (int i = 0; i < hiddenLayers.Count; i++)
        {
            if (i > 0)
            {
                inputs = outputs;
            }
            hiddenLayers[i].Evaluate(inputs, ref outputs);

        }
        inputs = outputs;
        //Process the layeroutputs through the output layer to
        outputLayer.Evaluate(inputs, ref outputs);

    }

    public void SetInput(List<float> input)
    {
        inputs = input;
    }

    public void SetWeights(List<float> weights)
    {
        int pos = 0;
        foreach(var layer in hiddenLayers)
        {
            layer.SetWeights(weights, ref pos);
        }
        outputLayer.SetWeights(weights, ref pos);
    }

    public float GetOutput(int ID)
    {
        if (ID >= outputAmount)
            return 0.0f;
        return outputs[ID];
    }

    public void CreateNet(int numOfInputs, int numOfHIddenLayers, int neuronsPerHidden, int numOfOutputs)
    {
        hiddenLayers.Clear();

        inputAmount = numOfInputs;
        outputAmount = numOfOutputs;

        inputlayer = new NLayer(LayerType.NInput);
        inputlayer.PopulateLayer(numOfInputs, 1);

        for (int i = 0; i < numOfHIddenLayers; i++)
        {
            NLayer layer = new NLayer(LayerType.NHidden);
            layer.PopulateLayer(neuronsPerHidden, numOfInputs);
            hiddenLayers.Add(layer);
            numOfInputs = neuronsPerHidden;
        }

        outputLayer = new NLayer(LayerType.NOutput);
        outputLayer.PopulateLayer(numOfOutputs, numOfInputs);
    }

    public void Mutate(float mutationRate, float maxPertebation)
    {
        // No need to mutate input layer.
        foreach (var hidden in hiddenLayers)
            hidden.Mutate(mutationRate, maxPertebation);
        outputLayer.Mutate(mutationRate, maxPertebation);
    }

    public List<float> GetNeuronWeights()
    {
        List<float> retval = new List<float>();
        foreach (var layer in hiddenLayers)
        {
            foreach (var neuron in layer.neurons)
            {
                retval.AddRange(neuron.weights);
            }
        }
        foreach (var neuron in outputLayer.neurons)
        {
            retval.AddRange(neuron.weights);
        }
        return retval;
    }
}


//=================================================================================================================
public enum LayerType { NInput, NHidden, NOutput };
public class NLayer
{
    
    //private int totalNeurons;
    private int totalInputs;
    public LayerType layerType;

    public Neuron[] neurons;

    public NLayer(LayerType type = LayerType.NHidden)
    {
        layerType = type;
        totalInputs = 0;
    }

    public float Sigmoid(float a, float p)
    {
        float ap = (-a) / p;
        return (1 / (1 + Mathf.Exp(ap)));
    }

    public float BiPolarSigmoid(float a, float p)
    {
        float ap = (-a) / p;
        return (2 / (1 + Mathf.Exp(ap)) - 1);
    }

    public void Mutate(float mutationRate, float maxPertebation)
    {
        foreach (var neuron in neurons)
            neuron.Mutate(mutationRate, maxPertebation);
    }

    public void Evaluate(List<float> input, ref List<float> output)
    {
        foreach(var neuron in neurons)
        {
            int inputIndex = 0;
            float activation = 0.0f;

            //sum the weights to the activation value
            //we do the sizeof the weights - 1 so that we can add in the bias to the activation afterwards.
            for (int j = 0; j < neuron.numInputs - 1; j++)
            {

                activation += input[inputIndex] * (neuron.weights[j]);
                inputIndex++;
            }

            //add the bias
            //the bias will act as a threshold value to
            activation += neuron.weights[neuron.numInputs] * (-1.0f);//BIAS == -1.0f

            output.Add(Sigmoid(activation, 1.0f));
        }
    }

    private Neuron[] CreateNeurons(int n)
    {
        Neuron[] neurons = new Neuron[n];
        for (int i = 0; i < n; i++)
        {
            neurons[i] = new Neuron();
        }
        return neurons;
    }

    public void PopulateLayer(int numOfNeurons, int numOfInputs)
    {
        totalInputs = numOfInputs;
        var totalNeurons = numOfNeurons;

        neurons = CreateNeurons(numOfNeurons);

        for (int i = 0; i < numOfNeurons; i++)
        {
            neurons[i].Populate(numOfInputs);
        }
    }

    public void SetWeights(List<float> weights, ref int pos)
    {
        for (int i = 0; i < neurons.Length; i++)
        {
            var neuron = neurons[i];
            int numWeights = neuron.numInputs + 1;
            neuron.Initilise(weights.GetRange(pos, numWeights), neuron.numInputs);
            pos += numWeights;
        }
    }
}


//=============================================================

public class Neuron
{
    public int numInputs;
    public float[] weights;

    public Neuron()
    {
        numInputs = 0;
        weights = new float[0];
    }

    public float RandomFloat()
    {
        // Generates a random number between 1.0 and 1.9999999f
        float rand = (float)UnityEngine.Random.Range(0.0f, 32767.0f);
        return rand / 32767.0f + 1.0f;
    }

    public float RandomClamped()
    {
        return RandomFloat() - RandomFloat();
    }

    public float Clamp(float val, float min, float max)
    {
        if (val < min)
        {
            return min;
        }
        if (val > max)
        {
            return max;
        }
        return val;
    }

    public void Populate(int num)
    {
        InitialiseEmpty(num);

        //Initilise the weights
        for (int i = 0; i < num; i++)
        {
            weights[i] = RandomClamped();
        }

        //add an extra weight as the bias (the value that acts as a threshold in a step activation).
        weights[num] = RandomClamped();
    }

    public void Mutate(float mutationRate, float maxPertebation)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            float rand = RandomFloat() - 1.0f;
            if (rand < mutationRate)
            {
                float mutant = (RandomClamped() * maxPertebation); 
                weights[i] += mutant;
            }
        }
    }

    public void Initilise(List<float> weightsIn, int num)
    {
        this.numInputs = num;
        weights = weightsIn.ToArray();
    }

    public void InitialiseEmpty(int num)
    {
        numInputs = num;
        weights = new float[num+1]; // Extra item for biasing
    }
}

public class Breeder
{
    private static bool Switch()
    {
        return UnityEngine.Random.Range(-1f, 1f) > 0;
    }

    public static void Breed(NNet parent1, NNet parent2, NNet child1, NNet child2)
    {
        var p1Enum = parent1.GetNeuronWeights().GetEnumerator();
        var p2Enum = parent2.GetNeuronWeights().GetEnumerator();

        var p1weights = new List<float>();
        var p2weights = new List<float>();
        
        var c1weights = new List<float>();
        var c2weights = new List<float>();

        while(p1Enum.MoveNext() && p2Enum.MoveNext())
        {
            p1weights.Add(p1Enum.Current);
            p2weights.Add(p2Enum.Current);

            if(Switch())
            {
                c1weights.Add(p1Enum.Current);
                c2weights.Add(p2Enum.Current);
            }
            else
            {
                c1weights.Add(p2Enum.Current);
                c2weights.Add(p1Enum.Current);
            }
        }

        child1.SetWeights(c1weights);
        child2.SetWeights(c2weights);
    }
}

//===================================
public class Genome
{
    public float fitness;
    public int ID;
    public NNet net;

    public Genome(int nInput, int nHiddenLayer, int nHiddenLayerNeurons, int nOutput, int genomeID)
    {
        net = new NNet();
        net.CreateNet(nInput, nHiddenLayer, nHiddenLayerNeurons, nOutput);
        this.ID = genomeID;
    }

    public Genome(Genome other, int ID)
    {
        this.fitness = other.fitness;
        this.ID = ID;
        this.net = other.net;
    }

    public void Mutate(float mutationRate, float maxPertebation)
    {
        net.Mutate(mutationRate, maxPertebation);
    }
}


