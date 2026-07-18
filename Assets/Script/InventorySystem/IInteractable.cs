using UnityEngine;

/// <summary>
/// 万能交互接口
/// </summary>
/// 让可以被交互的物品 都实现该接口
public interface IInteractable
{
    //获取提示, 按F 进入,  按F捡起, 按F 开门
    string GetInteractTip();

    //是否可以交互 
    bool CanInteract();

    //执行交互
    void Interact(GameObject interactor);


}