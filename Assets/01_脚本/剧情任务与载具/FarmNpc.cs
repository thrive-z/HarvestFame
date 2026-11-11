using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class FarmNpc : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcId;
    private string displayName;
    private string[] chats;
    private int chatIndex;
    private FarmCampaign campaign;
    private TextMeshPro nameplate;
    private Camera nameCamera;

    public string Id => npcId;
    public string DisplayName => displayName;
    public void Initialize(FarmCampaign owner, string name, string[] dialogue)
    {
        campaign = owner;
        displayName = name;
        chats = dialogue;
        nameplate = GetComponentInChildren<TextMeshPro>(true);
        nameCamera = Camera.main;
        if (nameplate != null)
        {
            nameplate.text = name;
            nameplate.fontSize *= 1.65f;
            nameplate.rectTransform.sizeDelta *= 1.65f;
        }
    }

    private void LateUpdate()
    {
        if (nameplate == null || nameCamera == null)
            return;
        var forward = Vector3.ProjectOnPlane(nameCamera.transform.forward, Vector3.up);
        if (forward.sqrMagnitude > 0.001f)
            nameplate.transform.rotation = Quaternion.LookRotation(forward);
    }

    public bool CanInteract() => campaign != null && campaign.IsRunning;
    public string GetInteractTip() => $"F · 与{displayName}交谈";
    public void Interact(GameObject interactor) => campaign.OpenNpc(this);

    public string NextChat()
    {
        if (chats == null || chats.Length == 0)
            return "今年的收成就靠你了。";
        return chats[chatIndex++ % chats.Length];
    }
}

public sealed class FarmNpcData
{
    public string npcId;
    public string name;
    public List<DialogueBand> dialogue;

    public sealed class DialogueBand
    {
        public string[] dialogues;
    }
}
