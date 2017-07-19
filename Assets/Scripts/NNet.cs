using UnityEngine;
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

    public float GetOutput(int ID)
    {
        if (ID >= outputAmount)
            return 0.0f;
        return outputs[ID];
    }

    public int GetTotalOutputs()
    {
        return outputAmount;
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

    public void ReleaseNet()
    {
        inputlayer = new NLayer(LayerType.NInput);
        outputLayer = new NLayer(LayerType.NOutput);
        hiddenLayers = new List<NLayer>();
    }

    public int GetNumofHIddenLayers()
    {
        return hiddenLayers.Count;
    }
    public void Mutate(float mutationRate, float maxPertebation)
    {
        // No need to mutate input layer.
        foreach (var hidden in hiddenLayers)
            hidden.Mutate(mutationRate, maxPertebation);
        outputLayer.Mutate(mutationRate, maxPertebation);
    }

    /*public Genome ToGenome()
    {
        Genome genome = new Genome(inputAmount, hiddenLayers.Count, hiddenLayers[0].NumberOfNeurons(), outputAmount, -1);
		
        for (int i=0; i<this.hiddenLayers.Count; i++) {
            List<float> weights = new List<float> ();
            hiddenLayers[i].GetWeights(ref weights);
            for(int j=0; j<weights.Count;j++){
                genome.weights.Add (weights[j]);
            }
        }
		
        List<float> outweights = new List<float> ();
        outputLayer.GetWeights(ref outweights);
        for (int i=0; i<outweights.Count; i++) {
            genome.weights.Add (outweights[i]);
        }
		
        return genome;
    }*/

    /*public void FromGenome(Genome genome)
    {
        ReleaseNet();

        outputAmount = genome.nOutput;
        inputAmount = genome.nInput;
        int neuronsPerHidden = genome.nHiddenNeurons;

        int weightsForHidden = outputAmount * neuronsPerHidden;

        int nInputs = inputAmount;

        inputlayer = new NLayer();
        //inputlayer.LoadLayer(genome.GetInputWeights())
        /// Input layer doesn't have 'weights' - the input is set directly.
        //inputlayer.SetWeights(genome.GetInputWeights(), inputAmount, nInputs);


        for (int n = 0; n < genome.nHiddenLayers; n++)
        {
            NLayer hidden = new NLayer();
            hidden.SetWeights(genome.GetHiddenWeights(n), genome.nHiddenNeurons, nInputs);
            hiddenLayers.Add(hidden);
            nInputs = genome.nHiddenNeurons;
        }
        outputLayer = new NLayer();
        outputLayer.SetWeights(genome.GetOutputWeights(), genome.nOutput, nInputs);

        /*
        //List<Neuron> neurons = new List<Neuron>();
        Neuron[] neurons = CreateNeurons(neuronsPerHidden);

        for (int i = 0; i < neurons.Length; i++)
        {
            //init
            //neurons.Add(new Neuron());
            List<float> weights = new List<float>();
            //init

            for (int j = 0; j < inputAmount + 1; j++)
            {
                weights.Add(0.0f);
                weights[j] = genome.weights[i * neuronsPerHidden + j];
            }
            neurons[i].weights = new List<float>();
            neurons[i].Initilise(weights, inputAmount);
        }
        hidden.LoadLayer(neurons);
        //Debug.Log ("fromgenome, hiddenlayer neruons#: " + neurons.Count);
        //Debug.Log ("fromgenome, hiddenlayer numInput: " + neurons [0].numInputs);
        this.hiddenLayers.Add(hidden);

        //Clear weights and reasign the weights to the output
        int weightsForOutput = neuronsPerHidden * outputAmount;
        
        //List<Neuron> outneurons = new List<Neuron>();
        Neuron[] outneurons = CreateNeurons(outputAmount);

        for (int i = 0; i < neurons.Length; i++)
        {
            //outneurons.Add(new Neuron());

            List<float> weights = new List<float>();

            for (int j = 0; j < neuronsPerHidden + 1; j++)
            {
                weights.Add(0.0f);
                weights[j] = genome.weights[i * neuronsPerHidden + j];
            }
            outneurons[i].weights = new List<float>();
            outneurons[i].Initilise(weights, neuronsPerHidden);
        }
        this.outputLayer = new NLayer();
        this.outputLayer.LoadLayer(outneurons);
        //Debug.Log ("fromgenome, outputlayer neruons#: " + outneurons.Count);
        //Debug.Log ("fromgenome, outputlayer numInput: " + outneurons [0].numInputs);
         
    }*/
}


//=================================================================================================================
public enum LayerType { NInput, NHidden, NOutput };
public class NLayer
{
    
    //private int totalNeurons;
    private int totalInputs;
    public LayerType layerType;

    private Neuron[] neurons;
    //List<Neuron> neurons = new List<Neuron>();

    public NLayer(LayerType type)
    {
        layerType = type;
        totalInputs = 0;
    }

    // Special constructor for creating cross-breeds from two parents.
    public NLayer(NLayer gen1, NLayer gen2)
    {
        layerType = gen1.layerType;
        totalInputs = gen1.totalInputs;
        neurons = new Neuron[totalInputs];
        for (int i = 0; i < gen1.neurons.Length; i++)
        {
            neurons[i] = new Neuron(gen1.neurons[i], gen2.neurons[i]);
        }
    }

    public int NumberOfNeurons()
    {
        return neurons.Length;
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
        //Debug.Log ("input.count " + input.Count);
        //Debug.Log ("totalneuron " + totalNeurons);
        //cycle over all the neurons and sum their weights against the inputs
        //for (int i = 0; i < neurons.Length; i++)
        foreach(var neuron in neurons)
        {
            int inputIndex = 0;
            float activation = 0.0f;

            //Debug.Log ("numInputs " + (neurons[i].numInputs - 1));

            //sum the weights to the activation value
            //we do the sizeof the weights - 1 so that we can add in the bias to the activation afterwards.
            for (int j = 0; j < neuron.numInputs - 1; j++)
            {

                activation += input[inputIndex] * neuron.weights[j];
                inputIndex++;
            }

            //add the bias
            //the bias will act as a threshold value to
            activation += neuron.weights[neuron.numInputs] * (-1.0f);//BIAS == -1.0f

            output.Add(Sigmoid(activation, 1.0f));
        }
    }

    public void LoadLayer(Neuron[] input)
    {
        //totalNeurons = input.Count;
        neurons = (Neuron[])input.Clone();
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
        /*if (neurons.Count < numOfNeurons)
        {
            for (int i = 0; i < numOfNeurons; i++)
            {
                neurons.Add(new Neuron());
            }
        }*/

        for (int i = 0; i < numOfNeurons; i++)
        {
            neurons[i].Populate(numOfInputs);
        }
    }

    public void SetWeights(float[] weights, int numOfNeurons, int numOfInputs)
    {
        int index = 0;
        totalInputs = numOfInputs;
        var totalNeurons = numOfNeurons;

        neurons = CreateNeurons(numOfNeurons);
        /*if (neurons.Count < numOfNeurons)
        {
            for (int i = 0; i < numOfNeurons - neurons.Count; i++)
            {
                neurons.Add(new Neuron());
            }
        }*/
        //Copy the weights into the neurons.
        for (int i = 0; i < numOfNeurons; i++)
        {
            neurons[i].InitialiseEmpty(numOfInputs);
            /*if (neurons[i].weights.Count < numOfInputs)
            {
                for (int k = 0; k < numOfInputs - neurons[i].weights.Count; k++)
                {
                    neurons[i].weights.Add(0.0f);
                }
            }*/
            for (int j = 0; j < numOfInputs; j++)
            {
                neurons[i].weights[j] = weights[index];
                index++;
            }
        }
    }

    public void GetWeights(ref List<float> output)
    {
        //Calculate the size of the output list by calculating the amount of weights in each neurons.
        output.Clear();

        for (int i = 0; i < neurons.Length; i++)
        {
            output.AddRange(neurons[i].weights);
            /*for (int j = 0; j < neurons[i].weights.Length; j++)
            {
                output[neurons.Length * i + j] = neurons[i].weights[j];
            }*/
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

    // Constructor for creating cross-breeds from two parents
    public Neuron(Neuron gen1, Neuron gen2)
    {
        InitialiseEmpty(gen1.numInputs);

        int crossover = (int)Random.Range(0, gen1.numInputs - 1);
        for (int i = 0; i < crossover; i++)
        {
            weights[i] = gen1.weights[i];
        }
        for (int i = crossover; i < gen1.weights.Length; i++)
        {
            weights[i] = gen2.weights[i];
        }
    }

    public float RandomFloat()
    {
        float rand = (float)Random.Range(0.0f, 32767.0f);
        return rand / 32767.0f/*32767*/ + 1.0f;
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
            if (RandomClamped() < mutationRate)
            {
                weights[i] += (RandomClamped() * maxPertebation);
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

//===================================
public class Genome
{
    public float fitness;
    public int ID;
    public NNet net;
    //public float[] weights;

    //public int nInput, nHiddenLayers, nHiddenNeurons, nOutput;

    //public Genome() { }

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

    /*public Genome(int nInput, int nHiddenLayer, int nHiddenLayerNeurons, int nOutput, int genomeID)
    {
        this.nInput = nInput;
        this.nHiddenLayers = nHiddenLayer;
        this.nHiddenNeurons = nHiddenLayerNeurons;
        this.nOutput = nOutput;
        weights = new float[NumberOfWeights()];
        fitness = 0f;
        ID = genomeID;
    }

    public Genome(Genome other)
    {
        fitness = other.fitness;
        ID = other.ID;
        nInput = other.nInput;
        nHiddenLayers = other.nHiddenLayers;
        nHiddenNeurons = other.nHiddenNeurons;
        nOutput = other.nOutput;
        weights = (float[]) other.weights.Clone();
    }

    public int NumberOfWeights()
    {
        int nCount = 0;
        int nPreviousLayer = nInput;

        for(int i = 0; i < nHiddenLayers; i++)
        {
            nCount += nPreviousLayer * nHiddenNeurons;
            nPreviousLayer = nHiddenNeurons;
        }

        nCount += nPreviousLayer * nOutput;
        return nCount;
    }

    public void Mutate(float mutationRate, float maxPertebation)
    {
        int nWeights = NumberOfWeights();
        for (int i = 0; i < nWeights; i++)
        {
            if (RandomClamped() < mutationRate)
            {
                weights[i] += (RandomClamped() * maxPertebation);
            }
        }
    }

    private float[] ExtractWeights(int nStart, int nCount)
    {
        float[] retval = new float[nCount];
        System.Array.Copy(weights, nStart, retval, 0, nCount);
        return retval;
    }

    public float[] GetHiddenWeights(int nLayer)
    {
        int nOffset = 0;
        int nCount = nInput;
        for (int i = 0; i < nLayer; i++)
        {
            nOffset += nCount;
            nCount = nHiddenNeurons;
        }
        int offset = nLayer * nHiddenNeurons;
        return ExtractWeights(offset, nHiddenNeurons);
    }

    public float[] GetOutputWeights()
    {
        int offset = nHiddenLayers * nHiddenNeurons;
        return ExtractWeights(offset, nOutput);
    }

    private float RandomFloat()
    {
        float rand = (float)Random.Range(0.0f, 32767.0f);
        return rand / 32767.0f/*32767* / + 1.0f;
    }

    private float RandomClamped()
    {
        return RandomFloat() - RandomFloat();
    }
     */
}


