﻿using ApplicationMyRoots.Common;
using ApplicationMyRoots.DAL;
using ApplicationMyRoots.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ApplicationMyRoots.Controllers
{
    public class HtmlBuilderController : ApiController
    {
        // Pierwszy raz użytkownik stworzył konto i dostaje na drzewie tylko swoją osobę
        [HttpGet]
        public string GetUserMainTree(int id)
        {
            using (var db = new DAL.DbContext())
            {
                try
                {

                    User user = db.Users.Find(id);
                    if (user != null)
                    {
                        var userTrees = db.UserTrees.Where(ut => ut.UserID == user.UserID);

                        if (userTrees != null && userTrees.Count() > 0) // user ma już drzewo - zwracamy które ma ustawione isMainTree na true
                        {
                            return userTrees.Where(ut => ut.isMainTree == true).First().TreeHtmlCode;
                        }
                        else //pierwsze zalogowanie
                        {

                            string htmlTree =
                                "<svg class=\"tree-elements\" id=\"" + user.UserID + "\" name=\"" + user.UserID + "\" width=\"200\" height=\"100\" x=\"0\" y=\"0\">" +
                                    "<rect class=\"tree-element-frames\" width=\"200\" height=\"100\" x=\"0\" y=\"0\" fill=\"white\" stroke=\"black\" />" +
                                    "<text class=\"tree-element-texts\" x=\"50%\" y=\"10%\" font-family=\"Verdana\" font-size=\"12\" fill=\"blue\" alignment-baseline=\"middle\" text-anchor=\"middle\">" + user.NameSurname + "</text>" +
                                "</svg>";


                            db.UserTrees.Add(new UserTree
                            {
                                isMainTree = true,
                                TreeHtmlCode = htmlTree,
                                UserID = user.UserID,
                                User = user,
                                TransformMatrix = "matrix(2 0 0 2 0 0)"
                            });
                            db.SaveChanges();

                            return htmlTree;
                        }
                    }
                    else // nie znaleziono usera o podanym id
                    {
                        db.Errors.Add(new Error
                        {
                            Message = "HtmlBuilderController - API - (GetUserMainTree) - user is null !! - sprawdź przekazywany id do metody przekazana wartość:"+id,
                            StackTrace = "HtmlBuilderController - API - (GetUserMainTree) - user is null !!",
                            DateThrow = DateTime.Now
                        });
                        db.SaveChanges();

                        return "";
                    }
                }catch(Exception e) // 
                {
                    db.Errors.Add(new Error
                    {
                        Message = e.Message,
                        StackTrace = e.StackTrace,
                        DateThrow = DateTime.Now
                    });
                    db.SaveChanges();

                    return "";
                }
            }
        }


        [HttpGet]
        public string GetUserMainTreeTransformMatrix(int id)
        {
            using (var db = new DAL.DbContext())
            {
                try
                {
                    if(db.UserTrees.Where(ut => ut.UserID == id && ut.isMainTree == true).Count() > 0)
                    {
                        return db.UserTrees.Where(ut => ut.UserID == id && ut.isMainTree == true).First().TransformMatrix;
                    }
                    else
                    {
                        return null;
                    }
                    

                }catch(Exception e)
                {
                    db.Errors.Add(new Error
                    {
                        Message = e.Message,
                        StackTrace = e.StackTrace,
                        DateThrow = DateTime.Now
                    });
                    db.SaveChanges();

                    return null; 
                }
            }
        }


        [HttpGet]
        public void SaveUserMainTree(int id)
        {
            using (var db = new DAL.DbContext())
            {
                try
                {
                    UserTree userTree = db.UserTrees.Where(ut => ut.UserID == id && ut.isMainTree == true).First();
                    userTree.TransformMatrix = this.Request.Headers.GetValues("matrix").First();

                    db.Entry(userTree).State = EntityState.Modified;
                    db.SaveChanges();
                }catch (Exception e)
                {

                    db.Errors.Add(new Error
                    {
                        Message = e.Message,
                        StackTrace = e.StackTrace,
                        DateThrow = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }
        }
    }
}