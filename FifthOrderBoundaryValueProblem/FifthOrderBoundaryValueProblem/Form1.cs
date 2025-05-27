using System.Diagnostics;

namespace FifthOrderBoundaryValueProblem;
public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    public static decimal q = 0.5M;
    string exportFileName = "";
    Stopwatch stopwatch = new Stopwatch();

    private void buttonEPowT_Click(object sender, EventArgs e)
    {
        textBoxF.Text = "e^t";
        textBoxFiT.Text = "e^t";
        textBoxXt.Text = "e^t";
        textBoxA.Text = "0";
        textBoxB.Text = "1";
        textBoxN.Text = "100";
        textBoxQ.Text = "0.5";
        textBoxEpsilon.Text = "0.000000000000001";

        textBoxC1.Text = "1";
        textBoxC2.Text = "1";
        textBoxC3.Text = "1";
        textBoxC4.Text = "e";
        textBoxC5.Text = "e";
    }

    private void buttonTPow6_Click(object sender, EventArgs e)
    {
        textBoxF.Text = "t^6";
        textBoxFiT.Text = "t^6";
        textBoxXt.Text = "t^6";
        textBoxA.Text = "0";
        textBoxB.Text = "1";
        textBoxN.Text = "100";
        textBoxEpsilon.Text = "0.000000000000001";

        textBoxC1.Text = "0";
        textBoxC2.Text = "0";
        textBoxC3.Text = "0";
        textBoxC4.Text = "1";
        textBoxC5.Text = "6";
    }

    private void button1OverTPlus1_Click(object sender, EventArgs e)
    {
        textBoxF.Text = "1/(t+1)";
        textBoxFiT.Text = "1/(t+1)";
        textBoxXt.Text = "1/(t+1)";
        textBoxA.Text = "0";
        textBoxB.Text = "1";
        textBoxN.Text = "100";
        textBoxEpsilon.Text = "0.000000000000001";

        textBoxC1.Text = "1";
        textBoxC2.Text = "-1";
        textBoxC3.Text = "2";
        textBoxC4.Text = "0.5";
        textBoxC5.Text = "-0.25";
    }

    private async void buttonCalculate_Click(object sender, EventArgs e)
    {
        bool isError = !decimal.TryParse(textBoxA.Text, out decimal a);
        isError = !decimal.TryParse(textBoxB.Text, out decimal b) || isError;
        isError = !int.TryParse(textBoxN.Text, out int n) || isError;
        isError = !decimal.TryParse(textBoxQ.Text, out q) || isError;
        isError = !decimal.TryParse(textBoxC1.Text, out decimal c1) || isError;
        isError = !decimal.TryParse(textBoxC2.Text, out decimal c2) || isError;
        isError = !decimal.TryParse(textBoxC3.Text, out decimal c3) || isError;
        isError = !decimal.TryParse(textBoxC4.Text, out decimal c4) && textBoxC4.Text != "e" || isError;
        isError = !decimal.TryParse(textBoxC5.Text, out decimal c5) && textBoxC5.Text != "e" || isError;
        isError = !decimal.TryParse(textBoxEpsilon.Text, out decimal epsilon) || isError;

        if (isError)
        {
            MessageBox.Show(Helpers.error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        c4 = textBoxC4.Text == "e" ? Helpers.e : c4;
        c5 = textBoxC4.Text == "e" ? Helpers.e : c5;
        int.TryParse(textBoxK.Text, out int k);

        stopwatch = new Stopwatch();
        stopwatch.Start();
        timer1.Start();

        var f = Helpers.GetF(textBoxF.Text);
        var fi = Helpers.GetFi(textBoxFiT.Text);
        var xt = Helpers.GetXt(textBoxXt.Text);
        var problem = new FifthOrderBVP(a, b, n, epsilon, c1, c2, c3, c4, c5, f, fi, k);
        await problem.Calculate();
        GenerateResultText(problem, xt);
        //problem.Dispose();

        stopwatch.Stop();
        timer1.Stop();

        var q_Prefix = textBoxF.Text == "e^t" ? $"_q{q * 100:0_00}" : "";
        exportFileName = $"{textBoxPrefix.Text}{textBoxF.Text}{q_Prefix}_n{textBoxN.Text}.txt".Replace('/', '_');
        buttonExport.Enabled = true;
    }

    private void GenerateResultText(FifthOrderBVP problem, Func<BigFloat, BigFloat> xt)
    {
        string[] header = ["ti", "x(m, i)", "x*(ti)", "| x(m, i) - x*(ti) |"];
        string[,] body = new string[problem.n + 1, 4];
        decimal[] errors = new decimal[problem.n + 1];
        for (int i = 0; i <= problem.n; i++)
        {
            body[i, 0] = decimal.Parse(problem.t[i].ToStringWithPrecision(4)).ToString("0.####");
            body[i, 1] = decimal.Parse(problem.xk[i].ToStringWithPrecision(28)).ToString(Helpers.numberFormat);
            body[i, 2] = decimal.Parse(xt(problem.t[i]).ToStringWithPrecision(28)).ToString(Helpers.numberFormat);
            errors[i] = decimal.Parse((problem.xk[i] - xt(problem.t[i])).Abs().ToStringWithPrecision(28));
            body[i, 3] = errors[i].ToString("E");
        }
        var table = Helpers.CreateTable(header, body);
        float timeInSeconds = (float)stopwatch.ElapsedMilliseconds / 1000;

        listBox1.Items.Clear();
        listBox1.Items.Add($"To approximate the values of the function {textBoxF.Text} " +
            $"on the interval [{problem.a}, {problem.b}], dividing it in n = {problem.n} equidistant subintervals,");
        listBox1.Items.Add($"the results were obtained after executing k = {problem.k} iterations in {timeInSeconds:0.000} seconds.");
        listBox1.Items.Add(textBoxF.Text == "e^t" ? $"The value used for q is {q}" : "");
        listBox1.Items.Add($"The obtained precision is {errors.Max():E}");
        listBox1.Items.Add("");
        for (int i = 0; i < table.Count; i++)
        {
            listBox1.Items.Add(table[i]);
        }
    }

    private void buttonExport_Click(object sender, EventArgs e)
    {
        using (var writer = new StreamWriter($"../../../Results/{exportFileName}"))
        {
            foreach (var line in listBox1.Items)
            {
                writer.WriteLine(line);
            }
        }
        Close();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        float timeInSeconds = (float)stopwatch.ElapsedMilliseconds / 1000;
        textBoxTime.Text = $"{timeInSeconds:0.000} s";
        textBoxTime.Update();
    }
}