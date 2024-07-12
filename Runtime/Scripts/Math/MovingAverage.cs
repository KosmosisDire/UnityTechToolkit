using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovingAverage
{
    Queue<float> samples;
    float[] sampleArray;

    int maxSamples;
    float average;

    public float Value
    {
        get { return average; }
    }

    public int SamplesTaken
    {
        get { return samples.Count; }
    }

    public int MaxSamples
    {
        get { return maxSamples; }
    }

    public bool IsFull
    {
        get { return samples.Count == maxSamples; }
    }


    public MovingAverage(int maxSamples)
    {
        Reset(maxSamples);
    }

    public float AddSample(float sample)
    {
        if(!IsFull)
        {
            sampleArray[SamplesTaken] = sample;
            samples.Enqueue(sample);

            average = sampleArray.Sum() / SamplesTaken;
        }
        else
        {
            average -= samples.Dequeue() / maxSamples;
            samples.Enqueue(sample);
            average += sample / maxSamples;
        }

        return average;
    }

    public void Clear()
    {
        samples.Clear();
        sampleArray = new float[maxSamples];
        average = 0;
    }

    public void Reset(int sampleCount)
    {
        samples = new Queue<float>(sampleCount);
        sampleArray = new float[sampleCount];
        maxSamples = sampleCount;
        average = 0;
    }
}


public class MovingAverageVector3
{
    MovingAverage x;
    MovingAverage y;
    MovingAverage z;

    public Vector3 Value
    {
        get { return new Vector3(x.Value, y.Value, z.Value); }
    }

    public int SamplesTaken
    {
        get { return x.SamplesTaken; }
    }

    public int MaxSamples
    {
        get { return x.MaxSamples; }
    }

    public bool IsFull
    {
        get { return x.IsFull; }
    }

    public MovingAverageVector3(int maxSamples)
    {
        x = new MovingAverage(maxSamples);
        y = new MovingAverage(maxSamples);
        z = new MovingAverage(maxSamples);
    }

    public Vector3 AddSample(Vector3 sample)
    {
        x.AddSample(sample.x);
        y.AddSample(sample.y);
        z.AddSample(sample.z);

        return Value;
    }

    public void Clear()
    {
        x.Clear();
        y.Clear();
        z.Clear();
    }

    public void Reset(int sampleCount)
    {
        x.Reset(sampleCount);
        y.Reset(sampleCount);
        z.Reset(sampleCount);
    }

}

public class MovingAverageRotation
{
    Quaternion average;
    int _samplesTaken;
    int _maxSamples;

    public Quaternion Value
    {
        get { return average; }
    }

    public Vector3 EulerValue
    {
        get { return average.eulerAngles; }
    }

    public int SamplesTaken
    {
        get { return _samplesTaken; }
        private set { _samplesTaken = value; }
    }

    public int MaxSamples
    {
        get { return _maxSamples; }
        private set { _maxSamples = value; }
    }

    public bool IsFull
    {
        get { return SamplesTaken == MaxSamples; }
    }

    public MovingAverageRotation(int maxSamples)
    {
        Reset(maxSamples);
    }

    public float AddSample(Quaternion sample)
    {
        average = Quaternion.Lerp(average, sample, 1f / (SamplesTaken + 1));
        if(!IsFull) SamplesTaken++;
        return average.eulerAngles.y;
    }

    public void Clear()
    {
        average = Quaternion.identity;
        SamplesTaken = 0;
    }

    public void Reset(int sampleCount)
    {
        MaxSamples = sampleCount;
        Clear();
    }
}
