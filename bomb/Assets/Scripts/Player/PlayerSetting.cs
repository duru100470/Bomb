using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetting
{
    public static string playerNickname;
    public static string hostIP;
    public static int playerNum;

    public enum BindKeys
    {
        Jump,
        Cast,
        Drop,
        Push
    }

    public static Dictionary<BindKeys, KeyCode> keyDict = new Dictionary<BindKeys, KeyCode>();
    public static List<KeyCode> AvailKeys = new List<KeyCode>();
    public static KeyCode JumpKey = KeyCode.Space;
    public static KeyCode CastKey = KeyCode.Q;
    public static KeyCode DropKey = KeyCode.S;
    public static KeyCode PushKey = KeyCode.E;

    public static int[] customState = new int[2];
   
}
