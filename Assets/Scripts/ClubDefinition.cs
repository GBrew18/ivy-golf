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
        // Wii Sports-inspired club colors:
        // Shaft: bright silver-white  |  Grip: near-black  |  Head: dark charcoal
        Color silver  = new Color(0.85f, 0.85f, 0.85f);  // bright silver shaft
        Color grip    = new Color(0.15f, 0.15f, 0.15f);  // near-black grip
        Color charcoal = new Color(0.2f, 0.2f, 0.2f);   // dark charcoal head (irons/wedge)
        Color driverHead = new Color(0.12f, 0.08f, 0.05f); // dark wood driver
        Color woodHead   = new Color(0.18f, 0.10f, 0.06f); // dark wood 3-wood
        Color hybridHead = new Color(0.22f, 0.22f, 0.26f); // dark steel hybrid
        Color putterHead = new Color(0.38f, 0.38f, 0.40f); // matte grey putter

        return new ClubDefinition[]
        {
            new ClubDefinition {
                clubName = "Driver",  clubType = ClubType.Driver,
                maxForceMultiplier = 1.00f, launchAngleDegrees =  18f,
                maxBackswingDegrees = -110f, rollingDragMultiplier = 0.50f,
                shaftLength = 1.20f,
                // Driver: wider rounded head
                headWidth = 0.18f, headHeight = 0.10f, headDepth = 0.14f,
                headColor = driverHead, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "3-Wood",  clubType = ClubType.Wood,
                maxForceMultiplier = 0.87f, launchAngleDegrees =  22f,
                maxBackswingDegrees = -105f, rollingDragMultiplier = 0.65f,
                shaftLength = 1.15f,
                headWidth = 0.16f, headHeight = 0.09f, headDepth = 0.12f,
                headColor = woodHead, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "Hybrid",  clubType = ClubType.Hybrid,
                maxForceMultiplier = 0.78f, launchAngleDegrees =  26f,
                maxBackswingDegrees = -100f, rollingDragMultiplier = 0.80f,
                shaftLength = 1.05f,
                headWidth = 0.14f, headHeight = 0.09f, headDepth = 0.10f,
                headColor = hybridHead, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "3-Iron",  clubType = ClubType.Iron,
                maxForceMultiplier = 0.74f, launchAngleDegrees =  28f,
                maxBackswingDegrees =  -98f, rollingDragMultiplier = 0.85f,
                shaftLength = 1.00f,
                // Irons: thin blade
                headWidth = 0.10f, headHeight = 0.06f, headDepth = 0.04f,
                headColor = charcoal, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "5-Iron",  clubType = ClubType.Iron,
                maxForceMultiplier = 0.68f, launchAngleDegrees =  32f,
                maxBackswingDegrees =  -95f, rollingDragMultiplier = 0.90f,
                shaftLength = 0.95f,
                headWidth = 0.10f, headHeight = 0.06f, headDepth = 0.04f,
                headColor = charcoal, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "7-Iron",  clubType = ClubType.Iron,
                maxForceMultiplier = 0.61f, launchAngleDegrees =  36f,
                maxBackswingDegrees =  -90f, rollingDragMultiplier = 1.00f,
                shaftLength = 0.90f,
                headWidth = 0.10f, headHeight = 0.06f, headDepth = 0.04f,
                headColor = charcoal, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "9-Iron",  clubType = ClubType.Iron,
                maxForceMultiplier = 0.54f, launchAngleDegrees =  40f,
                maxBackswingDegrees =  -85f, rollingDragMultiplier = 1.15f,
                shaftLength = 0.85f,
                headWidth = 0.10f, headHeight = 0.06f, headDepth = 0.04f,
                headColor = charcoal, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "Wedge",   clubType = ClubType.Wedge,
                maxForceMultiplier = 0.44f, launchAngleDegrees =  46f,
                maxBackswingDegrees =  -75f, rollingDragMultiplier = 1.60f,
                shaftLength = 0.82f,
                headWidth = 0.10f, headHeight = 0.07f, headDepth = 0.05f,
                headColor = charcoal, shaftColor = silver, gripColor = grip
            },
            new ClubDefinition {
                clubName = "Putter",  clubType = ClubType.Putter,
                maxForceMultiplier = 0.15f, launchAngleDegrees =   2f,
                maxBackswingDegrees =  -35f, rollingDragMultiplier = 2.00f,
                shaftLength = 0.75f,
                // Putter: flat wide mallet head
                headWidth = 0.20f, headHeight = 0.035f, headDepth = 0.06f,
                headColor = putterHead, shaftColor = silver, gripColor = grip
            },
        };
    }
}
