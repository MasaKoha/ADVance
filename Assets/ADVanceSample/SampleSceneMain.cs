using ADVanceSample;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.Sample
{
    public class SampleSceneMain : MonoBehaviour
    {
        [SerializeField]
        private ScenarioController _scenarioController = null;

        private void Start()
        {
            _scenarioController.Initialize();
            _scenarioController.StartScenarioAsync().Forget();
        }
    }
}