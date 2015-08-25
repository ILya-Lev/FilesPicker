using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FolderParser
{
	/// <summary>
	/// The class is used to send information about file or folder from Parser class to ones that fills either XML or UI tree.
	/// It has properties with Display Name Attribute - the value is stored in XML as Attribute name (sorry for wordplay).
	/// Moreover presence of the attribute defines whether the property will be stored in XML
	/// Property Id is used to check whether data hierarchy for XML is not violated - it's used for folders only, but class Item 
	/// should not know anything of how is it used. Therefore Id is generated for folders too.
	/// </summary>
	internal class Item
	{
		public Item(FileSystemInfo info)
		{
			Id = Guid.NewGuid().ToString();
			Name = info.Name;
			Created = info.CreationTime;
			Modified = info.LastWriteTime;
			LastAccess = info.LastAccessTime;

			var fileInfo = info as FileInfo;
			if (fileInfo != null)
			{
				m_isReadOnly = fileInfo.IsReadOnly;
				Size = fileInfo.Length;
				IsFile = true;
			}
			else
			{
				IsFile = false;
			}
			FileSecurity fs = File.GetAccessControl(info.FullName);
			var sidOwning = fs.GetOwner(typeof(SecurityIdentifier));
			var ntAccount = sidOwning.Translate(typeof(NTAccount));
			Owner = ntAccount.Value;

			// todo: it's not so important, but still put here something like read, write etc.
			var sidRules = fs.GetAccessRules(true, true, typeof(SecurityIdentifier));
			List<string> rulesList = new List<string>(sidRules.Count);
			for (int i = 0; i < sidRules.Count; i++)
			{
				rulesList.Add(sidRules[i].IdentityReference.Value);
			}
			Rights = string.Join("; ", rulesList);
		}

		public string Id { get; private set; }

		[DisplayNameAttribute("Name")]
		public string Name { get; private set; }

		[DisplayNameAttribute("Created")]
		public DateTime Created { get; private set; }

		[DisplayNameAttribute("Modified")]
		public DateTime Modified { get; private set; }

		[DisplayNameAttribute("LastAccess")]
		public DateTime LastAccess { get; private set; }

		private bool m_isReadOnly;
		private bool m_isSystem = false;
		private bool m_isHidden = false;
		private bool m_isTemporary = false;

		[DisplayNameAttribute("Attributes")]
		public string Attributes
		{
			get
			{
				List<string> attributes = new List<string>();
				if (m_isTemporary) attributes.Add("Temporary");
				if (m_isHidden) attributes.Add("Hidden");
				if (m_isSystem) attributes.Add("System");
				if (m_isReadOnly) attributes.Add("ReadOnly");

				return string.Join(", ", attributes);
			}
		}

		[DisplayNameAttribute("Size")]
		public long Size { get; private set; }

		[DisplayNameAttribute("Owner")]
		public string Owner { get; set; }

		[DisplayNameAttribute("Rights")]
		public string Rights { get; set; }

		public bool IsFile { get; private set; }

		public static string GetDisplayName(MemberInfo memberInfo)
		{
			var attr = memberInfo.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
			return attr == null ? "" : attr.DisplayName;
		}
	}
}
