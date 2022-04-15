using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FitToWaterSurface_Burst_2023 : MonoBehaviour
{
    // Public parameters
    public int resolution = 50;
    public WaterSurface waterSurface = null;
    public bool includeDeformation = true;
    public float currentSpeedMultiplier = 1.0f;

    // List of internal cubes
    List<GameObject> cubes = new List<GameObject>();

    // Input job parameters
    NativeArray<float3> targetPositionBuffer;

    // Output job parameters
    NativeArray<float> errorBuffer;
    NativeArray<float3> candidatePositionBuffer;
    NativeArray<float3> projectedPositionWSBuffer;
    NativeArray<float3> directionBuffer;
    NativeArray<int> stepCountBuffer;

    // Start is called before the first frame update
    void Start()
    {
        // Allocate the buffers
        targetPositionBuffer = new NativeArray<float3>(resolution * resolution, Allocator.Persistent);
        errorBuffer = new NativeArray<float>(resolution * resolution, Allocator.Persistent);
        candidatePositionBuffer = new NativeArray<float3>(resolution * resolution, Allocator.Persistent);
        projectedPositionWSBuffer = new NativeArray<float3>(resolution * resolution, Allocator.Persistent);
        stepCountBuffer = new NativeArray<int>(resolution * resolution, Allocator.Persistent);
        directionBuffer = new NativeArray<float3>(resolution * resolution, Allocator.Persistent);

        for (int y = 0; y < resolution; ++y)
        {
            for (int x = 0; x < resolution; ++x)
            {
                GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newCube.transform.parent = this.transform;
                newCube.transform.localPosition = new Vector3(x * 5, 0.0f, y * 5);
                cubes.Add(newCube);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (waterSurface == null)
            return;
        // Try to get the simulation data if available
        WaterSimSearchData simData = new WaterSimSearchData();
        if (!waterSurface.FillWaterSearchData(ref simData))
            return;

        // Fill the input positions
        int numElements = resolution * resolution;
        for (int i = 0; i < numElements; ++i)
            targetPositionBuffer[i] = cubes[i].transform.position;

        // Prepare the first band
        WaterSimulationSearchJob searchJob = new WaterSimulationSearchJob();

        // Assign the simulation data
        searchJob.simSearchData = simData;

        // Fill the input data
        searchJob.targetPositionWSBuffer = targetPositionBuffer;
        searchJob.startPositionWSBuffer = targetPositionBuffer;
        searchJob.maxIterations = 8;
        searchJob.error = 0.01f;
        searchJob.includeDeformation = includeDeformation;

        searchJob.projectedPositionWSBuffer = projectedPositionWSBuffer;
        searchJob.errorBuffer = errorBuffer;
        searchJob.candidateLocationWSBuffer = candidatePositionBuffer;
        searchJob.directionBuffer = directionBuffer;
        searchJob.stepCountBuffer = stepCountBuffer;

        // Schedule the job with one Execute per index in the results array and only 1 item per processing batch
        JobHandle handle = searchJob.Schedule(numElements, 1);
        handle.Complete();

        // Fill the input positions
        for (int i = 0; i < numElements; ++i)
        {
            float3 projectedPosition = projectedPositionWSBuffer[i];
            cubes[i].transform.position = projectedPosition + Time.deltaTime * directionBuffer[i] * currentSpeedMultiplier;
        }
    }

    private void OnDestroy()
    {
        directionBuffer.Dispose();
        targetPositionBuffer.Dispose();
        errorBuffer.Dispose();
        candidatePositionBuffer.Dispose();
        stepCountBuffer.Dispose();
    }
}
