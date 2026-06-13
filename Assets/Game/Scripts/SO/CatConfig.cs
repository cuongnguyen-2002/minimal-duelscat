using Spine.Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "CatConfig", menuName = "Data/CatConfig")]
public class CatConfig : ScriptableObject
{
    [SerializeField] private SkeletonDataAsset  _skeletonDataAsset;
    [SerializeField,SpineAnimation(dataField = "_skeletonDataAsset")]
    private string _idleAnimationName;
    [SerializeField,SpineAnimation(dataField = "_skeletonDataAsset")]
    private string _startAnimationName;
    [SerializeField,SpineAnimation(dataField = "_skeletonDataAsset")]
    private string _winAnimationName;
    [SerializeField,SpineAnimation(dataField = "_skeletonDataAsset")]
    private string _loseAnimationName;
    [SerializeField,SpineAnimation(dataField = "_skeletonDataAsset")]
    private string _eatAnimationName;
    
    public string IdleAnimationName => _idleAnimationName;
    public string StartAnimationName => _startAnimationName;    
    public string WinAnimationName => _winAnimationName;
    public string LoseAnimationName => _loseAnimationName;
    public string EatAnimationName => _eatAnimationName;
}