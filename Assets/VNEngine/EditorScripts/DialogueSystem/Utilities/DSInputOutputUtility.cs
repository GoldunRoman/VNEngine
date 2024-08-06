#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VNEngine.DS.Utilities
{
    using Data;
    using Data.Save;
    using ScriptableObjects;
    using Elements;
    using Windows;
    using UnityEditor.Experimental.GraphView;

    public static class DSInputOutputUtility
    {
        private static DSGraphView s_graphView;

        private static string s_graphFileName;
        private static string s_containerFolderPath;

        private static List<DSGroup> s_groups;
        private static List<DSNode> s_nodes;

        private static Dictionary<string, DSDialogueGroupSO> s_createdDialogueGroups;
        private static Dictionary<string, DSDialogueSO> s_createdDialogues;
        private static Dictionary<string, DSGroup> s_loadedGroups;
        private static Dictionary<string, DSNode> s_loadedNodes;

        public static void Initialize(DSGraphView dSGraphView, string graphName)
        {
            s_graphView = dSGraphView;

            s_graphFileName = graphName;
            s_containerFolderPath = $"Assets/VNEngine/DialoguesSavedAssets/Graphs/{s_graphFileName}";

            s_groups = new List<DSGroup>();
            s_nodes = new List<DSNode>();

            s_createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
            s_createdDialogues = new Dictionary<string, DSDialogueSO>();
            s_loadedGroups = new Dictionary<string, DSGroup>();
            s_loadedNodes = new Dictionary<string, DSNode>();
        }

        #region Load Methods
        public static void Load()
        {
            DSGraphSaveDataSO graphData = LoadAsset<DSGraphSaveDataSO>("Assets/VNEngine/DialoguesSavedAssets/Graphs", s_graphFileName);

            if(graphData == null)
            {
                EditorUtility.DisplayDialog(
                        "Couldn't load the file",
                        "The file at the following path could not be found: \n\n" +
                        $"Assets/VNEngine/DialoguesSavedAssets/Graphs/{s_graphFileName}\n\n" +
                        "Make sure you chose the right file and it's placed at the folder path mentioned above",
                        "Yep!"
                    );

                return;
            }

            DSEngineWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadGroups(List<DSGroupSaveData> groups)
        {
            foreach(DSGroupSaveData groupData in groups)
            {
                DSGroup group = s_graphView.CreateGroup(groupData.Name, groupData.Position);

                group.ID = groupData.ID;

                s_loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<DSNodeSaveData> nodes)
        {
            foreach(DSNodeSaveData nodeData in nodes)
            {
                List<DSChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);

                DSNode node = s_graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;

                node.Draw();

                s_graphView.AddElement(node);

                s_loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID))
                    continue;

                DSGroup group = s_loadedGroups[nodeData.GroupID];

                node.Group = group;

                group.AddElement(node);
            }
        }

        private static void LoadNodesConnections()
        {
            foreach(KeyValuePair<string, DSNode> loadedNode in s_loadedNodes)
            {
                foreach(Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    DSChoiceSaveData choiceData = (DSChoiceSaveData)choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NodeID))
                        continue;

                    DSNode nextNode = s_loadedNodes[choiceData.NodeID];

                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                    s_graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }
        #endregion

        #region Save Methods
        public static void Save()
        {
            CreateStaticFolders();
            GetElementsFromGraphView();

            DSGraphSaveDataSO graphData = CreateAsset<DSGraphSaveDataSO>("Assets/VNEngine/DialoguesSavedAssets/Graphs", $"{s_graphFileName}Graph");
            graphData.Initialize(s_graphFileName);

            DSDialogueContainerSO dialogueContainer = CreateAsset<DSDialogueContainerSO>(s_containerFolderPath, s_graphFileName);
            dialogueContainer.Initialize(s_graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        #region Groups
        private static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach(DSGroup group in s_groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
        {
            DSGroupSaveData groupData = new DSGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{s_containerFolderPath}/Groups", groupName);
            CreateFolder($"{s_containerFolderPath}/Groups/{groupName}", "Dialogues");

            DSDialogueGroupSO dialogueGroup = CreateAsset<DSDialogueGroupSO>($"{s_containerFolderPath}/Groups/{groupName}", groupName);
            dialogueGroup.Initialize(groupName);

            s_createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());

            SaveAsset(dialogueGroup);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, DSGraphSaveDataSO graphData)
        {
            if(graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach(string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{s_containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }
        #endregion

        #region Nodes
        private static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach(DSNode node in s_nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                ungroupedNodeNames.Add(node.DialogueName);
            }

            UpdateDialoguesChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
        {
            List<DSChoiceSaveData> choices = CloneNodeChoices(node.Choices);

            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };

            graphData.Nodes.Add(nodeData);
        }

        private static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
        {
            DSDialogueSO dialogue;

            if(node.Group != null)
            {
                dialogue = CreateAsset<DSDialogueSO>($"{s_containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);
                dialogueContainer.DialogueGroups.AddItem(s_createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DSDialogueSO>($"{s_containerFolderPath}/Global/Dialogues", node.DialogueName);
                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(
                node.DialogueName,
                node.DialogueTitle,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode()
                );

            s_createdDialogues.Add(node.ID, dialogue);

            SaveAsset(dialogue);
        }

        private static List<DSDialogueChioceData> ConvertNodeChoicesToDialogueChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSDialogueChioceData> dialogueChoices = new List<DSDialogueChioceData>();

            foreach(DSChoiceSaveData nodeChoice in nodeChoices)
            {
                DSDialogueChioceData choiceData = new DSDialogueChioceData()
                {
                    Text = nodeChoice.Text
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        private static void UpdateDialoguesChoicesConnections()
        {
            foreach(DSNode node in s_nodes)
            {
                DSDialogueSO dialogue = s_createdDialogues[node.ID];

                for (int choiceIndex = 0; choiceIndex < node.Choices.Count; ++choiceIndex)
                {
                    DSChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                    if (string.IsNullOrEmpty(nodeChoice.NodeID))
                        continue;

                    dialogue.Choices[choiceIndex].NextDialogue = s_createdDialogues[nodeChoice.NodeID];

                    SaveAsset(dialogue);
                }
            }
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if(graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach(KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach(string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{s_containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if(graphData.OldUngroupedNames != null && graphData.OldUngroupedNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNames.Except(currentUngroupedNodeNames).ToList();

                foreach(string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{s_containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNames = new List<string>(currentUngroupedNodeNames);
        }
        #endregion
        #endregion

        #region Creation Methods
        private static void CreateStaticFolders()
        {
            CreateFolder("Assets", "VNEngine");
            CreateFolder("Assets/VNEngine", "DialoguesSavedAssets");
            CreateFolder("Assets/VNEngine/DialoguesSavedAssets", "Graphs");
            CreateFolder($"Assets/VNEngine/DialoguesSavedAssets/Graphs", s_graphFileName);
            CreateFolder(s_containerFolderPath, "Groups");
            CreateFolder($"{s_containerFolderPath}/Groups", "Dialogues");
            CreateFolder(s_containerFolderPath, "Global");
            CreateFolder($"{s_containerFolderPath}/Global", "Dialogues");
        }
        #endregion

        #region Fetch Methods
        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(DSGroup);

            s_graphView.graphElements.ForEach(graphElement =>
            {
                if(graphElement is DSNode node)
                {
                    s_nodes.Add(node);

                    return;
                }

                if(graphElement.GetType() == groupType)
                {
                    DSGroup group = (DSGroup)graphElement;

                    s_groups.Add(group);

                    return;
                }
            });
        }
        #endregion

        #region Utility Methods
        private static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, folderName);
        }

        private static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }

        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        private static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<DSChoiceSaveData> CloneNodeChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();

            foreach (DSChoiceSaveData choice in nodeChoices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };

                choices.Add(choiceData);
            }

            return choices;
        }
        #endregion
    }
}
#endif