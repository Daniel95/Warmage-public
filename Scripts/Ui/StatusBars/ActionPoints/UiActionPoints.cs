using System.Collections.Generic;
using UnityEngine;

public class UiActionPoints : MonoBehaviour
{
    [SerializeField] private UiActionPointIcon actionPointIconPrefab;

    private List<UiActionPointIcon> actionPointIcons;
    private ActionPointManager actionPointManager;

    private void Awake()
    {
        actionPointManager = ActionPointManager.GetInstance();

        actionPointIcons = new List<UiActionPointIcon>();

        for (int i = 0; i < actionPointManager.totalCount; i++)
        {
            GameObject icon = Instantiate(actionPointIconPrefab.gameObject, transform);
            UiActionPointIcon uiActionPointIcon = icon.GetComponent<UiActionPointIcon>();
            actionPointIcons.Add(uiActionPointIcon);

            uiActionPointIcon.SetCooldown(actionPointManager.cooldown);
        }
    }

    private void Update()
    {
        for (int i = 0; i < actionPointManager.actionPoints.Count; i++)
        {
            float remainingCooldown = actionPointManager.actionPoints[i].remainingCooldown;

            actionPointIcons[i].UpdateRemainingCooldown(remainingCooldown);
        }
    }
}
