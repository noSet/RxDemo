using System;
using System.Collections.Generic;

namespace CustomControl
{
    public class LayoutItem : IEquatable<LayoutItem>
    {
        public string Id { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool Moving { get; set; }

        public object ItemRef { get; set; }

        /// <summary>
        /// 确定此<see cref="LayoutItem"/>是否与<paramref name="otherItem"/>相交
        /// </summary>
        /// <param name="otherItem">其他<see cref="LayoutItem"/></param>
        /// <returns>是否相交</returns>
        public bool IntersectsWith(LayoutItem otherItem)
        {
            if (otherItem is null)
            {
                return false;
            }

            if (this == otherItem)
            {
                return false;
            }

            return otherItem.X < X + Width && X < otherItem.X + otherItem.Width && otherItem.Y < Y + Height && Y < otherItem.Y + otherItem.Height;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LayoutItem);
        }

        public bool Equals(LayoutItem other)
        {
            return other != null
                && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);
        }

        public override string ToString()
        {
            return $"Id = {Id}, X = {X}, Y = {Y}, Width = {Width}, Heigth = {Height}";
        }

        public static bool operator ==(LayoutItem left, LayoutItem right)
        {
            return EqualityComparer<LayoutItem>.Default.Equals(left, right);
        }

        public static bool operator !=(LayoutItem left, LayoutItem right)
        {
            return !(left == right);
        }
    }
}
