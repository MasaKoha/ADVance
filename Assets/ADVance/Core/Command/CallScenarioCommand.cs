using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.Command
{
    public class CallScenarioCommand : CommandBase
    {
        public override string CommandName => "Call";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var scenarioId = args[0];
            var scenarioData = Manager.ResolveScenarioData(scenarioId);
            if (scenarioData == null)
            {
                Debug.LogError($"Call: scenario '{scenarioId}' not found.");
                Manager.SetNextLineId();
                return;
            }

            Manager.CallScenario(scenarioData);
            await UniTask.CompletedTask;
        }
    }
}