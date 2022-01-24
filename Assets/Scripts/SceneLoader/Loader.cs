using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        TestScene1, Menu
    }
    public static void Load(Scene scene)
    {
        SceneManager.LoadScene(Scene.Menu.ToString());

        SceneManager.LoadScene(scene.ToString());
    }




}
