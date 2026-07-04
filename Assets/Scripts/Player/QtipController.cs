// using UnityEngine;

// public class QtipController : MonoBehaviour
// {
//     [Header("设置")]
//     public float stickDistance = 0.5f;
//     public BoogerController boogerController;

//     [Header("状态")]
//     public bool isStuck = false;

//     void Start()
//     {
//         // 订阅 CursorManager 的事件
//         if (CursorManager.Instance != null)
//         {
//             CursorManager.Instance.OnClick += HandleClick;
//             CursorManager.Instance.OnScroll += HandleScroll;
//         }
//         Debug.Log("[Qtip] Start");
//     }

//     void OnDestroy()
//     {
//         if (CursorManager.Instance != null)
//         {
//             CursorManager.Instance.OnClick -= HandleClick;
//             CursorManager.Instance.OnScroll -= HandleScroll;
//         }
//     }

//     // ========== 阶段 2: 接触 B 点 → 粘住 ==========
//     void HandleClick(Vector3 worldPos)
//     {
//         if (GameStateJudge.Instance == null) return;
//         if (!GameStateJudge.Instance.IsInGame()) return;
//         if (isStuck) return;
//         if (boogerController == null) return;

//         // 只在 Sticking 阶段处理
//         if (!GameStateJudge.Instance.IsInSticking()) return;

//         Vector3 boogerTip = boogerController.GetBoogerTip();
//         float dist = Vector3.Distance(worldPos, boogerTip);
//         if (dist < stickDistance)
//         {
//             StickBooger();
//         }
//     }

//     void StickBooger()
//     {
//         isStuck = true;
//         GameStateJudge.Instance.SetSubState(GameSubState.Sticked);
//         Debug.Log("[Qtip] 粘住 B 点 → Sticked");
//     }

//     // ========== 滚轮处理 ==========
//     void HandleScroll(float scroll)
//     {
//         if (GameStateJudge.Instance == null) return;
//         if (!GameStateJudge.Instance.IsInGame()) return;
//         if (!isStuck) return;
//         if (GameplayData.Instance == null) return;

//         // Sticked → Rolling
//         if (GameStateJudge.Instance.IsInSticked())
//         {
//             GameStateJudge.Instance.SetSubState(GameSubState.Rolling);
//             Debug.Log("[Qtip] 第一次滚轮 → Rolling");
//             return;
//         }

//         // Rolling: 速度增减
//         if (GameStateJudge.Instance.IsInRolling())
//         {
//             if (scroll > 0)
//                 GameplayData.Instance.AddSpeed(GameplayData.Instance.speedAddPerScroll);
//             else if (scroll < 0)
//                 GameplayData.Instance.AddSpeed(-GameplayData.Instance.speedAddPerScroll);
//         }
//     }
// }