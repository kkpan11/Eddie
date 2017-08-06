﻿// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
	public class TableProvidersController : NSTableViewDataSource
	{
		public NSTableView tableView;

		public TableProvidersController(NSTableView tableView)
		{
			this.tableView = tableView;

			this.tableView.DataSource = this;

		}

		public void RefreshUI()
		{
			tableView.ReloadData();
		}

		public void DoubleClickItem()
		{
			int i = tableView.SelectedRow;
			if ((i >= 0) && (i < Engine.Instance.Stats.List.Count))
			{
				StatsEntry e = Engine.Instance.Stats.List[tableView.SelectedRow];

				Engine.Instance.Command("ui.stats." + e.Key, true);
				/*
				if (e.Key == "VpnGeneratedOVPN") {
					if (Engine.Instance.IsConnected () == false)
						return;
					WindowTextViewerController textViewer = new WindowTextViewerController ();
					(Engine.Instance as Engine).WindowsOpen.Add (textViewer);
					textViewer.Title = e.Caption;
					textViewer.Body = Engine.Instance.ConnectedOVPN;
					textViewer.ShowWindow (this);
				} else if (e.Key == "SystemReport") {
					WindowTextViewerController textViewer = new WindowTextViewerController ();
					(Engine.Instance as Engine).WindowsOpen.Add (textViewer);
					textViewer.Title = e.Caption;
					textViewer.Body = Platform.Instance.GenerateSystemReport ();
					textViewer.ShowWindow (this);
				} else if (e.Key == "ManifestLastUpdate") {
					Core.Threads.Manifest.Instance.ForceUpdate = true;
				}
				*/

			}
		}

		public override int GetRowCount(NSTableView tableView)
		{
			return Engine.Instance.ProvidersManager.Providers.Count;
		}

		public override NSObject GetObjectValue(NSTableView tableView,
												 NSTableColumn tableColumn,
												 int row)
		{
			Provider p = Engine.Instance.ProvidersManager.Providers[row];

			if (tableColumn.Identifier == "Enabled")
			{
				if (p.Enabled)
					return NSImage.ImageNamed("bool_true.png");
				else
					return NSImage.ImageNamed("bool_false.png");
			}
			else if (tableColumn.Identifier == "Title")
			{
				return new NSString(p.Title);
			}
			else if (tableColumn.Identifier == "DefinitionTitle")
			{
				return new NSString(p.DefinitionTitle);
			}
			else if (tableColumn.Identifier == "DefinitionSubTitle")
			{
				return new NSString(p.DefinitionSubTitle);
			}
			else if (tableColumn.Identifier == "DefinitionHref")
			{
				return new NSString(p.DefinitionHref);
			}
			else
				throw new NotImplementedException(string.Format("{0} is not recognized", tableColumn.Identifier));
		}

	}
}

