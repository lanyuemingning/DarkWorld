using System.Collections;
using System.Collections.Generic;
using QFramework;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Animation = Spine.Animation;

public class TEST10 : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic skeletonAnimation;
    [SerializeField] private SkeletonDataAsset skeldata;
    [SerializeField] private Animation animatino;
    [SerializeField] private Button btntest;

    int i = 0;
    void Start()
    {
        skeletonAnimation.AnimationState.Complete += AnimationState_Complete;
        btntest.onClick.AddListener(() =>
        {
            // ²¥·Åattack¶¯»­£¬loop = false
            skeletonAnimation.Show();
            //skeletonAnimation.AnimationState.SetAnimation(0, "animation", true);
            //skeletonAnimation.AnimationState.SetAnimation(0, "attack", false);
            /*         skeletonAnimation.AnimationState.SetAnimation(0, "ruchanghuangdong_mask", false);*/
            skeletonAnimation.AnimationState.SetAnimation(0, "walk", false);
            skeletonAnimation.gameObject.transform.position  += new Vector3(0.5f, 0, 0);

            
        });
    }

    private void AnimationState_Complete(Spine.TrackEntry trackEntry)
    {
        
        //skeletonAnimation.Hide();
    }
}
