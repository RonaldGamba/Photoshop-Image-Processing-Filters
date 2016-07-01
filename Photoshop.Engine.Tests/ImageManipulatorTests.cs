using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Photoshop.Engine.Tests
{
    [TestClass]
    public class ImageManipulatorTests
    {
        [TestMethod]
        public void TransformArrayByteToColorRepresentationMatrix()
        {
            //Arrange
            var bytes = new byte[] { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9 };
            var width = 3;
            var height = 3;

            //Act
            var matrix = ImageManipulator.TransformArrayByteToColorRepresentationMatrix(bytes, width, height);

            //Assert
            Assert.AreEqual(matrix[0, 0].R, 1);
            Assert.AreEqual(matrix[0, 0].G, 1);
            Assert.AreEqual(matrix[0, 0].B, 1);

            Assert.AreEqual(matrix[0, 1].R, 2);
            Assert.AreEqual(matrix[0, 1].G, 2);
            Assert.AreEqual(matrix[0, 1].B, 2);

            Assert.AreEqual(matrix[0, 2].R, 3);
            Assert.AreEqual(matrix[0, 2].G, 3);
            Assert.AreEqual(matrix[0, 2].B, 3);

            Assert.AreEqual(matrix[1, 0].R, 4);
            Assert.AreEqual(matrix[1, 0].G, 4);
            Assert.AreEqual(matrix[1, 0].B, 4);

            Assert.AreEqual(matrix[1, 1].R, 5);
            Assert.AreEqual(matrix[1, 1].G, 5);
            Assert.AreEqual(matrix[1, 1].B, 5);

            Assert.AreEqual(matrix[1, 2].R, 6);
            Assert.AreEqual(matrix[1, 2].G, 6);
            Assert.AreEqual(matrix[1, 2].B, 6);

            Assert.AreEqual(matrix[2, 0].R, 7);
            Assert.AreEqual(matrix[2, 0].G, 7);
            Assert.AreEqual(matrix[2, 0].B, 7);

            Assert.AreEqual(matrix[2, 1].R, 8);
            Assert.AreEqual(matrix[2, 1].G, 8);
            Assert.AreEqual(matrix[2, 1].B, 8);

            Assert.AreEqual(matrix[2, 2].R, 9);
            Assert.AreEqual(matrix[2, 2].G, 9);
            Assert.AreEqual(matrix[2, 2].B, 9);
        }

        [TestMethod]
        public void ApplyCorrelationOnMatrix()
        {
            //Arrange
            var bytes = new byte[] { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9 };
            var matrix = ImageManipulator.TransformArrayByteToColorRepresentationMatrix(bytes, 3, 3);
            var filter = new float[,] { {1 ,1 ,1},
                                        {1 ,1 ,1 },
                                        {1 ,1 ,1 } };

            //Act
            //ImageManipulator.ApplyCorrelation(matrix, filter);

            //Assert
            Assert.AreEqual(matrix[1, 1].R, 45);
            Assert.AreEqual(matrix[1, 1].G, 45);
            Assert.AreEqual(matrix[1, 1].B, 45);
        }

        [TestMethod]
        public void PixelMatrixToArrayByte()
        {
            //Arrange
            var bytes = new byte[] { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9 };
            var matrix = ImageManipulator.TransformArrayByteToColorRepresentationMatrix(bytes, 3, 3);

            //Act
            var result = ImageManipulator.PixelMatrixToArrayByte(matrix, 27);

            //Assert
            Assert.AreEqual(bytes.Length, result.Length);

            for (int i = 0; i < result.Length; i++)
                Assert.AreEqual(bytes[i], result[i]);
        }
    }
}
