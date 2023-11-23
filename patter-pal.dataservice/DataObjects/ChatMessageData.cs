﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.dataservice.DataObjects
{
    public class ChatMessageData
    {
        public int Id { get; set; }
        public bool IsUser { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;

        public ChatMessageData(int id, bool isUser, string text, string language)
        {
            Id = id;
            IsUser = isUser;
            Text = text;
            Language = language;
        }
    }
}