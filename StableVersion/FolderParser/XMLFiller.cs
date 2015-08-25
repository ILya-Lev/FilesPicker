using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;

namespace FolderParser
{
	class XMLFiller : Filler, INotifyPropertyChanged
	{
		private XmlWriter m_writer;
		private int m_indentsNumber = 0;
		private Stack<string> m_itemsStack = new Stack<string>();

		public XMLFiller(ProgresStateSynchronizer inProgress)
			: base(inProgress)
		{
			OutputFileName = "folderStructure.xml";
		}

		private string m_outputFileName;
		public string OutputFileName
		{
			get { return m_outputFileName; }
			set
			{
				try
				{
					if (m_outputFileName != value)
					{
						string dirName = Path.GetDirectoryName(value);
						string fileName = value.Substring(dirName.Length == 0 ? 0 : dirName.Length + 1);
						string extensionName = Path.GetExtension(value);
						if ((dirName.Count() == 0 || Directory.Exists(dirName))
							&& string.Compare(extensionName, ".XML", StringComparison.OrdinalIgnoreCase) == 0
							&& !(fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
						{
							BeforeFileChange();
							m_outputFileName = value;
							OnPropertyChanged("OutputFileName");
							AfterFileChange();
						}
						else
						{
							string message =
								string.Format("the file name {0} you've specified is not valid. try again or continue with the default one.",
									value);
							MessageBox.Show(message, "invalid file name", MessageBoxButton.OK, MessageBoxImage.Warning);
						}
					}
				}
				catch (Exception exc)
				{
					MessageBox.Show(exc.Message, "exception in OutputFileName setter", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}

		}
		private void BeforeFileChange()
		{
			if (m_writer != null)
			{
				m_writer.Dispose();
				m_writer = null;
			}
			if (File.Exists(OutputFileName))
			{
				File.Delete(OutputFileName);
			}
		}
		private void AfterFileChange()
		{
			m_writer = new XmlTextWriter(OutputFileName, Encoding.Unicode);
			m_writer.WriteStartDocument();
			m_writer.WriteWhitespace("\n");
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public override void FolderStartedHandler(Item item)
		{
			try
			{
				m_itemsStack.Push(item.Id);

				WriteIndention();
				m_writer.WriteStartElement("folder");
				WriteAttributes(item);
				m_writer.WriteWhitespace("\n");
				m_indentsNumber++;
			}
			catch (Exception exc)
			{
				OnExceptionOccured(exc.Message);
			}
		}

		public override void FolderFinishedHandler(Item item)
		{
			try
			{
				if (m_itemsStack.Peek() == item.Id)
				{
					m_thread = new Thread(this.RetreiveData);
					m_thread.Name = "XML thread";
					m_thread.Start();
					m_thread.Join();

					m_itemsStack.Pop();
					m_indentsNumber--;

					WriteIndention();
					m_writer.WriteEndElement();
					m_writer.WriteWhitespace("\n");

					if (m_itemsStack.Count == 0)
					{
						m_writer.WriteEndDocument();
						m_writer.Flush();
					}
				}
				else
				{
					throw new Exception(string.Format("invalid element structure {0} != {1}", m_itemsStack.Peek(), item.Id));
				}
			}
			catch (Exception exc)
			{
				m_thread.Abort();
				m_thread.Join();
				OnExceptionOccured(exc.Message);
			}
		}

		protected override void StoreData(Item anItem)
		{
			WriteIndention();
			m_writer.WriteStartElement("file");
			WriteAttributes(anItem);
			m_writer.WriteEndElement();
			m_writer.WriteWhitespace("\n");
		}

		private void WriteIndention()
		{
			for (int i = 0; i < m_indentsNumber; i++)
			{
				m_writer.WriteWhitespace("\t");
			}
		}

		private void WriteAttributes(Item anItem)
		{
			foreach (PropertyInfo propertyInfo in typeof(Item).GetProperties())
			{
				string displayName = Item.GetDisplayName(propertyInfo);
				if (displayName.Length != 0)
				{
					m_writer.WriteAttributeString(displayName, propertyInfo.GetValue(anItem).ToString());
				}
			}
		}

		public override void CleanData()
		{
			m_itemsInQueue.Clear();
			m_itemsStack.Clear();
			m_indentsNumber = 0;

			BeforeFileChange(); // cleanup previous results
			AfterFileChange(); // start new XML document
		}
	}
}
