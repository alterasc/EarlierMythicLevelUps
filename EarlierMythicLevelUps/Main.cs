using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ObjectiveEvents;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using System.Reflection;
using UnityModManagerNet;

namespace EarlierMythicLevelUps;

[HarmonyPatch]
public static class Main
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;

    public static bool Load(UnityModManager.ModEntry modEntry)
    {
        log = modEntry.Logger;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
    private static void LogGainMythicExperience()
    {
        // general M7
        var colyphyrM7Cue = Utils.GetBlueprint<BlueprintCue>("272eb406119540344a4fdfc506bd8f3e");
        colyphyrM7Cue.OnShow.Actions = [colyphyrM7Cue.OnShow.Actions[1]];
        // Angel special M7
        var colyphyrM7AngelCue = Utils.GetBlueprint<BlueprintCue>("b1eb561b4bd711d46bdb85c229519f8d");
        colyphyrM7AngelCue.OnShow.Actions = [colyphyrM7AngelCue.OnShow.Actions[1]];

        var slayXanthirObjective = Utils.GetBlueprint<BlueprintQuestObjective>("d44f91b07f9914349aa0b6c082d98c25");
        slayXanthirObjective.AddComponent(
            new ObjectiveStatusTrigger
            {
                objectiveState = Kingmaker.AreaLogic.QuestSystem.QuestObjectiveState.Completed,
                Conditions = new ConditionsChecker { Conditions = [] },
                Actions = new ActionList
                {
                    Actions = [new GainMythicLevel { Levels = 1 }]
                }
            }
            );

        // MR 10
        var mr10Cue = Utils.GetBlueprint<BlueprintCue>("83fceef22bce2474a881b44ab2fb9ef2");
        mr10Cue.OnStop.Actions = [mr10Cue.OnStop.Actions[0]];

        // Deskari mythic answer

        var deskariAnswerList = Utils.GetBlueprint<BlueprintAnswersList>("0487039270cde774e942660950581edf");
        foreach (var answerRef in deskariAnswerList.Answers)
        {
            var answer = (BlueprintAnswer)answerRef.Get();
            if (answer == null || answer.MythicRequirement == Mythic.None || answer.MythicRequirement == Mythic.PlayerIsLegend)
            {
                continue;
            }
            answer.OnSelect.Actions =
            [
                ..answer.OnSelect.Actions,
                new GainMythicLevel { Levels = 1 }
            ];
        }
    }
}
