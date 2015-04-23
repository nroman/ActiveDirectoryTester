using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Machine name: ");
            var machineName = Console.ReadLine();
            Console.Write("Username: ");
            var userName = Console.ReadLine();
            
            if (String.IsNullOrEmpty(userName))
            {
                ShowConsoleError("You should input the user name!");
                return;
            }

            var password = GetUserPassword();

            try
            {
                using (var pc = new PrincipalContext(ContextType.Machine, machineName))
                {
                    try
                    {
                        if (pc.ValidateCredentials(userName, password))
                        {
                            Console.WriteLine("\nValid credentials.\n");
                        }
                        else
                        {
                            ShowConsoleError("\nInvalid credentials!\n");
                        }
                    }
                    catch (PrincipalOperationException ex)
                    {
                        var user = UserPrincipal.FindByIdentity(pc, userName);

                        if (user != null && user.LastLogon == null)
                        {
                            ShowConsoleError("\nERROR: " + ex.Message);
                            Console.Write("Do you want to set password now? y/n: ");
                            String answer = Console.ReadLine();
                            if (answer.Equals("y") || answer.Equals("yes"))
                            {
                                password = GetUserPassword();
                                SetNewUserPassword(userName, password, pc);
                            }
                        }
                        else
                        {
                            ShowConsoleError("\nUSER VALIDATION ERROR: " + ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowConsoleError("\nUSER VALIDATION ERROR: " + ex.Message);
                    }

                    Console.WriteLine("\nPress Enter to show useres list...");
                    Console.ReadLine();
                    Console.WriteLine("\nThe list of users:\n-----------------\n");
                    ShowAllUserInAD(pc);
                }
            }
            catch (Exception ex)
            {
                ShowConsoleError("\nERROR: " + ex.Message);
            }
        }

        /// <summary>
        /// Show the list of users in AD
        /// </summary>
        /// <param name="context">
        /// The principal context
        /// </param>
        private static void ShowAllUserInAD(PrincipalContext context)
        {
            using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
            {
                foreach (var result in searcher.FindAll())
                {
                    
                    var de = result.GetUnderlyingObject() as DirectoryEntry;
                    /* Sample to rename the user
                    if (de.Name == "renamedUSER")
                    {
                        de.Rename("userWithTheDiffName");
                    }
                    */

                    Console.WriteLine("Login: " + de.Properties["Name"].Value);
                    Console.WriteLine("Full Name: "+ de.Properties["FullName"].Value);
                    Console.WriteLine("Last Login Time: " + de.Properties["LastLogin"].Value);
                    Console.WriteLine();
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Helper to inpun the psw from console. 
        /// Instead of inputed letters user see asterisks
        /// </summary>
        /// <returns>
        /// The psw string
        /// </returns>
        private static String GetUserPassword()
        {
            string pass = "";
            Console.Write("Enter your password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            return pass;
        }

        /// <summary>
        /// Setter of new psw
        /// </summary>
        /// <param name="userName">
        /// The user name
        /// </param>
        /// <param name="userPsw">
        /// The password
        /// </param>
        /// <param name="pc">
        /// The principal context
        /// </param>
        private static void SetNewUserPassword(String userName, String userPsw, PrincipalContext pc)
        {
            try
            {
                using (UserPrincipal usrPrincipal = UserPrincipal.FindByIdentity(pc, userName))
                {
                    if (usrPrincipal != null)
                    {
                        usrPrincipal.SetPassword(userPsw);
                        Console.WriteLine("Password was changed succesfully!");
                    }
                    else
                    {
                        ShowConsoleError("ERROR: Cannot change your password!");
                    }
                }
            }
            catch (PrincipalOperationException ex)
            {
                ShowConsoleError("\nERROR: " + ex.Message);
            }
            catch (Exception ex)
            {
                ShowConsoleError("\nERROR: " + ex.Message);
            }
        }

        /// <summary>
        /// Show in the console the red error message
        /// </summary>
        /// <param name="errorMessage">
        /// The message text
        /// </param>
        private static void ShowConsoleError(String errorMessage)
        {
            var consoleTextColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ForegroundColor = consoleTextColor;
        }
    }
}
