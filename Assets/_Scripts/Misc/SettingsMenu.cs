using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public TMPro.TMP_Dropdown resolutionDropdown;
    public TMPro.TMP_Dropdown difficultyDropdown;

    Resolution[] resolutions;

    private void Start()
    {
        UpdateResolutionDropdown();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", volume);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void UpdateResolutionDropdown()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width.ToString() + "x" + resolutions[i].height.ToString();
            options.Add(option);

            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetDifficulty(int difficultyIndex)
    {
        switch (difficultyDropdown.value)
        {
            case 0:
                BattleData.Difficulty = GameData.Difficulty.EASY;
                print(BattleData.Difficulty);
                break;
            case 1:
                BattleData.Difficulty = GameData.Difficulty.NORMAL;
                print(BattleData.Difficulty);
                break;
            case 2:
                BattleData.Difficulty = GameData.Difficulty.HARD;
                print(BattleData.Difficulty);
                break;
        }
    }

    public void UpdateDifficultyDropdown()
    {
        difficultyDropdown.value = (int)BattleData.Difficulty;
        resolutionDropdown.RefreshShownValue();
        print(BattleData.Difficulty);
    }
}
