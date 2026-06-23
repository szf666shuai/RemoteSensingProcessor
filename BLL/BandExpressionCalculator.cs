using RemoteSensingProcessor.DAL;
using RemoteSensingProcessor.Model;

namespace RemoteSensingProcessor.BLL
{
    public class BandExpressionCalculator
    {
        private readonly GdalDataAccess _dal = new();

        public Bitmap CalculateExpression(ImageInfo info, string expression)
        {
            expression = expression.Trim().ToUpper();
            var tokens = Tokenize(expression);
            var postfix = ShuntingYard(tokens);
            
            var bandIndices = ExtractBandIndices(tokens);
            if (bandIndices.Count == 0)
            {
                throw new ArgumentException("表达式中没有引用任何波段（如 B1、B2）");
            }
            
            foreach (int idx in bandIndices)
            {
                if (idx < 1 || idx > info.BandCount)
                {
                    throw new ArgumentException($"波段 B{idx} 不存在，当前影像只有 {info.BandCount} 个波段");
                }
            }
            
            float[][] bands = _dal.ReadMultiBandData(info.FilePath, 
                bandIndices.ToArray(), 0, 0, info.Width, info.Height);
            
            var bandMap = new Dictionary<int, float[]>();
            for (int i = 0; i < bandIndices.Count; i++)
            {
                bandMap[bandIndices[i]] = bands[i];
            }
            
            float[] result = new float[info.Width * info.Height];
            for (int pixel = 0; pixel < result.Length; pixel++)
            {
                var values = new Dictionary<string, float>();
                foreach (int idx in bandIndices)
                {
                    values[$"B{idx}"] = bandMap[idx][pixel];
                }
                result[pixel] = EvaluatePostfix(postfix, values);
            }
            
            float minVal = result.Min();
            float maxVal = result.Max();
            return RenderIndex(result, info.Width, info.Height, minVal, maxVal);
        }

        private List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < expression.Length)
            {
                char c = expression[i];
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }
                if (char.IsLetter(c))
                {
                    int j = i;
                    while (j < expression.Length && (char.IsLetterOrDigit(expression[j])))
                        j++;
                    tokens.Add(expression.Substring(i, j - i));
                    i = j;
                    continue;
                }
                if (char.IsDigit(c) || c == '.')
                {
                    int j = i;
                    while (j < expression.Length && (char.IsDigit(expression[j]) || expression[j] == '.'))
                        j++;
                    tokens.Add(expression.Substring(i, j - i));
                    i = j;
                    continue;
                }
                tokens.Add(c.ToString());
                i++;
            }
            return tokens;
        }

        private List<string> ShuntingYard(List<string> tokens)
        {
            var output = new List<string>();
            var operators = new Stack<string>();
            
            var precedence = new Dictionary<string, int>
            {
                { "+", 1 }, { "-", 1 },
                { "*", 2 }, { "/", 2 },
                { "^", 3 }
            };
            
            foreach (var token in tokens)
            {
                if (token.StartsWith("B") && int.TryParse(token.Substring(1), out _))
                {
                    output.Add(token);
                }
                else if (float.TryParse(token, out _))
                {
                    output.Add(token);
                }
                else if (IsFunction(token))
                {
                    operators.Push(token);
                }
                else if (token == "(")
                {
                    operators.Push(token);
                }
                else if (token == ")")
                {
                    while (operators.Count > 0 && operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Pop();
                    if (operators.Count > 0 && IsFunction(operators.Peek()))
                    {
                        output.Add(operators.Pop());
                    }
                }
                else if (precedence.ContainsKey(token))
                {
                    while (operators.Count > 0 && operators.Peek() != "(" &&
                           precedence[operators.Peek()] >= precedence[token])
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
            }
            
            while (operators.Count > 0)
            {
                output.Add(operators.Pop());
            }
            
            return output;
        }

        private bool IsFunction(string token)
        {
            return token is "SQRT" or "LOG" or "LOG10" or "LN" or "ABS" or 
                   "SIN" or "COS" or "TAN" or "ASIN" or "ACOS" or "ATAN" or 
                   "EXP" or "POW" or "MAX" or "MIN";
        }

        private float PowHelper(Stack<float> stack)
        {
            float exp = stack.Pop();
            float @base = stack.Pop();
            return (float)Math.Pow(@base, exp);
        }

        private List<int> ExtractBandIndices(List<string> tokens)
        {
            var indices = new HashSet<int>();
            foreach (var token in tokens)
            {
                if (token.StartsWith("B") && int.TryParse(token.Substring(1), out int idx))
                {
                    indices.Add(idx);
                }
            }
            return indices.OrderBy(x => x).ToList();
        }

        private float EvaluatePostfix(List<string> postfix, Dictionary<string, float> values)
        {
            var stack = new Stack<float>();
            
            foreach (var token in postfix)
            {
                if (token.StartsWith("B"))
                {
                    stack.Push(values.TryGetValue(token, out float val) ? val : 0);
                }
                else if (float.TryParse(token, out float num))
                {
                    stack.Push(num);
                }
                else if (IsFunction(token))
                {
                    float result = token switch
                    {
                        "SQRT" => (float)Math.Sqrt(stack.Pop()),
                        "ABS" => Math.Abs(stack.Pop()),
                        "LOG" => (float)Math.Log(stack.Pop()),
                        "LOG10" => (float)Math.Log10(stack.Pop()),
                        "LN" => (float)Math.Log(stack.Pop()),
                        "EXP" => (float)Math.Exp(stack.Pop()),
                        "SIN" => (float)Math.Sin(stack.Pop()),
                        "COS" => (float)Math.Cos(stack.Pop()),
                        "TAN" => (float)Math.Tan(stack.Pop()),
                        "ASIN" => (float)Math.Asin(stack.Pop()),
                        "ACOS" => (float)Math.Acos(stack.Pop()),
                        "ATAN" => (float)Math.Atan(stack.Pop()),
                        "MAX" => Math.Max(stack.Pop(), stack.Pop()),
                        "MIN" => Math.Min(stack.Pop(), stack.Pop()),
                        "POW" => PowHelper(stack),
                        _ => 0
                    };
                    stack.Push(result);
                }
                else
                {
                    float b = stack.Pop();
                    float a = stack.Pop();
                    float result = token switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => b != 0 ? a / b : 0,
                        "^" => (float)Math.Pow(a, b),
                        _ => 0
                    };
                    stack.Push(result);
                }
            }
            
            return stack.Count > 0 ? stack.Pop() : 0;
        }

        private Bitmap RenderIndex(float[] data, int width, int height, float minVal, float maxVal)
        {
            Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            
            float range = maxVal - minVal;
            if (range < 1e-6f) range = 1;
            
            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        float normalized = (data[y * width + x] - minVal) / range;
                        normalized = Math.Clamp(normalized, 0, 1);
                        
                        byte r, g, b;
                        if (normalized <= 0.5f)
                        {
                            float t = normalized * 2;
                            r = (byte)(60 + 195 * t);
                            g = (byte)(80 + 175 * t);
                            b = (byte)(200 + 55 * t);
                        }
                        else
                        {
                            float t = (normalized - 0.5f) * 2;
                            r = (byte)(255 - 205 * t);
                            g = (byte)(255 - 20 * t);
                            b = (byte)(255 - 200 * t);
                        }
                        
                        row[x * 3] = b;
                        row[x * 3 + 1] = g;
                        row[x * 3 + 2] = r;
                    }
                }
            }
            
            bitmap.UnlockBits(bmpData);
            return bitmap;
        }
    }
}
