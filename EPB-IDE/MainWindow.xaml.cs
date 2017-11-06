using System.Windows;
using MahApps.Metro.Controls;
using System.IO;
using System.Windows.Forms;
using System;
using System.Diagnostics;
using System.Reflection;
using EPB_IDE.Model;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EPB_IDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private string[] _currentEPBFileLines;
        private static string _currentEPOFileName = "";
        private static bool _ctrlPressed = false;
        private Brush _defaultButtonColor;

        private bool CtrlPressed
        {
            get { return _ctrlPressed; }
            set
            {
                _ctrlPressed = value;
                formatRunButtons();
            }
        }

        private void formatRunButtons()
        {
            if (CtrlPressed)
            {
                btnRun.Background = Brushes.Green;
                btnDebug.Background = Brushes.Green;
            }
            else
            {
                btnRun.Background = _defaultButtonColor;
                btnDebug.Background = _defaultButtonColor;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _defaultButtonColor = btnRun.Background;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "EPB files (*.epb)|*.epb";
            ofd.InitialDirectory = System.Windows.Forms.Application.StartupPath;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if (File.Exists(ofd.FileName))
                    {
                        _currentEPBFileLines = File.ReadAllLines(ofd.FileName);
                        txtViewer.Text = string.Join("\n", _currentEPBFileLines);
                        tabViewer.IsSelected = true;
                        processCodeFile();
                    }
                }
                catch (IOException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
        }

        private void processCodeFile()
        {
            if (_currentEPBFileLines.Length > 0)
            {
                string[] tokens = _currentEPBFileLines[0].Split(' ');
                
            }
        }

        private void btnBuild_Click(object sender, RoutedEventArgs e)
        {
            buildCode();
        }

        private void buildCode(bool optimize = false)
        {
            string[] lines = txtViewer.Text.Split('\n');
            if (lines != null && lines.Length > 0)
            {
                try
                {
                    string ns = GetType().Namespace;
                    Type type = Type.GetType($"{ns}.Model.CompilerFactory");
                    if (type == null) { throw new InvalidProgramException("Missing required '*.Model.CompilerFactory' class in current program"); }
                    ConstructorInfo factoryConstructor = type.GetConstructor(Type.EmptyTypes);
                    if (factoryConstructor == null) { throw new InvalidProgramException("'CompilerFactory' class missing paramaterless constructor CompilerFactory()"); }
                    var factory = (ICompilerFactory)factoryConstructor.Invoke(new object[] { });

                    // This is the only place where there is a hook into the Computer Simulator code
                    var compiler = (ICompiler)factory.Make();
                    compiler.LoadProgram(lines).Compile();
                    if (optimize)
                    {
                        compiler.Optimize();
                    }
                    txtEPML.Text = compiler.CompiledCode().ToString();
                    txtSymbolTable.Text = compiler.Symbols.ToString() + "\n\n" + compiler.Flags.ToString();
                    tabEPML.IsSelected = true;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Compiler Error!");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        private static void runProgramFromFile(bool runCurrentFile = true, bool debugging = false)
        {
            string computerSimulatorExe = Directory.GetCurrentDirectory();
            computerSimulatorExe += @"\Computer_Simulator.exe";

            if (runCurrentFile && _currentEPOFileName.Trim() != "")
            {
                launchProgram(computerSimulatorExe, _currentEPOFileName, debugging);
            }
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "EPO files (*.epo)|*.epo";
                //ofd.InitialDirectory = System.Windows.Forms.Application.StartupPath;
                ofd.RestoreDirectory = true;


                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        launchProgram(computerSimulatorExe, ofd.FileName, debugging);
                    }
                    catch (IOException ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                }
            }

        }

        private static void launchProgram(string computerSimulatorExe, string fileName, bool debugging)
        {
            if (File.Exists(fileName))
            {
                string arguments = $"--file \"{fileName}\"";
                if (debugging) { arguments += " --debug"; }
                Process.Start(computerSimulatorExe, arguments);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (tabEPML.IsSelected)
            {
                saveEPML();
            }
            else if (tabViewer.IsSelected)
            {
                saveEPB(txtViewer.Text);
            }
        }

        private void saveEPB(string content)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "EPB Files (*.epb)|*.epb";
            sfd.FilterIndex = 2;
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    sw.Write(content);
                }
            }
        }

        private void saveEPML()
        {
            // Write EPML file to a *.epo file
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "EPO Files (*.epo)|*.epo";
            sfd.FilterIndex = 2;
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _currentEPOFileName = sfd.FileName;
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    sw.Write(txtEPML.Text);
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtEPML.Text = "";
            txtViewer.Text = "";
            txtSymbolTable.Text = "";
        }

        private void btnBuildOptimized_Click(object sender, RoutedEventArgs e)
        {
            buildCode(optimize: true);
        }

        private void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            runProgramFromFile(debugging: true, runCurrentFile: !CtrlPressed);
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            runProgramFromFile(debugging: false, runCurrentFile: !CtrlPressed);
        }

        private void MetroWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                CtrlPressed = true;
            }
        }

        private void MetroWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            CtrlPressed = false;
        }


    }
}
