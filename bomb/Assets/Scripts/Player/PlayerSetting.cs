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
    
    public static List<KeyCode> keyList = new List<KeyCode>{KeyCode.Space, KeyCode.Q, KeyCode.S, KeyCode.E};
    public static List<KeyCode> originKey = new List<KeyCode>{KeyCode.Space, KeyCode.Q, KeyCode.S, KeyCode.E};
    public static List<KeyCode> AvailKeys = new List<KeyCode>()
    {
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.S, KeyCode.F, KeyCode.Space, KeyCode.G
    };

    public static int[] customState = new int[2];
   
}
