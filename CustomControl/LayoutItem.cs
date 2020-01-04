using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CustomControl
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LayoutItem : IEquatable<LayoutItem>
    {
        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty]
        public int X { get; set; }

        [JsonProperty]
        public int Y { get; set; }

        [JsonProperty]
        public int Width { get; set; }

        [JsonProperty]
        public int Height { get; set; }

        public bool Moving { get; set; }

        public bool Static { get; set; }

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

        public void Init()
        {
            X = -1;
            Y = -1;
            Width = -1;
            Height = -1;
        }

        public void Cover(LayoutItem otherItem)
        {
            if (otherItem is null)
            {
                throw new ArgumentNullException(nameof(otherItem));
            }

            this.X = otherItem.X;
            this.Y = otherItem.Y;
            this.Width = otherItem.Width;
            this.Height = otherItem.Height;
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
