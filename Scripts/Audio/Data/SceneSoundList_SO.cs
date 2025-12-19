using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneSoundList_SO", menuName = "Audio/SceneSoundList_SO")]
public class SceneSoundList_SO : ScriptableObject
{
    public List<SceneSoundItem> sceneSoundList;

    public SceneSoundItem GetSceneSoundItem(string sceneName)
    {
        return sceneSoundList.Find(sound => sound.sceneName == sceneName);
    }
}
