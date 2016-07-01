﻿using Microsoft.Win32;
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
        private Bitmap _originalImage1;
        private Bitmap _originalImage2;

        private RelayCommand _loadImage1Command;
        public RelayCommand LoadImage1Command
        {
            get
            {
                if (_loadImage1Command == null)
                    _loadImage1Command = new RelayCommand(() =>
                    {
                        var fileDialog = new OpenFileDialog();
                        var result = fileDialog.ShowDialog();

                        if (result.HasValue && result.Value)
                        {
                            _originalImage1 = new Bitmap(fileDialog.FileName);
                            Image = BitmapHelper.BitmapToBitmapImage(BitmapHelper.Fix(_originalImage1));
                        }
                    });

                return _loadImage1Command;
            }
        }

        private RelayCommand _loadImage2Command;
        public RelayCommand LoadImage2Command
        {
            get
            {
                if (_loadImage2Command == null)
                    _loadImage2Command = new RelayCommand(() =>
                    {
                        var fileDialog = new OpenFileDialog();
                        var result = fileDialog.ShowDialog();

                        if (result.HasValue && result.Value)
                        {
                            _originalImage2 = new Bitmap(fileDialog.FileName);
                            Image2 = BitmapHelper.BitmapToBitmapImage(BitmapHelper.Fix(_originalImage2));
                        }
                    });

                return _loadImage2Command;
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

        private RelayCommand _applyHighPassFilterCommand;
        public RelayCommand ApplyHighPassFilterCommand
        {
            get
            {
                return _applyHighPassFilterCommand ?? (_applyHighPassFilterCommand = new RelayCommand(ApplyHighPassFilter));
            }
        }

        private RelayCommand _bitwiseOrOperationCommand;
        public RelayCommand BitwiseOrOperationCommand
        {
            get
            {

                if (_bitwiseOrOperationCommand == null)
                    _bitwiseOrOperationCommand = new RelayCommand(BitwiseOrOperation);

                return _bitwiseOrOperationCommand;
            }
        }

        private RelayCommand _bitwiseXorOperationCommand;
        public RelayCommand BitwiseXorOperationCommand
        {
            get
            {

                if (_bitwiseXorOperationCommand == null)
                    _bitwiseXorOperationCommand = new RelayCommand(BitwiseXorOperation);

                return _bitwiseXorOperationCommand;
            }
        }

        private RelayCommand _bitwiseAndOperationCommand;
        public RelayCommand BitwiseAndOperationCommand
        {
            get
            {

                if (_bitwiseAndOperationCommand == null)
                    _bitwiseAndOperationCommand = new RelayCommand(BitwiseAndOperation);

                return _bitwiseAndOperationCommand;
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
            histogramView.DataContext = new HistogramViewModel(ImageManipulator.GenerateImageHistogram(_originalImage1));
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

        private BitmapImage _image2;
        public BitmapImage Image2
        {
            get { return _image2; }
            set
            {
                _image2 = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Image2"));
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
            var result = ImageManipulator.ApplyGrayScaleTo(_originalImage1);
            ResultImage = BitmapHelper.BitmapToBitmapImage(result);
        }
        
        private void ApplyLowPassFilter()
        {
            var bitmapResult = ImageManipulator.ApplyLowPassFilter(_originalImage1);
            ResultImage = BitmapHelper.BitmapToBitmapImage(bitmapResult);
        }

        private void ApplyHighPassFilter()
        {
            ImageManipulator.ApplyHighPassFilter(_originalImage1);
            ResultImage = BitmapHelper.BitmapToBitmapImage(_originalImage1);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void BitwiseOrOperation()
        {
            var img1 = _originalImage1.Clone() as Bitmap;
            var img2 = _originalImage2.Clone() as Bitmap;

            ResultImage = BitmapHelper.BitmapToBitmapImage(ImageManipulator.BitwiseOperation(img1, img2, (v1, v2) => (byte)(v1 | v2)));
        }

        private void BitwiseAndOperation()
        {
            var img1 = _originalImage1.Clone() as Bitmap;
            var img2 = _originalImage2.Clone() as Bitmap;

            ResultImage = BitmapHelper.BitmapToBitmapImage(ImageManipulator.BitwiseOperation(img1, img2, (v1, v2) => (byte)(v1 & v2)));
        }

        private void BitwiseXorOperation()
        {
            var img1 = _originalImage1.Clone() as Bitmap;
            var img2 = _originalImage2.Clone() as Bitmap;

            ResultImage = BitmapHelper.BitmapToBitmapImage(ImageManipulator.BitwiseOperation(img1, img2, (v1, v2) => (byte)(v1 ^ v2)));
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
}
