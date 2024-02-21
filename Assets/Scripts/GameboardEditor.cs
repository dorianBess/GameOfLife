using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Gameboard))]
class GameboardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Gameboard gameboard = (Gameboard)target;
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("Population : " + gameboard.population);
        GUILayout.Label("Iteration : " + gameboard.iterations);
        GUILayout.Label("Time : " + gameboard.time);

        GUILayout.Space(20);
        if (GUILayout.Button("Play"))
        {
            gameboard.simulationActive = true;
            gameboard.StartCoroutine(gameboard.Simulate());
        }
        if (GUILayout.Button("Stop"))
        {
            gameboard.simulationActive = false;
        }
        if (GUILayout.Button("Clear"))
        {
            gameboard.Clear();
            if (gameboard.useLimit)
            {
                gameboard.ReDrawLimit();
            }
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Use limit"))
        {
            gameboard.useLimit = !gameboard.useLimit;
            if (gameboard.useLimit)
            {
                Debug.Log("Draw");
                gameboard.ReDrawLimit();
            }
            else
            {
                gameboard.DeleteLimit();
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Get Pattern from json"))
        {
            Debug.Log("It's alive: ");
            string filePath = EditorUtility.OpenFilePanel("Select a Json", "", "json");
            gameboard.loadPatternFromJson(filePath);
        }
        if (GUILayout.Button("Save Pattern to json"))
        {
            string filename = "";
            filename = EditorUtility.SaveFilePanel("Save pattern", "", "", "json");
            gameboard.saveCurrentPatternJson(filename);
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Get Pattern from png"))
        {
            Debug.Log("It's alive: ");
            string filePath = EditorUtility.OpenFilePanel("Select a Png", "", "png");
            gameboard.loadPatternFromPng(filePath);
        }
        if (GUILayout.Button("Save Pattern to Png"))
        {
            string filename = "";
            filename = EditorUtility.SaveFilePanel("Save pattern", "", "", "png");
            gameboard.saveCurrentPatternPng(filename);
        }
        



    }
}
