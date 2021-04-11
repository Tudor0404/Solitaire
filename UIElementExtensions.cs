﻿using System;
using System.Windows;

namespace Solitaire {

    public static class UIElementExtensions {

        public static Int32 GetGroupID(DependencyObject obj) {
            return (Int32)obj.GetValue(GroupIDProperty);
        }

        public static void SetGroupID(DependencyObject obj, Int32 value) {
            obj.SetValue(GroupIDProperty, value);
        }

        // Using a DependencyProperty as the backing store for GroupID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupIDProperty =
            DependencyProperty.RegisterAttached("GroupID", typeof(Int32), typeof(UIElementExtensions), new UIPropertyMetadata(null));
    }
}
