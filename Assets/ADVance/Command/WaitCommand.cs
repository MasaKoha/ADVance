using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.Command
{
    public class WaitCommand : CommandBase
    {
        public override string CommandName => "Wait";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            if (float.TryParse(args[0], out var waitTime))
            {
                if (waitTime > 0)
                {
                    Debug.Log($"Waiting for {waitTime} seconds...");
                    await UniTask.Delay((int)(waitTime * 1000));
                }
            }
            else
            {
                Debug.LogWarning($"Invalid wait time: {args[0]}");
            }
        }
    }
}