using UnityEngine;

namespace ASQ
{
    internal static class Extensions
    {
        public static Rect SetHeight(this Rect r, float value)
        {
            r.height = value;
            return r;
        }
        
        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Rect Shift(this Rect r, float x, float y, float width, float height) => new Rect(r.x + x, r.y + y, r.width + width, r.height + height);

        public static Rect ShiftY(this Rect r, float y) => new Rect(r.x, r.y + y, r.width, r.height);

        public static Rect Expand(this Rect r, float amount)
        {
            float num = amount * 2f;
            return r.Shift(-amount, -amount, num, num);
        }

        
    }
}