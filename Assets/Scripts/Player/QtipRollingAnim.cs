using UnityEngine;

public class QtipRollingAnim : MonoBehaviour
{
    [Header("Animator")]
    public Animator qtipAnimator;
    public string isScrollingParam = "IsScrolling";

    void Start()
    {
        if (qtipAnimator == null) qtipAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (qtipAnimator == null || GameStateJudge.Instance == null) return;

        bool isRolling = (GameStateJudge.Instance.subState == GameSubState.Rolling);
        qtipAnimator.SetBool(isScrollingParam, isRolling);
    }
}