﻿using System;
using System.Collections.Generic;

namespace GanondorfMod.Modules
{
    internal static class Helpers
    {
        internal const string agilePrefix = "<style=cIsUtility>Agile.</style> ";
        internal const string stunningPrefix = "<style=cIsUtility>Stunning.</style>";
        internal const string heavyPrefix = "<style=cIsUtility>Heavy.</style>";

        internal static string ScepterDescription(string desc)
        {
            return "\n<color=#d299ff>SCEPTER: " + desc + "</color>";
        }

        internal static string DownsideDescription(string desc) {
            return "<color=#D2042D>" + desc + "</color>";
        }

        public static T[] Append<T>(ref T[] array, List<T> list)
        {
            var orig = array.Length;
            var added = list.Count;
            Array.Resize<T>(ref array, orig + added);
            list.CopyTo(array, orig);
            return array;
        }

        public static Func<T[], T[]> AppendDel<T>(List<T> list) => (r) => Append(ref r, list);
    }

    internal static class ArrayHelper
    {
        public static T[] Append<T>(ref T[] array, List<T> list)
        {
            var orig = array.Length;
            var added = list.Count;
            Array.Resize<T>(ref array, orig + added);
            list.CopyTo(array, orig);
            return array;
        }

        public static Func<T[], T[]> AppendDel<T>(List<T> list) => (r) => Append(ref r, list);
    }
}