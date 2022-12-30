using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CanHighVault", menuName = "Transitions/CanHighVault", order = 5)]
public class CanHighVault : Vault_Or_StepUp
{
    public float checkLowerRange;

    public float checkUpperRange;
    protected override bool UniversalChecks(ref PlayerController c)
    {
        return CheckForVaultableorLedge(ref c, checkLowerRange, checkUpperRange);
    }

    protected override bool IfCondition(ref PlayerController c)
    {
        if (CheckVaultCondition(ref c, c.VaultDistance, c.PlayerVaultHeight))
        {
            Debug.Log("Can High Vault");
            return true;
        }
        return false;
    }

    protected override bool IfElseCondition(ref PlayerController c)
    {   //We are only deriving from the base so we already have access to Universal Checks and the If Condition functions
        return false;
    }
}
