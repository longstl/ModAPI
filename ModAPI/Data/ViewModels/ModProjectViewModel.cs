﻿/*  
 *  ModAPI
 *  Copyright (C) 2015 FluffyFish / Philipp Mohrenstecher
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  To contact me you can e-mail me at info@fluffyfish.de
 */

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ModAPI;
using ModAPI.Configurations;
using System.Xml.Linq;
using ModAPI.Data.Models;
using ModAPI.Data;
using ModAPI.Components;

public class ModProjectViewModel : INotifyPropertyChanged
{
    public ModProject Project;

    public ModProjectViewModel(ModProject project)
    {
        this.Project = project;
        if (this.Project.Name == null)
            this.Project.Name = new MultilingualValue();
        this.Project.Name.OnChange += NameChanged;
        this.Project.Description.OnChange += NameChanged;
        foreach (string LangCode in this.Project.Languages)
        {
            AddLanguageButton(LangCode);
            /*<Button Style="{StaticResource NormalButton}">
                                                    <StackPanel Orientation="Horizontal" Margin="-10,-17,-10,-16">
                                                        <TextBlock Text="Englisch" VerticalAlignment="Center" Margin="10,0,10,0" />
                                                        <Image Source="/resources/textures/Icons/Icon_Delete.png" Height="20" Margin="0,0,5,0" />
                                                    </StackPanel>
                                                </Button>*/
        }

        foreach (ModProject.Button button in project.Buttons)
        {
            ModProjectButton _button = new ModProjectButton();
            _button.DataContext = new ModProjectButtonViewModel(this, button);
            _Buttons.Add(_button);
        }
        
        CheckForErrors();
    }

    public void AddButton()
    {
        try
        {
            ModProject.Button button = new ModProject.Button();
            button.project = Project;
            Project.Buttons.Add(button);
            ModProjectButton _button = new ModProjectButton();
            _button.DataContext = new ModProjectButtonViewModel(this, button);
            _Buttons.Add(_button);
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.ToString());
        }
    }

    public void RemoveButton(ModProject.Button button)
    {
        Project.Buttons.Remove(button);
        for (int i = 0; i < _Buttons.Count; i++)
        {
            ModProjectButton vm = _Buttons[i];
            if (((ModProjectButtonViewModel)vm.DataContext).Button == button)
            {
                _Buttons.RemoveAt(i);
                return;
            }
        }
    }

    protected void NameChanged(object sender, EventArgs e)
    {
        Project.SaveConfiguration();
        CheckForErrors();
    }

    protected void DescriptionChanged(object sender, EventArgs e)
    {
        Project.SaveConfiguration();
        CheckForErrors();
    }

    protected void RemoveLanguage(object sender, EventArgs e)
    {
        string LangCode = (string)(((Button)sender).DataContext);
        _Languages.Remove(LangCode);
        Project.Languages.Remove(LangCode);
        _LanguageButtons.Remove((Button)sender);
        CheckForErrors();
    }

    protected void AddLanguageButton(string LangCode)
    {
        Button newButton = new Button();
        newButton.Style = Application.Current.FindResource("NormalButton") as Style;
        newButton.DataContext = LangCode;
        newButton.Click += RemoveLanguage;
        newButton.Margin = new Thickness(0, 0, 10, 4);

        StackPanel panel = new StackPanel();
        panel.Orientation = Orientation.Horizontal;
        Image image = new Image();
        image.Height = 20;
        BitmapImage source = new BitmapImage();
        source.BeginInit();
        source.UriSource = new Uri("pack://application:,,,/ModAPI;component/resources/textures/Icons/Lang_" + LangCode + ".png");
        source.EndInit();
        image.Source = source;
        image.Margin = new Thickness(0, 0, 5, 0);
        panel.Children.Add(image);

        Image image2 = new Image();
        image2.Height = 24;
        BitmapImage source2 = new BitmapImage();
        source2.BeginInit();
        source2.UriSource = new Uri("pack://application:,,,/ModAPI;component/resources/textures/Icons/Icon_Delete.png");
        source2.EndInit();
        image2.Source = source2;
        image2.Margin = new Thickness(5, 0, 0, 0);
        
        TextBlock label = new TextBlock();
        label.FontSize = 16;
        label.SetResourceReference(TextBlock.TextProperty, "Lang.Languages." + LangCode);
        panel.Children.Add(label);
        panel.Children.Add(image2);

        newButton.Content = panel;
        _LanguageButtons.Add(newButton);
        _Languages.Add(LangCode);
    }

    public string Version
    {
        get
        {
            return Project.Version;
        }
        set
        {
            Project.Version = value;
            Project.SaveConfiguration();
            OnPropertyChanged("Version");
            CheckForErrors();
        }
    }

    protected Visibility _VersionError = Visibility.Collapsed;
    protected Visibility _SettingsError = Visibility.Collapsed;
    protected Visibility _ButtonsError = Visibility.Collapsed;
    protected Visibility _LanguagesError = Visibility.Collapsed;
    protected Visibility _NameError = Visibility.Collapsed;
    protected Visibility _SaveError = Visibility.Collapsed;
    protected Visibility _Error = Visibility.Collapsed;

    public Visibility NameError
    {
        get
        {
            return _NameError;
        }
    }

    public Visibility SaveError
    {
        get
        {
            return _SaveError;
        }
    }

    public Visibility Error
    {
        get
        {
            return _Error;
        }
    }

    public Visibility VersionError
    {
        get
        {
            return _VersionError;
        }
    }

    public Visibility LanguagesError
    {
        get
        {
            return _LanguagesError;
        }
    }
    public Visibility SettingsError
    {
        get
        {
            return _SettingsError;
        }
    }

    public Visibility ButtonsError
    {
        get
        {
            return _ButtonsError;
        }
    }
    
    public void CheckForErrors()
    {
        _VersionError = ModAPI.Data.Mod.Header.VerifyModVersion(Project.Version) ? Visibility.Collapsed : Visibility.Visible;
        _LanguagesError = Project.Languages.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        _NameError = Visibility.Collapsed;
        _SaveError = Project.SaveFailed ? Visibility.Visible : Visibility.Collapsed;

        foreach (string LangCode in Project.Languages)
        {
            if (Project.Name.GetString(LangCode).Trim() == "")
                _NameError = Visibility.Visible;
        }

        _SettingsError = _VersionError == Visibility.Visible || _NameError == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
        
        _ButtonsError = Visibility.Collapsed;
        foreach (ModProjectButton button in Buttons)
        {
            ModProjectButtonViewModel mv = (ModProjectButtonViewModel)button.DataContext;
            if (mv.Error == Visibility.Visible)
            {
                _ButtonsError = Visibility.Visible;
                break;
            }

        
        }

        _Error = _ButtonsError == Visibility.Visible || _SettingsError == Visibility.Visible || _LanguagesError == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;

        OnPropertyChanged("LanguagesError");
        OnPropertyChanged("ButtonsError");
        OnPropertyChanged("VersionError");
        OnPropertyChanged("SettingsError");
        OnPropertyChanged("NameError");
        OnPropertyChanged("SaveError");
        OnPropertyChanged("Error");
    }

    public void AddProjectLanguage(string langCode)
    {
        Project.Languages.Add(langCode);
        AddLanguageButton(langCode);
        CheckForErrors();
    }

    public string ID
    {
        get
        {
            return Project.ID;
        }
        set
        {
            Project.ID = value;
            Project.SaveConfiguration();
            OnPropertyChanged("ID");
            CheckForErrors();
        }
    }


    public MultilingualValue Name
    {
        get
        {
            return Project.Name;
        }
    }

    public MultilingualValue Description
    {
        get
        {
            return Project.Description;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected internal void OnPropertyChanged(string propertyname)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
    }

    protected ObservableCollection<Button> _LanguageButtons = new ObservableCollection<Button>();

    public ObservableCollection<Button> LanguageButtons
    {
        get
        {
            return _LanguageButtons;
        }
    }

    protected ObservableCollection<ModProjectButton> _Buttons = new ObservableCollection<ModProjectButton>();

    public ObservableCollection<ModProjectButton> Buttons
    {
        get
        {
            return _Buttons;
        }
    }

    protected ObservableCollection<string> _Languages = new ObservableCollection<string>();

    public ObservableCollection<string> Languages
    {
        get
        {
            return _Languages;
        }
    }
}
