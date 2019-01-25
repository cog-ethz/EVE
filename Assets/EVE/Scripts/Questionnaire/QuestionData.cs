namespace Assets.EVE.Scripts.Questionnaire
{
    public struct QuestionData {
        public string QuestionName;
        public string QuestionText;
        public string QuestionSet;
        public int QuestionType;
        public int[] Vals;
        public string[] Labels;
        public int[] Output;

        public QuestionData(string name, string text, string set, int type, int[] vals, string[] labels, int[] output)
        {
            QuestionName = name;
            QuestionText = text;
            QuestionSet = set;
            QuestionType = type;
            Vals = vals;
            Labels = labels;
            Output = output;
        }
    }
}
