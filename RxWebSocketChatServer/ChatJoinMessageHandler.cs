using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatJoinMessageHandler : IObserver<Object>
    {
        readonly ChatRoomManager _chatRoomManager;
        readonly ChatSession _session;

        public ChatJoinMessageHandler(ChatRoomManager chatRoomManager, ChatSession session)
        {
            _chatRoomManager = chatRoomManager;
            _session = session;
        }

        public void OnCompleted()
        {
            ClientLeaves();
            Console.WriteLine("ChatJoinMessage: Completed");
            _chatRoomManager.RemoveFromRoom(_session);
        }

        public void OnError(Exception error)
        {
            try
            {
                ClientLeaves();
                Console.WriteLine("ChatJoinMessage: " + error.Message);
            }
            finally
            {
                _chatRoomManager.RemoveFromRoom(_session);
            }
        }

        private void ClientLeaves()
        {
            if (!String.IsNullOrWhiteSpace(_session.Room))
            {
                Broadcast(new { cls = "msg", message = _session.Nick + " leaves the room.", room = _session.Room, nick = "Server", timestamp = DateTime.Now.ToString("hh:mm:ss") });
            }
        }

        private void Broadcast(Object anounce, params ChatSession[] excluded)
        {
            foreach (var client in _chatRoomManager[_session.Room].Except(excluded))
                client.Out.OnNext(anounce);
        }

        private void checkUserInRoom()
        {
            if (_session.Room != null)
            {
                if (_chatRoomManager.ContainsKey(_session.Room))
                {
                    ChatRoom oldRoom = _chatRoomManager[_session.Room];
                    oldRoom.Remove(_session);
                    ClientLeaves();
                }
            }
        }

        private bool isUserInRoom(string Room)
        {
            if (_session.Room == null)
            {
                return false;
            }
            else if (_session.Room == Room)
            {
                return true;
            }
            return false;
        }

        public void OnNext(Object omsgIn)
        {
            dynamic msgIn = omsgIn;
            if (!isUserInRoom((string)msgIn.room))
            {
                checkUserInRoom();
                String roomName = msgIn.room;
                var room = _chatRoomManager.GetOrAdd(roomName, new ChatRoom(roomName));
                room.Add(_session);
                _session.Nick = msgIn.nick;
                _session.Room = roomName;
                msgIn.participants = new JArray(room.Where(cc => cc.Nick != _session.Nick).Select(x => x.Nick).ToArray());
                Broadcast(new { cls = "msg", message = _session.Nick + " joined the room.", nick = "Server", timestamp = DateTime.Now.ToString("hh:mm:ss") }, _session);
                _session.Out.OnNext(new { cls = "msg", message = "You have joined the room: " + roomName, nick = "Server", timestamp = DateTime.Now.ToString("hh:mm:ss") });
            }
        }
    }
}
