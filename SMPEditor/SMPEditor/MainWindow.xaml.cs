using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using static Yu5h1Tools.DevelopmentKit.InformationViewer;
using System.Xml.Linq;
using NiDump;
using Int32 = System.Int32;
using Yu5h1Tools.WPFExtension;
using System.Windows.Input;

namespace SMPEditor
{
    public class testNode{
        public string name = "";
        public List<testNode> children = new List<testNode>();
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        List<TreeViewItem> selectedNodes { get { return treeView.selectedNodes; } }

        public MainWindow()
        {
            InitializeComponent();

            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            searchTextBox.KeyUp += SearchTextBox_KeyUp;
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) searchTextBox.Text = "";
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            treeView.ShowItems(searchTextBox.Text, (item) => {
                var tb = item.Header as TextBlock;
                tb.HighLight(searchTextBox.Text, Brushes.Yellow, Brushes.Black);
                return tb.Text;
            });
        }

        TreeViewItem CreateTreeNodes(NiHeader hdr, int nodeIndex) {
            if (nodeIndex >= hdr.blocks.Length) {
                MessageBox.Show("Node ("+nodeIndex.ToString()+") out of range.");
                return null;
            }
            NiNode node = null;
            try
            {
                node = hdr.GetObject<NiNode>(nodeIndex);
            }
            catch (Exception)
            {
                return null;
            }

            TreeViewItem branch = new TreeViewItem();
            var textBlock = new TextBlock() {
                Text = hdr.GetNiNodeName(nodeIndex),
            };
            textBlock.IsHitTestVisible = false;
            branch.Header = textBlock;
            

            if (node != null && node.children.Length > 0)
            {
                foreach (var childIndex in node.children)
                {
                    var subBranch = CreateTreeNodes(hdr, childIndex);
                    if (subBranch != null)
                        branch.Items.Add(subBranch);
                }
            }
            return branch;
        }
        private void NifSelector_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (File.Exists(NifSelector.text))
            {
                var hdr = NiHeader.Load(NifSelector.text);
                treeView.Items.Clear();
                treeView.Items.Add(CreateTreeNodes(hdr, 0));

            }
        }


        public string GetNodeName(NiHeader hdr,Int32 string_ref)
        {
            return string_ref != -1 ? hdr.strings[string_ref] : "(undefined)";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Args.Length > 0) {
                NifSelector.text = App.Args[0];                
            }
        }

        
        public void AppendText(RichTextBox box, string text, string color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(color));
            }
            catch (FormatException) { }
        }

        void InsertTextbox(string content) {
            int previouseCaretIndex = textBox.CaretIndex;
            textBox.Text = textBox.Text.Insert(textBox.CaretIndex, content);
            textBox.CaretIndex = previouseCaretIndex + content.Length;
            textBox.Focus();
        }
        private void Add_btn_Click(object sender, RoutedEventArgs e)
        {
          
            string result = "";
            switch (comboBox.SelectedItem.ToString())
            {
                case "bone":
                    if (treeView.SelectedItem == null) return;
                    foreach (var node in selectedNodes)
                    {
                        XElement xele = new XElement("bone");
                        xele.SetAttributeValue("name", ((TextBlock)node.Header).Text);
                        result += xele.ToString() + "\n";
                    }
                    break;
                case "generic-constraint":
                    foreach (var node in selectedNodes)
                    {
                        XElement constraintgroup = new XElement("constraint-group");
                        var curNode = node;
                        while (curNode.Items.Count > 0)
                        {
                            var next = curNode.Items[0] as TreeViewItem;
                            var ele = new XElement("generic-constraint");

                            
                            ele.SetAttributeValue("bodyA", ((TextBlock)next.Header).Text);
                            ele.SetAttributeValue("bodyB", ((TextBlock)curNode.Header).Text);
                            constraintgroup.Add(ele);
                            curNode = next;
                        }
                        result += constraintgroup.ToString() + "\n";
                    }
                    break;
                case "generic-constraint(Horizontal)":
                    if (selectedNodes.Count < 2) {
                        showWarnning(   "Select more than two Node & consistent Parents"
                                    +   "\n選擇超過兩個結點 & 相同的父母");
                        return;
                    }
                    object boneConsistentParent = selectedNodes[0].Parent;
                    List<List<string>> bunchOfBones = new List<List<string>>();
                    foreach (var node in selectedNodes)
                    {
                        if (node.Parent != boneConsistentParent) {
                            showWarnning(   "Parents are inconsistent"
                                        +   "\n爸媽不同唷 ^.<");
                            return;
                        }
                        List<string> curBones = new List<string>();
                        curBones.Add(((TextBlock)node.Header).Text);
                        var curNode = node;
                        while (curNode.Items.Count > 0)
                        {
                            var next = curNode.Items[0] as TreeViewItem;
                            curBones.Add(((TextBlock)next.Header).Text);                           
                            curNode = next;
                        }
                        bunchOfBones.Add(curBones);
                    }
                    for (int i = 0; i < bunchOfBones.Count; i++)
                    {
                        if (i + 1 < bunchOfBones.Count) {
                            for (int o = 0; o < bunchOfBones[i].Count; o++)
                            {
                                if (bunchOfBones[i + 1].Count > o && bunchOfBones[i].Count > o)
                                {
                                    var ele = new XElement("generic-constraint");
                                    ele.SetAttributeValue("bodyA", bunchOfBones[i + 1][o]);
                                    ele.SetAttributeValue("bodyB", bunchOfBones[i][o]);
                                    result += ele.ToString() + "\n";
                                }
                            }
                        }
                    }
                    break;
                case "generic-constraint-default":
                    XElement gcd = new XElement("generic-constraint-default");
                    XElement FrameIn = new XElement("frameInLerp");
                    FrameIn.Add(new XElement("translationLerp") { Value = "0" });
                    FrameIn.Add(new XElement("rotationLerp") { Value = "0" });
                    gcd.Add(FrameIn);
                    gcd.Add(Vector3Element("linearLowerLimit"));
                    gcd.Add(Vector3Element("linearUpperLimit"));
                    gcd.Add(Vector3Element("angularLowerLimit"));
                    gcd.Add(Vector3Element("angularUpperLimit"));
                    gcd.Add(Vector3Element("linearStiffness"));
                    gcd.Add(Vector3Element("angularStiffness"));
                    gcd.Add(Vector3Element("linearDamping"));
                    gcd.Add(Vector3Element("angularDamping"));
                    gcd.Add(Vector3Element("linearEquilibrium"));
                    gcd.Add(Vector3Element("angularEquilibrium"));
                    result += gcd.ToString()+"\n";
                    break;
            }

            InsertTextbox(result);
            treeView.Items.Refresh();
            treeView.UpdateLayout();
        }
        XElement Vector3Element(string name) {
            XElement ele = new XElement(name);
            ele.SetAttributeValue("x", 0);
            ele.SetAttributeValue("y", 0);
            ele.SetAttributeValue("z", 0);
            return ele;
        }

        private void Clear_btn_Click(object sender, RoutedEventArgs e)
        {
            textBox.Text = "";
        }
    }
}
