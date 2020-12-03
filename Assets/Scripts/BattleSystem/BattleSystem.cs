using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    public void StartBattle()
    {
        StartCoroutine(SetupBattle());
    }

    private IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        enemyUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild { enemyUnit.Pokemon.Base.Name} appeared.");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    public void HandleUpdate()
    {
        switch (state)
        {
            case BattleState.PlayerAction:
                HandleActionSelection();
                break;
            case BattleState.PlayerMove:
                HandleMoveSelection();
                break;
        }
       
    }

    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
            {
                ++currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
            {
                --currentMove;
            }
        }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
            {
                currentMove += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
            {
                currentMove -= 2;
            }
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }    
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}!!");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1.0f);

        enemyUnit.PlayHitAnimation();

        if (move.Base.Category == MoveBase.MoveCategory.Status)
        {
            CauseStatusEffect(move, playerUnit, enemyUnit);
        }
        else
        {
            var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
            yield return enemyHud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if (enemyUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Fainted!!");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetAIMove(playerUnit.Pokemon);
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}!!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1.0f);

        playerUnit.PlayHitAnimation();

        if (move.Base.Category == MoveBase.MoveCategory.Status)
        {
            CauseStatusEffect(move, enemyUnit, playerUnit);
        }
        else
        {
            var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
            yield return playerHud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if (playerUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted!!");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1)
        {
            yield return dialogBox.TypeDialog("A critical hit!!");
        }

        if (damageDetails.TypeEffectiveness > 1)
        {
            yield return dialogBox.TypeDialog("It's super effective!!");
        }
        else if (damageDetails.TypeEffectiveness < 1)
        {
            yield return dialogBox.TypeDialog("It wasn't very effective...");
        }
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
            {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
            {
                --currentAction;
            }
        }

        dialogBox.UpdateActionSelectedAction(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                // Run
                OnBattleOver(false);
            }
        }
    }

    void CauseStatusEffect(Move move, BattleUnit source, BattleUnit target = null)
    {
        if (move.Base.Effects.Boosts != null)
        {
            if(move.Base.Target == MoveBase.MoveTarget.Self)
            {
                source.Pokemon.ApplyStatus(move.Base.Effects.Boosts);
            }
            else
            {
                target.Pokemon.ApplyStatus(move.Base.Effects.Boosts);
            }
        }
    }
}
