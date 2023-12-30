using System;
using System.IO;
using System.Windows.Forms;
using NetFwTypeLib;

namespace FirewallAppBlockerConsole
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.Title = "App Internet Restrictor";
            Console.WriteLine("Type 'help' for available commands. Type 'exit' to close the application.");

            while (true)
            {
                Console.Write("Enter command: ");
                string command = Console.ReadLine().ToLower().Replace("-","");
                Console.Clear();

                switch (command)
                {
                    case "help":
                        DisplayHelpCommands();
                        break;
                    case "add":
                        OpenDialogue();
                        break;
                    case "remove":
                        RemoveRuleFromFirewall();
                        break;
                    case "exit":
                        return;
                    case "list":
                        DisplayAddedRestrictions("Internet Restricted");
                        break;
                    default:
                        Console.WriteLine("-" + command + " is an unknown command.");
                        DisplayHelpCommands();
                        break;
                }
            }
        }


        private static void RemoveRuleFromFirewall()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe",
                Title = "Select Program"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                RemoveFirewallRulesByPath(selectedFilePath);
            }
            else
            {
                Console.WriteLine("No file was selected.");
            }
        }

        private static void RemoveFirewallRulesByPath(string filePath)
        {
            try
            {
                Type netFwPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy = (INetFwPolicy2)Activator.CreateInstance(netFwPolicyType);

                foreach (INetFwRule rule in fwPolicy.Rules)
                {
                    if (!string.IsNullOrEmpty(rule.ApplicationName) &&
                        rule.ApplicationName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                    {
                        fwPolicy.Rules.Remove(rule.Name);
                        Console.WriteLine($"Removed firewall rule: {rule.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error removing firewall rules: " + ex.Message);
            }
        }



        private static void DisplayHelpCommands()
        {
            Console.WriteLine("Available commands:\n" +
                            " -add    | Create a new restricting rule for a program\n" +
                            " -remove | Remove already existing rule for a program\n" +
                            " -list   | Display current block-list of the programs added through this app\n" +
                            " -exit   | Close the application." +
                            " -help   | Show all available commands\n\n" +
                            "Developed by Alexander Nester, github.com/anestercommprod\n");
        }

        private static void OpenDialogue()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Executable Files (*.exe)|*.exe",
                Title = "Select Programs to Restrict"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    if (File.Exists(file) && file.ToLower().EndsWith(".exe"))
                    {
                        if (!IsRuleExists(file, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN) &&
                            !IsRuleExists(file, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT))
                        {
                            bool resultInbound = CreateFirewallRule(file, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
                            bool resultOutbound = CreateFirewallRule(file, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);

                            if (resultInbound && resultOutbound)
                            {
                                Console.WriteLine($"Rules created successfully for {file}");
                            }
                            else
                            {
                                Console.WriteLine($"Failed to create rules for {file}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Firewall rules already exist for {file}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid file path or file is not an executable: {file}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No files were selected.");
            }
        }

        private static bool CreateFirewallRule(string filePath, NET_FW_RULE_DIRECTION_ direction)
        {
            try
            {
                string appName = Path.GetFileNameWithoutExtension(filePath);
                string ruleName = "Internet Restricted: " + appName + (direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN ? " Inbound" : " Outbound");

                Type netFwPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy = (INetFwPolicy2)Activator.CreateInstance(netFwPolicyType);

                INetFwRule newRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                newRule.Name = ruleName;
                newRule.Description = "Block " + (direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN ? "inbound" : "outbound") + " traffic for " + appName;
                newRule.ApplicationName = filePath;
                newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                newRule.Direction = direction;
                newRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;
                newRule.Profiles = (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
                newRule.Enabled = true;

                fwPolicy.Rules.Add(newRule);

                DisplayFirewallRuleFilePaths(newRule.Name);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating " + (direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN ? "inbound" : "outbound") + " firewall rule for " + filePath + ": " + ex.Message);
                return false;
            }
        }

        private static void DisplayAddedRestrictions(string ruleNamePattern)
        {
            try
            {
                Type netFwPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy = (INetFwPolicy2)Activator.CreateInstance(netFwPolicyType);

                Console.WriteLine($"Firewall rules containing '{ruleNamePattern}' in their name:");
                foreach (INetFwRule rule in fwPolicy.Rules)
                {
                    if (rule.Name.Contains(ruleNamePattern) && rule.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT)
                    {
                        Console.WriteLine($"Rule Name: {rule.Name } \\ Outbound,\nApplication Path: {rule.ApplicationName}\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while displaying specific firewall rules: " + ex.Message);
            }
        }

        private static void DisplayFirewallRuleFilePaths(string ruleNamePattern)
        {
            Console.WriteLine("Checking for rule name pattern: " + ruleNamePattern);
            try
            {
                Type netFwPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy = (INetFwPolicy2)Activator.CreateInstance(netFwPolicyType);

                foreach (INetFwRule rule in fwPolicy.Rules)
                {
                    if (!string.IsNullOrEmpty(rule.Name) && rule.Name.Contains(ruleNamePattern))
                    {
                        Console.WriteLine($"Rule: {rule.Name}, Application Path: {rule.ApplicationName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while displaying firewall rule file paths: " + ex.Message);
            }
        }


        private static bool IsRuleExists(string filePath, NET_FW_RULE_DIRECTION_ direction)
        {
            try
            {
                Type netFwPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy = (INetFwPolicy2)Activator.CreateInstance(netFwPolicyType);

                foreach (INetFwRule rule in fwPolicy.Rules)
                {
                    if (rule.ApplicationName != null && rule.ApplicationName.Equals(filePath, StringComparison.OrdinalIgnoreCase) && rule.Direction == direction && rule.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking for existing firewall rule: " + ex.Message);
                return false;
            }
        }
    }
}
