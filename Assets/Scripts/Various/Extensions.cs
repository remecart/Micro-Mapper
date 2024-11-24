using UnityEngine;

public static class Extensions
{
    public static Color ToColor(this Vector4 self)
    {
        return new Color(self.x, self.y, self.z, self.w);
    }
}