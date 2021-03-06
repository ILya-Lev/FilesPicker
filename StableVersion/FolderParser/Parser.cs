﻿using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;

namespace FolderParser
{
	/// <summary>
	/// The class picks folder by folder from disk and feeds the information (about folders and files) to 
	/// outer world. It's designed not to know anything about any receivers of the data that Parser has gained.
	/// therefore events are used and they are subscribed in client code.
	/// </summary>
	class Parser : INotifyPropertyChanged, IExceptionNotifier
	{
		private string m_initFolder;
		private Thread m_thread;
		private volatile ProgresStateSynchronizer m_inProgress;
		public string InitFolder
		{
			get { return m_initFolder; }
			set
			{
				if (m_initFolder != value)
				{
					if (Directory.Exists(value))
					{
						m_initFolder = value;
						OnPropertyChanged("InitFolder");
					}
					else
					{
						string message =
							string.Format("the folder name {0} you've specified is not valid. try again or continue with the default one.",
								value);
						MessageBox.Show(message, "invalid folder name", MessageBoxButton.OK, MessageBoxImage.Warning);
					}
				}
			}
		}

		public Parser(ProgresStateSynchronizer inProgress)
		{
			m_inProgress = inProgress;
			InitFolder = "c:\\";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event Action<Item> ItemGrabbed;

		private void OnItemGrabbed(Item anItem)
		{
			if (ItemGrabbed != null)
			{
				ItemGrabbed(anItem);
			}
		}

		public event Action<Item> FolderStarted;

		private void OnFolderStarted(Item item)
		{
			if (FolderStarted != null)
			{
				FolderStarted(item);
			}
		}
		public event Action<Item> FolderFinished;

		private void OnFolderFinished(Item item)
		{
			if (FolderFinished != null)
			{
				FolderFinished(item);
			}
		}

		public event Action<object, string> ParserFinishEvent;

		public void OnParserFinishEvent(string message)
		{
			if (ParserFinishEvent != null)
			{
				ParserFinishEvent(this, message);
			}
		}

		public void Start()
		{
			m_thread = new Thread(this.Run);
			m_thread.Name = "Parser Thread";
			m_thread.Start();
		}

		private void Run()
		{
			try
			{
				ParseFolder(InitFolder);
			}
			catch (Exception exc)
			{
				OnExceptionOccured(exc.Message);
			}
			finally
			{
				OnParserFinishEvent(string.Format("Folder {0} has been traversed", InitFolder));
			}
		}

		private void ParseFolder(string folderName)
		{
			if (!m_inProgress.InProgress)
				return;

			DirectoryInfo info = new DirectoryInfo(folderName);
			Item anItem = new Item(info);

			OnFolderStarted(anItem);
			foreach (DirectoryInfo dirInfo in info.EnumerateDirectories())
			{
				ParseFolder(dirInfo.FullName);
			}

			if (!m_inProgress.InProgress)
				return;

			foreach (FileInfo fileInfo in info.EnumerateFiles())
			{
				OnItemGrabbed(new Item(fileInfo));
			}
			OnFolderFinished(anItem);
		}

		public event Action<object, string> ExceptionOccuredEvent;
		public void OnExceptionOccured(string message)
		{
			if (ExceptionOccuredEvent != null)
			{
				ExceptionOccuredEvent(this, message);
			}
		}
	}
}
