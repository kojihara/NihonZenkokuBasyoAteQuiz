using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundButtonScript : MonoBehaviour
{
    [SerializeField] Text soundButtonText;
    [SerializeField] Button soundButton;
    [SerializeField] GameObject soundOffIcon;

    public void Toggle(bool isMute)
    {
        if (isMute)
        {
            soundButtonText.text = "サウンド　オフ";
            var colors = soundButton.colors;
            colors.normalColor = new Color(0.784f, 0.784f, 0.784f);
            soundOffIcon.SetActive(true);
        }
        else
        {
            soundButtonText.text = "サウンド　オン";
            var colors = soundButton.colors;
            colors.normalColor = new Color(1, 1, 1);
            soundOffIcon.SetActive(false);
        }
    }
}
