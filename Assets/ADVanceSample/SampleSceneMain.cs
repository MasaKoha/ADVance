using ADVance.Data;
using ADVanceSample;
using UnityEngine;

namespace ADVance.Sample
{
    public class SampleSceneMain : MonoBehaviour
    {
        [SerializeField]
        private ScenarioController _scenarioController = null;

        [SerializeField]
        private ScenarioData _scenarioData = null;

        private void Start()
        {
            _scenarioController.Initialize();
            _scenarioController.StartScenario(_scenarioData);
        }
    }
}