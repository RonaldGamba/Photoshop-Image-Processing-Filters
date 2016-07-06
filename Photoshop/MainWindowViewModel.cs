using Microsoft.Win32;
using Photoshop.Engine;
using Photoshop.ViewModels;
using Photoshop.Views;
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
        private Bitmap _originalImage;

        private RelayCommand _loadImageCommand;
        public RelayCommand LoadImageCommand
        {
            get
            {
                if (_loadImageCommand == null)
                    _loadImageCommand = new RelayCommand(() =>
                    {
                        var loadedImage = LoadImage();

                        if (loadedImage != null)
                        {
                            _originalImage = loadedImage;
                            Image = BitmapHelper.BitmapToBitmapImage(_originalImage);
                        }
                    });

                return _loadImageCommand;
            }
        }

        private RelayCommand _restoreOriginalImageCommand;
        public RelayCommand RestoreOriginalImageCommand
        {
            get
            {
                if (_restoreOriginalImageCommand == null)
                    _restoreOriginalImageCommand = new RelayCommand(() =>
                    {
                        Image = BitmapHelper.BitmapToBitmapImage(_originalImage);
                    });

                return _restoreOriginalImageCommand;
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

        private RelayCommand _applyLowPassFilterCommand;
        public RelayCommand ApplyLowPassFilterCommand
        {
            get
            {
                return _applyLowPassFilterCommand ?? (_applyLowPassFilterCommand = new RelayCommand(ApplyLowPassFilter));
            }
        }

        private RelayCommand _applyLaplacianFilterCommand;
        public RelayCommand ApplyLaplacianFilterCommand
        {
            get
            {
                return _applyLaplacianFilterCommand ?? (_applyLaplacianFilterCommand = new RelayCommand(ApplyLaplacianFilter));
            }
        }

        private RelayCommand _applyHighPassFilterCommand;
        public RelayCommand ApplyHighPassFilterCommand
        {
            get
            {
                return _applyHighPassFilterCommand ?? (_applyHighPassFilterCommand = new RelayCommand(ApplyHighPassFilter));
            }
        }

        private RelayCommand _applyPrewittCommand;
        public RelayCommand ApplyPrewittCommand
        {
            get
            {
                return _applyPrewittCommand ?? (_applyPrewittCommand = new RelayCommand(ApplyPrewitt));
            }
        }

        private RelayCommand _applySobelCommand;
        public RelayCommand ApplySobelCommand
        {
            get
            {
                return _applySobelCommand ?? (_applySobelCommand = new RelayCommand(ApplySobel));
            }
        }

        private RelayCommand _applyRobertCommand;
        public RelayCommand ApplyRobertCommand
        {
            get
            {
                return _applyRobertCommand ?? (_applyRobertCommand = new RelayCommand(ApplyRobert));
            }
        }

        private RelayCommand<eMedianFilterQuality> _applyMedianFilterCommand;
        public RelayCommand<eMedianFilterQuality> ApplyMedianFilterCommand
        {
            get
            {
                return _applyMedianFilterCommand ?? (_applyMedianFilterCommand = new RelayCommand<eMedianFilterQuality>(ApplyMedianFilter));
            }
        }

        private RelayCommand _showHistogramCommand;
        public RelayCommand ShowHistogramCommand
        {
            get
            {
                if (_showHistogramCommand == null)
                    _showHistogramCommand = new RelayCommand(ShowHistogram);

                return _showHistogramCommand;
            }
        }

        private void ShowHistogram()
        {
            var histogramView = new HistogramView();
            histogramView.DataContext = new HistogramViewModel(ImageManipulator.GenerateImageHistogram(_originalImage), ImageManipulator.EqualizeHistogram(_originalImage));
            histogramView.ShowDialog();
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

        private BitmapImage _resultImage;
        public BitmapImage ResultImage
        {
            get
            {
                return _resultImage;
            }
            set
            {
                _resultImage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ResultImage"));
            }
        }

        private void ChangeToGrayScale()
        {
            var result = ImageManipulator.ApplyGrayScaleTo(_originalImage);
            Image = BitmapHelper.BitmapToBitmapImage(result);
        }

        private void ApplyLowPassFilter()
        {
            var bitmapResult = ImageManipulator.ApplyLowPassFilter(_originalImage);
            Image = BitmapHelper.BitmapToBitmapImage(bitmapResult);
        }

        private void ApplyLaplacianFilter()
        {
            var result = ImageManipulator.ApplyLaplacianFilter(_originalImage);
            Image = BitmapHelper.BitmapToBitmapImage(result);
        }

        private void ApplyHighPassFilter()
        {
            var result = ImageManipulator.ApplyHighPassFilter(_originalImage);
            Image = BitmapHelper.BitmapToBitmapImage(result);
        }

        private void ApplyPrewitt()
        {
            var result = ImageManipulator.ApplyPrewittFilter(_originalImage);
            Image = BitmapHelper.BitmapToBitmapImage(result);
        }

        private void ApplySobel()
        {
            var result = ImageManipulator.ApplySobelFilter(_originalImage);
            Image = BitmapHelper.BitmapToBitmapImage(result);
        }

        private void ApplyRobert()
        {
            var result = ImageManipulator.ApplyRobertFilter(_originalImage);
            Image = BitmapHelper.BitmapToBitmapImage(result);
        }

        private void ApplyMedianFilter(eMedianFilterQuality quality)
        {
            var result = ImageManipulator.ApplyMedianPassFilter(_originalImage, quality);
            Image = BitmapHelper.BitmapToBitmapImage(result);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        
        private Bitmap LoadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Selecione uma image.";
            ofd.Filter = "Arquivo de imagens|*.bmp;*.jpg;*.jpeg;*.png";
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                StreamReader streamReader = new StreamReader(ofd.FileName);
                var image = (Bitmap)Bitmap.FromStream(streamReader.BaseStream);
                streamReader.Close();
                return BitmapHelper.Fix(image);
            }

            return null;
        }
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

    public class RelayCommand<T> : ICommand
    {
        private Action<T> _a;

        public RelayCommand(Action<T> a)
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
            if (typeof(T).IsEnum)
                _a.Invoke((T)Enum.Parse(typeof(T), parameter.ToString()));
            else
                _a.Invoke((T)parameter);
        }
    }
}
