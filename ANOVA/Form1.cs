using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.DataVisualization.Charting;
using System.Windows.Forms;
using Accord.Statistics;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Testing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ANOVA
{
    public partial class Form1 : Form
    {
        public double Ftablas = 0;

        public Form1()
        {
            InitializeComponent();
            cbx_significancia.SelectedIndex = 0;
            generarTabla();
            pbx_rechazo.Image = new Bitmap(EcuacionPNG.CrearEcuacion("F_0 \\ge F_{\\alpha,a-1,N-a}"));
            Rectangle r = new Rectangle(0, 0, pbx_hipotesis1.Width, pbx_hipotesis1.Height);
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            int d = 50;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            pbx_hipotesis1.Region = new Region(gp);
            pbx_hipotesis2.Region = new Region(gp);
            pbx_f.Region = new Region(gp);
            pbx_rechazo.Region = new Region(gp);

        }


        private void generarTabla()
        {
            dgv_datos.Rows.Clear();
            dgv_datos.Columns.Clear();
            dgv_datos.Columns.Add("tratamientos", "Tratamientos");
            dgv_datos.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            string h0 = "H_0:", h1 = "H_1:\\mu_i\\neq\\mu_j \\forall par(i,j) ";//\\in 

            for (int i=1;i<=num_obseervaciones.Value;i++) { 
                dgv_datos.Columns.Add("obs"+i, "Observación "+i); 
                dgv_datos.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                //h1 += i;
            }
            for (int i = 1; i <= num_tratamientos.Value; i++) { 
                dgv_datos.Rows.Add(i.ToString());
                h0 += "\\mu_{"+i+"}";
                if (i != num_tratamientos.Value)
                    h0 += "=";
            }

            pbx_hipotesis1.Image = new Bitmap(EcuacionPNG.CrearEcuacion(h0));
            pbx_hipotesis2.Image = new Bitmap(EcuacionPNG.CrearEcuacion(h1));
            actualizarF();
        }

        private void num_tratamientos_ValueChanged(object sender, EventArgs e)
        {
            generarTabla();
        }

        private void num_obseervaciones_ValueChanged(object sender, EventArgs e)
        {
            generarTabla();
        }

        private void btn_calcular_Click(object sender, EventArgs e)
        {
            dgv_ANOVA.Rows.Clear();

            double[][] samples = LeerDatos();

            OneWayAnova anova = new OneWayAnova(samples);


            dgv_resultado.DataSource = anova.Table;

            string[,] datos = new string[dgv_resultado.Rows.Count, dgv_resultado.Columns.Count];

            foreach (DataGridViewRow row in dgv_resultado.Rows)
            {

                foreach (DataGridViewColumn column in dgv_resultado.Columns)
                {
                    if (row.Cells[column.Index].Value != null)
                    {
                        switch (row.Cells[column.Index].Value.ToString())
                        {
                            case "Between-Groups":
                                datos[row.Index, column.Index] = "Tratamiento";
                                break;
                            case "Within-Groups":
                                datos[row.Index, column.Index] = "Error";
                                break;
                            default:
                                datos[row.Index, column.Index] = row.Cells[column.Index].Value.ToString();
                                break;
                        }
                        if (row.Index == 2 && column.Index == 3)
                            datos[row.Index, column.Index] = "-";
                    }
                    else { datos[row.Index, column.Index] = "-"; }
                }
            }

            for (int i = 0; i < datos.GetLength(0); i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(this.dgv_ANOVA);
                for (int j = 0; j < datos.GetLength(1); j++)
                {
                    row.Cells[j].Value = datos[i, j];
                    //Console.Write(datos[i, j] + "\t");
                }
                this.dgv_ANOVA.Rows.Add(row);
                //Console.WriteLine();
            } 
            double Fanova = 0;
            
            if (dgv_ANOVA.Rows[0].Cells["dataGridViewTextBoxColumn5"].Value.ToString()!="NaN")
            {
                Console.WriteLine(Fanova);
                Fanova=Convert.ToDouble(dgv_ANOVA.Rows[0].Cells["dataGridViewTextBoxColumn5"].Value.ToString());
                pbx_rechazo.Image = new Bitmap(EcuacionPNG.CrearEcuacion(Math.Round(Fanova,4) + "\\ge"+Ftablas ));
                if (Fanova >= Ftablas)
                {
                    rtb_Conclusion.Text="Se Rechaza Ho\nPor lo que con una confianza de "+((1 -Convert.ToDouble(cbx_significancia.Text))*100)+"% se concluye que no existe igualdad en cada una de las medias de los tratamientos";
                }
                else
                {
                    rtb_Conclusion.Text = "Se Acepta Ho\nPor lo que con una confianza de " + ((1 - Convert.ToDouble(cbx_significancia.Text))*100) + "% se concluye que si existe igualdad en cada una de las medias de los tratamientos";
                }
            }


            


        }
        private double[][] LeerDatos() {

            double[][] datos=new double[Convert.ToInt32(num_tratamientos.Value)][];
            double[] observaciones = new double[Convert.ToInt32(num_obseervaciones.Value)];

            //datos[1][2] = 0;

            for (int i = 0; i < num_tratamientos.Value;i++)
            {
                for (int j = 0; j < num_obseervaciones.Value; j++)
                {
                    //Console.WriteLine(i+" "+ (j+1));
                    if(dgv_datos.Rows[i].Cells[j + 1].Value != null) 
                        observaciones[j] = double.Parse(dgv_datos.Rows[i].Cells[(j+1)].Value.ToString());
                    else
                        observaciones[j] = 0;
                }
                datos[i]=observaciones;
                observaciones = new double[Convert.ToInt32(num_obseervaciones.Value)];
            }
            
            return datos;
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void num_tratamientos_KeyUp(object sender, KeyEventArgs e)
        {
            //dgv_datos.Rows.Add(num_tratamientos.Value.ToString());
        }

        private void cbx_significancia_SelectedIndexChanged(object sender, EventArgs e)
        {
            actualizarF();
        }
        private void actualizarF()
        {
            //cbx_significancia.SelectedIndex = 0;
            //Console.WriteLine(cbx_significancia.SelectedItem);
            double significancia= Convert.ToDouble(cbx_significancia.SelectedItem);
            int grado1 = Convert.ToInt32(num_tratamientos.Value) - 1; 
            int grado2=Convert.ToInt32((num_tratamientos.Value * num_obseervaciones.Value) - num_tratamientos.Value);
            var chart = new Chart();

//            Console.WriteLine(significancia + " " + grado1 + " " + grado2);
            var value = chart.DataManipulator.Statistics.InverseFDistribution(significancia, grado1, grado2);
            
            string f = "F_{" + significancia + "," + grado1 + "," + grado2 + "}="+ Math.Round(value, 4) ;
            pbx_f.Image = new Bitmap(EcuacionPNG.CrearEcuacion(f));
            Ftablas = Math.Round(value, 4) ;
        }

        private void rtb_Conclusion_TextChanged(object sender, EventArgs e)
        {

        }

        private void dgv_datos_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            double i;

            if (!double.TryParse(Convert.ToString(e.FormattedValue), out i))
            {
                e.Cancel = true;
                MessageBox.Show("El contenido debe ser un numero", "Error al ingresar datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btn_limpiar_Click(object sender, EventArgs e)
        {
            num_obseervaciones.Value = 4;
            num_tratamientos.Value = 4;
            num_obseervaciones.Value = 3;
            num_tratamientos.Value = 3;
            pbx_f.Image = null;
            pbx_rechazo.Image = null;
            pbx_hipotesis1.Image = null;
            pbx_hipotesis2.Image = null;
            dgv_ANOVA.Rows.Clear();
            rtb_Conclusion.Text = "";
        }
    }
}
