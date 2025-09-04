using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.Command
{
    public static class TaskRegistry
    {
        private static readonly ConcurrentDictionary<string, List<TaskInfo>> TaskGroups = new();

        public static void AddTask(string tag, TaskInfo taskInfo)
        {
            TaskGroups.AddOrUpdate(tag,
                new List<TaskInfo> { taskInfo },
                (_, existingList) =>
                {
                    existingList.Add(taskInfo);
                    return existingList;
                });
        }

        public static List<TaskInfo> GetTasks(string tag)
        {
            return TaskGroups.TryGetValue(tag, out var tasks) ? new List<TaskInfo>(tasks) : new List<TaskInfo>();
        }

        public static void ClearTasks(string tag)
        {
            TaskGroups.TryRemove(tag, out _);
        }

        public static void ClearAllTasks()
        {
            TaskGroups.Clear();
        }
    }

    public class TaskInfo
    {
        public string CommandName { get; set; }
        public List<string> Args { get; set; }
        public string TaskId { get; set; }

        public TaskInfo(string commandName, List<string> args, string taskId = null)
        {
            CommandName = commandName;
            Args = args;
            TaskId = taskId ?? System.Guid.NewGuid().ToString();
        }
    }

    public class TaskCommand : CommandBase
    {
        public override string CommandName => "Task";

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            if (args.Count < 3)
            {
                Debug.LogError("TaskCollector requires at least 3 arguments: [tag] [commandName] [arg1] [arg2...]");
                return UniTask.CompletedTask;
            }

            var tag = args[0];
            var commandName = args[1];
            var commandArgs = args.Skip(2).ToList();

            if (!Manager.CommandRegistry.HasCommand(commandName))
            {
                Debug.LogError($"Command '{commandName}' not found");
                return UniTask.CompletedTask;
            }

            var taskInfo = new TaskInfo(commandName, commandArgs);
            TaskRegistry.AddTask(tag, taskInfo);

            Debug.Log($"Task added to group '{tag}': {commandName} with {commandArgs.Count} arguments");
            Manager.SetNextLineId();
            return UniTask.CompletedTask;
        }
    }

    public class ExecCommand : CommandBase
    {
        public override string CommandName => "Exec";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            if (args.Count == 0)
            {
                Debug.LogError("ExecuteTask requires a tag argument");
                return;
            }

            var tag = args[0];
            var tasks = TaskRegistry.GetTasks(tag);

            if (tasks.Count == 0)
            {
                Debug.LogError($"No tasks found for tag '{tag}'");
                return;
            }

            await tasks.Select(async taskInfo => { _ = await Manager.CommandRegistry.ExecuteAsync(taskInfo.CommandName, taskInfo.Args); })
                .ToList();

            TaskRegistry.ClearTasks(tag);
            Manager.SetNextLineId();
        }
    }
}