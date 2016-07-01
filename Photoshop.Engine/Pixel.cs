namespace Photoshop.Engine
{
    public struct Pixel
    {
        private readonly double _r;
        private readonly double _g;
        private readonly double _b;
        private readonly double _a;

        public Pixel(double r, double g, double b, double a)
        {
            _r = r;
            _g = g;
            _b = b;
            _a = a;
        }

        public double R
        {
            get
            {
                return _r;
            }
        }

        public double G
        {
            get
            {
                return _g;
            }
        }

        public double B
        {
            get
            {
                return _b;
            }
        }

        public double A
        {
            get
            {
                return _a;
            }
        }

        public override string ToString()
        {
            return string.Format("R:{0} - G:{1} - B:{2}", _r, _g, _b);
        }
    }
}
