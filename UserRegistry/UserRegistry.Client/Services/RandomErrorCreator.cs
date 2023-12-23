using MathNet.Numerics.Distributions;
using System.Text;
using UserRegistry.Client.Models;

namespace UserRegistry.Client.Services
{
    public class RandomErrorCreator(int seed, decimal errorValue,
        DataGenerator dataGenerator)
    {
        private int _errorPosition;
        private int _errorType;
        private int errorCharCounter = 0;
        private readonly int seed = seed;
        private readonly decimal errorValue = errorValue;
        private readonly DataGenerator dataGenerator = dataGenerator;

        public int NameMistakesCount { get; set; }
        public int AddressMistakesCount { get; set; }
        public int PhoneMistakesCount { get; set; }

        public void ResetErrorCounters()
        {
            ResetMistakesCounters();
            _errorPosition = 0;
            _errorType = 0;
            errorCharCounter = 0;
        }

        private void ResetMistakesCounters()
        {
            NameMistakesCount = 0;
            AddressMistakesCount = 0;
            PhoneMistakesCount = 0;
        }

        public string AddMistakes(string name, int count)
        {
            string tmp = name;
            if (name.Length > 2)
            {
                tmp = AddMistakesToRecord(count, tmp);
            }

            return tmp;
        }

        private string AddMistakesToRecord(int count, string tmp)
        {
            for (int i = 0; i < count; i++)
            {
                tmp = AddRandomError(tmp);
            }

            return tmp;
        }

        public void DefineErrorDistribution()
        {
            for (int i = 0; i < (int)errorValue; i++)
            {
                GetErrorType(i);
            }
        }

        private void GetErrorType(int i)
        {
            var t = new Random(seed + (int)errorValue + i);
            int temp = GetNormalDistributedValue(0, 4, t);
            PickErrorType(temp);
        }

        private void PickErrorType(int temp)
        {
            if (temp == 1)
            {
                NameMistakesCount++;
            }
            else if (temp == 2)
            {
                AddressMistakesCount++;
            }
            else
            {
                PhoneMistakesCount++;
            }
        }

        private static string DeleteSymbolError(string input, int position)
        {
            StringBuilder sb = new(input, 100);
            sb.Remove(position + 1, 1);
            return sb.ToString();
        }

        private string AddSymbolError(string input, int position)
        {
            StringBuilder sb = new(input, 100);
            sb.Insert(position, GetChar());
            return sb.ToString();
        }

        private static string ReplaceNeighboursError(string input, int position)
        {
            StringBuilder sb = new(input, 100);
            (sb[position - 1], sb[position]) =
                (sb[position], sb[position - 1]);
            return sb.ToString();
        }

        private int DefineErrorType()
        {
            var t = new Random(seed + (int)errorValue + _errorType++);
            return GetNormalDistributedValue(0, 4, t);
        }

        private string AddRandomError(string input)
        {
            if (input.Length > 2)
            {
                return PickRandomError(input);
            }

            return input;
        }

        private string PickRandomError(string input)
        {
            int type = DefineErrorType();
            int position = DefineErrorPosition(input);
            if (type == 1)
            {
                return AddSymbolError(input, position);
            }
            else if (type == 2)
            {
                return ReplaceNeighboursError(input, position);
            }
            else
            {
                return DeleteSymbolError(input, position);
            }
        }

        private int DefineErrorPosition(string name)
        {
            Random t = new(seed + (int)errorValue + _errorPosition++);
            return GetNormalDistributedValue(0, name.Length - 1, t);
        }

        private static int GetNormalDistributedValue(
            int lower, int upper, Random random)
        {
            double sample;
            ContinuousUniform continuousDistribution =
                new(lower, upper, random);
            do
            {
                sample = RetrieveSample(continuousDistribution);
            } while (sample < lower + 1 || sample > upper - 1);
            return Convert.ToInt32(sample);
        }

        private static double RetrieveSample(
            ContinuousUniform continuousDistribution)
        {
            double sample;
            double continuusDistribution = continuousDistribution.Sample();
            sample = Math.Round(continuusDistribution);
            return sample;
        }

        private char GetChar()
        {
            string result = dataGenerator.GenerateLetters(seed).AlfaNumericSet;
            Random t = new(seed + (int)errorValue +
                errorCharCounter++ + result.Length);
            int index = GetNormalDistributedValue(0, result.Length - 1, t);
            return result[index];
        }
    }
}