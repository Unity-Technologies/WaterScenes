using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FitToWaterSurface_2023 : MonoBehaviour
{
    public WaterSurface targetSurface = null;
    public bool includeDeformation = true;
    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (targetSurface != null)
        {
            // Build the search parameters
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = gameObject.transform.position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;
            searchParameters.includeDeformation = includeDeformation;

            // Do the search
            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                gameObject.transform.position = searchResult.projectedPositionWS;
            }else Debug.LogError("Can't Find Height");
        }
    }
}
