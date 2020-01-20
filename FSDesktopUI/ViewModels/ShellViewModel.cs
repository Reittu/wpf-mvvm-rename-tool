using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using File = FSDesktopUI.Models.ShellModel.File;

namespace FSDesktopUI.ViewModels
{
    public class ShellViewModel : PropertyChangedBase
    {
        private string _name;
        private string _dirPath;
        private string _baseString;
        private bool _addSuffix = true;
        private BindableCollection<File> _files;
        private System.Windows.Threading.DispatcherTimer _throttleTimer;
        private List<int> _selectedItemsIndices = new List<int>();

        // CLI:
        // dotnet publish -r win10-x64 -p:PublishSingleFile=true

        #region Public Properties
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string DirPath
        {
            get { return _dirPath; }
            set
            {
                _dirPath = value;
                NotifyOfPropertyChange(() => DirPath);
            }
        }

        public string BaseString
        {
            get { return _baseString; }
            set
            {
                _baseString = value;
                NotifyOfPropertyChange(() => BaseString);
            }
        }

        public bool AddSuffix
        {
            get { return _addSuffix; }
            set
            {
                _addSuffix = value;
                UpdateItemNames();
                NotifyOfPropertyChange(() => AddSuffix);
            }
        }

        // BindableCollection is not necessarily needed right now.
        // Could just use a normal List as we are replacing the list and not removing or adding items.
        public BindableCollection<File> Files
        {
            get { return _files; }
            set 
            {
                _files = value;
                NotifyOfPropertyChange(() => Files);
                NotifyOfPropertyChange(() => CanRenameFiles);
                NotifyOfPropertyChange(() => CanUpdateItemNamesThrottle);
            }
        }

        #endregion

        #region Public Methods
        // Caliburn checks for all Can-prefixed methods and does the magic behind the scenes
        // to handle the interactivity of each element referring to those methods (Guard Method).
        public bool CanRenameFiles
        {
            get { return _selectedItemsIndices.Count > 0; }
        }

        public void RenameFiles()
        {
            try
            {
                for (int i = 0; i < _selectedItemsIndices.Count; i++)
                {
                    File currentFile = Files[_selectedItemsIndices[i]];
                    System.IO.File.Move(_dirPath + "/" + currentFile.OldName, _dirPath + "/" + currentFile.NewName);
                }
                MessageBox.Show(_selectedItemsIndices.Count.ToString() + " files successfully renamed.");
                UpdateList(Directory.GetFiles(_dirPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            
        }

        // As ListView's SelectedItems are not accessible through binding, we have to pass the whole UIElement.
        public void HandleSelection(ListView listView)
        {
            _selectedItemsIndices.Clear();
            int foundIndex;
            for(int i = 0; i < listView.SelectedItems.Count; i++)
            {
                foundIndex = Files.IndexOf((File)listView.SelectedItems[i]);
                _selectedItemsIndices.Add(foundIndex);
            }
            // Makes sure the selected items are assigned an id affix from top to bottom (NewName).
            _selectedItemsIndices.Sort();

            int counter = 0;
            for(int i = 0; i < Files.Count; i++)
            {
                if (counter < _selectedItemsIndices.Count && _selectedItemsIndices[counter] == i)
                {
                    Files[i].NewName = GetNewName(Files[i].OldName, counter++);
                    Files[i].IsSelected = true;
                }
                    
                else
                {
                    Files[i].NewName = Files[i].OldName;
                    Files[i].IsSelected = false;
                } 
            }
            Files = new BindableCollection<File>(Files); // Forces an UI refresh.
        }

        public void GetDirectoryPath()
        {
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            var result = fbd.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                DirPath = fbd.SelectedPath;
                if (!Directory.Exists(fbd.SelectedPath)) MessageBox.Show("Invalid path: directory does not exist.");
                else UpdateList(Directory.GetFiles(fbd.SelectedPath));
            }
            fbd.Dispose();
        }


        public bool CanUpdateItemNamesThrottle
        {
            get { return Files?.Count > 0; }
        }

        public void UpdateItemNamesThrottle()
        {
            // The timer is reseted if called again while it is running.
            if (_throttleTimer != null)
                _throttleTimer.Stop();

            _throttleTimer = new System.Windows.Threading.DispatcherTimer();
            _throttleTimer.Tick += (s, args) =>
            {
                // This block is run 500ms after no new calls.
                UpdateItemNames();
                _throttleTimer.Stop();
            };
            _throttleTimer.Interval = TimeSpan.FromMilliseconds(500);
            _throttleTimer.Start();
        }

        #endregion

        #region Private Helper Methods
        private void UpdateList(string[] fileEntries)
        {
            BindableCollection<File> newFiles = new BindableCollection<File>();

            for (int i = 0; i < fileEntries.Length; i++)
            {
                string fileName = Path.GetFileName(fileEntries[i]);
                newFiles.Add(new File()
                {
                    IsSelected = false,
                    OldName = fileName,
                    NewName = fileName
                });
            }
            Files = newFiles;
        }

        private void UpdateItemNames()
        {
            for (int i = 0; i < _selectedItemsIndices.Count; i++)
            {
                File currentFile = Files[_selectedItemsIndices[i]];
                currentFile.NewName = GetNewName(currentFile.OldName, i);
            }
            Files = new BindableCollection<File>(Files); // Forces an UI refresh.
        }

        private string GetNewName(string oldName, int i)
        {
            return (AddSuffix
                        ? BaseString + (i + 1)
                        : (i + 1) + BaseString
                    )
                    + Path.GetExtension(oldName);
        }
        #endregion


    }


}
