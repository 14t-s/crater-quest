using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    
    // Controls is the C# class generated by the Input System 
    private Controls _controls;

    private InputManager()
    {
        _controls = new Controls();
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        OnGameStateChanged(GameStateManager.Instance.CurrentGameState);
    }

    ~InputManager()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        // Disable all controls
        _controls.Gameplay.Disable();
        _controls.Paused.Disable();

        // Enable certain controls based on new game state
        switch (newGameState)
        {
            case GameState.Gameplay:
                _controls.Gameplay.Enable();
                break;
            case GameState.Paused:
                _controls.Paused.Enable();
                break;
            default:
                break;
        }
    }
    
}