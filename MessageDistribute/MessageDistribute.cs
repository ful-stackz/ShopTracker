using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ShopTracker.MessageDistribute
{
    public static class MessageDistribute
    {
        public static void AddMessage(ITempDataDictionary tempData, Message message)
        {
            // Get current tempData messages
            if (tempData["Messages"] == null)
            {
                List<Message> msgList = new List<Message> { message };

                tempData["Messages"] = JsonConvert.SerializeObject(msgList);

                // Message added
                // Quit method
                //
                return;
            }

            // Message not added
            tempData.TryGetValue("Messages", out object obj);

            if (obj != null)
            {
                try
                {
                    obj = JsonConvert.DeserializeObject<List<Message>>((string)obj);
                }
                catch (Exception ex)
                {
                    Console.Write("EXCEPTION CAUGHT:\n{0}", ex);
                    return;
                }

                if (obj.GetType() == typeof(List<Message>))
                {
                    (obj as List<Message>).Add(message);
                    tempData["Messages"] = JsonConvert.SerializeObject(obj);
                }
                
            }
        }

        public static List<Message> GetMessages(ITempDataDictionary tempData)
        {
            // If tempdata has no records for messages then return a null object
            //
            if (tempData["Messages"] == null) return null;

            //
            // There are messages in TempData
            //

            // Get serialized expected List<Messages> from TempData
            //
            tempData.TryGetValue("Messages", out object Messages);

            // Check if the retrieved object is null
            // if it is return a null object
            //
            if (Messages == null) return null;

            // Attempt to Deserialize retrieved object as List<Message>
            //
            Messages = JsonConvert.DeserializeObject<List<Message>>((string)Messages);

            // Check the type of the deserialized object and if it is not List<Message>
            // return a null object
            //
            if (Messages.GetType() != typeof(List<Message>)) return null;

            //
            // Retrieved object from TempData is List<Message>
            //

            // Return the list of messages
            //
            return Messages as List<Message>;
        }

        public static void AddErrorMessage(ITempDataDictionary tempData, string body, string title = "Error Message!")
        {
            AddMessage(tempData, new Message()
            {
                Type = "Danger",
                Title = title,
                Body = body
            });
        }

        public static void AddOkMessage(ITempDataDictionary tempData, string body, string title = "Success!")
        {
            AddMessage(tempData, new Message()
            {
                Type = "Success",
                Title = title,
                Body = body
            });
        }

        public static void AddInfoMessage(ITempDataDictionary tempData, string body, string title = "Info Message!")
        {
            AddMessage(tempData, new Message()
            {
                Type = "Info",
                Title = title,
                Body = body
            });
        }
    }

    public class Message
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
