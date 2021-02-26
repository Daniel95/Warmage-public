using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SkillIconSlot))]
public class SkillButton : MonoBehaviour
{
    public Action<int> clickEvent;

    public Image iconImage => icon;

    [SerializeField] private KeyCode keyCode = KeyCode.Alpha1;
    [SerializeField] private Image cooldown = null;
    [SerializeField] private Image icon = null;
    [SerializeField] private Text inputText = null;
    [SerializeField] private Text chargesText = null;
    [SerializeField] private Color inRangeColor = Color.white;
    [SerializeField] private Color outOfRangeColor = Color.red;
    [SerializeField] private Color noTargetColor = Color.grey;

    private SkillIconSlot skillIconSlot;

    private void TryCastSpell()
    {
        if (skillIconSlot.skill == null) { return; }

        SkillBarInput.CastResult castResult = SkillBarInput.CanCastSpell(skillIconSlot.skill);

        if (castResult == SkillBarInput.CastResult.Success)
        {
            SkillBarInput.GetInstance().SkillInput(skillIconSlot.skill);
        }
        else
        {
            if (castResult == SkillBarInput.CastResult.BusyChanneling) Debug.Log("Already channeling!");
            if (castResult == SkillBarInput.CastResult.BusyPlacing) Debug.Log("Already placing skill!");
            if (castResult == SkillBarInput.CastResult.InvalidTarget) Debug.Log("No valid target!");
            if (castResult == SkillBarInput.CastResult.Cooldown) Debug.Log("Skill is on cooldown!");
            if (castResult == SkillBarInput.CastResult.OutOfRange) Debug.Log("Target is too far!");
        }
    }

    private void Update()
    {
        if (ControlModeManager.mode != ControlMode.CharacterControl) { return; }
        if (skillIconSlot.skill == null) { return; }

        if(Input.GetKeyDown(keyCode))
        {
            TryCastSpell();
        }

        {
            SkillCooldownManager skillCooldownManager = SkillBarInput.GetInstance().skillCooldownManager;

            cooldown.fillAmount = skillCooldownManager.GetCooldownFillAmount(skillIconSlot.skill.GetId());

            if (skillIconSlot.skill.GetMaxCharges() > 1)
            {
                int charges = skillCooldownManager.GetSkillCharges(skillIconSlot.skill.GetId());
                chargesText.text = charges.ToString();

                if(charges > 0) 
                {
                    Color coolDownColor = cooldown.color;
                    coolDownColor.a = 0.9f;
                    cooldown.color = coolDownColor;
                } 
                else 
                {
                    Color coolDownColor = cooldown.color;
                    coolDownColor.a = 1.0f;
                    cooldown.color = coolDownColor;
                }
            } 
            else
            {
                chargesText.text = string.Empty;

                Color coolDownColor = cooldown.color;
                coolDownColor.a = 1.0f;
                cooldown.color = coolDownColor;
            }
        }

        SkillBarInput.CastResult castResult = SkillBarInput.CanCastSpell(skillIconSlot.skill);

        if (castResult == SkillBarInput.CastResult.InvalidTarget)
        {
            inputText.color = noTargetColor;
        }
        else if(castResult == SkillBarInput.CastResult.OutOfRange)
        {
            inputText.color = outOfRangeColor;
        } 
        else
        {
            inputText.color = inRangeColor;
        }
    }

    private void OnValidate()
    {
        //Set button text to keycode
        {
            inputText.text = keyCode.ToString();

            if (inputText.text == KeyCode.Alpha1.ToString()) inputText.text = "1";
            if (inputText.text == KeyCode.Alpha2.ToString()) inputText.text = "2";
            if (inputText.text == KeyCode.Alpha3.ToString()) inputText.text = "3";
            if (inputText.text == KeyCode.Alpha4.ToString()) inputText.text = "4";

            if (inputText.text == KeyCode.Mouse0.ToString()) inputText.text = "Left";
            if (inputText.text == KeyCode.Mouse1.ToString()) inputText.text = "Right";
        }
    }

    private void Awake()
    {
        skillIconSlot = GetComponent<SkillIconSlot>();
    }
}
