﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Globalization;
using ModAPI.Components;
using ModAPI.Components.Panels;

namespace ModAPI.Utils
{
    public class Language
    {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached("Key", typeof(string), typeof(Language), new PropertyMetadata(default(string), KeyChanged));

        private static void KeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject current = d;
            bool found = false;
            string langRoot = "";
            while (!found && (current = GetParent(current)) != null) 
            {
                string rootPart = "";
                if (current.GetValue(KeyProperty) != null && current.GetValue(KeyProperty) != "")
                {
                    rootPart = current.GetValue(KeyProperty) as string;
                }
                else if (current is IPanel)
                {
                    try
                    {
                        rootPart = ((IPanel)current).GetLangRoot();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("LanguageHelper", "It seems like InitializeComponent is called in constructor of \"" + current.GetType().FullName + "\".", Debug.Type.WARNING);
                    }
                }
                if (rootPart != "")
                {
                    if (!rootPart.EndsWith(".")) rootPart += ".";
                    langRoot = rootPart + langRoot;
                }
            }

            if (d is TextBlock)
            {
                (d as TextBlock).SetResourceReference(TextBlock.TextProperty, langRoot + d.GetValue(KeyProperty));
            }
            if (d is Window)
            {
                (d as Window).SetResourceReference(Window.TitleProperty, langRoot + d.GetValue(KeyProperty) + ".Title");
            }
        }

        private static DependencyObject GetParent(DependencyObject o)
        {
            if (o == null) return null;
            if (o is ContentElement)
            {
                DependencyObject parent = ContentOperations.GetParent((ContentElement)o);
                if (parent != null) return parent;

                if (o is FrameworkContentElement)
                {
                    return ((FrameworkContentElement)o).Parent;
                }
            }
            else if (o is FrameworkElement)
            {
                return ((FrameworkElement)o).Parent;
            }
            return VisualTreeHelper.GetParent(o);
        }

        
        public static void SetKey(UIElement element, string value)
        {
            element.SetValue(KeyProperty, value);
        }

        public static string GetKey(UIElement element)
        {
            return (string)element.GetValue(KeyProperty);
        }

    }
}
