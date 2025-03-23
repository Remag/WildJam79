using Godot;
using System;

public static class Game
{
    public static int PlayerLayer = 1;
    public static int EnemyShipLayer = 2;


    public static Player Player;
    public static CustomCamera2D Camera;
    public static GameField Field;
    public static MainScene MainScene;
    public static TravelMap TravelMap;
    public static string StageName = "Title";
    public static Node2D TestSprite;
    
    public static bool ResetXpAfterAssimilate = false;
}