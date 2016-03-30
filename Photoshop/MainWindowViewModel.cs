using Microsoft.Win32;
using Photoshop.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Photoshop
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Bitmap _originalBitmap;

        private RelayCommand _openDialogCommand;
        public RelayCommand OpenDialogCommand
        {
            get
            {
                if (_openDialogCommand == null)
                    _openDialogCommand = new RelayCommand(() =>
                    {
                        var fileDialog = new OpenFileDialog();
                        fileDialog.ShowDialog();
                        _originalBitmap = new Bitmap(fileDialog.FileName);
                        this.Image = BitmapToImageSource(new Bitmap(_originalBitmap));
                    });

                return _openDialogCommand;
            }
        }

        private RelayCommand _changeToGrayScaleCommand;
        public RelayCommand ChangeToGrayScaleCommand
        {
            get
            {
                return _changeToGrayScaleCommand ?? (_changeToGrayScaleCommand = new RelayCommand(ChangeToGrayScale));
            }
        }

        private RelayCommand _applyCorrelationCommand;
        public RelayCommand ApplyCorrelationCommand
        {
            get
            {
                return _applyCorrelationCommand ?? (_applyCorrelationCommand = new RelayCommand(ApplyCorrelation));
            }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Image"));
            }
        }


        private void ChangeToGrayScale()
        {
            ImageManipulator.ApplyGrayScaleTo(_originalBitmap);
            this.Image = BitmapToImageSource(_originalBitmap);
        }

        private void ApplyCorrelation()
        {
            ImageManipulator.ApplyCorrelation(_originalBitmap);
            this.Image = BitmapToImageSource(_originalBitmap);
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    public class RelayCommand : ICommand
    {
        private Action _a;

        public RelayCommand(Action a)
        {
            _a = a;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _a.Invoke();
        }
    }
}
