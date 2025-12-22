using System.Collections;
using System.Collections.Generic;
using MyFrameWork;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Instance.Init();
        // UIMgr.Instance.ShowPanel<GameStartPanel>(isSync:true);
    }

}
