using ADVance.Data;
using UnityEngine;

namespace ADVance.Sample.Scenario
{
    public class ScenarioSceneMain : MonoBehaviour
    {
        [SerializeField] private ScenarioScenePresenter _presenter = null;
        [SerializeField] private ScenarioController _controller = null;
        [SerializeField] private ScenarioData _data = null;

        protected void Start()
        {
            _presenter.Initialize();
            _controller.Initialize();
            _controller.StartScenario(_data);
        }
    }
}
