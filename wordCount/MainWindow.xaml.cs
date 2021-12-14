using Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Microsoft.ML;

namespace wordCount
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MainWindowViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm = new MainWindowViewModel(); // used MVVM for button handling 



        }
    }

    // Validation for empty TextBox. It turns the texbox into red. 
    public partial class NameValidator : ValidationRule
    {
        public NameValidator()
        {
        }
        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value.ToString();
            if (str.Length <1)
            {
                return new System.Windows.Controls.ValidationResult(false, "Please Enter text for Word Counting");
            }
            else
            {
                return System.Windows.Controls.ValidationResult.ValidResult;
            }
        }
    }






}
public class MainWindowViewModel : ViewModelBase
{

    

     private string _wordPool;

    public string wordPool
    {
        get { return _wordPool; }
        set
        {
            if (_wordPool != value)
            {
                _wordPool = value;
                OnPropertyChanged();
            }
        }
    }


    public ViewModelCommand wordButton { get; set; }
    public MainWindowViewModel() : base()
    {
        wordButton = new ViewModelCommand(searchWords);


    }

    public void searchWords(object obj)
    {
        if (string.IsNullOrWhiteSpace(wordPool) || wordPool.Length==1)
        {
            MessageBox.Show("Please Enter input to check for number of words");
        }
        else
        {
            // I used Regex to get number of words  
            MatchCollection collection = Regex.Matches(wordPool, @"[\S]+");


            // Decided to use  Miscrososft.ML package to get rid off stop words and have two results for Total number of words and number of words without stopwords 
            var mlContext = new MLContext();

            // Create an empty list as the dataset. The empty list is only needed to pass  input schema to the pipeline.
            var emptySamples = new List<TextData>();

            // Convert sample list to an empty IDataView.
            var emptyDataView = mlContext.Data.LoadFromEnumerable(emptySamples);

            // A pipeline for removing stop words from input text/string
            // 'tHe' and 'the' are considered the same stop words.
            var textPipeline = mlContext.Transforms.Text.TokenizeIntoWords("Words",
                "Text")
                .Append(mlContext.Transforms.Text.RemoveStopWords(
                "WordsWithoutStopWords", "Words", stopwords:
                new[] { "a", "the", "from", "by", "is" }));

            // Fit to data.
            var textTransformer = textPipeline.Fit(emptyDataView);

            // Create the prediction engine to remove the stop words from the input
            // text /string.
            var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData,
                TransformedTextData>(textTransformer);

         


            var data = new TextData();
            data.Text = wordPool;
          

            var prediction = predictionEngine.Predict(data);

            MessageBox.Show("Total number of words: " + collection.Count + Environment.NewLine + Environment.NewLine + "Number of words without StopWords: " + prediction.WordsWithoutStopWords.Length);

        }
            
    }
}

public class TextData
{
    public string Text { get; set; }
}

public class TransformedTextData : TextData
{
    public string[] WordsWithoutStopWords { get; set; }
}