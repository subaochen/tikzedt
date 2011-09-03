﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using TikzEdt.Views;
using System.Windows.Threading;

namespace TikzEdt.ViewModels
{
    public class MainWindowVM : ViewModelBase
    {
        public ObservableCollection<TEDocumentView> Documents { get; private set; }

        private TEDocumentView _ActiveView = null;
        public TEDocumentView ActiveView
        {
            get { return _ActiveView; }
            set
            {
                if (_ActiveView != value)
                {
                    _ActiveView = value;
                    NotifyPropertyChanged("ActiveView");
                    NotifyPropertyChanged("ActiveVM");
                }
            }
        }
        public TEDocumentVM ActiveVM
        {
            get
            {
                if (_ActiveView != null)
                    return ActiveView.TheVM;
                else
                    return null;
            }
        }

        TEMode _EditorMode = TEMode.Wysiwyg;
        public TEMode EditorMode
        {
            get { return _EditorMode; }
            set
            {
                if (value != _EditorMode)
                {
                    _EditorMode = value;
                    NotifyPropertyChanged("EditorMode");
                }
            }
        }

        bool _ShowOverlay = true;
        /// <summary>
        /// Indicates whether the overlay should be displayed in Wysiwyg mode
        /// </summary>
        public bool ShowOverlay
        {
            get { return _ShowOverlay; }
            set
            {
                if (value != _ShowOverlay)
                {
                    _ShowOverlay = value;
                    NotifyPropertyChanged("ShowOverlay");
                }
            }
        }

        double _RasterWidth = 1;
        public double RasterWidth
        {
            get { return _RasterWidth; }
            set
            {
                if (value != _RasterWidth)
                {
                    _RasterWidth = value;
                    NotifyPropertyChanged("RasterWidth");
                }
            }
        }
        double _RasterRadialOffset = 0;
        public double RasterRadialOffset
        {
            get { return _RasterRadialOffset; }
            set
            {
                if (value != _RasterRadialOffset)
                {
                    _RasterRadialOffset = value;
                    NotifyPropertyChanged("RasterRadialOffset");
                }
            }
        }
        int _RasterSteps = 18;
        public int RasterSteps
        {
            get { return _RasterSteps; }
            set
            {
                if (value != _RasterSteps)
                {
                    _RasterSteps = value;
                    NotifyPropertyChanged("RasterSteps");
                }
            }
        }

        OverlayToolType _CurrentTool = OverlayToolType.move;
        public OverlayToolType CurrentTool
        {
            get { return _CurrentTool; }
            set
            {
                if (value != _CurrentTool)
                {
                    _CurrentTool = value;
                    NotifyPropertyChanged("CurrentTool");
                }
            }
        }


        //ObservableCollection<AvalonDock.DocumentContent> _DocumentsAD = new ObservableCollection<AvalonDock.DocumentContent>();
        //[CLSCompliant(false)]
        //public ObservableCollection<AvalonDock.DocumentContent> DocumentsAD { get { return _DocumentsAD; } set { DocumentsAD = value; } }
        //{ get { return Documents.Select<TEDocumentVM, TEDocumentContent>( (s, i) => s.doccontent ); } } 
        public AvalonDock.DocumentContent SelectedTab { get; set; }

        public MainWindowVM()
        {
            Documents = new ObservableCollection<TEDocumentView>();
            // create one document to start with
            //AddDocument();

            /*  for (int i = 0; i < 3; i++)
              {
                  AvalonDock.DocumentContent ad = new AvalonDock.DocumentContent() { Title = "Hallo Welt" };
                  ICSharpCode.AvalonEdit.TextEditor aedit = new ICSharpCode.AvalonEdit.TextEditor();
                  //avalonDoc.DataContext = tabViewModel;
                  ad.Content = aedit;
                  //DocumentContents.Insert(0, avalonDoc);
                  //avalonDoc.Focus();

                  DocumentsAD.Add(ad);
              }*/
        }

        /// <summary>
        /// Tries to close all open files.
        /// </summary>
        /// <returns>True if successful, false if some file(s) could not be closed.</returns>
        public bool TryCloseAll()
        {
            for (int i = Documents.Count - 1; i >= 0; i--)
            {
                Documents[i].TheVM.CloseCommand.Execute(this);
                if (Documents.Count > i)
                    return false;
            }
            return true;
        }

        public RelayCommand NewFileCommand
        {
            get { return new RelayCommand((object sender) => AddDocument()); }
        }


        /// <summary>
        /// Adds a new document. If cFile != null, it is loaded from file.
        /// </summary>
        /// <param name="cFile"></param>
        public void AddDocument(string cFile = null)
        {
            TEDocumentVM doc;
            try
            {
                doc = new TEDocumentVM(cFile);
                doc.OnClose += new EventHandler(doc_OnClose);
                TEDocumentView view = new TEDocumentView(doc);
                doc.OnSaved += ((s, e) => MainWindow.recentFileList.InsertFile((s as TEDocumentVM).FilePath));
                Documents.Insert(0, view);
                ActiveView = view;
                if (cFile != null)
                    MainWindow.recentFileList.InsertFile(cFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (cFile != null)
                    MainWindow.recentFileList.RemoveFile(cFile);

            }
        }

        public void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ActiveVM != null)
                ofd.InitialDirectory = System.IO.Path.GetDirectoryName(ActiveVM.FilePath);
            //ofd.InitialDirectory = Directory.GetCurrentDirectory();
            //ofd.FileName = System.IO.Path.GetFileName(CurFile);
            if (ofd.ShowDialog() == true)
            {
                AddDocument(ofd.FileName);
            }
        }

        void doc_OnClose(object sender, EventArgs e)
        {
            TEDocumentView view = ViewFromVM(sender as TEDocumentVM);
            if (view != null)
            {
                (sender as TEDocumentVM).OnClose -= new EventHandler(doc_OnClose);
                Documents.Remove(view);
            }
        }

        TEDocumentView ViewFromVM(TEDocumentVM vm)
        {
            return Documents.FirstOrDefault(view => (view.TheVM == vm));
        }
    }

    public class ViewModelBase : DispatcherObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }



    /// <summary>
    /// The mode TE can be operated in
    /// </summary>
    public enum TEMode { Wysiwyg, Preview, Production }
}
