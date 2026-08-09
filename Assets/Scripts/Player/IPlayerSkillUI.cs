using UnityEngine.UI;

public interface IPlayerSkillUI
{
    void SetPlayerMana(PlayerMana mana);

    void SetSkillUI(
        Image cooldownImage,
        Image skillIcon
    );
}