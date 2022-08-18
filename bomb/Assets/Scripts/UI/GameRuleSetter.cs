using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
public class GameRuleSetter : NetworkBehaviour
{
    [SerializeField] RectTransform Panel_setting;

    [Header ("GameRule")]

    [SyncVar(hook = nameof(OnChangeMaxBombTime))] private int maxBombTime;
    [SerializeField] private Text maxBombTimeText;
    [SyncVar(hook = nameof(OnChangeMinBombTime))] private int minBombTime;
    [SerializeField] private Text minBombTimeText;
    [SyncVar(hook = nameof(OnChangeScorePerRound))] private int scorePerRound;
    [SerializeField] private Text scorePerRoundText;
    [SyncVar(hook = nameof(OnChangeGhostSkilCount))] private int ghostSkillCount;
    [SerializeField] private Text ghostSKillCountText;

    private void Start()
    {
        UpdateRule();
    }

    public void UpdateRule()
    {
        maxBombTimeText.text = GameRuleStore.Instance.CurGameRule.maxBombTime.ToString();
        minBombTimeText.text = GameRuleStore.Instance.CurGameRule.minBombTime.ToString();
        scorePerRoundText.text = GameRuleStore.Instance.CurGameRule.roundWinningPoint.ToString();
        ghostSKillCountText.text = GameRuleStore.Instance.CurGameRule.ghostSkillCount.ToString();
    }

    public void EnterRuleSetting()
    {
        Panel_setting.gameObject.SetActive(true);
    }

    public void OnChangeMaxBombTime(int _, int value)
    {
        maxBombTimeText.text = value.ToString();
        GameRuleStore.Instance.SetMaxBombTime(value);
    }

    public void OnMaxBombTime(bool isPlus)
    {
        maxBombTime = Mathf.Clamp(maxBombTime + (isPlus ? 5 : -5), 80, 100);
    }

    public void OnChangeMinBombTime(int _, int value)
    {
        minBombTimeText.text = value.ToString();
        GameRuleStore.Instance.SetMinBombTime(value);
    }

    public void OnMinBombTime(bool isPlus)
    {
        minBombTime = Mathf.Clamp(minBombTime + (isPlus ? 5 : -5), 60, 80);
    }

    public void OnChangeScorePerRound(int _, int value)
    {
        scorePerRoundText.text = value.ToString();
        GameRuleStore.Instance.SetScorePerRound(value);
    }

    public void OnScorePerRound(bool isPlus)
    {
        scorePerRound = Mathf.Clamp(scorePerRound + (isPlus ? 1 : -1), 3, 6);
    }

    public void OnChangeGhostSkilCount(int _, int value)
    {
        ghostSKillCountText.text = value.ToString();
        GameRuleStore.Instance.SetGhostSkillCount(value);
    }

    public void OnGhostSKillCount(bool isPlus)
    {
        ghostSkillCount = Mathf.Clamp(ghostSkillCount + (isPlus ? 1 : -1), 0, 3);
    }
}
