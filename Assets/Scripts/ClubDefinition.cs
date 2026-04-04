using UnityEngine;

public enum ClubType { Driver, Wood, Hybrid, Iron, Wedge, Putter }

[System.Serializable]
public struct ClubDefinition
{
    public string    clubName;
    public ClubType  clubType;
    public float     maxForceMultiplier;
    public float     launchAngleDegrees;
    public float     maxBackswingDegrees;
    public float     rollingDragMultiplier;
    public float     shaftLength;
    public float     headWidth;
    public float     headHeight;
    public float     headDepth;
    public Color     headColor;
    public Color     shaftColor;
    public Color     gripColor;
}

public static class ClubBag
{
    public static ClubDefinition[] GetFullBag()
    {
        Color silver  = new Color(0.82f, 0.82f, 0.78f);
        Color grip    = new Color(0.12f, 0.12f, 0.12f);
        Color iron    = new Color(0.72f, 0.72f, 0.74f);
        Color driver  = new Color(0.10f, 0.06f, 0.03f);  // dark wood
        Color wood3   = new Color(0.18f, 0.10f, 0.05f);
        Color hybrid  = new Color(0.25f, 0.25f, 0.30f);
        Color putter  = new Color(0.52f, 0.52f, 0.54f);  // matte grey

        return new ClubDefinition[]
        {
            new ClubDefinition {
                clubName = "Driver",  clubType = ClubType.Driver,
                maxForceMultiplier = 1.00f, launchAngleDegrees =  12f,
                maxBackswingDegrees = -110f, rollingDragMultiplier = 0.50f,
                shaftLength = 1.20f,
                headWidth = 0.22f, headHeight = 0.14f, headDepth = 0.18f,
                headColor = driver, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "3-Wood",  clubType = ClubType.Wood,
                maxForceMultiplier = 0.87f, launchAngleDegrees =  14f,
                maxBackswingDegrees = -105f, rollingDragMultiplier = 0.65f,
                shaftLength = 1.15f,
                headWidth = 0.22f, headHeight = 0.14f, headDepth = 0.18f,
                headColor = wood3, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "Hybrid",  clubType = ClubType.Hybrid,
                maxForceMultiplier = 0.78f, launchAngleDegrees =  18f,
                maxBackswingDegrees = -100f, rollingDragMultiplier = 0.80f,
                shaftLength = 1.05f,
                headWidth = 0.18f, headHeight = 0.12f, headDepth = 0.14f,
                headColor = hybrid, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "3-Iron",  clubType = ClubType.Iron,
                maxForceMultiplier = 0.74f, launchAngleDegrees =  15f,
                maxBackswingDegrees =  -98f, rollingDragMultiplier = 0.85f,
                shaftLength = 1.00f,
                headWidth = 0.08f, headHeight = 0.06f, headDepth = 0.04f,
                headColor = iron, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "5-Iron",  clubType = ClubType.Iron,
                maxForceMultiplier = 0.68f, launchAngleDegrees =  19f,
                maxBackswingDegrees =  -95f, rollingDragMultiplier = 0.90f,
                shaftLength = 0.95f,
                headWidth = 0.08f, headHeight = 0.06f, headDepth = 0.04f,
                headColor = iron, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "7-Iron",  clubType = ClubType.Iron,
                maxForceMultiplier = 0.61f, launchAngleDegrees =  23f,
                maxBackswingDegrees =  -90f, rollingDragMultiplier = 1.00f,
                shaftLength = 0.90f,
                headWidth = 0.08f, headHeight = 0.06f, headDepth = 0.04f,
                headColor = iron, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "9-Iron",  clubType = ClubType.Iron,
                maxForceMultiplier = 0.54f, launchAngleDegrees =  28f,
                maxBackswingDegrees =  -85f, rollingDragMultiplier = 1.15f,
                shaftLength = 0.85f,
                headWidth = 0.08f, headHeight = 0.06f, headDepth = 0.04f,
                headColor = iron, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "Wedge",   clubType = ClubType.Wedge,
                maxForceMultiplier = 0.44f, launchAngleDegrees =  34f,
                maxBackswingDegrees =  -75f, rollingDragMultiplier = 1.60f,
                shaftLength = 0.82f,
                headWidth = 0.09f, headHeight = 0.07f, headDepth = 0.05f,
                headColor = iron, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "Putter",  clubType = ClubType.Putter,
                maxForceMultiplier = 0.15f, launchAngleDegrees =   2f,
                maxBackswingDegrees =  -35f, rollingDragMultiplier = 2.00f,
                shaftLength = 0.75f,
                headWidth = 0.25f, headHeight = 0.04f, headDepth = 0.12f,
                headColor = putter, shaftColor = silver, gripColor = grip
            },
        };
    }
}
