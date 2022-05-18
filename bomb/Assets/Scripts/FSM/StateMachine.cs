using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public IState CurruentState { get; private set; }

    // 생성자
    public StateMachine(IState defaultState)
    {
        CurruentState = defaultState;
    }

    // State 전이
    public void SetState(IState state)
    {
        // 같은 상태로 전환
        if (CurruentState == state)
        {
            return;
        }

        CurruentState.OperateExit();

        CurruentState = state;

        CurruentState.OperateEnter();
    }

    // Update
    public void DoOperateUpdate()
    {
        CurruentState.OperateUpdate();
    }
}

// State 인터페이스
public interface IState
{
    void OperateEnter();
    void OperateUpdate();
    void OperateExit();
}