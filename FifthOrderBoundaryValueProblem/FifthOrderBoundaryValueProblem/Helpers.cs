namespace FifthOrderBoundaryValueProblem;
public static class Helpers
{
    public const string error = "Please check your input values and try again!";
    public const string numberFormat = "0.############################";
    public const decimal e = 2.7182818284590452353602874713M;

    public static Func<BigFloat, BigFloat, BigFloat, BigFloat> GetF(string fAsString)
    {
        var _0 = new BigFloat(0);
        var _1 = new BigFloat(1);
        var _2 = new BigFloat(2);
        var _5 = new BigFloat(5);
        var _10 = new BigFloat(10);
        var _120 = new BigFloat(120);
        var _q = new BigFloat(Form1.q);
        return fAsString switch
        {
            "e^t" => (s, v, w) => v / _._2 + ((_1 - _q) * s).Exp() * w / _._2,
            "t^6" => (s, v, w) => _._720 * s + v * w / _._4 - v * v * v / _._4,
            "1/(t+1)" => (s, v, w) => _0 - _120 / (s + _1) / (s + _1) / (s + _1) / (s + _1) / (s + _1) / (s + _1)
                                                + (s + _2) / _10 * v * v * w - v / _5 / (s + _1),
            _ => (s, v, w) => _0,
        };
    }

    public static Func<BigFloat, BigFloat> GetFi(string fiAsString)
    {
        var _q = new BigFloat(Form1.q);
        return fiAsString switch
        {
            "e^t" => t => _q * t,
            "t^6" => t => t * t,
            "1/(t+1)" => t => t / _._2,
            _ => t => _q,
        };
    }

    public static Func<BigFloat, BigFloat> GetXt(string xtAsString)
    {
        var _1 = new BigFloat(1);
        return xtAsString switch
        {
            "e^t" => t => t.Exp(),
            "t^6" => t => t * t * t * t * t * t,
            "1/(t+1)" => t => _1 / (t + _1),
            _ => t => _1,
        };
    }

    public static List<string> CreateTable(string[] header, string[,] body)
    {
        int n = body.GetLength(0), m = body.GetLength(1);
        int[] maxLength = new int[m];
        for (int i = 0; i < m; i++)
        {
            maxLength[i] = header[i].Length;
        }

        for (int j = 0; j < m; j++)
        {
            for (int i = 0; i < n; i++)
            {
                if (body[i, j].Length > maxLength[j])
                {
                    maxLength[j] = body[i, j].Length;
                }
            }
        }

        string tableDelimiter = "+";
        for (int i = 0; i < m; i++)
        {
            tableDelimiter = $"{tableDelimiter}-{string.Join("", Enumerable.Repeat('-', maxLength[i]))}-+";
        }
        List<string> table = [tableDelimiter];

        string headerLine = "|";
        for (int i = 0; i < m; i++)
        {
            int spacesLeft = (maxLength[i] - header[i].Length) / 2;
            int spacesRight = maxLength[i] - header[i].Length - spacesLeft;
            headerLine = $"{headerLine} {string.Join("", Enumerable.Repeat(' ', spacesLeft))}{header[i]}{string.Join("", Enumerable.Repeat(' ', spacesRight))} |";
        }
        table.Add(headerLine);
        table.Add(tableDelimiter);

        for (int i = 0; i < n; i++)
        {
            string line = "|";
            for (int j = 0; j < m; j++)
            {
                int spacesRight = maxLength[j] - body[i, j].Length;
                line = $"{line} {body[i, j]}{string.Join("", Enumerable.Repeat(' ', spacesRight))} |";
            }
            table.Add(line);
        }
        table.Add(tableDelimiter);
        return table;
    }
}
