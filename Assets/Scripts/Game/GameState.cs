using System;
using System.Collections.Generic;
using UnityEngine;

public enum EndingType { None, CorporatePressure, GovernmentShutdown, AuthorityDistrust }

public static class GameState
{
    public const int DeadAnimalLimit = 5;
    public const int FalseReportLimit = 1;
    public const int IndustrialShipCount = 3;

    public static int DeadAnimals { get; private set; }
    public static int FalseReports { get; private set; }
    public static int TrashCollected { get; private set; }
    public static int LitterbugsReported { get; private set; }
    public static int Coins { get; private set; }
    public static EndingType Ending { get; private set; }

    static readonly HashSet<string> photographedShips = new HashSet<string>();
    public static int ShipsPhotographed => photographedShips.Count;

    public static event Action<EndingType> EndingTriggered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
        DeadAnimals = 0;
        FalseReports = 0;
        TrashCollected = 0;
        LitterbugsReported = 0;
        Coins = 0;
        Ending = EndingType.None;
        photographedShips.Clear();
        Time.timeScale = 1f;
    }

    public static void AddDeadAnimal()
    {
        DeadAnimals++;
        if (DeadAnimals >= DeadAnimalLimit) Trigger(EndingType.GovernmentShutdown);
    }

    public static void AddFalseReport()
    {
        FalseReports++;
        if (FalseReports >= FalseReportLimit) Trigger(EndingType.AuthorityDistrust);
    }

    public const int CoinsPerTrash = 1;
    public const int CoinsPerLitterbug = 5;

    public static void AddTrashCollected()
    {
        TrashCollected++;
        Coins += CoinsPerTrash;
    }

    public static void AddLitterbugReported()
    {
        LitterbugsReported++;
        Coins += CoinsPerLitterbug;
    }

    public static bool TrySpend(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        return true;
    }

    // For Map 2: call with each industrial ship's id when it is validly photographed.
    public static void AddShipPhoto(string shipId)
    {
        photographedShips.Add(shipId);
        if (photographedShips.Count >= IndustrialShipCount) Trigger(EndingType.CorporatePressure);
    }

    static void Trigger(EndingType ending)
    {
        if (Ending != EndingType.None) return;
        Ending = ending;
        Time.timeScale = 0f;
        EndingTriggered?.Invoke(ending);
    }

    public static string EndingTitle(EndingType e)
    {
        switch (e)
        {
            case EndingType.CorporatePressure: return "Ending 1: Corporate Pressure Shutdown";
            case EndingType.GovernmentShutdown: return "Ending 2: Government Shutdown";
            case EndingType.AuthorityDistrust: return "Ending 3: Authority Distrust Shutdown";
            default: return "";
        }
    }

    public static string EndingText(EndingType e)
    {
        switch (e)
        {
            case EndingType.CorporatePressure:
                return "Under pressure from the companies, the sponsor withdrew funding, and the cleanup robot was switched off.";
            case EndingType.GovernmentShutdown:
                return "Too many animals died on your watch. The government judged the project a failure and shut it down.";
            case EndingType.AuthorityDistrust:
                return "Too many innocent people were wrongly reported. The authorities stopped trusting the robot's evidence, and the project was shut down.";
            default: return "";
        }
    }
}
