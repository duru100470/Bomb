using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRuleStore : MonoBehaviour
{
    public struct GameRule
    {
        public int maxPlayer;
        public int minPlayer;
        public bool isPlayerEliminated;
        public float maxBombTime;
        public float minBombTime;
        public int bombCount;
        public int setCount;
        public int roundCount;
        public int ghostSkillCount;
        public int roundWinningPoint;
    }
    private static GameRuleStore _instance = null;

    [SerializeField]
    private GameRule curGameRule;
    public GameRule CurGameRule { get { return curGameRule; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        SetRuleDefault();
    }

    public static GameRuleStore Instance
    {
        get
        {
            if (_instance == null)
            {
                return null;
            }
            return _instance;
        }
    }

    private void SetRuleDefault()
    {
        curGameRule.maxPlayer = 6;
        curGameRule.minPlayer = 3;
        curGameRule.isPlayerEliminated = true;
        curGameRule.maxBombTime = 80;
        curGameRule.minBombTime = 60;
        curGameRule.bombCount = 1;
        curGameRule.setCount = 3;
        curGameRule.roundCount = curGameRule.maxPlayer - 1;
        curGameRule.ghostSkillCount = 1;
        curGameRule.roundWinningPoint = 1;
    }
}
