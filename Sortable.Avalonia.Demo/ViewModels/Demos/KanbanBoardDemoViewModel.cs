// MIT License
// 
// Copyright (c) 2026 Russell Camo (russkyc)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sortable.Avalonia.Demo.Models;

namespace Sortable.Avalonia.Demo.ViewModels.Demos;

public partial class KanbanBoardDemoViewModel : DemoViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<KanbanColumn> _columns = new();

    public KanbanBoardDemoViewModel()
    {
        var triage = new KanbanColumn("Support Triage");
        triage.Items.Add(new SortableItem("P1 outage: EU tenant affected")     { Tag = "🔴", Note = "On-call: @jlee · SLA breach imminent" });
        triage.Items.Add(new SortableItem("Billing mismatch for annual invoice") { Tag = "🟡", Note = "Finance notified · Awaiting refund approval" });
        triage.Items.Add(new SortableItem("Mobile app login timeout")           { Tag = "🟡", Note = "Reproduced in staging · iOS 17 only" });

        var inProgress = new KanbanColumn("In Progress");
        inProgress.Items.Add(new SortableItem("Retry policy for payment webhook") { Tag = "🟢", Note = "PR #412 under review · ETA tomorrow" });
        inProgress.Items.Add(new SortableItem("Fix stale cache in dashboard")     { Tag = "🟡", Note = "Root cause identified · Redis config" });

        var ready = new KanbanColumn("Ready for Release");
        ready.Items.Add(new SortableItem("Runbook links in alert messages")  { Tag = "🟢", Note = "QA passed · Docs updated" });
        ready.Items.Add(new SortableItem("SOC2 audit evidence export")       { Tag = "🟢", Note = "Awaiting final sign-off · Compliance" });

        Columns.Add(triage);
        Columns.Add(inProgress);
        Columns.Add(ready);
    }

    [RelayCommand]
    private void OnItemDropped(SortableDropEventArgs e)
    {
        if (e.Item is not SortableItem item) return;

        KanbanColumn? sourceColumn = null;
        KanbanColumn? targetColumn = null;

        foreach (var column in Columns)
        {
            if (ReferenceEquals(column.Items, e.SourceCollection)) sourceColumn = column;
            if (ReferenceEquals(column.Items, e.TargetCollection)) targetColumn = column;
        }

        if (sourceColumn is null || targetColumn is null) return;

        if (ReferenceEquals(sourceColumn, targetColumn))
        {
            e.IsAccepted = true;
            e.TransferMode = SortableTransferMode.Move;
            _ = TryApplyDropMutation(e);
        }
        else
        {
            if (targetColumn.Items.Contains(item)) { e.IsAccepted = false; return; }
            e.IsAccepted = true;
            e.TransferMode = SortableTransferMode.Move;
            if (!TryApplyDropMutation(e)) return;
            LogEvent("📋", $"'{item.Name}' → '{targetColumn.Title}'");
        }
    }
}
