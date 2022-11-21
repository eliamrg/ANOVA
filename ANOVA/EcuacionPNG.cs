using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WpfMath;

namespace ANOVA
{
    static class EcuacionPNG
    {
        static int i = 0;
        public static string CrearEcuacion(string StringLatexizada)
        {

            string nombreArchivo = @"EcuacionPNG" + i + ".png";

            var parser = new TexFormulaParser();
            var formula = parser.Parse(StringLatexizada);
            var pngByte = formula.RenderToPng(50.0, 0.0, 0.0, "Arial");
            File.WriteAllBytes(nombreArchivo, pngByte);
            i++;
            return nombreArchivo;
        }
    }
}