using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using static DialogueSetBackgroundNode;

// Walks every DialogueGroup/DialogueBlock currently in the open scene and
// exports their jump relationships (PlayGroup, Choice, Split, EndSplit,
// ForceEndSplit, WindowClose outcomes) as a Graphviz DOT file. Open the
// result with any DOT viewer (e.g. the VS Code Graphviz extension, or
// https://dreampuf.github.io/GraphvizOnline) to see the story as a graph.
// Purely a read-only export — doesn't touch any dialogue data.
public static class DialogueGraphExporter
{
    [MenuItem("Tools/Dialogue/Export Graph (DOT)")]
    public static void Export()
    {
        string path = EditorUtility.SaveFilePanel(
            "Export Dialogue Graph",
            Application.dataPath + "/..",
            "DialogueGraph",
            "dot");

        if (string.IsNullOrEmpty(path)) return;

        DialogueGroup[] groups = Object.FindObjectsByType<DialogueGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        DialogueBlock[] allBlocks = Object.FindObjectsByType<DialogueBlock>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        HashSet<DialogueBlock> groupedBlocks = new HashSet<DialogueBlock>();
        foreach (DialogueGroup group in groups)
        {
            foreach (DialogueBlock block in group.blocks)
            {
                if (block != null) groupedBlocks.Add(block);
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("digraph DialogueGraph {");
        sb.AppendLine("  rankdir=LR;");
        sb.AppendLine("  node [shape=box, style=filled, fillcolor=\"#f0f0f0\", fontname=\"Helvetica\"];");
        sb.AppendLine("  edge [fontname=\"Helvetica\", fontsize=10];");
        sb.AppendLine();

        int clusterIndex = 0;

        foreach (DialogueGroup group in groups)
        {
            sb.AppendLine($"  subgraph cluster_{clusterIndex++} {{");
            string clusterLabel = string.IsNullOrEmpty(group.chapterName) ? group.ID : $"{group.ID} — {group.chapterName}";
            sb.AppendLine($"    label=\"{Escape(clusterLabel)}\";");
            sb.AppendLine("    style=dashed;");

            for (int i = 0; i < group.blocks.Count; i++)
            {
                DialogueBlock block = group.blocks[i];
                if (block == null) continue;

                sb.AppendLine($"    \"{NodeID(block)}\" [label=\"{Escape(block.ID)}\"];");
            }

            // Implicit fallthrough: a block plays into the next one in the
            // group once it finishes, unless something redirects first.
            for (int i = 0; i < group.blocks.Count - 1; i++)
            {
                DialogueBlock from = group.blocks[i];
                DialogueBlock to = group.blocks[i + 1];
                if (from == null || to == null) continue;

                sb.AppendLine($"    \"{NodeID(from)}\" -> \"{NodeID(to)}\" [style=dashed, color=gray, label=\"next\"];");
            }

            sb.AppendLine("  }");
            sb.AppendLine();
        }

        // Any block not inside a group's list still gets a node, outside
        // the clusters, so dangling references don't silently disappear.
        foreach (DialogueBlock block in allBlocks)
        {
            if (groupedBlocks.Contains(block)) continue;
            sb.AppendLine($"  \"{NodeID(block)}\" [label=\"{Escape(block.ID)}\", fillcolor=\"#ffe0e0\"];");
        }

        sb.AppendLine();

        // Explicit jumps, gathered from every node type that can redirect
        // the story.
        foreach (DialogueBlock block in allBlocks)
        {
            if (block.nodes == null) continue;

            foreach (DialogueBlockNode node in block.nodes)
            {
                switch (node)
                {
                    case DialoguePlayGroupNode playGroup:
                        AddEdge(sb, block, playGroup.group, playGroup.block, "PlayGroup");
                        break;

                    case DialogueChoiceNode choice:
                        if (choice.choices != null)
                        {
                            foreach (DialogueChoice c in choice.choices)
                            {
                                string label = string.IsNullOrEmpty(c.text) ? "Choice" : $"Choice: {Truncate(c.text, 24)}";
                                AddEdge(sb, block, c.linkedGroup, c.linkedBlock, label);
                            }
                        }
                        break;

                    case SplitPlayGroupNode split:
                        AddEdge(sb, block, split.window1Group, split.window1Block, "Split -> Win1", "#2060c0");
                        AddEdge(sb, block, split.window2Group, split.window2Block, "Split -> Win2", "#c02060");
                        break;

                    case EndSplitGroupNode endSplit:
                        AddEdge(sb, block, endSplit.nextGroup, endSplit.nextBlock, $"EndSplit ({endSplit.splitID})", "#20a020");
                        break;

                    case DialogueForceEndSplitNode forceEnd:
                        AddEdge(sb, block, forceEnd.nextGroup, forceEnd.nextBlock, "ForceEndSplit", "#a02020");
                        break;

                    case DialogueWindowCloseChoiceNode windowClose:
                        AddEdge(sb, block, windowClose.groupIfHostCloses, windowClose.blockIfHostCloses, "IfHostCloses", "#806000");
                        AddEdge(sb, block, windowClose.groupIfClientCloses, windowClose.blockIfClientCloses, "IfClientCloses", "#806000");
                        break;
                }
            }
        }

        sb.AppendLine("}");

        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[DialogueGraphExporter] Exported {groups.Length} groups / {allBlocks.Length} blocks to: {path}");
        EditorUtility.RevealInFinder(path);
    }

    private static void AddEdge(StringBuilder sb, DialogueBlock from, DialogueGroup targetGroup, DialogueBlock targetBlock, string label, string color = null)
    {
        if (targetGroup == null) return;

        DialogueBlock resolvedTarget = targetBlock;

        if (resolvedTarget == null)
        {
            if (targetGroup.blocks.Count == 0) return;
            resolvedTarget = targetGroup.blocks[0];
        }

        if (resolvedTarget == null) return;

        string colorAttr = string.IsNullOrEmpty(color) ? "" : $", color=\"{color}\", fontcolor=\"{color}\"";
        sb.AppendLine($"  \"{NodeID(from)}\" -> \"{NodeID(resolvedTarget)}\" [label=\"{Escape(label)}\"{colorAttr}];");
    }

    private static string NodeID(DialogueBlock block)
    {
        return string.IsNullOrEmpty(block.ID) ? $"block_{block.GetInstanceID()}" : block.ID;
    }

    private static string Escape(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ");
    }

    private static string Truncate(string text, int maxLength)
    {
        text = text.Replace("\n", " ").Trim();
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }
}