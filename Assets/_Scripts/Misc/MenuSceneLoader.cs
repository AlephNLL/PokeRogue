using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneLoader : MonoBehaviour
{
    public string daycare;
    public void LoadDaycare()
    {
        SceneManager.LoadSceneAsync(daycare);
    }
}
