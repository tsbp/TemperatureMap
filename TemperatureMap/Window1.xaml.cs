/*
 * Created by SharpDevelop.
 * User: Voodoo
 * Date: 05.02.2019
 * Time: 13:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
//using System.Windows;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Threading;

namespace TemperatureMap
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	/// 
	
	
	
	public partial class Window1 : Window
	{
		
		
		public static int MAX_BEAM_COUNT = 8;		
		
		ObservableCollection<ItemData>    dsItem = new ObservableCollection<ItemData>();
		ObservableCollection<ItemData> [] beam   = new ObservableCollection<ItemData>[MAX_BEAM_COUNT];
		
		int beamCntr = 0;
		Point startPoint;
		ListView []listBox = new ListView[MAX_BEAM_COUNT];
		
		string [] temps = {"123", "234", "456", "567", "678","789", "890", };
		string [] jsonBeam = new string[MAX_BEAM_COUNT];
		
		DispatcherTimer dispatcherTimer;
		
		
		public Window1()
		{
			InitializeComponent();
			
//			 for(int i = 0; i < 7; i++)
//			 	dsItem.Add(new ItemData(i + ".DS", temps[i]));
//			 lvSensorList.ItemsSource = dsItem;		
			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000);//new TimeSpan(0, 0, 1);
//			dispatcherTimer.Start();			
		}
		//==================================================================
		int tick = 0;
		//==================================================================
		void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			tick++;
			for(int i = 0; i < dsItem.Count; i++)
				dsItem[i].tvalue = "" + (tick + i).ToString();
		}
		//==================================================================
		void bCreate_Click(object sender, RoutedEventArgs e)
		{			
			if (beamCntr < MAX_BEAM_COUNT) 
			{
				CreateBeam();
				beamCntr++;
			}
		}	
		//==================================================================
		void bChange_Click(object sender, RoutedEventArgs e)
		{			
			dsItem[2].tvalue = lb.Text;
		}	
		//==================================================================
		void bJson_Click(object sender, RoutedEventArgs e)
		{
			//=========== global ==========================
			string sensorList = "{\"Name\":\"Global\",\"id\":[\"";
			for(int i = 0; i < dsItem.Count; i++)
			{
				sensorList += dsItem[i].id + "\"";
				if (i < dsItem.Count - 1) sensorList += ",\"";
			}
			sensorList += "]}";
			//=========== beams ===========================
			if	(beamCntr > 0)
			{
				for(int i = 0; i < beamCntr; i++)
				{
					//jsonBeam[i] = ( "{\"Name"  +  "\":\"" + listBox[i].Name + "\"," );
					jsonBeam[i] = ( "{\"Title" +  "\":\"" + tbTitle[i].Text + "\",\"id\":[\"" );
					for(int j = 0; j < beam[i].Count; j++)
					{
						jsonBeam[i] += beam[i][j].id + "\"";
						if (j < beam[i].Count - 1) jsonBeam[i] += ",\"";
					}
					jsonBeam[i] += "]}";
					if(i < beamCntr -1) jsonBeam[i] += ",";
				}
			}
			
			//------------ save -----------------------
			StreamWriter sw = new StreamWriter("config.json");
			sw.WriteLine(sensorList);
			for(int j = 0; j < beam.Length; j++)
				sw.WriteLine(jsonBeam[j]);          
            sw.Close();
		}	
		//==================================================================
		string cfgString = "";
		//==================================================================
		void bLoad_Click(object sender, RoutedEventArgs e)
		{				
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if(openFileDialog.ShowDialog() == true)
				cfgString = File.ReadAllText(openFileDialog.FileName);
			
			
			
			//---- strings count ------
			int bCntr = 0;
			for(int i = 0; i < cfgString.Length; i++)
				if(cfgString[i].Equals('{')) 
					bCntr++;
				
			string [] str = new string[bCntr];
			for(int i = 0; i < bCntr; i++)
			{
				str[i] = cfgString.Substring(0, 1 + cfgString.IndexOf('\n'));
				cfgString = cfgString.Replace(str[i],"");
			}
			//----- parse Global -------
			string tmp = str[0];
			string []gIds = idsFromString(str[0]);
			
			for(int i = 0; i < gIds.Length; i++)
				dsItem.Add(new ItemData(gIds[i], "23.8"));
			lvSensorList.ItemsSource = dsItem;
			
			//------ parse beams
			string[][] bIds = new string[str.Length - 1][];
			for(int i = 0; i < str.Length - 1; i++)
				bIds[i] = idsFromString(str[i + 1]);
			
			//----- beam titles ------
			for(int i = 1; i < str.Length; i++)
			{
				string s = str[i].Replace(str[i].Substring(0, str[i].IndexOf("Title")), "");
				beamTitle[i-1] = s.Substring(8, s.IndexOf("\",") - 8);
			}
			
			//--- create beam colections ---
			for(int k = 0; k < bIds.Length; k++)
			{
				CreateBeam();
				for(int i = 0; i < bIds[k].Length; i++)
				{
					for(int a = 0; a < dsItem.Count; a++)
					{
						if(dsItem[a].id.Equals(bIds[k][i]) || bIds[k][i] == "")
						{							
							beam[k].Add(dsItem[a]);							
						}
					}
				}
				beamCntr++;
			}	
			dispatcherTimer.Start();
		}
		//==================================================================
		string [] idsFromString(string tmp)
		{
			tmp = tmp.Replace(tmp.Substring(0, 1 + tmp.IndexOf('[')), "");
			tmp = tmp.Replace(tmp, tmp.Substring(0, tmp.IndexOf(']')));
			
			int comaCntr = 0;
			for(int i = 0; i < tmp.Length; i++)
				if(tmp[i].Equals('"')) comaCntr++;
			comaCntr /= 2;
			
			string [] globalid = new string[comaCntr];
			if(comaCntr > 0)
			{
				//globalid = new string[comaCntr];
				for(int i = 0; i < comaCntr; i++)
				{
					if(i == comaCntr - 1)
					{
						globalid[i] = tmp;
						globalid[i] = globalid[i].Replace("\"", "");
						globalid[i] = globalid[i].Replace(",", "");
					}
					else
					{
						int a =  tmp.IndexOf(',');
						globalid[i] = tmp.Substring(0, 1 + a);						
						tmp = tmp.Substring(1 + a, tmp.Length - 1 - a);//tmp.Replace(globalid[i], "");
						globalid[i] = globalid[i].Replace("\"", "");
						globalid[i] = globalid[i].Replace(",", "");
					}
				}				
			}
			//else return null;
			return globalid;
		}
		//==================================================================
		ColumnDefinition [] gridCol = new ColumnDefinition[MAX_BEAM_COUNT];		
		ColumnDefinition [] gridColTitle = new ColumnDefinition[MAX_BEAM_COUNT];
		TextBox [] tbTitle = new TextBox[MAX_BEAM_COUNT];
		string [] beamTitle = new string[MAX_BEAM_COUNT];
		//==================================================================
		void addTitles()
		{
			gridColTitle[beamCntr] = new ColumnDefinition();
			gTitles.ColumnDefinitions.Add(gridColTitle[beamCntr]);
			tbTitle[beamCntr] = new TextBox();
			tbTitle[beamCntr].Text = beamTitle[beamCntr];
			tbTitle[beamCntr].HorizontalContentAlignment = HorizontalAlignment.Center;
			tbTitle[beamCntr].VerticalContentAlignment   = VerticalAlignment.Center;
			tbTitle[beamCntr].BorderThickness = new Thickness(0);
			tbTitle[beamCntr].Margin = new Thickness(2);
			tbTitle[beamCntr].Background = Brushes.DarkBlue;
			tbTitle[beamCntr].FontSize = 15;
			tbTitle[beamCntr].Foreground = Brushes.WhiteSmoke;
			Grid.SetColumn(tbTitle[beamCntr], beamCntr);
			gTitles.Children.Add(tbTitle[beamCntr]);
		}
		
		//==================================================================
		void  CreateBeam()
		{
			 addTitles();
			 gridCol[beamCntr] = new ColumnDefinition();;
			 stPanel.ColumnDefinitions.Add(gridCol[beamCntr]);		
			
			
			 listBox[beamCntr] = new ListView();
			 listBox[beamCntr].Name = "lvBeam" + beamCntr;
             listBox[beamCntr].Margin = new Thickness(2);
             listBox[beamCntr].HorizontalAlignment = HorizontalAlignment.Stretch;
             listBox[beamCntr].HorizontalContentAlignment = HorizontalAlignment.Stretch;
             listBox[beamCntr].VerticalAlignment = VerticalAlignment.Stretch;
             listBox[beamCntr].AllowDrop = true;
             listBox[beamCntr].Drop += lbTwo_Drop;   
             listBox[beamCntr].BorderThickness = new Thickness(0);
             listBox[beamCntr].Background = Brushes.SteelBlue;

             DataTemplate dt = new DataTemplate();
             Binding bindingID = new Binding();
             bindingID.Path = new PropertyPath("id"); 

             FrameworkElementFactory textID = new FrameworkElementFactory(typeof(TextBlock));

             textID.SetBinding(TextBlock.TextProperty, bindingID);
             textID.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);         

             Binding bindingValue = new Binding();
             bindingValue.Path = new PropertyPath("tvalue"); 

             FrameworkElementFactory textValue = new FrameworkElementFactory(typeof(TextBlock));
             textValue.SetBinding(TextBlock.TextProperty, bindingValue);
             textValue.SetValue(ForegroundProperty, Brushes.Blue);
             textValue.SetValue(FontWeightProperty, FontWeights.Bold);
             textValue.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
             textValue.SetValue(FontSizeProperty, 15D);
 
             FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
             stack.SetValue(MarginProperty, new Thickness(5));
             stack.AppendChild(textID);
             stack.AppendChild(textValue); 

             FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
             border.SetValue(BackgroundProperty, Brushes.LightYellow);
             border.SetValue(BorderBrushProperty, Brushes.Brown);
             border.SetValue(BorderThicknessProperty, new Thickness(3));
             border.SetValue(Border.CornerRadiusProperty, new CornerRadius(20));
             border.SetValue(MarginProperty, new Thickness(1));
             border.AppendChild(stack); 
             dt.VisualTree = border;
             
             listBox[beamCntr].ItemTemplate = dt;             
             beam[beamCntr] = new ObservableCollection<ItemData>();
             listBox[beamCntr].ItemsSource = beam[beamCntr];
             
             Grid.SetColumn(listBox[beamCntr], beamCntr);
             stPanel.Children.Add(listBox[beamCntr]);
		}
		
		//==================================================================			
		private void lbOne_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			startPoint = e.GetPosition(null);
		}
		
		//==================================================================		
		private void lbTwo_Drop(object sender, DragEventArgs e)
		{
			ListView listView = sender as ListView;
			
			string name = ((Control)sender).Name;
			
			int curBeam;
			for(curBeam = 0; curBeam < beamCntr; curBeam++)
				if(name.Equals(listBox[curBeam].Name)) break;
			
			IDataObject dataObject = e.Data;
			if (dataObject != null)
			{
				ItemData itemData = (ItemData)dataObject.GetData("ItemData");
				if (itemData != null)
				{
					beam[curBeam].Add(itemData);
				}
			}			
		}
		
		//==================================================================
		private static T FindAnchestor<T>(DependencyObject current)
			where T : DependencyObject
		{
			do
			{
				if (current is T)
				{
					return (T)current;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			while (current != null);
			return null;
		}
		
		//==================================================================
		private void lbOne_MouseMove(object sender, MouseEventArgs e)
		{
			Point mousePos = e.GetPosition(null);
			Vector diff = startPoint - mousePos;
			
			if (e.LeftButton == MouseButtonState.Pressed && Math.Abs(diff.X) > SystemParameters.MinimumVerticalDragDistance)
			{
				// Get the dragged ListViewItem
				ListView listView = sender as ListView;
				ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
				if (listViewItem == null) return;           // Abort
				// Find the data behind the ListViewItem
				ItemData item = (ItemData)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
				if (item == null) return;                   // Abort
				// Initialize the drag & drop operation
				int startIndex = listView.SelectedIndex;			
				
				
				DataObject dragData = new DataObject();
				dragData.SetData("ItemData", item);
				DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
			}
		}
		//==================================================================
		public class ItemData : INotifyPropertyChanged
		{
			public ItemData(String aId, String aVal)
			{
				id = aId;
				tvalue = aVal;
			}

			public String id    { get; set; }
			
			public string _tvalue;
			public string tvalue
			{
				get                	{ return _tvalue;}
				set
				{
					_tvalue = value;
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs("tvalue"));
				}
			}
			public event PropertyChangedEventHandler PropertyChanged;
			
		}
	}
	
	
}