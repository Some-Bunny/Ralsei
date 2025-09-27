using System;

[Serializable]
public class AttachPointData
{
    public AttachPointData(tk2dSpriteDefinition.AttachPoint[] bcs)
    {
        this.attachPoints = bcs;
    }
    public tk2dSpriteDefinition.AttachPoint[] attachPoints;
}