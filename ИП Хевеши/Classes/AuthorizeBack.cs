﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.Views;

namespace ИП_Хевеши.Classes
{
    public class AuthorizeBack : AuthorizeWn
    {
        public static string userName;
        public static int UserRole;
        public static List<Users> GetUserList()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                return users = (from u in context.Users select u).ToList();
            }
        }

        public static void AuthorizeUser(TextBox tbLogin, PasswordBox pbPassword, TextBox tbVisiblePassword, CheckBox cbHideShowPassword, List<Users> users, AuthorizeWn authorizeWn)
        {
            try
            {
                int CorrectUserId = 0;
                bool IsCorrectUser = false;
                if (cbHideShowPassword.IsChecked == false )
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i].Login == tbLogin.Text && users[i].Password == pbPassword.Password)
                        {
                            IsCorrectUser = true;
                            CorrectUserId = i;
                            userName = users[i].UserName + " " + users[i].UserSurname;
                            UserRole = users[i].RoleID;
                        }
                    }
                    if (!IsCorrectUser)
                    {
                        MessageBox.Show("Неправильный логин или пароль", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                    else
                    {
                        
                        MainContentWn mainContentWn = new MainContentWn(userName,  CorrectUserId, UserRole); /*users[CorrectUserId].Login*/
                        Application.Current.MainWindow =  mainContentWn;
                        authorizeWn.Close();
                        mainContentWn.Show();
                        
                    }
                }
                else if (cbHideShowPassword.IsChecked == true)
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i].Login == tbLogin.Text && users[i].Password == tbVisiblePassword.Text)
                        {
                            IsCorrectUser = true;
                            CorrectUserId = i;
                            userName = users[i].UserName + " " + users[i].UserSurname;
                            UserRole = users[i].RoleID;
                        }
                    }
                    if (!IsCorrectUser)
                    {
                        MessageBox.Show("Неправильный логин или пароль", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                    else
                    {
                      
                        MainContentWn mainContentWn = new MainContentWn(userName, CorrectUserId, UserRole); /*users[CorrectUserId].Login*/
                        authorizeWn.Close();
                        mainContentWn.Show();

                    }
                }  
            } 
            catch(Exception ex) 
            { 
                MessageBox.Show($"Возникла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

