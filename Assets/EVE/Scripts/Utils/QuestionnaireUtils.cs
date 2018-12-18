using System.Collections.Generic;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.Questionnaire.Enums;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;
using Assets.EVE.Scripts.XML;
using VisualStimuliEnums = Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli;

namespace Assets.EVE.Scripts.Utils
{
    public static class QuestionnaireUtils
    {
        public static void CreateExampleQuestionnaire(LoggingManager loggingManager, ExperimentSettings experimentSettings)
        {
            var qs = new QuestionSet("TestSet");
            qs.Questions.Add(new InfoScreen("example_info", "<b><size=48>People questionnaire</size></b>:\n\nPerception of people in the neighborhood"));
            qs.Questions.Add(new InfoScreen("example_confirm", "<b><size=48>Read this</size></b>\n\nWait for 1 seconds before this continues and confirm you really want this to continue", new ConfirmationRequirement { Required = true, ConfirmationDelay = 1 }));
            qs.Questions.Add(new TextQuestion("exercise", "What type of exercise have you done recently?"));
            qs.Questions.Add(new TextQuestion("born_in", "Where were you born?", new List<Label> { new Label("Country:"), new Label("City:") }));
            qs.Questions.Add(new ChoiceQuestion("yes_no", "Is this a Yes/No Question?", Choice.Single, new List<Label> { new Label("Yes", 1), new Label("No", 0) }, null));
            qs.Questions.Add(new ChoiceQuestion("likert_scale", "Is this a good question?", Choice.Single, null, new List<Label> { new Label("Not at all", 0), new Label("This entry has text that is way tooooooooo long", 1), new Label("", 2), new Label("It is okay", 3), new Label("", 4), new Label("", 5), new Label("Very Good", 6) }));
            qs.Questions.Add(new ChoiceQuestion("multiple_yes_no", "Answer quickly without thinking:", Choice.Single, new List<Label> { new Label("Do you like questionnaires?"), new Label("Have you ever been to New York?"), new Label("Coffee with Milk?"), new Label("Coffee with Sugar?"), new Label("Black Coffee?") }, new List<Label>() { new Label("Yes", 1), new Label("No", 0) }));
            qs.Questions.Add(new ChoiceQuestion("coffe_consumption", "Did you have coffee, espresso, or another beverage containing caffeine in the past 24 hours?", Choice.Single, new List<Label>() { new Label("If yes, how many hours ago", 1, true), new Label("No, I did not have any caffeine", 0) }, null));
            qs.Questions.Add(new ChoiceQuestion("band_judgement", "Please check with which assessment you agree most about the bands:", Choice.Multiple, new List<Label> { new Label("Coldplay"), new Label("Maroon5"), new Label("Nsync"), new Label("Red Hot Chili Peppers") }, new List<Label> { new Label("Mainstream", 0), new Label("Best of their Genre", 1), new Label("Original", 2) }));
            qs.Questions.Add(new ChoiceQuestion("multi_c_text", "Which animals do you like:", Choice.Multiple, new List<Label> { new Label("Bears", 0), new Label("Lions", 1), new Label("Frogs", 2), new Label("Axolotls", 3), new Label("Humans", 4), new Label("Bats", 5), new Label("None", 6), new Label("Other:", 7, true) }, null));
            qs.Questions.Add(new ChoiceQuestion("image_example", "Which image looks the best?", Choice.Single, new List<Label> { new Label("This", 0, "Images/test_image"), new Label("This", 1, "Images/test_image"), new Label("This", 2, "Images/test_image") }, null));
            qs.Questions.Add(new ScaleQuestion("amuesment_scale", "How much did you feel AMUSEMENT?", null, Scale.Line, "Did not experience at all", "Strongest experience ever felt", true));
            qs.Questions.Add(new ScaleQuestion("SAM_pleasure", "Please rate how happy-unhappy you actually felt", null, Scale.Pleasure, "SAD", "CHEERFUL"));
            qs.Questions.Add(new ScaleQuestion("SAM_arousal", "Please rate how excited - calm you actually felt", null, Scale.Arousal, "QUIET", "ACTIVE"));
            qs.Questions.Add(new ScaleQuestion("SAM_dominance", "Please rate how controlled vs. in-control you actually felt", null, Scale.Dominance, "DEPENDENT", "INDEPENDENT", true));
            qs.Questions.Add(new ScaleQuestion("custom_amuesment_scale", "How much did you feel AMUSEMENT?", "Textures/questionline", Scale.Custom, "Did not experience at all", "Strongest experience ever felt", false));
            qs.Questions.Add(new LadderQuestion("swiss_status", "Swiss comparison of social status", "At the TOP of the ladder are the people who are the best off. The lower you are, the closer you are to the people at the very bottom. Where would you place yourself on this ladder, compared to all the other people in Switzerland?"));
            qs.Questions.Add(new VisualStimuli("test_stimuli",
                "Which stimulus is more pleasing?\n\nPress \"1\" for the first or \"2\" for the second.",
                VisualStimuliEnums.Separator.FixationCross,
                VisualStimuliEnums.AnswerMode.ArrowKeys,
                VisualStimuliEnums.Randomisation.None,
                VisualStimuliEnums.Type.Image, "none", false, 3, 5, 5,
                new List<string>
                {
                    "Images/test_image", "Images/test_image"
                }));
            qs.Questions.Add(new ChoiceQuestion("flu_med", "Have you taken any medication for cold or flu symptoms today?", Choice.Single, new List<Label> { new Label("Yes", 1), new Label("No", 0) }, null, new List<Jump>() { new Jump("inflam_med", "FT") }));
            qs.Questions.Add(new TextQuestion("flu_med_name", "Which medication(s)?"));
            qs.Questions.Add(new ChoiceQuestion("inflam_med", "Have you taken any anti-inflammatory medication today?", Choice.Single, new List<Label> { new Label("Yes", 1), new Label("No", 0) }, null, new List<Jump>() { new Jump("*", "FT") }));
            qs.Questions.Add(new TextQuestion("inflam_med_name", "Which medication(s)?"));





            var qn = new Questionnaire.Questionnaire("ExampleQuestionnaire");


            qn.QuestionSets.Add(qs.Name);

            var qf = new QuestionnaireFactory(loggingManager, experimentSettings);
            qf.WriteQuestionSetToXml(qs, "TestSet.xml");
            qf.WriteQuestionnaireToXml(qn, "ExampleQuestionnaire");
        }
    }
}
