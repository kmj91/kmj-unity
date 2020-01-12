using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static PlayerMovement;

public class GameManager : MonoBehaviour
{
    private Transform playerHeadTrans;      // 플레이어 트랜스폼
    private PlayerMovement playerMovement;  // 플레이어 무브먼트
    private PlayerState playerState;        // 플레이어 상태
    private LayerMask layerMaskCube;        // 큐브 레이어 마스크

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.Find("Player");

        // 플레이어 머리
        playerHeadTrans = player.transform.Find("Head");
        // 플레이어 무브먼트
        playerMovement = player.GetComponent<PlayerMovement>();
        // 플레이어 상태
        playerState = playerMovement.playerState;
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

        Debug.Log("playerState : " + playerState);
    }
}
