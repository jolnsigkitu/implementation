using System.Collections.Generic;

namespace ITU.Lang.SampleProgram.Util
{
    public class ClassNames
    {
        private Dictionary<string, bool> classes = new Dictionary<string, bool>();

        public bool this[string className]
        {
            get => classes[className];
            set => classes[className] = value;
        }

        public override string ToString()
        {
            var buf = new List<string>();

            foreach (var (className, isIncluded) in classes)
            {
                if (isIncluded)
                {
                    buf.Add(className);
                }
            }

            return string.Join(" ", buf);
        }
    }
}
