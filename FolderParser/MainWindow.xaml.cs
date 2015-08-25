using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FolderParser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Parser m_parser = null;
		private TreeFiller m_treeFiller = null;
		private XMLFiller m_xmlFiller = null;
		private volatile ProgresStateSynchronizer m_progressToken = new ProgresStateSynchronizer(false);
		private List<string> m_errorMessages = new List<string>();
		public MainWindow()
		{
			InitializeComponent();
			Thread.CurrentThread.Name = "Main Thread";

			CountdownEvent folderEndByXML = new CountdownEvent(1);
			CountdownEvent folderEndByTree = new CountdownEvent(1);
			CountdownEvent fileEndByXML = new CountdownEvent(1);
			CountdownEvent fileEndByTree = new CountdownEvent(1);

			m_parser = new Parser(m_progressToken, folderEndByTree, folderEndByXML, fileEndByTree, fileEndByXML);
			m_treeFiller = new TreeFiller(m_treeView, m_progressToken, folderEndByTree, fileEndByTree);
			m_xmlFiller = new XMLFiller(m_progressToken, folderEndByXML, fileEndByXML);

			m_folderName.DataContext = m_parser;
			m_xmlFileName.DataContext = m_xmlFiller;

			m_parser.ItemGrabbed += m_xmlFiller.ItemGrabbedHandler;
			m_parser.ItemGrabbed += m_treeFiller.ItemGrabbedHandler;
			m_parser.FolderStarted += m_xmlFiller.FolderStartedHandler;
			m_parser.FolderStarted += m_treeFiller.FolderStartedHandler;
			m_parser.FolderFinished += m_xmlFiller.FolderFinishedHandler;
			m_parser.FolderFinished += m_treeFiller.FolderFinishedHandler;

			m_parser.ParserFinishEvent += this.ParserFinishEventHandler;

			m_xmlFiller.ExceptionOccuredEvent += this.ExceptionOccuredHandler;
			m_treeFiller.ExceptionOccuredEvent += this.ExceptionOccuredHandler;
			m_parser.ExceptionOccuredEvent += this.ExceptionOccuredHandler;
		}

		private void BrowseFolderButton_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = BrowseDialogFactory("Select initial folder to start with");
			if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
			{
				m_parser.InitFolder = dialog.FileName;
			}
		}
		private void BrowseOutputFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = BrowseDialogFactory("Select folder to store output file in it");
			if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
			{
				int slashIndex = m_xmlFiller.OutputFileName.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
				slashIndex = slashIndex >= 0 ? slashIndex : 0;
				string fileName = System.IO.Path.Combine(dialog.FileName, m_xmlFiller.OutputFileName.Substring(slashIndex));
				m_xmlFiller.OutputFileName = fileName;
			}
		}
		/// <summary>
		/// creates native looking browse dialog for picking up folder.
		/// </summary>
		/// <remarks>
		/// need to run command in command shell: Install-Package WindowsAPICodePack-Shell  - for fancy CommonOpenFileDialog
		/// </remarks>
		/// <param name="name">caption for the browse dialog</param>
		/// <returns>dialog for selecting a folder</returns>
		private CommonOpenFileDialog BrowseDialogFactory(string name)
		{
			if (!CommonFileDialog.IsPlatformSupported)
			{
				MessageBox.Show(this, "cannot use fancy open file dialog. please run the app on Windows Vista+", "too old OS",
					MessageBoxButton.OK, MessageBoxImage.Error);
				Close();
			}
			var dialog = new CommonOpenFileDialog(name);
			dialog.IsFolderPicker = true;
			dialog.Multiselect = false;
			dialog.AllowNonFileSystemItems = false;
			return dialog;
		}

		private void StartButton_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				EnableControls(false);
				m_errorMessages.Clear();

				m_progressToken.InProgress = true;

				m_xmlFiller.Start();	// todo: make it event handler for one from parser
				m_treeFiller.Start();
				m_parser.Start();
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private void EnableControls(bool bEnable)
		{
			m_folderName.IsEnabled = bEnable;
			m_xmlFileName.IsEnabled = bEnable;
			m_btnBrowseXML.IsEnabled = bEnable;
			m_btnBrowseFolder.IsEnabled = bEnable;
			m_btnStart.IsEnabled = bEnable;
		}

		private void ExceptionOccuredHandler(object sender, string message)
		{
			lock (m_errorMessages)
			{
				m_errorMessages.Add(message);
				m_progressToken.InProgress = false;
			}
		}

		private void ParserFinishEventHandler(object sender, string message)
		{
			lock (m_errorMessages)
			{
				m_progressToken.InProgress = false;
				this.Dispatcher.Invoke(() =>
				{
					if (m_errorMessages.Count != 0)
					{
						string allErrors = string.Join("\n", m_errorMessages);
						MessageBox.Show(this, allErrors, "Exceptions!", MessageBoxButton.OK, MessageBoxImage.Error);
					}
					else
					{
						MessageBox.Show(this, message, "Well done!", MessageBoxButton.OK, MessageBoxImage.Information);
					}
					EnableControls(true);
				});
			}
		}

		private void MainWindow_OnClosing(object sender, CancelEventArgs e)
		{
			if (m_progressToken.InProgress)
			{
				m_progressToken.InProgress = false;
				MessageBox.Show(this, "Cannot close while processing. Progress is being stopped. Try again!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				e.Cancel = true;
			}
			else
			{
				e.Cancel = false;
			}
		}
	}
}
