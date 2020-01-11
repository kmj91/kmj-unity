using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Transform playerHeadTrans;      // 플레이어 트랜스폼
    private LayerMask layerMaskCube;    // 큐브 레이어 마스크

    // Start is called before the first frame update
    void Start()
    {
        // 플레이어 머리
        playerHeadTrans = GameObject.Find("Player").transform.Find("Head");
        // 레이어 마스크
        layerMaskCube = 1 << LayerMask.NameToLayer("Cube");

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 box;            // 박스 크기

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        if (Physics.CheckBox(playerHeadTrans.position, box, Quaternion.identity, layerMaskCube))
        {
            Debug.Log("겹침");
        }
    }
}
