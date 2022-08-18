using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomManager : MonoBehaviour
{
    public static CustomManager Instance;

    public readonly Dictionary<int, List<GameObject>> listDic = new Dictionary<int, List<GameObject>>();
    public readonly List<GameObject> HeadPrefab = new List<GameObject>();
    public readonly List<GameObject> BodyPrefab = new List<GameObject>();

    private void Awake()
    {
        if(Instance != null) Destroy(this);
        Instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        listDic.Add(0, HeadPrefab);
        listDic.Add(1, BodyPrefab);

        for(int i=0; i<PlayerSetting.customState.Length; i++)
        {
            PlayerSetting.customState[i] = listDic[i].Count + 1;
        }


        for(int i=0; i<(int)Customization.Parts.Length; i++)
        {
            GameObject[] temp = Resources.LoadAll<GameObject>($"Custom/{((Customization.Parts)i).ToString()}");
            foreach(var item in temp)
            {
                listDic[i].Add(item);
            }
        }
    }

}
