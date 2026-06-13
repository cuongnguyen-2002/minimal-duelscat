using Spine;
using Spine.Unity;
using UnityEngine;

public class CatAnimation : MonoBehaviour
{
    [SerializeField] private  SkeletonAnimation _skeletonAnimation;
    [SerializeField] private CatConfig _catConfig;
    public TrackEntry PlayIdleAnimation()
    {
        return Play(_catConfig.IdleAnimationName, true);
    }

    public TrackEntry PlayEatAnimation()
    {
        return Play(_catConfig.EatAnimationName, true);
    }

    public TrackEntry PlayStartAnimation()
    {
        return Play(_catConfig.StartAnimationName, true);
    }

    public TrackEntry PlayWinAnimation()
    {
        return Play(_catConfig.WinAnimationName, true);
    }

    public TrackEntry PlayLoseAnimation()
    {
        return Play(_catConfig.LoseAnimationName, true);
    }
    
    private TrackEntry Play(string animationName, bool loop)
    {
        return _skeletonAnimation.state.SetAnimation(0,animationName,  loop);
    }
}
