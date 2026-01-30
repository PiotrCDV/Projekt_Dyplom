using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Perform Boss Attack", story: "Boss attacks target [Target] using attack [Index]", category: "Boss Combat", id: "PerformBossAttack")]
public class PerformBossAttack : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<int> Index;

    // Prywatna referencja do Twojego managera
    private BossCombatManager combatManager;

    protected override Status OnStart()
    {
        if (GameObject == null)
        {
            return Status.Failure;
        }

        if (combatManager == null)
        {
            combatManager = GameObject.GetComponent<BossCombatManager>();
        }

        if (combatManager == null)
        {
            LogFailure("Brak skryptu BossCombatManager na obiekcie!");
            return Status.Failure;
        }

        combatManager.TryAttack(Index.Value, Target.Value);

        return Status.Success;
    }
}