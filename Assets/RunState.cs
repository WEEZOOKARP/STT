using System;
using System.Security.Cryptography;
using UnityEngine;

public class RunState : MonoBehaviour
{
    public static RunState Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private int runSeed;
    [SerializeField] private string runIdentifier;

    private System.Random runRandom;

    public int RunSeed => runSeed;
    public string RunIdentifier => runIdentifier;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BeginNewRun(int? forcedSeed = null)
    {
        runSeed = forcedSeed ?? GenerateSeed();
        runIdentifier = Guid.NewGuid().ToString("N");
        runRandom = new System.Random(runSeed);
        Debug.Log($"RunState: new run started. Seed={runSeed}, id={runIdentifier}");
    }

    public System.Random GetRunRandom()
    {
        if (runRandom == null)
        {
            runRandom = new System.Random(runSeed);
        }

        return runRandom;
    }

    public System.Random CreateWaveRandom(int waveNumber)
    {
        int waveSeed = DeriveWaveSeed(waveNumber);
        return new System.Random(waveSeed);
    }

    private int DeriveWaveSeed(int waveNumber)
    {
        var rng = GetRunRandom();
        int waveHash = rng.Next();
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + runSeed;
            hash = hash * 31 + waveNumber;
            hash = hash * 31 + waveHash;
            if (hash == int.MinValue)
            {
                hash += 1;
            }
            return hash;
        }
    }

    private int GenerateSeed()
    {
        Span<byte> buffer = stackalloc byte[4];
        RandomNumberGenerator.Fill(buffer);
        int seed = BitConverter.ToInt32(buffer);
        if (seed == int.MinValue)
        {
            seed += 1;
        }
        return seed;
    }
}
