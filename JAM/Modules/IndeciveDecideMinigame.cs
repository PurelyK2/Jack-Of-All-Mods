using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Modules.Components;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;
using static Il2CppSystem.Runtime.Remoting.RemotingServices;

namespace JAM.Modules;

[RegisterInIl2Cpp]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity")]
public sealed class IndeciveDecideMinigame(IntPtr cppPtr) : Minigame(cppPtr)
{
    public enum IndecisiveChoice
    {
        Crewmate,
        Impostor
    }

    public Transform choiceHolder;
    public GameObject cardPrefab;
    public TextMeshPro statusText;
    
    readonly Color _bgColor = new Color32(0, 6, 0, 215);
    IndecisiveChoice choice;
    Action<IndecisiveChoice> clickHandler;
    public static int CurrentCard { get; set; }

    void Awake()
    {
        if(Instance)
        {
            Instance.Close();
        }

        choiceHolder = transform.FindChild("Roles");
        cardPrefab = transform.FindChild("RoleCardHolder").gameObject;
        statusText = transform.FindChild("Status").gameObject.GetComponent<TextMeshPro>();

        statusText.font = HudManager.Instance.TaskPanel.taskText.font;
        statusText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        statusText.text = "Select Alignment";
        statusText.gameObject.SetActive(false);
    }

    public static IndeciveDecideMinigame Create()
    {
        var gameObject = Instantiate(TouAssets.RoleSelectionGame.LoadAsset(), HudManager.Instance.transform);
        gameObject.GetComponent<Minigame>().DestroyImmediate();
        gameObject.SetActive(false);

        return gameObject.AddComponent<IndeciveDecideMinigame>();
    }

    [HideFromIl2Cpp]
    public void Open(Action<IndecisiveChoice> onClick)
    {
        clickHandler = onClick;
        choice = IndecisiveChoice.Crewmate;

        Coroutines.Start(CoOpen(this));
    }

    [HideFromIl2Cpp]
    private IEnumerator CoAnimateCards()
    {
        foreach (var o in choiceHolder!.transform)
        {
            var card = o.Cast<Transform>();
            if (card == null)
            {
                continue;
            }

            var child = card.GetChild(0);
            yield return CoAnimateCardIn(child);
            Coroutines.Start(MiscUtils.BetterBloop(child, finalSize: 0.55f, duration: 0.22f, intensity: 0.16f));
            yield return new WaitForSeconds(0.1f);
        }

        CurrentCard = -1;
    }

    private static IEnumerator CoAnimateCardIn(Transform card)
    {
        var randY = (CurrentCard * CurrentCard * 0.5f - CurrentCard) * 0.1f + UnityEngine.Random.RandomRange(-0.15f, 0f);
        var randZ = -10f + CurrentCard * 5f + UnityEngine.Random.RandomRange(-1.5f, 0f);
        if (CurrentCard == 0)
        {
            randY = 0f;
            randZ = -2f;
        }

        card.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -randZ));
        card.transform.localPosition = new Vector3(card.transform.localPosition.x, card.transform.localPosition.y - 5f,
            card.transform.localPosition.z);
        card.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 14f));
        card.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        card.parent.gameObject.SetActive(true);
        for (var timer = 0f; timer < 0.4f; timer += Time.deltaTime)
        {
            var num = timer / 0.4f;
            card.localPosition =
                new Vector3(card.localPosition.x, Mathf.SmoothStep(-5f, randY, num), card.localPosition.z);
            card.transform.localRotation =
                Quaternion.Euler(new Vector3(0, 0, Mathf.SmoothStep(-randZ + 2.5f, -randZ, num)));
            yield return null;
        }

        CurrentCard++;

        card.localPosition = new Vector3(card.localPosition.x, randY, card.localPosition.z);
        card.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -randZ));
    }

    private static IEnumerator CoOpen(IndeciveDecideMinigame minigame)
    {
        while (ExileController.Instance)
        {
            yield return new WaitForSeconds(0.65f);
        }

        minigame.gameObject.SetActive(true);
        minigame.Begin();
    }

    public override void Close()
    {
        Coroutines.Stop(CoAnimateCards());
        HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(_bgColor, Color.clear));
        CurrentCard = -1;
        MinigameStubs.Close(this);
    }

    private PassiveButton CreateCard(int choiceNum, Sprite? sprite)
    {
        if (choiceNum == 1) choiceNum++; // spread cards out a bit more

        var newRoleObj = Instantiate(cardPrefab, choiceHolder);
        var actualCard = newRoleObj!.transform.GetChild(0);
        var roleText = actualCard.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
        var roleImage = actualCard.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        var teamText = actualCard.transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
        var passiveButton = actualCard.GetComponent<PassiveButton>();
        var buttonRollover = actualCard.GetComponent<ButtonRolloverHandler>();

        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            newRoleObj.transform.localPosition = new Vector3(newRoleObj.transform.localPosition.x,
                newRoleObj.transform.localPosition.y, newRoleObj.transform.localPosition.z - 10);
        }));
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            newRoleObj.transform.localPosition = new Vector3(newRoleObj.transform.localPosition.x,
                newRoleObj.transform.localPosition.y, newRoleObj.transform.localPosition.z + 10);
        }));

        var randZ = -10f + choiceNum * 5f + UnityEngine.Random.RandomRange(-1.5f, 1.5f);
        newRoleObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -randZ));
        newRoleObj.transform.localPosition =
            new Vector3(newRoleObj.transform.localPosition.x, newRoleObj.transform.localPosition.y, choiceNum);

        roleText.text = choiceNum == 0 ? "Crewmate" : "Impostor";
        teamText.text = "";

        if (sprite != null)
        {
            roleImage.sprite = sprite;
        }

        roleImage.SetSizeLimit(2.8f);

        Color color = choiceNum == 0 ? TownOfUsColors.Crewmate : TownOfUsColors.Impostor;

        buttonRollover.OverColor = color;
        roleText.color = color;
        teamText.color = color;
        return passiveButton;
    }

    private void Begin()
    {
        HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, _bgColor));

        statusText!.gameObject.SetActive(true);

        for (int i = 0; i < 2; i++)
        {
            var choiceSprite = i == 0 ? TouRoleIcons.Crewmate : TouRoleIcons.Impostor;
            var alignmentChoice = i == 0 ? IndecisiveChoice.Crewmate : IndecisiveChoice.Impostor;

            var card = CreateCard(i, choiceSprite.LoadAsset());

            card.OnClick.RemoveAllListeners();
            card.OnClick.AddListener((UnityAction)(() => { clickHandler.Invoke(alignmentChoice); }));
        }

        Coroutines.Start(CoAnimateCards());
        TransType = TransitionType.None;
        Begin(null);
    }


}
